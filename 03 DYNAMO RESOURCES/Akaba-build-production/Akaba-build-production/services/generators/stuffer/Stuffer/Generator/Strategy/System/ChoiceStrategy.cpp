#include <stdafx.h>
#include <ChoiceStrategy.h>
#include <Args.h>

const string_t& ChoiceStrategy::name() const
{
  static string_t name(U("ChoiceStrategy"));
  return name;
}

void ChoiceStrategy::addStrategy(float range, unique_ptr<const Strategy> strategy)
{
  if (range <= 0.0)
    return;

  // NOTE: Strategy can be nullptr
  ranges.addRange(range, strategy.get());
  strategies.push_back(move(strategy));
}

void ChoiceStrategy::execute(const Args& args) const
{
  if (ranges.getRangeMax() <= 0.0f)
    return;

  auto rangeValue(args.getRibosome().getValue(ranges.getRangeMax()));
  auto strategy(ranges.getValue(rangeValue, nullptr));
  if (strategy)
    strategy->execute(args);
}
