#pragma once

#include <Strategy.h>

class ListStrategy : public Strategy
{
public:
  void addStrategy(unique_ptr<const Strategy> pStrategy);

protected:
  const string_t& name() const;
  void execute(const Args& args) const;

private:
  using strategy_list = list<unique_ptr<const Strategy>>;
  strategy_list m_strategies;
  strategy_list::const_iterator m_currentStrategy;
};
