#pragma once

#include <Level.h>

class JobRequest;

class Shell
{
public:
  Shell(const GridBasis& basis);
  virtual ~Shell() = default;

  using level_map = map<int, unique_ptr<Level>>;
  const level_map& getLevels() const;

protected:
  GridBasis basis;
  level_map levels;

  Level& createLevel(int levelNum);
};
