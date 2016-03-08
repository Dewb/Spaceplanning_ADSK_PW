#include <stdafx.h>
#include <Shaft.h>

Shaft::Shaft(int level, const Rect2i& rect)
: bottomLevel(level),
  topLevel(level + 1),
  rect(rect)
{
}

int Shaft::getBottomLevel() const
{
  return bottomLevel;
}

int Shaft::getTopLevel() const
{
  return topLevel;
}

const Rect2i& Shaft::getRect() const
{
  return rect;
}
