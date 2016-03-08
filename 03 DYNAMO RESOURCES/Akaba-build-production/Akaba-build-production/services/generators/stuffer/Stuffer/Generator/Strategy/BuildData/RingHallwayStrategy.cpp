#include <stdafx.h>
#include <RingHallwayStrategy.h>
#include <CellHelperNotClaimed.h>

RingHallwayStrategy::RingHallwayStrategy(int hallwayGridWidth)
{
  setRange(Rangei(hallwayGridWidth, hallwayGridWidth));
  setShutoff(0);
}

const string_t& RingHallwayStrategy::name() const
{
  static string_t name(U("RingHallwayStrategy"));
  return name;
}

Usage::name_tag RingHallwayStrategy::getUsage() const
{
  return Usage::tag(U("Hall"));
}

unique_ptr<EdgeVisitor> RingHallwayStrategy::getVisitor(const Args& args, unique_ptr<CellHelper> helper) const
{
  return EdgeEffectStrategy::getVisitor(args, make_unique<CellHelperNotClaimed>());
}
