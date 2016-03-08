#pragma once

#include <CellHelper.h>

class CellHelperConnectedUp : public CellHelper
{
public:
  bool test(const Cell& cell) const;
};
