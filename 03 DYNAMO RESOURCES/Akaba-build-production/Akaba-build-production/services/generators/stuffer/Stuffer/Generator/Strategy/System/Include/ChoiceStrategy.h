#pragma once

#include <Strategy.h>
#include <RangeTable.h>

class ChoiceStrategy : public Strategy
{
public:
  void addStrategy(float range, unique_ptr<const Strategy> strategy);

protected:
  list<unique_ptr<const Strategy>> strategies;
  RangeTable<float, const Strategy*> ranges;

  const string_t& name() const;
  void execute(const Args& args) const;
};
