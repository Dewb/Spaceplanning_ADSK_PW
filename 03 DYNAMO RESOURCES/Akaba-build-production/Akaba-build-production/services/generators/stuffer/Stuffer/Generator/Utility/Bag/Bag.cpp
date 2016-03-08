#include <stdafx.h>
#include <Bag.h>

Bag::Bag(int cacheLevel, ref next)
: cacheLevel(cacheLevel),
  next(next)
{
}

int Bag::getCacheLevel() const
{
  return cacheLevel;
}

Bag::ref Bag::getNext() const
{
  return next;
}

void Bag::setBoolParam(param_tag tag, bool value)
{
  data[tag].boolValue = value;
}

bool Bag::getBoolParam(param_tag tag, bool defaultValue) const
{
  auto it(data.find(tag));
  if (it != data.end())
    return it->second.boolValue;

  if (next)
    return next->getBoolParam(tag, defaultValue);

  return defaultValue;
}

void Bag::setIntParam(param_tag tag, int value)
{
  data[tag].intValue = value;
}

int Bag::getIntParam(param_tag tag, int defaultValue) const
{
  auto it(data.find(tag));
  if (it != data.end())
    return it->second.intValue;

  if (next)
    return next->getIntParam(tag, defaultValue);

  return defaultValue;
}

void Bag::overwriteParam(const Bag& other)
{
  for (auto value : other.data)
    data[value.first] = value.second;
}
