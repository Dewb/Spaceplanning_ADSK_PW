#pragma once

class Section
{
  friend class Phenotype;
  friend class Floor;

public:
  static Bag::param_tag usage;

  const Rect2f& getRect() const;

  Bag::ref data;

private:
  Rect2f rect;
  weak_ptr<Section> above;
  weak_ptr<Section> below;

  Section(const Rect2i& rect, const GridBasis& basis, BagCache::ref cache);
};
