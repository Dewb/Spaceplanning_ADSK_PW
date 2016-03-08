#include <stdafx.h>
#include <EdgeSpacesStrategy.h>
#include <Args.h>
#include <Phenotype.h>
#include <BuildData.h>
#include <Floor.h>
#include <EdgeVisitor.h>
#include <CellScanner.h>
#include <CellHelperNotClaimed.h>

EdgeSpacesStrategy::EdgeSpacesStrategy(const Rangei& range, int shutoff)
{
  setRange(range);
  setShutoff(shutoff);
  setAtEdgeOnly(true);
}

EdgeSpacesStrategy::EdgeSpacesStrategy(int amount)
: EdgeSpacesStrategy(Rangei(amount, amount), 0)
{
}

EdgeSpacesStrategy::EdgeSpacesStrategy(const Rangei& range)
: EdgeSpacesStrategy(range, 0)
{
}

const string_t& EdgeSpacesStrategy::name() const
{
  static string_t name(U("EdgeSpacesStrategy"));
  return name;
}

Usage::name_tag EdgeSpacesStrategy::getUsage() const
{
  return Usage::tag(U("office"));
}

unique_ptr<EdgeVisitor> EdgeSpacesStrategy::getVisitor(const Args& args, unique_ptr<CellHelper> helper) const
{
  return EdgeEffectStrategy::getVisitor(args, make_unique<CellHelperNotClaimed>());
}
