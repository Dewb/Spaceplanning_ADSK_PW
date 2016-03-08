#include <stdafx.h>
#include <EdgeEffectStrategy.h>
#include <Args.h>
#include <CellHelper.h>

EdgeEffectStrategy::EdgeEffectStrategy()
: m_range(Rangei(0, 0)),
  m_shutoff(0),
  m_atEdgeOnly(false)
{
}

void EdgeEffectStrategy::setRange(const Rangei& range)
{
  m_range = range;
}

void EdgeEffectStrategy::setShutoff(int shutoff)
{
  m_shutoff = shutoff;
}

void EdgeEffectStrategy::setAtEdgeOnly(bool atEdgeOnly)
{
  m_atEdgeOnly = atEdgeOnly;
}

void EdgeEffectStrategy::execute(const Args& args) const
{
  processSides(args);
}

void EdgeEffectStrategy::processSides(const Args& args) const
{
  if (m_shutoff >= m_range.h())
    return;

  SideStrategy::processSides(args);
}

unique_ptr<EdgeVisitor> EdgeEffectStrategy::getVisitor(const Args& args, unique_ptr<CellHelper> helper) const
{
  Rangei adjusted(
    m_range.h(),
    (m_shutoff > m_range.l()) ? 0 : m_range.l() - m_shutoff);

  int amount = args.getRibosome().getAmount(adjusted);
  if (amount < m_range.l())
    return nullptr;

  return make_unique<EdgeVisitor>(amount, m_atEdgeOnly, move(helper));
}
