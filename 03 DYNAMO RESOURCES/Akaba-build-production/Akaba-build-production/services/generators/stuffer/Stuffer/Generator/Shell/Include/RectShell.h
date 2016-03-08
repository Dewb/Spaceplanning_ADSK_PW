#pragma once

#include <Shell.h>

class RectShell : public Shell
{
public:
  RectShell(
    const Point2f size,
    const unsigned int z,
    const GridBasis& basis);
};
