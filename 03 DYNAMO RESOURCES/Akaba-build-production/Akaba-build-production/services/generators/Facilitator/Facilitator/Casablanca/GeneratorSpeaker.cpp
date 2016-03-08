#include <stdafx.h>
#include <GeneratorSpeaker.h>
#include <cpprest/http_client.h>
#include <cpprest/filestream.h>

using namespace web;
using namespace web::http;
using namespace web::http::client;
using namespace concurrency::streams;

namespace
{
  http_response doHTTPTask(pplx::task<http_response>& task, const string_t& name)
  {
    try
    {
      ucout << U("Before wait... (do ") << name << U(" task)") << endl;
      string_t done(U("...after wait (") + name + U(" task complete)\n"));
      task.wait();

      // TRIVIA; pre-made string here instead of using << operators because of thread switching
      ucout << done;
    }
    catch (const std::exception &e)
    {
      ucout << U("Error exception: ") << e.what() << endl;
    }

    return task.get();
  }

  void doGet(const string_t& base, const string_t& request, string_t& body)
  {
    http_client getClient(base);
    ucout << U("POST completed, creating GET task") << endl;
    pplx::task<http_response> getTask = getClient.request(methods::GET, request);
    getTask.then([&body](http_response response)
    {
      ucout << U("Received GET response status code: ") << response.status_code() << endl;
      ucout << U("Received GET response body: ") << response.body() << endl;
      //body = response.body();
      stringstream_t stream;
      stream << response.body();
      body = stream.str();

      return;
    });

    http_response response(doHTTPTask(getTask, U("GET")));
    ucout << U("GET status code: ") << response.status_code() << endl;
    return;
  }
}

status_code GeneratorSpeaker::postJob(const string_t& base, json::value& data)
{
  http_client postClient(base);
  ucout << U("Creating POST task") << endl;
  pplx::task<http_response> postTask = postClient.request(methods::POST, U("/generator"), data);
  postTask.then([](http_response response)
  {
    status_code code(response.status_code());
    ucout << U("Received POST response status code: ") << code << endl;
    ucout << U("Received POST response body: ") << response.body() << endl;
    return;
  }).then([base]()
  {
    string_t response;
    int count = 0;
    while (response.empty() && ++count < 10)
    {
      doGet(base, U("/generator"), response);
      if (!response.empty())
        break;

      ucout << U("Attempt ") << count << U(": Requested design not found") << endl;
      this_thread::sleep_for(std::chrono::milliseconds(2000));
    }

    string_t jobId;
    if (!response.empty())
    {
      jobId = response;
    }

    return jobId;
  })
    .then([base](string_t jobId)
  {
    if (jobId.empty())
      return status_codes::NotFound;

    string_t response;
    int count = 0;
    while (response.empty() && ++count < 10)
    {
      doGet(base, U("/generator/job/") + jobId + U("/design/1"), response);
      if (!response.empty())
        break;

      ucout << U("Attempt ") << count << U(": Requested design not found") << endl;
      this_thread::sleep_for(std::chrono::milliseconds(2000));
    }

    if (response.empty())
      return status_codes::NotFound;

    string_t out(response); // stream.str());

    return status_codes::OK; // response.status_code();
  })
    ;

  doHTTPTask(postTask, U("POST"));

  // TODO: Figure out what the correct return values are (OK: 200 = possible graph, NotFound: 404 = no possible graph?)
  status_code ret(status_codes::OK);
  ucout << U("Returning: ") << ret << endl;
  return ret;
}
