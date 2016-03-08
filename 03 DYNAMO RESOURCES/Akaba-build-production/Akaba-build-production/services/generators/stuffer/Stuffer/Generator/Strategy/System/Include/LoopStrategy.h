#pragma once

#include <Strategy.h>

class LoopStrategy : public Strategy
{
public:
  LoopStrategy(unique_ptr<const Strategy> strategy, int count);

protected:
  const string_t& name() const;
  void execute(const Args& args) const;

private:
  unique_ptr<const Strategy> strategy;
  int count;
};
