#!/usr/bin/env python

import argparse
import json
from flask import Flask, Response, request, jsonify
from flask.ext.api import status as Status
from flask.ext.cors import CORS

import togo

app = Flask(__name__)
CORS(app)
JobControl = togo.service.TogoJobController()

@app.route('/generator', methods=["POST"])
def generator_post():
    assert request.json != None
    specifications = request.json
    job_id = JobControl.create(specifications=specifications)
    # XXX: job control mechanism needed
    JobControl.start(job_id)
    response = app.response_class(json.dumps(job_id), mimetype='application/json')
    return response

@app.route('/generator', methods=["GET"])
def generator_get():
    job_count = json.dumps(len(JobControl))
    return json_response(job_count)

@app.route('/generator/job/<job_id>', methods=["GET"])
def generator_job_get(job_id):
    try:
        status = JobControl.get_status(job_id)
        return jsonify(status)
    except togo.service.TogoJobIDError, err:
        return "", Status.HTTP_404_NOT_FOUND

@app.route('/generator/job/<job_id>/design/<design_index>', methods=["GET"])
def generator_job_design_get(job_id, design_index):
    try:
        result = JobControl.get_result(job_id)
        return jsonify(result.as_dict())
    except togo.service.TogoJobIDError, err:
        return "", Status.HTTP_404_NOT_FOUND

@app.route('/generator/job/<job_id>/designs', methods=["GET"])
def generator_job_designs_get(job_id):
    return "Hello, World!"

def cli():
    Defaults = {
        "port": 34568,
        "address": "127.0.0.1",
        "debug": False,
    }

    parser = argparse.ArgumentParser(description="Togo design service.")
    parser.add_argument("--port", "-p", type=int, help="Port number to listen on (Default: 34568)")
    parser.add_argument("--address", "-a", help="Host address to listen on (Default: 127.0.0.1)")
    parser.add_argument("--debug", "-d", action="store_true", help="Enable debug mode")
    parser.set_defaults(**Defaults)

    args = parser.parse_args()
    return args

if __name__ == '__main__':
    args = cli()
    app.run(debug=args.debug, port=args.port, host=args.address)
