#pragma once

#include <LayoutJob.h>
#include <JobRequest.h>

class Vision;

class GeneratorGraphicsAPI;

class StufferJob : public LayoutJob
{
public:
  StufferJob(GeneratorGraphicsAPI* graphics, const web::json::object& jobData);
  ~StufferJob();

protected:
  unique_ptr<Design> generateDesign();

private:
  JobRequest jobRequest;
  shared_ptr<Vision> vision;
  GeneratorGraphicsAPI* graphics;
};
