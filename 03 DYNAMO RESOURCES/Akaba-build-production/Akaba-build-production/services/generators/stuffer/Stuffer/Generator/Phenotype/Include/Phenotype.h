#pragma once

#include <SpaceLayout.h>
#include <Usage.h>
#include <Floor.h>

class Section;
class Design;
class Space;
class AdjacencyGraph;

class Phenotype
{
public:
  explicit Phenotype(const GridBasis& basis);
  Phenotype(const Phenotype& other);
  Phenotype& operator=(const Phenotype& other);

  void addMesh(const Mesh& mesh);
  const vector<Mesh>& getMeshes() const;

  void createFloor(int level);
  void createFloor(const Rangei& levels);
  Floor::section_ref createSection(int level, const Rect2i& rect, Usage::name_tag tag);
  Floor::section_ref createSection(const Rangei& levels, const Rect2i& rect, Usage::name_tag tag);

  const Floor& getFloor(int floorNum) const;

  using floor_map = map<const int, unique_ptr<Floor>>;
  floor_map& getFloors();
  const floor_map& getFloors() const;

  Phenotype& createSnapshot();
  void mergeSnapshot();
  void discardSnapshot();

  unique_ptr<Design> convert() const;

  unique_ptr<AdjacencyGraph> getAdjacencies() const;
  const Usage& getUsage() const;

private:
  vector<Mesh> meshes;
  GridBasis basis;
  floor_map floors;
  BagCache::ref cache;
  Usage usages;

  Space createSpace(int level, const Section& section) const;
  Space createSpace(const Rangei& levels, const Section& section) const;
};
