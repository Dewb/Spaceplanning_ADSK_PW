#include <stdafx.h>
#include <Shell.h>

Shell::Shell(const GridBasis& basis)
: basis(basis)
{
}

Level& Shell::createLevel(int levelNum)
{
  return *(levels[levelNum] = unique_ptr<Level>(new Level(basis)));
}

const Shell::level_map& Shell::getLevels() const
{
  return levels;
}
