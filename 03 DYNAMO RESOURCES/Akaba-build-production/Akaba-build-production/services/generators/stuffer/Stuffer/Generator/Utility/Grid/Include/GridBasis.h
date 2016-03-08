#pragma once

class GridBasis
{
public:
  static float epsilon;

  using coord_base = vector<string_t>;

  GridBasis(const coord_base& coordBase, float hRes, float vRes);

  // Needed by Grid
  const coord_base& axis() const;

  // Needed by STLShell
  float h() const;

  int toGrid(float value) const;
  float fromGrid(int value) const;

  Point2i toGrid(const Point2f& pt) const;
  Point2f fromGrid(const Point2i& pt) const;

  Rect2i toGrid(const Rect2f& rect) const;
  Rect2f fromGrid(const Rect2i& rect) const;

  Rangei shrinkToGrid(const Rangef& range) const;
  Rect2i shrinkToGrid(const Rect2f& rect) const;

  int levelNum(float height) const;
  float levelHeight(int num) const;

private:
  coord_base coordBase;
  float hRes;
  float vRes;
};
