#include <stdafx.h>
#include <SetUsageStrategy.h>
#include <Args.h>
#include <JobData.h>
#include <JobRequest.h>
#include <Phenotype.h>
#include <Floor.h>
#include <Section.h>
#include <RangeTable.h>

const string_t& SetUsageStrategy::name() const
{
  static string_t name(U("SetUsageStrategy"));
  return name;
}

void SetUsageStrategy::execute(const Args& args) const
{
  const JobRequest& request(args.getJobData().getRequest());

  RangeTable<int, Usage::name_tag> usageTable;
  for (const auto& it : request.requirements.spaces)
    usageTable.addRange(it.minimumArea, Usage::tag(it.usage));

  auto unknownTag(Usage::tag(U("Unknown")));
  for (auto& floor : args.getPhenotype().getFloors())
  {
    for (auto& section : floor.second->getSections())
    {
      if (section->data->getIntParam(Section::usage, -1) == unknownTag)
      {
        int rangeValue(args.getRibosome().getAmount(Rangei(1, usageTable.getRangeMax())));
        section->data->setIntParam(Section::usage, usageTable.getValue(rangeValue, unknownTag));
      }
    }
  }
}
