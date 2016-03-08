#pragma once

#include <Shell.h>

class STLShell : public Shell
{
public:
  STLShell(const string& stlData, const GridBasis& basis);

private:
  static unique_ptr<vector<Facet3f>> loadSTLFromASCII(const string& data);

  unique_ptr<vector<Facet3f>> facets;
  unique_ptr<vector<Outline2f>> outlines;

  void calculateOutlines(const Rangei& levelRange);
  void calculateOutline(float level, bool isFloorOutline);

  void calculateGrids();
  void calculateGrid(const Outline2f& outline);
};
