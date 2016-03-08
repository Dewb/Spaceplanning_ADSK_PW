#pragma once

class Bag
{
public:
  using ref = shared_ptr<Bag>;
  using param_tag = int;

  Bag(int cacheLevel, ref next);

  int getCacheLevel() const;
  ref getNext() const;

  void setBoolParam(param_tag tag, bool value);
  bool getBoolParam(param_tag tag, bool defaultValue) const;

  void setIntParam(param_tag tag, int value);
  int getIntParam(param_tag tag, int defaultValue) const;

  void overwriteParam(const Bag& other);

private:
  int cacheLevel;
  ref next;

  union bag_data
  {
    bool boolValue;
    int intValue;
  };

  map<param_tag, bag_data> data;
};
