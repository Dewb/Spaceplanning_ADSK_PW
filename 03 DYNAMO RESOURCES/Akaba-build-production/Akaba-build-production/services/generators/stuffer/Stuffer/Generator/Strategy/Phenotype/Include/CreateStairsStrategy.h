#pragma once

#include <Strategy.h>

class CreateStairsStrategy : public Strategy
{
public:
  CreateStairsStrategy();
  CreateStairsStrategy(const Rangei& floors, const Point2i& offset);

protected:
  const string_t& name() const;
  void execute(const Args& args) const;

private:
  Rangei floors;
  Point2i offset;
};
