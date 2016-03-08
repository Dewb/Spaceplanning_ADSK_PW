#pragma once

#include <Point2.h>
#include <Segment.h>
#include <Range.h>

template <typename value_type>
class Rect2
{
public:
  using point_type = Point2<value_type>;

  Rect2()
  : tlPt(),
    brPt()
  {
  }

  Rect2(const point_type& tl, const point_type& br)
  : tlPt(tl),
    brPt(br)
  {
  }

  Rect2(const point_type& size)
  : tlPt(point_type()),
    brPt(size)
  {
  }

  Rect2(const Segment<Point2, value_type>& seg)
  : tlPt(seg.pt[0]),
    brPt(seg.pt[0])
  {
    inflate(seg.pt[1]);
  }

  Rect2(const Rect2& other)
  {
    *this = other;
  }

  const point_type& tl() const
  {
    return tlPt;
  }

  const point_type& br() const
  {
    return brPt;
  }

  value_type t() const
  {
    return tlPt.y();
  }

  value_type l() const
  {
    return tlPt.x();
  }

  value_type b() const
  {
    return brPt.y();
  }

  value_type r() const
  {
    return brPt.x();
  }

  value_type& t()
  {
    return tlPt.y();
  }

  value_type& l()
  {
    return tlPt.x();
  }

  value_type& b()
  {
    return brPt.y();
  }

  value_type& r()
  {
    return brPt.x();
  }

  value_type w() const
  {
    return r() - l();
  }

  value_type h() const
  {
    return b() - t();
  }

  Rect2& operator=(const Rect2& other)
  {
    tlPt = other.tlPt;
    brPt = other.brPt;

    return *this;
  }

  Rect2& operator+=(const point_type& offset)
  {
    tlPt += offset;
    brPt += offset;

    return *this;
  }

  bool operator==(const Rect2& other) const
  {
    return (tlPt == other.tlPt && brPt == other.brPt);
  }

  Rect2& inflate(const point_type& pt)
  {
    if (pt.x() < l())
      l() = pt.x();
    else if (pt.x() > r())
      r() = pt.x();

    if (pt.y() < t())
      t() = pt.y();
    else if (pt.y() > b())
      b() = pt.y();

    return *this;
  }

  Rect2& inflate(const Rect2& r)
  {
    inflate(r.tl());
    inflate(r.br());

    return *this;
  }

  Rect2& inflate(const Segment<Point2, value_type>& seg)
  {
    inflate(seg.pt[0]);
    inflate(seg.pt[1]);

    return *this;
  }

  Rect2& shrink(int border)
  {
    if (w() < border * 2)
    {
      int x = l() + w() / 2;
      l() = x;
      r() = x;
    }
    else
    {
      l() += border;
      r() -= border;
    }

    if (h() < border * 2)
    {
      int y = t() + h() / 2;
      t() = y;
      b() = y;
    }
    else
    {
      t() += border;
      b() -= border;
    }

    return *this;
  }

  Rect2& square()
  {
    if (w() < h())
    {
      int delta = h() / 2 - w() / 2;
      t() += delta;
      b() -= delta;
    }
    else if (w() > h())
    {
      int delta = w() / 2 - h() / 2;
      l() += delta;
      r() -= delta;
    }

    return *this;
  }

  Rect2& offset(const point_type& pt)
  {
    tlPt += pt;
    brPt += pt;

    return *this;
  }

  point_type center() const
  {
    return point_type((l() + r()) / 2.0f, (t() + b()) / 2.0f);
  }

  int inset(const Rect2& other)
  {
    if (aspect() > other.aspect())
    {
      float scale = h() / static_cast<float>(other.h());
      int off = static_cast<int>((w() - other.w()*scale) / 2);
      l() += off;
      r() -= off;
    }
    else
    {
      float scale = w() / static_cast<float>(other.w());
      int off = static_cast<int>((h() - other.h()*scale) / 2);
      t() += off;
      b() -= off;
    }

    return w() > 0 && h() > 0;
  }

  point_type size() const
  {
    return point_type(w(), h());
  }

  float aspect() const
  {
    return w()/static_cast<float>(h());
  }

  float area() const
  {
    return w()*static_cast<float>(h());
  }

  bool contains(const point_type& pt)
  {
    return
      Range<value_type>(l(), r()).contains(pt.x()) &&
      Range<value_type>(t(), b()).contains(pt.y());
  }

private:
  point_type tlPt;
  point_type brPt;
};

template <typename value_type>
Rect2<value_type> operator+(Rect2<value_type> lhs, const Point2<value_type>& rhs)
{
  return lhs += rhs;
}

using Rect2f = Rect2<float>;
using Rect2i = Rect2<int>;
