#pragma once

template <typename value_type>
class Plane3
{
public:
  using point_type = Point3<value_type>;

  Plane3(const point_type& pos, const point_type& n)
  : pos(pos),
    n(n)
  {
  }

  point_type pos;
  point_type n;
};

using Plane3f = Plane3<float>;
