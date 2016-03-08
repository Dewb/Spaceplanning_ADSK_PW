#include <stdafx.h>
#include <DominoBacktrackStrategy.h>

DominoBacktrackStrategy::DominoBacktrackStrategy(bool requireCirculation)
: requireCirculation(requireCirculation)
{
}

const string_t& DominoBacktrackStrategy::name() const
{
  static string_t name(U("DominoBacktrackStrategy"));
  return name;
}

void DominoBacktrackStrategy::execute(const Args& args) const
{
  if (requireCirculation)
    backtrackCirculation(args);
  else
    backtrack(args);
}

void DominoBacktrackStrategy::backtrackCirculation(const Args& /*args*/) const
{
}

void DominoBacktrackStrategy::backtrack(const Args& /*args*/) const
{
}
