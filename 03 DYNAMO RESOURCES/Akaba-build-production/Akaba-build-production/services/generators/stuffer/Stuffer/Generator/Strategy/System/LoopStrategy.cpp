#include <stdafx.h>
#include <LoopStrategy.h>

LoopStrategy::LoopStrategy(unique_ptr<const Strategy> strategy, int count)
: strategy(move(strategy)),
  count(count)
{
}

const string_t& LoopStrategy::name() const
{
  static string_t name(U("LoopStrategy"));
  return name;
}

void LoopStrategy::execute(const Args& args) const
{
  for (auto index = 0; index < count; ++index)
    strategy->execute(args);
}
