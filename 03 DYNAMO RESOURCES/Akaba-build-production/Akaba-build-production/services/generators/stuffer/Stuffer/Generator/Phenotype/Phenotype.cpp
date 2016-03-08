#include <stdafx.h>
#include <Phenotype.h>
#include <Usage.h>
#include <Floor.h>
#include <Section.h>
#include <SpaceLayout.h>

Phenotype::Phenotype(const GridBasis& basis)
: basis(basis),
  cache(new BagCache())
{
}

Phenotype::Phenotype(const Phenotype& other)
: basis(other.basis)
{
  *this = other;
}

Phenotype& Phenotype::operator=(const Phenotype& other)
{
  basis = other.basis;

  floors.clear();
  for (const auto& floor : other.getFloors())
    floors[floor.first] = make_unique<Floor>(*floor.second);

  return *this;
}

Phenotype& Phenotype::createSnapshot()
{
  cache->createSnapshot();

  return *this;
}

void Phenotype::mergeSnapshot()
{
  cache->mergeSnapshot();
}

void Phenotype::discardSnapshot()
{
  cache->discardSnapshot();
}

void Phenotype::addMesh(const Mesh& mesh)
{
  meshes.push_back(mesh);
}

const vector<Mesh>& Phenotype::getMeshes() const
{
  return meshes;
}

void Phenotype::createFloor(int level)
{
  auto& floor(floors[level]);
  if (!floor)
    floors[level].reset(new Floor());
}

void Phenotype::createFloor(const Rangei& levels)
{
  for (auto level(levels.l()); level <= levels.h(); ++level)
    createFloor(level);
}

Floor::section_ref Phenotype::createSection(int floorNum, const Rect2i& rect, Usage::name_tag tag)
{
  auto& floor(floors[floorNum]);
  if (!floor)
    return nullptr;

  Floor::section_ref section(floor->createSection(rect, basis, cache));
  section->data->setIntParam(Section::usage, tag);
  usages.add(tag, section->getRect().area());

  return section;
}

Floor::section_ref Phenotype::createSection(const Rangei& floors, const Rect2i& rect, Usage::name_tag tag)
{
  auto bottom(createSection(floors.l(), rect, tag));
  auto current(bottom);
  for (auto index = floors.l() + 1; index <= floors.h(); ++index)
  {
    auto created(createSection(index, rect, tag));
    if (!created)
      break;

    created->below = current;
    created->below.lock()->above = created;
    current = created;
  }

  return bottom;
}

const Floor& Phenotype::getFloor(int num) const
{
  return *floors.at(num);
}

Phenotype::floor_map& Phenotype::getFloors()
{
  return floors;
}

const Phenotype::floor_map& Phenotype::getFloors() const
{
  return floors;
}

unique_ptr<Design> Phenotype::convert() const
{
  unique_ptr<Design> design(make_unique<Design>());

  for (const auto& floor : floors)
  {
    for (const auto& section : floor.second->getSections())
    {
      if (!section->below.lock())
      {
        auto above(section->above.lock());
        if (above)
        {
          Rangei levels(floor.first);
          do
          {
            levels.inflate(levels.h() + 1);
            above = above->above.lock();
          } while (above);
          design->spaces.push_back(createSpace(levels, *section));
        }
        else
        {
          design->spaces.push_back(createSpace(floor.first, *section));
        }
      }
    }
  }

  for (const auto& mesh : meshes)
  {
    design->meshes.push_back(mesh);
  }

  return design;
}

const Usage& Phenotype::getUsage() const
{
  return usages;
}

Space Phenotype::createSpace(int level, const Section& section) const
{
  return createSpace(Rangei(level), section);
}

Space Phenotype::createSpace(const Rangei& levels, const Section& section) const
{
  Space space;
  space.usage = Usage::name(section.data->getIntParam(Section::usage, -1));

  const Rect2f& rect(section.getRect());
  Point2f center(rect.center());

  space.dimensions.x() = rect.w();
  space.dimensions.y() = rect.h();
  space.dimensions.z() = basis.levelHeight(levels.h() - levels.l() + 1);
  space.origin.x() = center.x();
  space.origin.y() = center.y();
  space.origin.z() = basis.levelHeight(levels.l());

  space.isCirculation = false;

  return space;
}
