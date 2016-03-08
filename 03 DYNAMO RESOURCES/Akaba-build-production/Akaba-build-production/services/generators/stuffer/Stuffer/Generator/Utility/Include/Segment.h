#pragma once

template <template<class> class point_type, typename value_type>
class Segment
{
public:
  Segment()
  {
    pt[0] = point_type<value_type>();
    pt[1] = point_type<value_type>();
  }

  Segment(const point_type<value_type>& p0, const point_type<value_type>& p1)
  {
    pt[0] = p0;
    pt[1] = p1;
  }

  bool empty() const
  {
    return pt[0] == pt[1];
  }

  value_type length() const
  {
    return pt[0].distance(pt[1]);
  }

  point_type<value_type> pt[2];
};

using Segment2f = Segment<Point2, float>;
using Segment3f = Segment<Point3, float>;
