#pragma once

#include <EdgeEffectStrategy.h>

class OutdoorConnectionStrategy : public EdgeEffectStrategy
{
public:
  OutdoorConnectionStrategy(Rangei range);

protected:
  int amount;
  int other;

  const string_t& name() const;
  void execute(const Args& args) const;
  Usage::name_tag getUsage() const;
  unique_ptr<EdgeVisitor> getVisitor(const Args& args, unique_ptr<CellHelper> helper) const;
};
