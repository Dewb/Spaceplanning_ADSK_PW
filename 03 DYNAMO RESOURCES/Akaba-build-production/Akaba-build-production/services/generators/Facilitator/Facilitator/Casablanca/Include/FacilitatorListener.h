#pragma once

#include <GeneratorListener.h>

class FacilitatorListener : public GeneratorListener
{
public:
  static void start(web::http::uri url);

  FacilitatorListener(web::http::uri url);

protected:
  unique_ptr<LayoutJob> createJob(const web::json::object& jobData) const;
};
