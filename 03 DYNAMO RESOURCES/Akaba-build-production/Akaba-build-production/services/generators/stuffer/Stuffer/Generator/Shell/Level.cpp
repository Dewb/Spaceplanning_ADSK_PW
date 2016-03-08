#include <stdafx.h>
#include <Level.h>

Level::Level(const GridBasis& basis)
: grid(basis)
{
}

const Grid& Level::getGrid() const
{
  return grid;
}

Grid& Level::getGrid()
{
  return grid;
}
