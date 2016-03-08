#include <stdafx.h>
#include <SpaceDivisionStrategy.h>
#include <Args.h>
#include <BuildData.h>
#include <Area.h>
#include <Cell.h>
#include <CellHelperNotClaimed.h>
#include <Phenotype.h>
#include <Floor.h>

const string_t& SpaceDivisionStrategy::name() const
{
  static string_t name(U("SpaceDivisionStrategy"));
  return name;
}

void SpaceDivisionStrategy::execute(const Args& args) const
{
  int unknownTag(Usage::tag(U("Unknown")));

  const BuildData& data = args.getBuildData();
  for (const auto& areaIt : data.getAreas())
  {
    auto subAreas(areaIt.second->calculateSubAreas(nullptr, CellHelperNotClaimed()));
    for (const auto& subArea : *subAreas)
    {
      const Rect2i& subAreaBounds(subArea.first);
      if (subAreaBounds.w() < subAreaBounds.h())
      {
        int cols((subAreaBounds.w() / 2 >= 8) ? 2 : 1);
        int rows(subAreaBounds.h() / 7);
        for (int col = 0; col < cols; ++col)
        {
          int colDiv = subAreaBounds.w();
          if (cols == 2)
            colDiv /= 2;

          int l = subAreaBounds.l();
          if (col == 1)
            l += colDiv + 1;

          int r = subAreaBounds.l() + colDiv;
          if (col == 1)
            r = subAreaBounds.r();

          for (int row = 0; row < rows; ++row)
          {
            int rowDiv = subAreaBounds.h() / rows;

            int t = subAreaBounds.t() + row * (rowDiv + 1);
            int b = t + rowDiv;
            if (row == rows - 1)
              b = subAreaBounds.b();

            Rect2i space(Point2i(l, t), Point2i(r, b));
            args.getPhenotype().createSection(areaIt.first, space, unknownTag);
            areaIt.second->claimSpace(space);
          }
        }
      }
      else
      {
        int cols(subAreaBounds.w() / 7);
        int rows((subAreaBounds.h() / 2 >= 8) ? 2 : 1);
        for (int row = 0; row < rows; ++row)
        {
          int rowDiv = subAreaBounds.h();
          if (rows == 2)
            rowDiv /= 2;

          int t = subAreaBounds.t();
          if (row == 1)
            t += rowDiv + 1;

          int b = subAreaBounds.t() + rowDiv;
          if (row == 1)
            b = subAreaBounds.b();

          for (int col = 0; col < cols; ++col)
          {
            int colDiv = subAreaBounds.w() / cols;

            int l = subAreaBounds.l() + col * (colDiv + 1);
            int r = l + colDiv;
            if (col == cols - 1)
              r = subAreaBounds.r();

            Rect2i space(Point2i(l, t), Point2i(r, b));
            args.getPhenotype().createSection(areaIt.first, space, unknownTag);
            areaIt.second->claimSpace(space);
          }
        }
      }
    }
  }
}
