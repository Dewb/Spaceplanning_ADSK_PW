#pragma once

#include <Segment.h>
#include <Point2.h>
#include <Rect2.h>

template <typename value_type>
class Outline2
{
public:
  using point_type = Point2<value_type>;
  using segment_type = Segment<Point2, value_type>;
  using box_type = Rect2<value_type>;

  Outline2(float height)
  : m_height(height)
  {
  }

  float height() const
  {
    return m_height;
  }

  const box_type& boundingBox() const
  {
    return m_boundingBox;
  }

  void add(const segment_type& seg)
  {
    m_segments.push_back(seg);

    if (m_segments.size() == 1)
      m_boundingBox = box_type(seg);
    else
      m_boundingBox.inflate(seg);
  }

  bool empty() const
  {
    return m_segments.size() < 3;
  }

  const vector<segment_type>& segments() const
  {
    return m_segments;
  }

private:
  float m_height;
  vector<segment_type> m_segments;
  box_type m_boundingBox;
};

using Outline2f = Outline2<float>;
