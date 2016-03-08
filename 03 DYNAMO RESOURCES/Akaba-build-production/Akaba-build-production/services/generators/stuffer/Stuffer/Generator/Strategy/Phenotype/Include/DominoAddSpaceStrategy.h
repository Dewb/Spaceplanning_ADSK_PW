#pragma once

#include <Strategy.h>

class DominoAddSpaceStrategy : public Strategy
{
public:
  DominoAddSpaceStrategy(bool requireCirculation);

protected:
  const string_t& name() const;
  void execute(const Args& args) const;

private:
  bool requireCirculation;
};
