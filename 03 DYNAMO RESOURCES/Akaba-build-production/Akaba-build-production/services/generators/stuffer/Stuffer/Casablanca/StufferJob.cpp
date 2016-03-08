#include <stdafx.h>
#include <StufferJob.h>
#include <Vision.h>
#include <GeneratorGraphicsAPI.h>

//#include <ShellAndCoreVision.h>
//using vision_type = ShellAndCoreVision;
#include <TrapelloVision.h>
using vision_type = TrapelloVision;
//#include <StairPlacementVision.h>
//using vision_type = StairPlacementVision;
//#include <BuildingCoreVision.h>
//using vision_type = BuildingCoreVision;

using namespace web;

StufferJob::StufferJob(GeneratorGraphicsAPI* graphics, const json::object& jobData)
: jobRequest(JobRequest::FromJSON(jobData)),
  graphics(graphics),
  vision(new vision_type(jobRequest))
{
}

StufferJob::~StufferJob() = default;

unique_ptr<Design> StufferJob::generateDesign()
{
  unique_ptr<Design> design(vision->execute());
  if (graphics)
    graphics->displayPlanState(vision);

  return design;
}
