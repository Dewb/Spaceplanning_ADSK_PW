#pragma once

#include <Strategy.h>

class SpaceDivisionStrategy : public Strategy
{
protected:
  const string_t& name() const;
  void execute(const Args& args) const;
};
