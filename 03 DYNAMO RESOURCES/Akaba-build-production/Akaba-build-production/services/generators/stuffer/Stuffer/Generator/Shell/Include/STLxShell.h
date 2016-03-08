#pragma once

#include <Shell.h>

class STLxShell : public Shell
{
public:
  static unique_ptr<STLxShell> load(const string_t& data, const GridBasis& basis);

private:
  STLxShell(const string_t& data, const GridBasis& basis);

  void loadSTLxFromASCII(const string_t& data);
};
