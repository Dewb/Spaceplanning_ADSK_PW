#include <stdafx.h>
#include <GeneratorListener.h>
#include <LayoutJob.h>
#include <SpaceLayout.h>

using namespace std;
using namespace web;
using namespace http;
using namespace experimental::listener;
using namespace chrono;

unique_ptr<GeneratorListener> GeneratorListener::s_pListener;

void GeneratorListener::start(unique_ptr<GeneratorListener> pListener)
{
  ucout << U("Starting listener\n");
  s_pListener.reset(pListener.release());
  try
  {
    (*s_pListener)
      .open()
      .then([](){ ucout << U("Starting to listen\n"); })
      .wait();
  }
  catch (exception const & e)
  {
    wcout << e.what() << endl;
  }
}

void GeneratorListener::stop()
{
  if (s_pListener)
  {
    ucout << U("Stopping listener...");
    s_pListener->close().wait();
    s_pListener.reset();
    ucout << U(" done.") << endl;
  }
}

GeneratorListener::GeneratorListener(web::http::uri url)
: m_listener(url),
  m_nextId(1)
{
  m_listener.support(methods::GET,     bind(&GeneratorListener::handle_get,     this, placeholders::_1));
  m_listener.support(methods::PUT,     bind(&GeneratorListener::handle_put,     this, placeholders::_1));
  m_listener.support(methods::POST,    bind(&GeneratorListener::handle_post,    this, placeholders::_1));
  m_listener.support(methods::DEL,     bind(&GeneratorListener::handle_delete,  this, placeholders::_1));
  m_listener.support(methods::OPTIONS, bind(&GeneratorListener::handle_options, this, placeholders::_1));
}

GeneratorListener::~GeneratorListener() = default;

string_t GeneratorListener::addJob(const json::object& jobData)
{
  utility::ostringstream_t nextIdString;
  nextIdString << m_nextId++;

  unique_ptr<LayoutJob> job(createJob(jobData));
  job->startJob();
  m_jobs[nextIdString.str()] = move(job);

  return nextIdString.str();
}

string_t time()
{
  time_t rawTime(chrono::system_clock::to_time_t(chrono::system_clock::now()));
#ifdef _WIN32
  static const size_t bufferSize(26);
  static char_t buffer[bufferSize];
  _wctime_s(buffer, bufferSize, &rawTime);
  return buffer;
#else
  return ctime(&rawTime);
#endif
}

void GeneratorListener::handle_get(const http_request& message)
{
  ucout << U("F -> GET: ") << time();
  auto paths = http::uri::split_path(http::uri::decode(message.relative_uri().path()));

  http_response response(status_codes::OK);
  //response.headers().add(U("Access-Control-Allow-Origin"), U("http://localhost:8080"));
  response.headers().add(U("Access-Control-Allow-Origin"), U("*"));

  // Handle GET /generator
  if (paths.empty()) 
  {
    message.reply(status_codes::OK, json::value::number((int)m_jobs.size()));
    return;
  }

  string_t route = paths[0];
  if (route == U("job") && paths.size() >= 2) 
  {
    string_t job_id = paths[1];
    ucout << U("Job: ") << job_id;

    auto found(m_jobs.find(job_id));
    if (found == m_jobs.end()) 
    {
      ucout << U(" -> Not Found...") << endl;
      response.set_status_code(status_codes::NotFound);
      message.reply(response);
      return;
    }
    else 
    {
      if (paths.size() == 2) 
      {
        ucout << U(" -> Found, returning design.") << endl;
        // ucout << found->second->AsJSON();

        // Handle GET /generator/job/[job id]
        response.set_body(found->second->AsJSON());
      }
      else if (paths.size() == 3 && paths[2] == U("designs")) 
      {
        ucout << U(" -> Found, returning all designs.") << endl;
        // Handle GET /generator/job/[job id]/designs
        response.set_body(found->second->DesignsAsJSON());
      }
      else if (paths.size() == 4 && paths[2] == U("design")) 
      {
        // Handle GET /generator/job/[job id]/design/[design index]
        string_t design_index = paths[3];
        unsigned int index = static_cast<unsigned int>(stoi(design_index) - 1);
        if (index >= 0 && index < found->second->getDesigns().size())
        {
          ucout << U(" -> Found, returning design# ") << design_index << endl;
          Design* design(found->second->getDesigns()[index].get());
          if (design != nullptr)
            response.set_body(design->AsJSON());
          else
          {
            ucout << U(" -> Design is null: (Not Found)...") << endl;
            response.set_status_code(status_codes::NotFound);
            message.reply(response);
            return;
          }
        }
        else 
        {
          ucout << U(" -> Found, Design: ") << design_index << U(" Not Found...") << endl;
          response.set_status_code(status_codes::NotFound);
        }
      }
      else 
      {
        ucout << U(" -> Error (Bad Syntax)") << endl;
        response.set_status_code(status_codes::NotFound);
      }

      message.reply(response);
      return;
    }
  }

  ucout << U("Error (Bad Syntax)") << endl;
  response.set_status_code(status_codes::NotFound);
  message.reply(response);
};

#define POST_REPLY
void GeneratorListener::handle_post(const http_request& message)
{
  ucout << U("F -> POST: ") << time();
  auto paths = uri::split_path(uri::decode(message.relative_uri().path()));

  http_response response(status_codes::OK);
  //response.headers().add(U("Access-Control-Allow-Origin"), U("http://localhost:8080"));
  response.headers().add(U("Access-Control-Allow-Origin"), U("*"));
  response.headers().add(U("Access-Control-Allow-Headers"), U("content-type"));

  // Handle POST /generator
  if (paths.empty()) 
  {
    const auto& rawjson(message.extract_json().get());
    if (!rawjson.is_null() && rawjson.is_object())
      response.set_body(json::value::string(addJob(rawjson.as_object())));

#ifdef POST_REPLY
    message.reply(response);
#endif

    return;
  }

  // All other POSTs disallowed
  response.set_status_code(status_codes::MethodNotAllowed);
  message.reply(response);
};

void GeneratorListener::handle_delete(const http_request& message)
{
  ucout << U("F -> DELETE: ") << time();
  auto paths = uri::split_path(uri::decode(message.relative_uri().path()));

  http_response response(status_codes::OK);
  //response.headers().add(U("Access-Control-Allow-Origin"), U("http://localhost:8080"));
  response.headers().add(U("Access-Control-Allow-Origin"), U("*"));

  string_t route = paths[0];
  if (route == U("job")) 
  {
    string_t job_id = paths[1];

    // Handle DELETE /generator/job/[job id]
    auto found(m_jobs.find(job_id));
    if (found == m_jobs.end())
    {
      response.set_status_code(status_codes::NotFound);
      message.reply(response);
      return;
    }

    m_jobs.erase(found);
    message.reply(response);
    return;
  }

  // All other DELETEs disallowed
  response.set_status_code(status_codes::MethodNotAllowed);
  message.reply(response);
  return;
};

void GeneratorListener::handle_put(const http_request& message)
{
  ucout << U("F -> PUT: ") << time();

  http_response response(status_codes::MethodNotAllowed);
  //response.headers().add(U("Access-Control-Allow-Origin"), U("http://localhost:8080"));
  response.headers().add(U("Access-Control-Allow-Origin"), U("*"));

  message.reply(response);
};

void GeneratorListener::handle_options(const http_request& message) 
{
  ucout << U("F -> OPTIONS: ") << time();

  http_response response(status_codes::OK);
  // TODO: Make this conditional on URL, also add header when we return MethodNotAllowed
  response.headers().add(U("Allow"), "GET, POST, DELETE");
  //response.headers().add(U("Access-Control-Allow-Origin"), U("http://localhost:8080"));
  response.headers().add(U("Access-Control-Allow-Origin"), U("*"));
  response.headers().add(U("Access-Control-Allow-Headers"), U("content-type"));
  message.reply(response);
};
