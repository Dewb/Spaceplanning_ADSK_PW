#pragma once

class DrawInfo
{
public:
  explicit DrawInfo(int brushIndex)
  : DrawInfo(brushIndex, 1.0f, -1)
  {
  }

  DrawInfo(int brushIndex, float width)
  : DrawInfo(brushIndex, width, -1)
  {
  }

  DrawInfo(int brushIndex, float width, int strokeIndex)
  : m_brushIndex(brushIndex), 
    m_width(width), 
    m_strokeIndex(strokeIndex)
  {
  }

  int brushIndex() const
  {
    return m_brushIndex;
  }

  float width() const
  {
    return m_width;
  }

  int strokeIndex() const
  {
    return m_strokeIndex;
  }

private:
  int m_brushIndex;
  float m_width;
  int m_strokeIndex;
};

class DisplayContextAPI
{
public:
  virtual ~DisplayContextAPI() = default;

  virtual int createSolidBrush(unsigned long color) = 0;

  virtual void reset() = 0;

  virtual Point2i getSize() const = 0;

  virtual void drawLine(const DrawInfo& info, const Point2i& p0, const Point2i& p1) const = 0;
  virtual void drawRect(const DrawInfo& info, const Rect2i& r) const = 0;
  virtual void fillRect(const DrawInfo& info, const Rect2i& r) const = 0;
};
