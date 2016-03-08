#pragma once

template <typename value_type>
class Point3
{
public:
  Point3()
  : xVal(),
    yVal(),
    zVal()
  {
  }

  Point3(const value_type& xVal, const value_type& yVal, const value_type& zVal)
  : xVal(xVal),
    yVal(yVal),
    zVal(zVal)
  {
  }

  Point3(const Point3& other)
  {
    *this = other;
  }

  Point3(const Point3& p0, const Point3& p1)
  : xVal(p1.x() - p0.x()),
    yVal(p1.y() - p0.y()),
    zVal(p1.z() - p0.z())
  {
  }

  value_type x() const
  {
    return xVal;
  }

  value_type y() const
  {
    return yVal;
  }

  value_type z() const
  {
    return zVal;
  }

  value_type& x()
  {
    return xVal;
  }

  value_type& y()
  {
    return yVal;
  }

  value_type& z()
  {
    return zVal;
  }

  Point3& operator=(const Point3& other)
  {
    xVal = other.xVal;
    yVal = other.yVal;
    zVal = other.zVal;

    return *this;
  }

  value_type distance(const Point3& other) const
  {
    float dx(xVal - other.xVal);
    float dy(yVal - other.yVal);
    float dz(zVal - other.zVal);
    return sqrt(dx*dx + dy*dy + dz*dz);
  }

  void normalize()
  {
    float length = sqrt(xVal*xVal + yVal*yVal + zVal*zVal);

    if (length != 0)
    {
      xVal /= length;
      yVal /= length;
      zVal /= length;
    }
  }

  float dot(const Point3& other) const
  {
    return xVal*other.xVal + yVal*other.yVal + zVal*other.zVal;
  }

  bool operator==(const Point3& other) const
  {
    return (
      xVal == other.xVal &&
      yVal == other.yVal &&
      zVal == other.zVal);
  }

private:
  value_type xVal;
  value_type yVal;
  value_type zVal;
};

using Point3f = Point3<float>;
