#include <stdafx.h>
#include <Axis.h>

Axis::Axis()
{
}

void Axis::addGridLine(int major, const Rangei& range)
{
  m_gridLines[major] = range;
}
