#pragma once

class DisplayContextAPI;

class GridContext
{
public:
  GridContext(DisplayContextAPI& context, const Rect2i& r, const Point2i& div);

  GridContext& operator=(const GridContext& other) = delete;

  DisplayContextAPI& api();
  const Rect2i& r() const;
  const Point2i& div() const;

  Point2i pt(const Point2i& pt) const;
  Rect2i rect(const Rect2i& rect) const;

private:
  DisplayContextAPI& m_context;
  const Rect2i& m_r;
  const Point2i& m_div;
  const float m_xScale;
  const float m_yScale;
};
