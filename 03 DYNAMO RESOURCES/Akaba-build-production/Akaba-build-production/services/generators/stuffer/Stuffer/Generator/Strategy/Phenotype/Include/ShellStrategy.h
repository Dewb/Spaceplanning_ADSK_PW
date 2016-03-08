#pragma once

#include <Strategy.h>

class JobRequest;
class BuildData;
class Shell;
class Phenotype;
class Mesh;

class ShellStrategy : public Strategy
{
public:
  const string_t& name() const;
  void execute(const Args& args) const;

private:
  unique_ptr<const Shell> createShell(const Mesh* mesh, const Args& args) const;
};
