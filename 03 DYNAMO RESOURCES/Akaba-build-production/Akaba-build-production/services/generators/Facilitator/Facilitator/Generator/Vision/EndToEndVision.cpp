#include <stdafx.h>
#include <EndToEndVision.h>
#include <JobRequest.h>
#include <GeneratorSpeaker.h>

EndToEndVision::EndToEndVision(const JobRequest& jobRequest)
: Vision(jobRequest)
{
}

unique_ptr<Design> EndToEndVision::execute(std::function<void(unique_ptr<Design>)> callback)
{
  GeneratorSpeaker::postJob(U("http://localhost:34571/"), requestData.AsJSON());
  callback(make_unique<Design>());

  return nullptr;
}
