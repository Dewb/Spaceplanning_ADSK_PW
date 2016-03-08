#pragma once

template <typename value_type>
class Facet3
{
public:
  Facet3()
  {
    pt[0] = Point3<value_type>();
    pt[1] = Point3<value_type>();
    pt[2] = Point3<value_type>();
    n = Point3<value_type>();
  }

  Point3<value_type> pt[3];
  Point3<value_type> n;
};

using Facet3f = Facet3<float>;
