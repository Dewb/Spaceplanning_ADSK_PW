#pragma once

class JobRequest;
class Shell;
class Design;
class Area;
class Phenotype;

class BuildData
{
public:
  BuildData(const GridBasis& basis);
  ~BuildData();

  const GridBasis& getBasis() const;

  void setExistingDesign(const Design& design, Phenotype& phenotype);
  void setXYShell(unique_ptr<const Shell> shell_, Phenotype& phenotype);
  const Shell& getShell() const;

  void reset();
  BuildData& createSnapshot();
  void mergeSnapshot();
  void discardSnapshot();

  Rangei getLevelRange() const;
  Point2i getMaxAreaSize() const;
  const Area* getLevelArea(int level) const;
  Area* getLevelArea(int level);

  using level_map = map<const int, unique_ptr<Area>>;
  const level_map& getAreas() const;

  void setClaimed(int level, const Rect2i& rect);
  void setClaimed(const Rangei& levels, const Rect2i& rect);

private:
  GridBasis basis;
  unique_ptr<const Shell> shell;

  level_map levelAreas;
  BagCache::ref cache;

  Area& createLevelArea(int levelNum);
  void createCellIfNeeded(
    Area& levelArea,
    const Point2i& loc,
    const vector<Rangei>& xLines1,
    const vector<Rangei>& xLines2,
    const vector<Rangei>& yLines1,
    const vector<Rangei>& yLines2);
  void createCell(
    Area& levelArea,
    const Point2i& loc);
};
