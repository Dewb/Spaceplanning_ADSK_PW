#include <stdafx.h>
#include <DominoAddSpaceStrategy.h>

DominoAddSpaceStrategy::DominoAddSpaceStrategy(bool requireCirculation)
: requireCirculation(requireCirculation)
{
}

const string_t& DominoAddSpaceStrategy::name() const
{
  static string_t name(U("DominoAddSpaceStrategy"));
  return name;
}

void DominoAddSpaceStrategy::execute(const Args& /*args*/) const
{
  // Create space requirements data
  // While requirements not met (or 1000 iterations):
  //   Select space to add
  //   Select direction to try
  //   Using space and direction do until one succeeds (or none)
  //     Action:
  //       Basic place
  //       Basic place, rotate space 90deg
  //       Basic place, reflect direction
  //       Basic place, reflect direction, rotate space 90deg
  //       Maybe move to new level
  //       Maybe move to new level, rotate space 90deg
  //       Maybe move to new level, reflect direction
  //       Maybe move to new level, reflect direction, rotate space 90deg
  //     Test: No overlaps and fulfills site requirements
}
