#include <stdafx.h>
#include <Grid.h>

Grid::Grid(const GridBasis& basis)
{
  for (auto& name : basis.axis())
    m_axis[name] = line_map();
}

void Grid::addLine(const string_t& axis, int major, const Rangei& range)
{
  m_axis[axis][major].push_back(range);
}

const Grid::line_map& Grid::getLines(const string_t& axis) const
{
  return m_axis.at(axis);
}
