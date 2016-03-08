#include <stdafx.h>
#include <GridRef.h>

GridRef::GridRef(bool normal, const GridValue& majorValue, const GridValue& minorValue)
: normal(normal),
  majorValue(&(normal ? minorValue : majorValue)),
  minorValue(&(normal ? majorValue : minorValue))
{
}

GridRef::operator const Point2i() const
{
  return Point2i(majorValue->getValue(), minorValue->getValue());
}
