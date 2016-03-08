#pragma once

#include <Strategy.h>

class ExistingDesignStrategy : public Strategy
{
public:
  const string_t& name() const;
  void execute(const Args& args) const;
};
