#pragma once

class Shaft
{
public:
  Shaft(int level, const Rect2i& rect);

  int getBottomLevel() const;
  int getTopLevel() const;
  const Rect2i& getRect() const;

private:
  int bottomLevel;
  int topLevel;
  Rect2i rect;
};
