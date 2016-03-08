
#include "stdafx.h"
#include "SpaceLayout.h"
#include "BuildingGenerator.h"
#include "GeneratorListener.h"

#include <chrono>
#include "boost/date_time/posix_time/posix_time.hpp"

using namespace std;
using namespace web; 
using namespace utility;
using namespace http;
using namespace web::http::experimental::listener;
using namespace std::chrono;

using boost::posix_time::ptime;
using boost::posix_time::microsec_clock;
using boost::posix_time::to_iso_extended_string;

LSystem g_system;
BuildingGenerator g_building_gen;

class Job {
public:
    
    std::vector<std::shared_ptr<Design> > designs;
    
    ptime timeSubmitted;
    ptime timeStarted;
    ptime timeCompleted;

    bool completed;

    Job() {
        timeSubmitted = microsec_clock::universal_time();
        completed = false;
    }

    void generateDesigns(int count, int iterations) {
        timeStarted = microsec_clock::universal_time();
        while (count-- > 0) {
            std::shared_ptr<Design> design = std::make_shared<Design>();
            
            BuildingGeneratorState state;
            state.position = Vec3f(0, 0, 0);
            state.results = &design->spaces;
            g_system.reseed();
            g_building_gen.generate(g_system, state, iterations);

            designs.push_back(design);
        }
        completed = true;
        timeCompleted = microsec_clock::universal_time();
    }

    web::json::value AsJSON() const {
        web::json::value result = web::json::value::object();
        result[STATUS] = web::json::value::string(completed ? "completed" : "in-progress");
        result[DESIGNCOUNT] = web::json::value::number((int)designs.size());
        result[TIMESUBMITTED] = web::json::value::string(to_iso_extended_string(timeSubmitted) + "Z");
        result[TIMESTARTED] = web::json::value::string(to_iso_extended_string(timeStarted) + "Z");
        if (completed) {
            result[TIMECOMPLETED] = web::json::value::string(to_iso_extended_string(timeCompleted) + "Z");
        }

        return result;
    }

    web::json::value DesignsAsJSON() const {
        web::json::value jdesigns = web::json::value::array(designs.size());
        int idx = 0;
        for (auto& design : designs) {
            jdesigns[idx++] = design->AsJSON();
        }
        return jdesigns;
    }
};

map<utility::string_t, std::shared_ptr<Job> > s_jobs;
int nextId = 1;


GeneratorListener::GeneratorListener(utility::string_t url) : m_listener(url) {
    m_listener.support(methods::GET,  std::bind(&GeneratorListener::handle_get, this, std::placeholders::_1));
    m_listener.support(methods::PUT,  std::bind(&GeneratorListener::handle_put, this, std::placeholders::_1));
    m_listener.support(methods::POST, std::bind(&GeneratorListener::handle_post, this, std::placeholders::_1));
    m_listener.support(methods::DEL,  std::bind(&GeneratorListener::handle_delete, this, std::placeholders::_1));
    m_listener.support(methods::OPTIONS, std::bind(&GeneratorListener::handle_options, this, std::placeholders::_1));

    g_system.setAxiom("E[++C-V]B");
    g_system.addRule("B", "[B]-[B]").setProbability(0.55);
    g_system.addRule("B", "P_dS").setProbability(0.15);
    g_system.addRule("B", "P_lS").setProbability(0.15);
    g_system.addRule("B", "P_rS").setProbability(0.15);
    g_system.addRule("P_d", "C[+F][-F]P_d").setProbability(0.9);
    g_system.addRule("P_d", "C[+F][-F]-P_l").setProbability(0.05);
    g_system.addRule("P_d", "C[+F][-F]+P_r").setProbability(0.05);
    g_system.addRule("P_l", "C_n[+F]P_l").setProbability(0.9);
    g_system.addRule("P_l", "C_n[+F]-P_d").setProbability(0.1);
    g_system.addRule("P_r", "C_n[-F]P_r").setProbability(0.9);
    g_system.addRule("P_r", "C_n[-F]+P_d").setProbability(0.1);
    g_system.addRule("C_n", "+C").setProbability(0.2);
    g_system.addRule("C_n", "-C").setProbability(0.2);
    g_system.addRule("C_n", "C").setProbability(0.6);

    milliseconds ms = duration_cast<milliseconds>(high_resolution_clock::now().time_since_epoch());
    g_system.reseed(ms.count());

    defaultIterationCount = 12;
}

void GeneratorListener::setDefaultIterationCount(int iterations) {
    defaultIterationCount = iterations;
}


std::string GeneratorListener::testLSystem(int iterations) {
    return to_string(g_system.generate(iterations, false));
}

void GeneratorListener::handle_get(http_request message) {
    ucout <<  message.to_string() << endl;
    auto paths = http::uri::split_path(http::uri::decode(message.relative_uri().path()));

    http_response response(status_codes::OK);
    //response.headers().add("Access-Control-Allow-Origin", "http://localhost:8080");
    response.headers().add("Access-Control-Allow-Origin", "*");

    // Handle GET /generator
    if (paths.empty()) {
        message.reply(status_codes::OK, web::json::value::number((int)s_jobs.size()));
        return;
    }

    utility::string_t route = paths[0];
    if (route == "job" && paths.size() >= 2) {
        utility::string_t job_id = paths[1];

        auto found = s_jobs.find(job_id);
        if (found == s_jobs.end()) {
            response.set_status_code(status_codes::NotFound);
            message.reply(response);
            return;
        } else {
            if (paths.size() == 2) {
                // Handle GET /generator/job/[job id]
                response.set_body(found->second->AsJSON());
            } else if (paths.size() == 3 && paths[2] == "designs") {
                // Handle GET /generator/job/[job id]/designs
                response.set_body(found->second->DesignsAsJSON());
            } else if (paths.size() == 4 && paths[2] == "design") {
                // Handle GET /generator/job/[job id]/design/[design index]
                utility::string_t design_index = paths[3];
                int index = stoi(design_index);
                if (index >= 0 && index < found->second->designs.size()) {
                    response.set_body(found->second->designs[index]->AsJSON());
                } else {
                    response.set_status_code(status_codes::NotFound);
                }
            } else {
                response.set_status_code(status_codes::NotFound);
            }

            message.reply(response);
            return;
        }
    }

    response.set_status_code(status_codes::NotFound);
    message.reply(response);
};

void GeneratorListener::handle_post(http_request message) {
    ucout <<  message.to_string() << endl;
    auto paths = uri::split_path(uri::decode(message.relative_uri().path()));

    http_response response(status_codes::OK);
    //response.headers().add("Access-Control-Allow-Origin", "http://localhost:8080");
    response.headers().add("Access-Control-Allow-Origin", "*");
    response.headers().add("Access-Control-Allow-Headers", "content-type");

    // Handle POST /generator
    if (paths.empty()) {
        utility::ostringstream_t nextIdString;
        nextIdString << nextId;

        std::shared_ptr<Job> job = std::make_shared<Job>();
        job->generateDesigns(3, defaultIterationCount);
        s_jobs[nextIdString.str()] = job;
        nextId += 1;

        response.set_body(web::json::value::string(nextIdString.str()));
        message.reply(response);
        return;
    }

    // All other POSTs disallowed
    response.set_status_code(status_codes::MethodNotAllowed);
    message.reply(response);
};
 
void GeneratorListener::handle_delete(http_request message) {
    ucout <<  message.to_string() << endl;
    auto paths = uri::split_path(uri::decode(message.relative_uri().path()));

    http_response response(status_codes::OK);
    //response.headers().add("Access-Control-Allow-Origin", "http://localhost:8080");
    response.headers().add("Access-Control-Allow-Origin", "*");

    utility::string_t route = paths[0];
    if (route == "job") {
        utility::string_t job_id = paths[1];

        // Handle DELETE /generator/job/[job id]
        auto found = s_jobs.find(job_id);
        if (found == s_jobs.end()) {
            response.set_status_code(status_codes::NotFound);
            message.reply(response);
        } else {
            s_jobs.erase(found);
            message.reply(response);
        }
    }

    // All other DELETEs disallowed
    response.set_status_code(status_codes::MethodNotAllowed);
    message.reply(response);
    return;
};

void GeneratorListener::handle_put(http_request message) {
    ucout <<  message.to_string() << endl;

    http_response response(status_codes::MethodNotAllowed);
    //response.headers().add("Access-Control-Allow-Origin", "http://localhost:8080");
    response.headers().add("Access-Control-Allow-Origin", "*");

    message.reply(response);
};

void GeneratorListener::handle_options(http_request message) {
    ucout <<  message.to_string() << endl;

    http_response response(status_codes::OK);
    response.headers().add(U("Allow"), "GET, POST, DELETE"); // todo: make this conditional on URL, also add header when we return MethodNotAllowed
    //response.headers().add("Access-Control-Allow-Origin", "http://localhost:8080");
    response.headers().add("Access-Control-Allow-Origin", "*");
    response.headers().add("Access-Control-Allow-Headers", "content-type");
    message.reply(response);
};
