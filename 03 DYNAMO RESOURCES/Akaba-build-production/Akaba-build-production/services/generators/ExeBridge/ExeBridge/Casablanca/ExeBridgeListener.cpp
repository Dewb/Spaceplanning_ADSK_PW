#include <stdafx.h>
#include <ExeBridgeListener.h>
#include <LayoutJob.h>

#include <BuildingCoreVision.h>

using namespace web;
using namespace http;

void ExeBridgeListener::start(uri url)
{
  GeneratorListener::start(make_unique<ExeBridgeListener>(url));
}

ExeBridgeListener::ExeBridgeListener(uri url)
: GeneratorListener(url)
{
}

unique_ptr<LayoutJob> ExeBridgeListener::createJob(const json::object& jobData) const
{
  JobRequest jobRequest(JobRequest::FromJSON(jobData));
  unique_ptr<Vision> vision(new BuildingCoreVision(jobRequest));
  unique_ptr<LayoutJob> job(new LayoutJob(move(vision)));
  return job;
}
