#pragma once

#include <CellHelper.h>

class CellHelperNotClaimed : public CellHelper
{
public:
  CellHelperNotClaimed();

  void setID(int id);
  bool test(const Cell& cell) const;

private:
  int m_id;
};
