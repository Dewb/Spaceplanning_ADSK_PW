#pragma once

#include <DisplayContextAPI.h>

class D2DDisplay;
struct ID2D1HwndRenderTarget;

class D2DContext : public DisplayContextAPI
{
public:
  D2DContext(D2DDisplay& display, ID2D1HwndRenderTarget* pTarget, int targetIndex);
  ~D2DContext();

protected:
  int createSolidBrush(unsigned long color);

  void reset();

  Point2i getSize() const;

  void drawLine(const DrawInfo& info, const Point2i& p0, const Point2i& p1) const;
  void drawRect(const DrawInfo& info, const Rect2i& r) const;
  void fillRect(const DrawInfo& info, const Rect2i& r) const;

private:
  D2DDisplay& m_display;
  ID2D1HwndRenderTarget* m_pTarget;
  int m_pTargetIndex;
};
