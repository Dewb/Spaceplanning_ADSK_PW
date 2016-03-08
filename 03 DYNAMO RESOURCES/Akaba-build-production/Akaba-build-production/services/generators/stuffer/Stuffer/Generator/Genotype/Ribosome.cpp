#include <stdafx.h>
#include <Ribosome.h>

////////////////////////////////////////////////////////////////////////////
// Static class items

float Ribosome::fromBool(bool value)
{
  return value ? 1.0f : 0.0f;
}

float Ribosome::fromAmount(const Rangei& range, int value)
{
  return (value - range.l()) / static_cast<float>(range.h() - range.l() + 1);
}

default_random_engine* Ribosome::generator(nullptr);
int Ribosome::count(0);

float Ribosome::newValue()
{
  if (!generator)
  {
    generator = new default_random_engine;
    unsigned long seed(static_cast<unsigned long>(chrono::high_resolution_clock::now().time_since_epoch().count()));
    generator->seed(seed);
  }

  return uniform_real_distribution<float>(0.0f, 1.0f)(*generator);
}

bool Ribosome::getBool(float gene)
{
  return gene > 0.5;
}

int Ribosome::getAmount(float gene, const Rangei& range)
{
  int dr(range.h() - range.l());
  return static_cast<int>((dr + 1)*gene) + range.l();
}

////////////////////////////////////////////////////////////////////////////
// Regular class methods

Ribosome::Ribosome(Genome& genome)
: genome(&genome),
  it(genome.genes.begin())
{
  ++count;
}

Ribosome::~Ribosome()
{
  if (--count == 0 && generator)
  {
    delete generator;
    generator = nullptr;
  }
}

bool Ribosome::getBool()
{
  return getBool(next());
}

float Ribosome::getValue(float val)
{
  return next()*val;
}

int Ribosome::getAmount(const Rangei& pt)
{
  return getAmount(next(), pt);
}

list<int> Ribosome::getOrder(const list<int>& original)
{
  map<float, int> items;
  for (const auto& item : original)
    items[next()] = item;

  list<int> order;
  for (const auto& item : items)
    order.push_back(item.second);

  return order;
}

Rect2i Ribosome::insetRect(const Rect2i& outer, const Rect2i& inner)
{
  Point2i off(
    getAmount(Rangei(0, outer.w() - inner.w())),
    getAmount(Rangei(0, outer.h() - inner.h())));

  Rect2i rect(inner);
  rect.offset(off);

  return rect;
}

float Ribosome::next()
{
  if (it != genome->genes.end())
    return *it++;

  float gene(newValue());
  genome->genes.push_back(gene);
  it = genome->genes.end();

  return gene;
}
