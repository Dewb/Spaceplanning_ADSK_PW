#include <stdafx.h>

#ifdef USE_DIALOG

#include <D2DContext.h>
#include <D2DDisplay.h>

D2DContext::D2DContext(D2DDisplay& display, ID2D1HwndRenderTarget* pTarget, int targetIndex)
: m_display(display),
  m_pTarget(pTarget),
  m_pTargetIndex(targetIndex)
{
  m_pTarget->BeginDraw();
}

D2DContext::~D2DContext()
{
  m_pTarget->EndDraw();
}

int D2DContext::createSolidBrush(unsigned long color)
{
  return m_display.createSolidBrush(m_pTargetIndex, D2D1::ColorF(color));
}

void D2DContext::reset()
{
  //m_pTarget->SetTransform(D2D1::Matrix3x2F::Identity());
  m_pTarget->SetTransform(D2D1::Matrix3x2F(
    -1.0f, 0.0f, 0.0f, 1.0f,
    static_cast<float>(getSize().x()), 0.0f));

  m_pTarget->Clear(D2D1::ColorF(D2D1::ColorF::White));
}

Point2i D2DContext::getSize() const
{
  D2D1_SIZE_F size = m_pTarget->GetSize();
  return Point2i(
    static_cast<int>(size.width),
    static_cast<int>(size.height));
}

namespace
{
  D2D1_POINT_2F toD2Point(const Point2i& p)
  {
    return D2D1::Point2F(
      static_cast<FLOAT>(p.x()),
      static_cast<FLOAT>(p.y()));
  }

  D2D1_RECT_F toD2Rect(const Rect2i& r)
  {
    return D2D1::RectF(
      static_cast<FLOAT>(r.l()),
      static_cast<FLOAT>(r.t()),
      static_cast<FLOAT>(r.r()),
      static_cast<FLOAT>(r.b()));
  }
}

void D2DContext::drawLine(const DrawInfo& info, const Point2i& p0, const Point2i& p1) const
{
  ID2D1SolidColorBrush* pBrush = m_display.getBrush(m_pTargetIndex, info.brushIndex());
  if (!pBrush)
    return;

  int strokeIndex = info.strokeIndex();
  ID2D1StrokeStyle* pStroke = (strokeIndex != -1) ? m_display.getStroke(strokeIndex) : nullptr;

  m_pTarget->DrawLine(toD2Point(p0), toD2Point(p1), pBrush, info.width(), pStroke);
}

void D2DContext::drawRect(const DrawInfo& info, const Rect2i& r) const
{
  ID2D1SolidColorBrush* pBrush = m_display.getBrush(m_pTargetIndex, info.brushIndex());
  if (!pBrush)
    return;

  int strokeIndex = info.strokeIndex();
  ID2D1StrokeStyle* pStroke = (strokeIndex != -1) ? m_display.getStroke(strokeIndex) : nullptr;

  m_pTarget->DrawRectangle(toD2Rect(r), pBrush, info.width(), pStroke);
}

void D2DContext::fillRect(const DrawInfo& info, const Rect2i& r) const
{
  ID2D1SolidColorBrush* pBrush = m_display.getBrush(m_pTargetIndex, info.brushIndex());
  if (!pBrush)
    return;

  m_pTarget->FillRectangle(toD2Rect(r), pBrush);
}

#endif
