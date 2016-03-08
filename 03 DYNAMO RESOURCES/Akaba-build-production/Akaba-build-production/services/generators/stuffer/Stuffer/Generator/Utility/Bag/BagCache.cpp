#include <stdafx.h>
#include <BagCache.h>

Bag::param_tag BagCache::last(0);

Bag::param_tag BagCache::reserve()
{
  return ++last;
}

BagCache::BagCache()
: level(0)
{
  db.emplace_back();
}

Bag::ref BagCache::createItem(int id, Bag::ref next)
{
  Bag::ref newData(new Bag(level, next));
  db[level].push_back(newData);

  if (index.size() <= id)
    index.resize(id + 1);
  index[id] = newData;

  return newData;
}

Bag::ref BagCache::createBag()
{
  int id(static_cast<int>(db[level].size()));
  return createItem(id, nullptr);
}

void BagCache::createSnapshot()
{
  db.emplace_back();
  ++level;
}

void BagCache::mergeSnapshot()
{
  if (level == 0)
    return;

  for (auto item : index)
  {
    auto next(item->getNext());
    if (next && item->getCacheLevel() == level)
      next->overwriteParam(*item);
  }

  discardSnapshot();
}

void BagCache::discardSnapshot()
{
  db.pop_back();
  --level;
}
