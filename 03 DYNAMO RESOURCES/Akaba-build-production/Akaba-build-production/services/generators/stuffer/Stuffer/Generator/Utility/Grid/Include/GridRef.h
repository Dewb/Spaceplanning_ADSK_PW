#pragma once

class GridPoint;

class GridRef
{
  friend class GridPoint;

public:
  GridRef(bool normal, const GridValue& majorValue, const GridValue& minorValue);

  operator const Point2i() const;

private:
  bool normal;
  const GridValue* majorValue;
  const GridValue* minorValue;
};
