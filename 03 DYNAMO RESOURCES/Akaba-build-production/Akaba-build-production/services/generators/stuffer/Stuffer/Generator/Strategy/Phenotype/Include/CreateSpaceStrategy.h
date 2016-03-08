#pragma once

#include <Strategy.h>
#include <Usage.h>

class CreateSpaceStrategy : public Strategy
{
public:
  CreateSpaceStrategy(int floor, const Rect2i& rect, Usage::name_tag tag);

protected:
  const string_t& name() const;
  void execute(const Args& args) const;

private:
  int floor;
  Rect2i rect;
  Usage::name_tag tag;
};
