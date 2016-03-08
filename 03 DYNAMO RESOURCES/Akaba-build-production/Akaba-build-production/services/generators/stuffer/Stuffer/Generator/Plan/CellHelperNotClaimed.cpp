#include <stdafx.h>
#include <CellHelperNotClaimed.h>
#include <Cell.h>

CellHelperNotClaimed::CellHelperNotClaimed()
: m_id(-1)
{
}

void CellHelperNotClaimed::setID(int id)
{
  m_id = id;
}

bool CellHelperNotClaimed::test(const Cell& cell) const
{
  if (m_id != -1 && cell.data->getIntParam(Cell::areaId, -1) != m_id)
    return false;

  return !cell.data->getBoolParam(Cell::claimed, false);
}
