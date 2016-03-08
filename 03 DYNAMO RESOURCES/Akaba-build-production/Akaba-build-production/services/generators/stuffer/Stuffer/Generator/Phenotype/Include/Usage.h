#pragma once

class Phenotype;

class Usage
{
public:
  using name_tag = int;
  static name_tag tag(const string_t& name);
  static const string_t& name(name_tag tag);

  void init(name_tag tag, float area, int count);
  void add(name_tag tag, float area);

  using usages = map<string_t, pair<float, int>>;
  using value_type = pair<float, int>;
  const value_type* find(name_tag tag);

private:
  static map<string_t, name_tag> tags;
  static name_tag lastTag;

  map<name_tag, pair<float, int>> current;
};
