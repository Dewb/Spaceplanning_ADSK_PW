#include <stdafx.h>
#include <ListStrategy.h>

void ListStrategy::addStrategy(unique_ptr<const Strategy> pStrategy)
{
  m_strategies.push_back(move(pStrategy));
}

const string_t& ListStrategy::name() const
{
  static string_t name(U("ListStrategy"));
  return name;
}

void ListStrategy::execute(const Args& args) const
{
  ucout << U("Starting list...") << endl;
  for (const auto& strategy : m_strategies)
  {
    ucout << U("Strategy: ") << strategy->name() << U(" ... ");
    strategy->execute(args);
    ucout << U("complete.") << endl;
  }
  ucout << U("List complete.") << endl;
}
