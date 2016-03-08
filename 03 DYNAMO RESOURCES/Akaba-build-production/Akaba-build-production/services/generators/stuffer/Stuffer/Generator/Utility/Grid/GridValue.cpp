#include <stdafx.h>
#include <GridValue.h>

GridValue::GridValue(bool normal)
: normal(normal),
  value(0)
{
}

GridValue::GridValue(const GridValue& other)
: normal(other.normal),
  value(other.value)
{
}

int GridValue::getValue() const
{
  return value;
}

void GridValue::setValue(int other)
{
  value = other;
}

int GridValue::next() const
{
  return value + step();
}

int GridValue::prev() const
{
  return value - step();
}

void GridValue::inc()
{
  value += step();
}

void GridValue::dec()
{
  value -= step();
}

int GridValue::step() const
{
  return ((normal) ? 1 : -1);
}

bool GridValue::compare(const int other) const
{
  if (normal)
    return (value <= other);
  else
    return (value >= other);
}
