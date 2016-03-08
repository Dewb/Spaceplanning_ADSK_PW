#include <stdafx.h>
#include <Section.h>

Bag::param_tag Section::usage(BagCache::reserve());

Section::Section(const Rect2i& rect, const GridBasis& basis, BagCache::ref cache)
: rect(basis.fromGrid(rect)),
  data(cache->createBag())
{
}

const Rect2f& Section::getRect() const
{
  return rect;
}
