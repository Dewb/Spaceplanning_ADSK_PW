#include <stdafx.h>
#include <Floor.h>
#include <Section.h>
#include <AdjacencyGraph.h>
#include <Adjacency.h>
#include <Usage.h>

Floor::section_ref Floor::createSection(const Rect2i& rect, const GridBasis& basis, BagCache::ref cache)
{
  auto section(shared_ptr<Section>(new Section(rect, basis, cache)));
  sections.push_back(section);

  return section;
}

Floor::section_data& Floor::getSections()
{
  return sections;
}

const Floor::section_data& Floor::getSections() const
{
  return sections;
}

void Floor::getAdjacencies(float minOverlap, adjacency_data& adjacencies, section_data& vertical) const
{
  adjacencies.clear();
  vertical.clear();

  auto stairsTag(Usage::tag(U("Stairs")));
  for (auto outer(0); outer < sections.size(); ++outer)
  {
    auto section1(sections[outer]);
    if (section1->data->getIntParam(Section::usage, -1) == stairsTag)
      vertical.push_back(section1);

    for (auto inner(outer + 1); inner < sections.size(); ++inner)
    {
      auto section2(sections[inner]);
      unique_ptr<Adjacency> adjacency(Adjacency::calculate(*section1, *section2));
      if (adjacency && adjacency->getOverlapAmount() >= minOverlap)
        adjacencies.push_back(move(adjacency));
    }
  }
}
