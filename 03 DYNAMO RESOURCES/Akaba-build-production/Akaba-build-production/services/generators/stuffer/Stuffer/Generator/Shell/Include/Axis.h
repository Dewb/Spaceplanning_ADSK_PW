#pragma once

class Axis
{
public:
  Axis();

  void addGridLine(int major, const Rangei& range);

private:
  map<int, Rangei> m_gridLines;
};
