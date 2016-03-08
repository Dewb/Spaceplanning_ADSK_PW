#include <stdafx.h>
#include <FacilitatorListener.h>
#include <LayoutJob.h>

#include <EndToEndVision.h>

using namespace web;
using namespace http;

void FacilitatorListener::start(uri url)
{
  GeneratorListener::start(make_unique<FacilitatorListener>(url));
}

FacilitatorListener::FacilitatorListener(uri url)
: GeneratorListener(url)
{
}

unique_ptr<LayoutJob> FacilitatorListener::createJob(const json::object& jobData) const
{
  JobRequest jobRequest(JobRequest::FromJSON(jobData));
  unique_ptr<Vision> vision(new EndToEndVision(jobRequest));
  unique_ptr<LayoutJob> job(new LayoutJob(move(vision)));
  return job;
}
