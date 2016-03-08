#pragma once

#include <Strategy.h>

class DominoBacktrackStrategy : public Strategy
{
public:
  DominoBacktrackStrategy(bool requireCirculation);

protected:
  const string_t& name() const;
  void execute(const Args& args) const;

private:
  bool requireCirculation;

  void backtrackCirculation(const Args& args) const;
  void backtrack(const Args& args) const;
};
