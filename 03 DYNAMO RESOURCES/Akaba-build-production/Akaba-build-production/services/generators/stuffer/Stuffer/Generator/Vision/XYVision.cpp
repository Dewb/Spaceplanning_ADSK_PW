#include <stdafx.h>
#include <XYVision.h>
#include <JobRequest.h>

XYVision::XYVision(const JobRequest& jobRequest)
: Vision(jobRequest, GridBasis({ U("x"), U("y") }, jobRequest.settings.grid, jobRequest.settings.floorHeight))
{
}
