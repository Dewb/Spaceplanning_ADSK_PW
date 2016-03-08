#pragma once

#include <SideStrategy.h>

class EdgeEffectStrategy : public SideStrategy
{
protected:
  EdgeEffectStrategy();
    
  void setRange(const Rangei& range);
  void setShutoff(int shutoff);
  void setAtEdgeOnly(bool atEdgeOnly);

  void execute(const Args& args) const;

  unique_ptr<EdgeVisitor> getVisitor(const Args& args, unique_ptr<CellHelper> helper) const;
  void processSides(const Args& args) const;

  Rangei m_range;
  int m_shutoff;
  bool m_atEdgeOnly;
};
