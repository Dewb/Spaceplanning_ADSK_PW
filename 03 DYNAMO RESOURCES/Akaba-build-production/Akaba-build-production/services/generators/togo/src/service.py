import os
import subprocess
import tempfile
import datetime
import uuid
import pickle
import json
from utils import which

PYTHON_EXE = which("pypy") or which("python")
TOGO_EXE = which("togo_runner.py")

class TogoException(Exception):
    pass

class TogoJobIDError(Exception):
    pass

def munge_specification(specification):
    requirements = specification["requirements"]
    site = requirements["site"]
    spaces = requirements["spaces"]
    togo_spec = {}
    togo_spec["site"] = {"size": (site["width"], site["height"])}
    togo_spec["spaces"] = []
    for space in spaces:
        togo_space = {}
        # XXX: ignore circulation for now
        if space.get("circulation", False):
            continue
        togo_space["name"] = space["usage"]
        togo_space["individual_min_area"] = space["minimumArea"]
        togo_space["quantity"] = space.get("minimumCount", 1)
        togo_spec["spaces"].append(togo_space)
    # XXX: circulation defaults
    togo_spec["circulation"] = {"width": 10, "max_circulation": 4}
    return togo_spec

class TogoServiceResult(object):
    Z_SCALE_FACTOR = 3.5

    def __init__(self, spaces):
        self.spaces = spaces

    def json_vector(self, vector, z=0):
        res = [vector[0], vector[1], z * self.Z_SCALE_FACTOR]
        return res

    def as_dict(self):
        spaces = []
        for (name, space) in self.spaces.iteritems():
            json_space = {}
            if space.circulatable:
                # XXX: circulation name hardcoded
                json_space["usageName"] = "hallway"
            else:
                json_space["usageName"] = space.specification.name
            json_space["dimensions"] = self.json_vector(space.size, z=1)
            json_space["position"] = self.json_vector(space.center, z=.5)
            json_space["isCirculation"] = space.circulatable
            spaces.append(json_space)
        container = {"spaces": spaces}
        return container

class TogoJob(object):
    def __init__(self, specification):
        self.process = None
        self.output_filename = None
        self.specification_filename = None
        self.specification = specification
        self.time_submitted = datetime.datetime.now()
        self.time_completed = None
        self.time_started = None
        self.uuid = uuid.uuid4()

    def __del__(self):
        self.cleanup()

    def get_exit_status(self):
        return self.process.returncode

    def write_specification(self):
        (spec_file_fd, spec_filename) = tempfile.mkstemp()
        spec_file = os.fdopen(spec_file_fd, 'wb')
        togo_specification = munge_specification(self.specification)
        pickle.dump(togo_specification, spec_file)
        spec_file.close()
        return spec_filename

    def get_output_filename(self):
        (output_file, output_filename) = tempfile.mkstemp()
        os.close(output_file)
        return output_filename
    
    def get_result(self):
        assert self.is_finished()
        assert os.path.exists(self.output_filename)
        output_file = open(self.output_filename, 'rb')
        result = pickle.load(output_file)
        return result

    def get_job_id(self):
        return str(self.uuid)
    job_id = property(get_job_id)

    # service level API
    def start(self):
        self.time_started = datetime.datetime.now()
        self.specification_filename = self.write_specification()
        self.output_filename = self.get_output_filename()
        cmd = "%s %s --specification %s --output %s" % (PYTHON_EXE, TOGO_EXE, self.specification_filename, self.output_filename)
        self.process = subprocess.Popen(cmd, shell=True, close_fds=True)

    def is_started(self):
        return self.process != None

    def is_finished(self):
        if self.process == None:
            # XXX: raise error?
            return False
        is_finished = self.process.poll() != None
        if is_finished and self.time_completed == None:
            # XXX: likely stale / late
            self.time_completed = datetime.datetime.now()
        return is_finished

    def cleanup(self):
        if self.process != None and not self.is_finished():
            self.process.terminate()
        if self.output_filename and os.path.exists(self.output_filename):
            os.unlink(self.output_filename)
        if self.specification_filename and os.path.exists(self.specification_filename):
            os.unlink(self.specification_filename)

    def get_status(self):
        status = {}
        status["timeSubmitted"] = self.time_submitted.isoformat()
        if self.is_started:
            status["timeStarted"] = self.time_started.isoformat()
            if self.is_finished():
                status["timeCompleted"] = self.time_completed.isoformat()
                status["status"] = "completed"
                status["designCount"] = 1
            else:
                status["status"] = "in-progress"
        else:
            status["status"] = "queued"
        return status

class TogoJobController(object):
    def __init__(self):
        self.jobs = {}

    def get_job(self, job_id):
        try:
            job = self.jobs[job_id]
        except KeyError, err:
            raise TogoJobIDError, err
        return job

    def create(self, specifications):
        job = TogoJob(specifications)
        self.jobs[job.job_id] = job
        return job.job_id

    def start(self, job_id):
        job = self.get_job(job_id)
        job.start()
        return True

    def get_result(self, job_id):
        job = self.get_job(job_id)
        return job.get_result()

    def get_status(self, job_id):
        job = self.get_job(job_id)
        return job.get_status()

    def cleanup(self, job_id):
        job = self.get_job(job_id)
        del self[self.job_id]
        job.cleanup()

    def __len__(self):
        return len(self.jobs)
