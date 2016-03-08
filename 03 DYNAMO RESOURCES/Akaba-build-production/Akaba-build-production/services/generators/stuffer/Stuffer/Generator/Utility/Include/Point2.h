#pragma once

#include <Point3.h>

template <typename value_type>
class Point2
{
public:
  Point2()
  : xVal(),
    yVal()
  {
  }

  Point2(const value_type& xVal, const value_type& yVal)
  : xVal(xVal),
    yVal(yVal)
  {
  }

  Point2(const Point2& other)
  {
    *this = other;
  }

  value_type x() const
  {
    return xVal;
  }

  value_type y() const
  {
    return yVal;
  }

  value_type& x()
  {
    return xVal;
  }

  value_type& y()
  {
    return yVal;
  }

  Point2& operator=(const Point2& other)
  {
    xVal = other.xVal;
    yVal = other.yVal;

    return *this;
  }

  Point2& operator+=(const Point2& rhs)
  {
    xVal += rhs.xVal;
    yVal += rhs.yVal;

    return *this;
  }

  Point2& operator-=(const Point2& rhs)
  {
    xVal -= rhs.xVal;
    yVal -= rhs.yVal;

    return *this;
  }

  Point2& operator/=(const Point2& rhs)
  {
    xVal /= rhs.xVal;
    yVal /= rhs.yVal;

    return *this;
  }

  Point2& operator*=(const Point2& rhs)
  {
    xVal *= rhs.xVal;
    yVal *= rhs.yVal;

    return *this;
  }

  Point2& operator+=(const value_type& rhs)
  {
    Point2 val(rhs, rhs);
    *this += val;

    return *this;
  }

  Point2& operator-=(const value_type& rhs)
  {
    Point2 val(rhs, rhs);
    *this -= val;

    return *this;
  }

  Point2& operator/=(const value_type& rhs)
  {
    Point2 val(rhs, rhs);
    *this /= val;

    return *this;
  }

  Point2& operator*=(const value_type& rhs)
  {
    Point2 val(rhs, rhs);
    *this *= val;

    return *this;
  }

  bool operator<(const Point2& other)
  {
    if (yVal < other.yVal)
      return true;

    if (xVal < other.xVal)
      return true;

    return false;
  }

  bool operator==(const Point2& other) const
  {
    return (
      xVal == other.xVal &&
      yVal == other.yVal);
  }

  value_type distance(const Point2& other) const
  {
    float dx(xVal - other.xVal);
    float dy(yVal - other.yVal);
    return sqrt(dx*dx + dy*dy);
  }

  void normalize()
  {
    float length = sqrt(xVal*xVal + yVal*yVal);

    if (length != 0)
    {
      xVal /= length;
      yVal /= length;
    }
  }

private:
  value_type xVal;
  value_type yVal;
};

template <typename value_type>
Point2<value_type> operator+(Point2<value_type> lhs, const Point2<value_type>& rhs)
{
  return lhs += rhs;
}

template <typename value_type>
Point2<value_type> operator-(Point2<value_type> lhs, const Point2<value_type>& rhs)
{
  return lhs -= rhs;
}

template <typename value_type>
Point2<value_type> operator*(Point2<value_type> lhs, const Point2<value_type>& rhs)
{
  return lhs *= rhs;
}

template <typename value_type>
Point2<value_type> operator/(Point2<value_type> lhs, const Point2<value_type>& rhs)
{
  return lhs /= rhs;
}

template <typename value_type>
Point2<value_type> operator+(Point2<value_type> lhs, const value_type& rhs)
{
  return lhs += Point2<value_type>(rhs, rhs);
}

template <typename value_type>
Point2<value_type> operator-(Point2<value_type> lhs, const value_type& rhs)
{
  return lhs -= Point2<value_type>(rhs, rhs);
}

template <typename value_type>
Point2<value_type> operator*(Point2<value_type> lhs, const value_type& rhs)
{
  return lhs *= Point2<value_type>(rhs, rhs);
}

template <typename value_type>
Point2<value_type> operator/(Point2<value_type> lhs, const value_type& rhs)
{
  return lhs /= Point2<value_type>(rhs, rhs);
}

using Point2i  = Point2<int>;
using Point2ui = Point2<unsigned int>;
using Point2f  = Point2<float>;
