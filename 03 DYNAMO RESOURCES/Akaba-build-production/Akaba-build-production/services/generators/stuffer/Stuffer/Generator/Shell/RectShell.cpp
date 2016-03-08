#include <stdafx.h>
#include <RectShell.h>

RectShell::RectShell(
  const Point2f size,
  const unsigned int numFloors,
  const GridBasis& basis)
: Shell(basis)
{
  for (unsigned int index = 0; index < numFloors; ++index)
  {
    auto& level(createLevel(index));
    auto& grid(level.getGrid());
    Rangei xRange(0, basis.toGrid(size.x()));
    Rangei yRange(0, basis.toGrid(size.y()));
    for (auto xIndex(xRange.l()); xIndex <= xRange.h(); ++xIndex)
      grid.addLine(U("x"), xIndex, yRange);
    for (auto yIndex(yRange.l()); yIndex <= yRange.h(); ++yIndex)
      grid.addLine(U("y"), yIndex, xRange);
  }
}
