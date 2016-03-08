#pragma once

template <typename value_type>
class Range
{
public:
  class const_iterator
  {
  public:
    const_iterator(const Range* owner, int pos)
    : owner(owner),
      pos(pos)
    {
    }

    bool operator!=(const const_iterator& other) const
    {
      return pos != other.pos;
    }

    int operator*() const
    {
      return pos;
    }

    const const_iterator& operator++()
    {
      ++pos;
      return *this;
    }

  private:
    const Range* owner;
    int pos;
  };


public:
  Range()
  : lVal(),
    hVal()
  {
  }

  Range(const value_type& val)
  : lVal(val),
    hVal(val)
  {
    ensureOrder();
  }

  Range(const value_type& lVal, const value_type& hVal)
  : lVal(lVal),
    hVal(hVal)
  {
    ensureOrder();
  }

  Range(const Range& other)
  {
    *this = other;
  }

  const value_type& l() const
  {
    return lVal;
  }

  const value_type& h() const
  {
    return hVal;
  }

  const_iterator begin() const
  {
    return const_iterator(this, lVal);
  }

  const_iterator end() const
  {
    return const_iterator(this, hVal + 1);
  }

  Range& operator=(const Range& other)
  {
    lVal = other.lVal;
    hVal = other.hVal;
    ensureOrder();

    return *this;
  }

  bool operator<(const Range& other) const
  {
    if (lVal < other.lVal)
      return true;

    return (hVal < other.hVal);
  }

  Range& inflate(const value_type& value)
  {
    if (value < lVal)
      lVal = value;
    else if (value > hVal)
      hVal = value;

    return *this;
  }

  Range& inflate(const Range& other)
  {
    inflate(other.lVal());
    inflate(other.hVal());

    return *this;
  }

  bool overlap(const Range& other) const
  {
    if (lVal >= other.l() && lVal <= other.l())
      return true;

    if (hVal <= other.h() && hVal >= other.h())
      return true;

    return false;
  }

  bool contains(const value_type& val) const
  {
    return (val >= lVal && val <= hVal);
  }

  void clamp(value_type& val) const
  {
    if (val < lVal)
      val = lVal;
    if (val > hVal)
      val = hVal;
  }

private:
  value_type lVal;
  value_type hVal;

  void ensureOrder()
  {
    if (lVal > hVal)
    {
      const value_type temp = lVal;
      lVal = hVal;
      hVal = temp;
    }
  }
};

using Rangei = Range<int>;
using Rangef = Range<float>;
