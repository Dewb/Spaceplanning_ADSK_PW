#pragma once

#include <Strategy.h>
#include <EdgeVisitor.h>
#include <Usage.h>

class SideStrategy : public Strategy
{
public:
  SideStrategy();

  void setSides(const list<int>& sides);

protected:
  list<int> m_sides;

  void processSides(const Args& args) const;

  virtual unique_ptr<EdgeVisitor> getVisitor(const Args& args, unique_ptr<CellHelper> helper) const;
  virtual Usage::name_tag getUsage() const = 0;
};
