#pragma once

#include <GeneratorGraphicsAPI.h>

class DisplayAppAPI;
class DisplayContextAPI;
class GridContext;
class Area;
class Cell;

class GeneratorGraphics : public GeneratorGraphicsAPI
{
public:
  GeneratorGraphics(DisplayAppAPI& displayApp);

  GeneratorGraphics& operator=(const GeneratorGraphics& other) = delete;

  void setTargetIndex(int targetIndex);
  void releaseResources();

  int getDisplayLevel() const;
  void setDisplayLevel(int displayLevel);

protected:
  void displayPlanState(shared_ptr<Vision> vision);

  // Higher level graphics items
private:
  void displayAreaFill(GridContext& context, const Area& area, list<Rect2i>& segments);
  void displayCellFill(GridContext& context, const Cell& cell, list<Rect2i>& segments);

  // Lower level graphics items
private:
  DisplayAppAPI& m_displayApp;
  int m_targetIndex;
  map<int, int> m_brushes;
  shared_ptr<Vision> m_pLastVision;
  int m_displayLevel;

  int getBrush(DisplayContextAPI& context, int);

  void drawGrid(GridContext& context, int brush);
  void drawTestPattern(DisplayContextAPI& context);
};
