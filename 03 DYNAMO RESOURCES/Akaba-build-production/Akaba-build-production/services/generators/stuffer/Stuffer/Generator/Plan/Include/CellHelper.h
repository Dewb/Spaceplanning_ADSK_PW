#pragma once

class Cell;

class CellHelper
{
public:
  virtual ~CellHelper() = default;
  virtual bool test(const Cell& cell) const = 0;
};
