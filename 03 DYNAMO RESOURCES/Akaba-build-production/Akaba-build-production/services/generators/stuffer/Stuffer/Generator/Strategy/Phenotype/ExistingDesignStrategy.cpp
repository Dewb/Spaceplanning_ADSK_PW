#include <stdafx.h>
#include <ExistingDesignStrategy.h>
#include <Args.h>
#include <JobData.h>
#include <JobRequest.h>
#include <BuildData.h>

const string_t& ExistingDesignStrategy::name() const
{
  static string_t name(U("ExistingDesignStrategy"));
  return name;
}

void ExistingDesignStrategy::execute(const Args& args) const
{
  auto designs(args.getJobData().getRequest().requirements.designs);
  auto design(designs.size() > 0 ? &designs[0] : nullptr);
  if (design == nullptr)
    return;

  args.getBuildData().setExistingDesign(*design, args.getPhenotype());
}
