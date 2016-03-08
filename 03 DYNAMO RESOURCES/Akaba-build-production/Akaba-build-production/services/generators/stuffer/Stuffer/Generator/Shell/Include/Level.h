#pragma once
#include <Grid.h>

class Level
{
public:
  Level(const GridBasis& basis);

  const Grid& getGrid() const;
  Grid& getGrid();

private:
  Grid grid;
};
