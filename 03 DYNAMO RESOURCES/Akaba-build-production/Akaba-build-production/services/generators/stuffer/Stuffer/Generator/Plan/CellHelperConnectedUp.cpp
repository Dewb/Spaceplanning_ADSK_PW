#include <stdafx.h>
#include <CellHelperConnectedUp.h>
#include <Cell.h>

bool CellHelperConnectedUp::test(const Cell& cell) const
{
  return cell.getVNeighbor(true) != nullptr;
}
