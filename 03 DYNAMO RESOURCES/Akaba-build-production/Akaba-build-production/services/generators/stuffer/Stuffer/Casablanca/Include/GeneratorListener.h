#pragma once

class LayoutJob;

class GeneratorListener 
{
public:
  static void start(unique_ptr<GeneratorListener> pListener);
  static void stop();

  GeneratorListener(web::http::uri url);
  virtual ~GeneratorListener();

  pplx::task<void> open() { return m_listener.open(); }
  pplx::task<void> close() { return m_listener.close(); }

protected:
  static unique_ptr<GeneratorListener> s_pListener;

  using job_map = map<string_t, unique_ptr<LayoutJob>>;
  job_map m_jobs;
  int m_nextId;

  string_t addJob(const web::json::object& requirements);

  virtual unique_ptr<LayoutJob> createJob(const web::json::object& jobData) const = 0;

private:
  void handle_get(const web::http::http_request& message);
  void handle_put(const web::http::http_request& message);
  void handle_post(const web::http::http_request& message);
  void handle_delete(const web::http::http_request& message);
  void handle_options(const web::http::http_request& message);

  web::http::experimental::listener::http_listener m_listener;
};
