#pragma once

#include <Axis.h>

class Grid
{
public:
  Grid(const GridBasis& basis);

  void addLine(const string_t& axis, int major, const Rangei& line);

  using line_map = map<int, vector<Rangei>>;
  const line_map& getLines(const string_t& axis) const;

private:
  using axis_map = map<const string_t, line_map>;
  axis_map m_axis;
};
