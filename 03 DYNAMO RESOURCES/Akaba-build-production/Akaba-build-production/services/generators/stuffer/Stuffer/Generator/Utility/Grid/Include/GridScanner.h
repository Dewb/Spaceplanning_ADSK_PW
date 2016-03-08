#pragma once

class GridScanner
{
private:
  class AxisInfo
  {
    friend class GridScanner;

  public:
    AxisInfo(int from, int to);

    void inc();
    bool compare() const;

  private:
    GridValue index;
    int from;
    int to;
  };

public:
  GridScanner(const Rect2i& b, int index);
  virtual ~GridScanner() = default;

  GridRef loc() const;
  void process(GridVisitor& visitor);

  virtual bool processLoc(const GridRef& loc, GridVisitor& visitor) const = 0;

private:
  bool normal;
  AxisInfo majorInfo;
  AxisInfo minorInfo;
};
