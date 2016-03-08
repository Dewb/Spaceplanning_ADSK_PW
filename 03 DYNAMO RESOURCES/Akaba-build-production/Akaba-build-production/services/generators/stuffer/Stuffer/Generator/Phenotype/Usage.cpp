#include <stdafx.h>
#include <Usage.h>
#include <Phenotype.h>
#include <Floor.h>
#include <Section.h>

map<string_t, Usage::name_tag> Usage::tags{ make_pair(U("Unknown"), -1) };
Usage::name_tag Usage::lastTag(0);

Usage::name_tag Usage::tag(const string_t& name)
{
  auto tagIt(tags.find(name));
  if (tagIt != tags.end())
    return tagIt->second;

  int tag(++lastTag);
  tags[name] = tag;

  return tag;
}

const string_t& Usage::name(name_tag tag)
{
  for (const auto& tagIt : tags)
    if (tagIt.second == tag)
      return tagIt.first;

  return name(-1);
}

void Usage::init(name_tag tag, float area, int count)
{
  auto usage(current.find(tag));
  current[tag] = make_pair(area, count);
}

void Usage::add(name_tag tag, float area)
{
  auto usage(current.find(tag));
  if (usage == current.end())
  {
    current[tag] = make_pair(area, 1);
    return;
  }
  
  usage->second.first += area;
  ++usage->second.second;
}

const Usage::value_type* Usage::find(name_tag tag)
{
  auto it(current.find(tag));
  if (it == current.end())
    return nullptr;

  return &it->second;
}
