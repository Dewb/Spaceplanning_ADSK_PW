#include <stdafx.h>
#include <GridContext.h>

GridContext::GridContext(DisplayContextAPI& context, const Rect2i& r, const Point2i& div)
: m_context(context),
  m_r(r),
  m_div(div),
  m_xScale(r.w() / static_cast<float>(div.x())),
  m_yScale(r.h() / static_cast<float>(div.y()))
{
}

DisplayContextAPI& GridContext::api()
{
  return m_context;
}

const Rect2i& GridContext::r() const
{
  return m_r;
}

const Point2i& GridContext::div() const
{
  return m_div;
}

Point2i GridContext::pt(const Point2i& pt) const
{
  int x = min(max(pt.x(), 0), m_div.x());
  x = m_r.l() + static_cast<int>(x*m_xScale);

  int y = min(max(pt.y(), 0), m_div.y());
  y = m_r.t() + static_cast<int>(y*m_yScale);

  return Point2i(x, y);
}

Rect2i GridContext::rect(const Rect2i& rect) const
{
  return Rect2i(pt(rect.tl()), pt(Point2i(rect.r() + 1, rect.b() + 1)));
}
