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

  status_code doGet(const string_t& base, const string_t& request)
  {
    status_code code(status_codes::NotFound);

    http_client getClient(base);
    ucout << U("POST completed, creating GET task") << endl;
    pplx::task<http_response> getTask = getClient.request(methods::GET, request);
    getTask.then([&code](http_response response)
    {
      code = response.status_code();
      ucout << U("Received GET response status code: ") << code << endl;
      ucout << U("Received GET response body: ") << response.body() << endl;
      return;
    });

    http_response response(doHTTPTask(getTask, U("GET")));
    status_code code2(response.status_code());

    ucout << U("Returning GET status code: ") << code << endl;
    return code;
  }
}

status_code GeneratorSpeaker::postJob(const string_t& base, const string_t& request, json::value& data)
{
  http_client postClient(base);
  ucout << U("Creating POST task") << endl;
  pplx::task<http_response> postTask = postClient.request(methods::POST, request, data);
  postTask.then([](http_response response)
  {
    status_code code(response.status_code());
    ucout << U("Received POST response status code: ") << code << endl;
    ucout << U("Received POST response body: ") << response.body() << endl;
    return;
  }).then([base, request]()
  {
    status_code code(status_codes::NotFound);
    int count = 0;
    while (code == status_codes::NotFound && ++count < 10)
    {
      code = doGet(base, request);
      if (code == status_codes::OK)
        break;

      ucout << U("Attempt ") << count << U(": Requested design not found") << endl;
      this_thread::sleep_for(std::chrono::milliseconds(2000));
    }

    return code;
  });

  doHTTPTask(postTask, U("POST"));

  // TODO: Figure out what the correct return values are (OK: 200 = possible graph, NotFound: 404 = no possible graph?)
  status_code ret(status_codes::OK);
  ucout << U("Returning: ") << ret << endl;
  return ret;
}
