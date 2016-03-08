#pragma once

#include <Strategy.h>
#include <Area.h>

class Shaft;

class LevelConnectionStrategy : public Strategy
{
public:
  LevelConnectionStrategy(const Rect2i& stairRect);

protected:
  Rect2i stairRect;

  const string_t& name() const;
  void execute(const Args& args) const;

  void processLevelConnection(list<Shaft>& shafts, int level, const Area::sub_areas& areas) const;
};
