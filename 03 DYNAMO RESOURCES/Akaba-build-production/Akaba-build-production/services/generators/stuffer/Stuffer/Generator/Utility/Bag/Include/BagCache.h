#pragma once

class BagCache
{
public:
  static Bag::param_tag reserve();

  using ref = shared_ptr<BagCache>;

  BagCache();

  Bag::ref createBag();

  void createSnapshot();
  void mergeSnapshot();
  void discardSnapshot();

private:
  static Bag::param_tag last;

  using cache_type = vector<Bag::ref>;

  vector<cache_type> db;
  int level;
  cache_type index;

  Bag::ref createItem(int id, Bag::ref next);
};
