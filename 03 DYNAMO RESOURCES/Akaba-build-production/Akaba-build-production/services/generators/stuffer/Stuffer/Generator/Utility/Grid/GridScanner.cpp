#include <stdafx.h>
#include <GridScanner.h>

GridScanner::AxisInfo::AxisInfo(int from, int to)
: index(from < to),
  from(from),
  to(to)
{
}

void GridScanner::AxisInfo::inc()
{
  index.inc();
}

bool GridScanner::AxisInfo::compare() const
{
  return index.compare(to);
}

GridScanner::GridScanner(const Rect2i& b, int index)
: normal((index & 0x02) == 0),
  majorInfo(
  (index & 0x01) ?
  (index & 0x02) ?
  AxisInfo(b.t(), b.b()) :
  AxisInfo(b.l(), b.r()) :
  (index & 0x02) ?
  AxisInfo(b.b(), b.t()) :
  AxisInfo(b.r(), b.l())),

  minorInfo(
  (index & 0x01) ?
  (index & 0x02) ?
  AxisInfo(b.l(), b.r()) :
  AxisInfo(b.t(), b.b()) :
  (index & 0x02) ?
  AxisInfo(b.r(), b.l()) :
  AxisInfo(b.b(), b.t()))
{
}

GridRef GridScanner::loc() const
{
  return GridRef(normal, minorInfo.index, majorInfo.index);
}

void GridScanner::process(GridVisitor& visitor)
{
  visitor.outerLoopReset();
  for (majorInfo.index.setValue(majorInfo.from); majorInfo.compare(); majorInfo.inc())
  {
    visitor.innerLoopReset();
    for (minorInfo.index.setValue(minorInfo.from); minorInfo.compare(); minorInfo.inc())
    {
      if (processLoc(loc(), visitor))
        break;
    }
    visitor.innerLoopComplete();
  }
  visitor.outerLoopComplete();
}
