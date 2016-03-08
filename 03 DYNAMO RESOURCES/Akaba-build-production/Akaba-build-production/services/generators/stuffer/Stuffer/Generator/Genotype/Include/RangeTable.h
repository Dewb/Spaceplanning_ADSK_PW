#pragma once

template <typename range_value_type, typename value_type>
class RangeTable
{
public:
  using range_type = Range<range_value_type>;

  RangeTable()
  : rangeMax(0)
  {
  }

  void addRange(range_value_type rangeSize, const value_type& value)
  {
    Range<range_value_type> range(rangeMax + 1, rangeMax + rangeSize);
    valueTable.push_back({ range, value });
    rangeMax = range.h();
  }

  range_value_type getRangeMax() const
  {
    return rangeMax;
  }

  const value_type& getValue(range_value_type pt, const value_type& defaultValue) const
  {
    for (const auto& it : valueTable)
      if (it.first.contains(pt))
        return it.second;

    return defaultValue;
  }

private:
  using value_range = pair<range_type, value_type>;
  list<value_range> valueTable;
  range_value_type rangeMax;

  range_type nextRange(int current, int size)
  {
    return range_type(current + 1, current + size);
  }

  range_type nextRange(float current, float size)
  {
    return range_type(current + GridBasis::epsilon, current + size);
  }
};
