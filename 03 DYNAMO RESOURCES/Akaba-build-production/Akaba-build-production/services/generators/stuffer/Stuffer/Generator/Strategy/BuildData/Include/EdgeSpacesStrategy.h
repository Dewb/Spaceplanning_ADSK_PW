#pragma once

#include <EdgeEffectStrategy.h>

class EdgeSpacesStrategy : public EdgeEffectStrategy
{
public:
  EdgeSpacesStrategy(int amount);
  EdgeSpacesStrategy(const Rangei& range);
  EdgeSpacesStrategy(const Rangei& range, int shutoff);

protected:
  const string_t& name() const;
  Usage::name_tag getUsage() const;
  unique_ptr<EdgeVisitor> getVisitor(const Args& args, unique_ptr<CellHelper> helper) const;
};
