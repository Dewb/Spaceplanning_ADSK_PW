#pragma once

class Args;

class Strategy
{
public:
  virtual ~Strategy() = default;

  virtual const string_t& name() const = 0;
  virtual void execute(const Args& args) const = 0;

  template <typename strategy_type>
  static unique_ptr<const Strategy> const_ref(strategy_type& strategy)
  {
    return unique_ptr<const Strategy>(strategy.release());
  }
};
