#pragma once

class Section;
class Adjacency;

class Floor
{
  friend class Phenotype;

public:
  using section_ref = shared_ptr<Section>;
  using section_data = vector<section_ref>;
  section_data& getSections();
  const section_data& getSections() const;

private:
  section_data sections;

  section_ref createSection(const Rect2i& rect, const GridBasis& basis, BagCache::ref cache);

  using adjacency_data = list<unique_ptr<Adjacency>>;
  void getAdjacencies(float minOverlap, adjacency_data& adjacencies, section_data& vertical) const;
};
