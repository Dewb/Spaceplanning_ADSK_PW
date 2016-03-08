#include <stdafx.h>
#include <GeneratorGraphics.h>
#include <GridContext.h>
#include <DisplayAppAPI.h>
#include <DisplayContextAPI.h>
#include <Vision.h>
#include <JobRequest.h>
#include <Plan.h>
#include <Phenotype.h>
#include <BuildData.h>
#include <Area.h>
#include <Cell.h>
#include <CellHelperNotClaimed.h>
#include <Floor.h>
#include <Section.h>

GeneratorGraphics::GeneratorGraphics(DisplayAppAPI& displayApp)
: m_displayApp(displayApp),
  m_targetIndex(-1),
  m_displayLevel(0)
{
}

void GeneratorGraphics::setTargetIndex(int targetIndex)
{
  m_targetIndex = targetIndex;
}

void GeneratorGraphics::releaseResources()
{
  m_brushes.clear();
  m_targetIndex = -1;
}

int GeneratorGraphics::getDisplayLevel() const
{
  return m_displayLevel;
}

void GeneratorGraphics::setDisplayLevel(int displayLevel)
{
  m_displayLevel = displayLevel;
}

namespace
{
  enum BrushColor
  {
    White,
    FaintGray,
    LightGray,
    Gray,
    Black,
    FaintBlue,
    LightBlue,
    LightYellow,
    Lavender
  };

  map<int, unsigned long> m_colors
  {
    make_pair(White, 0xFFFFFF),
    make_pair(FaintGray, 0xF3F3F3),
    make_pair(LightGray, 0xBEBEBE),
    make_pair(Gray, 0x7F7F7F),
    make_pair(Black, 0x000000),
    make_pair(FaintBlue, 0xDDEEEE),
    make_pair(LightBlue, 0x6495ED),
    make_pair(LightYellow, 0xFFFF99),
    make_pair(Lavender, 0xFFCCFF)
  };
}

int GeneratorGraphics::getBrush(DisplayContextAPI& context, int color)
{
  if (m_brushes.find(color) == m_brushes.end())
    m_brushes[color] = context.createSolidBrush(m_colors[color]);
  else
  {
    int nDummy = 1;
    ++nDummy;
  }

  return m_brushes[color];
}

//#define TEST_PATTERN
void GeneratorGraphics::displayPlanState(shared_ptr<Vision> vision)
{
  if (m_targetIndex == -1)
    return;

#ifdef TEST_PATTERN
  drawTestPattern(rawContext);
#else
  unique_ptr<DisplayContextAPI> rawContext(m_displayApp.createContext(m_targetIndex));
  if (!rawContext)
    return;

  rawContext->reset();

  if (!vision)
    vision = m_pLastVision;
  if (!vision)
    return;
  m_pLastVision = vision;

  bool showDisplayRect = true;
  bool showDisplayGrid = true;
  bool showLevel = true;
  bool showSpaces = true;
  bool showWalls = true;
  bool showRectAreas = false;

  const BuildData& buildData(vision->getPlan().getBuildData());
  Point2i maxAreaSize(buildData.getMaxAreaSize());
  Rect2i displayRect(rawContext->getSize());
  displayRect.shrink(25);
  if (!displayRect.inset(maxAreaSize))
    return;

  buildData.getLevelRange().clamp(m_displayLevel);

  GridContext context(*rawContext, displayRect, maxAreaSize);

  list<Rect2i> segments;
  if (showLevel)
  {
    const auto& area(buildData.getLevelArea(m_displayLevel));
    if (area)
      displayAreaFill(context, *area, segments);
  }

  if (showDisplayGrid)
  {
    int brGrid = getBrush(context.api(), LightGray);
    drawGrid(context, brGrid);
  }

  if (showDisplayRect)
  {
    int brGridOutline = getBrush(context.api(), Gray);
    context.api().drawRect(DrawInfo(brGridOutline, 1.0f), displayRect);
  }

  if (showSpaces)
  {
    const auto& area(buildData.getLevelArea(m_displayLevel));
    if (area)
    {
      const auto& phenotype(vision->getPlan().getPhenotype());
      const auto& floor(phenotype.getFloor(m_displayLevel));

      auto brHallFill = getBrush(context.api(), LightYellow);
      auto brLobbyFill = getBrush(context.api(), LightBlue);
      auto hallTag(Usage::tag(U("Hall")));
      auto lobbyTag(Usage::tag(U("lobby")));
      for (const auto& section : floor.getSections())
      {
        auto rect(context.rect(buildData.getBasis().toGrid(section->getRect())));
        auto usage(section->data->getIntParam(Section::usage, -1));
        if (usage == hallTag)
          context.api().fillRect(DrawInfo(brHallFill, 1.0f, 1), rect);
        else if (usage == lobbyTag)
          context.api().fillRect(DrawInfo(brLobbyFill, 1.0f, 1), rect);
      }

      int brSectionOutline = getBrush(context.api(), Black);
      for (const auto& section : floor.getSections())
        context.api().drawRect(
          DrawInfo(brSectionOutline, 1.0f, 1), 
          context.rect(buildData.getBasis().toGrid(section->getRect())));
    }
  }

  if (showRectAreas)
  {
    const auto& pArea(buildData.getLevelArea(m_displayLevel));
    if (pArea)
    {
      int brAreaFill = getBrush(context.api(), Lavender);
      auto subAreas(pArea->calculateSubAreas(nullptr, CellHelperNotClaimed()));
      for (const auto& subArea : *subAreas)
      {
        context.api().fillRect(DrawInfo(brAreaFill), context.rect(subArea.first));
      }
    }
  }

  if (showWalls)
  {
    int brWall1 = getBrush(context.api(), Black);
    int brWall2 = getBrush(context.api(), White);
    for (const auto& segment : segments)
      context.api().drawLine(DrawInfo(brWall1, 6, 0), segment.tl(), segment.br());
    for (const auto& segment : segments)
      context.api().drawLine(DrawInfo(brWall2, 2, 0), segment.tl(), segment.br());
  }
#endif
}

void GeneratorGraphics::displayAreaFill(GridContext& context, const Area& area, list<Rect2i>& segments)
{
  for (const auto& pCell : area.getCells())
    displayCellFill(context, *pCell.second, segments);
}

void GeneratorGraphics::displayCellFill(GridContext& context, const Cell& cell, list<Rect2i>& segments)
{
  int brCellArea = getBrush(context.api(), cell.data->getBoolParam(Cell::claimed, false) ? FaintBlue : FaintGray);

  Point2i tl(context.pt(cell.getLoc()));
  Point2i br(context.pt(Point2i(cell.getLoc().x() + 1, cell.getLoc().y() + 1)));
  Rect2i cellArea(tl, br);
  context.api().fillRect(DrawInfo(brCellArea), cellArea);
  
  if (!cell.getHNeighbor(0)) // Left
    segments.push_back(Rect2i(tl, Point2i(tl.x(), br.y())));
  if (!cell.getHNeighbor(1)) // Top
    segments.push_back(Rect2i(tl, Point2i(br.x(), tl.y())));
  if (!cell.getHNeighbor(2)) // Right
    segments.push_back(Rect2i(Point2i(br.x(), tl.y()), br));
  if (!cell.getHNeighbor(3)) // Bottom
    segments.push_back(Rect2i(Point2i(tl.x(), br.y()), br));
}

void GeneratorGraphics::drawGrid(GridContext& context, int brush)
{
  for (int x = 0; x <= context.div().x(); ++x)
  {
    Point2i p0(context.pt(Point2i(x, 0)));
    Point2i p1(context.pt(Point2i(x, context.div().y())));
    context.api().drawLine(DrawInfo(brush), p0, p1);
  }

  for (int y = 0; y <= context.div().y(); ++y)
  {
    Point2i p0(context.pt(Point2i(0, y)));
    Point2i p1(context.pt(Point2i(context.div().x(), y)));
    context.api().drawLine(DrawInfo(brush), p0, p1);
  }
}

void GeneratorGraphics::drawTestPattern(DisplayContextAPI& context)
{
  int brGray = getBrush(context, Gray);
  int brBlue = getBrush(context, LightBlue);

  context.reset();

  Point2i size(context.getSize());
  for (int x = 0; x < size.x(); x += 10)
    context.drawLine(DrawInfo(brGray), Point2i(x, 0), Point2i(x, size.y()));
  for (int y = 0; y < size.y(); y += 10)
    context.drawLine(DrawInfo(brGray), Point2i(0, y), Point2i(size.x(), y));

  int cx = size.x() / 2;
  int cy = size.y() / 2;
  context.fillRect(DrawInfo(brGray), Rect2i(Point2i(cx - 50, cy - 50), Point2i(cx + 50, cy + 50)));
  context.drawRect(DrawInfo(brBlue, 1.0f), Rect2i(Point2i(cx - 100, cy - 100), Point2i(cx + 100, cy + 100)));
}
