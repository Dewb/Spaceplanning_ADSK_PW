#pragma once

class GridPoint
{
public:
  GridPoint(const GridRef& ref);

  void incMajor();
  void decMajor();

  void incMinor();
  void decMinor();

  operator const Point2i() const;

private:
  bool normal;
  GridValue majorValue;
  GridValue minorValue;
};
