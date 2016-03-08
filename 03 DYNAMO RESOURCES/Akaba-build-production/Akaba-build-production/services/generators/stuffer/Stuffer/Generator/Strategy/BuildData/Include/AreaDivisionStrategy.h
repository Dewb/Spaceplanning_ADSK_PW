#pragma once

#include <Strategy.h>
#include <Genome.h>
#include <Area.h>

class Phenotype;

class AreaDivisionStrategy : public Strategy
{
public:
  AreaDivisionStrategy(int minSize, const Rangei& numDiv, bool vertical);
  AreaDivisionStrategy(int minSize, const Rangei& numDiv);

protected:
  const string_t& name() const;
  void execute(const Args& args) const;

private:
  int m_minSize;
  Rangei m_numDiv;
  bool m_forceVertical;
  bool m_vertical;

  class DivInfo
  {
  public:
    DivInfo(
      Ribosome& geneIt,
      int width,
      Area& area,
      Phenotype& phenotype,
      int floor)
    : geneIt(geneIt),
      width(width),
      area(area),
      phenotype(phenotype),
      floor(floor)
    {
    }

    DivInfo& operator=(const DivInfo& other) = delete;

    Ribosome& geneIt;
    int width;
    Area& area;
    Phenotype& phenotype;
    int floor;
  };

  void divideSubAreas(
    DivInfo& info,
    int depth,
    bool vertical,
    unique_ptr<Area::sub_areas> pSubAreas) const;
    
  bool divideSubArea(
    DivInfo& info,
    bool vertical,
    const Area::sub_area& subArea) const;

  list<int> calculateSlices(
    Ribosome& geneIt,
    int id, 
    int width,
    int size,
    int off,
    int num) const;

  void slice(
    Area& area, 
    Phenotype& phenotype, 
    int floor, 
    const list<int>& slices, 
    const Rect2i& bounds, 
    bool vertical, 
    int width) const;
};
