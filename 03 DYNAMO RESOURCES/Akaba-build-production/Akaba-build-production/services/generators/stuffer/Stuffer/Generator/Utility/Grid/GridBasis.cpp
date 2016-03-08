#include <stdafx.h>
#include <GridBasis.h>

float GridBasis::epsilon{ 0.001f };

GridBasis::GridBasis(const coord_base& coordBase, float hRes, float vRes)
: coordBase(coordBase),
  hRes(hRes),
  vRes(vRes)
{
}

const GridBasis::coord_base& GridBasis::axis() const
{
  return coordBase;
}

float GridBasis::h() const
{
  return hRes;
}

int GridBasis::toGrid(float value) const
{
  return static_cast<int>(value/hRes);
}

float GridBasis::fromGrid(int value) const
{
  return value*hRes;
}

Point2i GridBasis::toGrid(const Point2f& pt) const
{
  return Point2i(toGrid(pt.x()), toGrid(pt.y()));
}

Point2f GridBasis::fromGrid(const Point2i& pt) const
{
  return Point2f(fromGrid(pt.x()), fromGrid(pt.y()));
}

Rect2i GridBasis::toGrid(const Rect2f& rect) const
{
  Point2i br(toGrid(rect.br()));
  br.x() -= 1;
  br.y() -= 1;
  return Rect2i(toGrid(rect.tl()), br);
}

Rect2f GridBasis::fromGrid(const Rect2i& rect) const
{
  return Rect2f(fromGrid(rect.tl()), fromGrid(Point2i(rect.r() + 1, rect.b() + 1)));
}

Rangei GridBasis::shrinkToGrid(const Rangef& range) const
{
  float l((range.l() - epsilon*10.0f) / hRes);
  float h((range.h() + epsilon*10.0f) / hRes);
  return Rangei(
    static_cast<int>(ceil(l)), 
    static_cast<int>(floor(h)));
}

Rect2i GridBasis::shrinkToGrid(const Rect2f& rect) const
{
  Rangef xRangef(rect.l(), rect.r());
  Rangei xRangei(shrinkToGrid(xRangef));

  Rangef yRangef(rect.t(), rect.b());
  Rangei yRangei(shrinkToGrid(yRangef));

  return Rect2i(
    Point2i(xRangei.l(), yRangei.l()),
    Point2i(xRangei.h(), yRangei.h()));
}

int GridBasis::levelNum(float height) const
{
  return static_cast<int>(floor(height/vRes + epsilon));
}

float GridBasis::levelHeight(int num) const
{
  return num*vRes;
}
