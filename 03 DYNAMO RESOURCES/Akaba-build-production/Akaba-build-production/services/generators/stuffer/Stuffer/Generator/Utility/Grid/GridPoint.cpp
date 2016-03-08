#include <stdafx.h>
#include <GridPoint.h>

GridPoint::GridPoint(const GridRef& ref)
: normal(ref.normal),
  majorValue(*ref.majorValue),
  minorValue(*ref.minorValue)
{
}

void GridPoint::incMajor()
{
  normal ? majorValue.inc() : minorValue.inc();
}

void GridPoint::decMajor()
{
  normal ? majorValue.dec() : minorValue.dec();
}

void GridPoint::incMinor()
{
  normal ? minorValue.inc() : majorValue.inc();
}

void GridPoint::decMinor()
{
  normal ? minorValue.dec() : majorValue.dec();
}

GridPoint::operator const Point2i() const
{
  return Point2i(majorValue.getValue(), minorValue.getValue());
}
