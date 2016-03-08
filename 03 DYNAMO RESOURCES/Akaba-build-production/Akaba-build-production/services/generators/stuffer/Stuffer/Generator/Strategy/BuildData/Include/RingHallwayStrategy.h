#pragma once

#include <EdgeEffectStrategy.h>

class RingHallwayStrategy : public EdgeEffectStrategy
{
public:
  RingHallwayStrategy(int hallwayGridWidth);

protected:
  const string_t& name() const;
  virtual Usage::name_tag getUsage() const;
  unique_ptr<EdgeVisitor> getVisitor(const Args& args, unique_ptr<CellHelper> helper) const;
};
