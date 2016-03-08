#include <stdafx.h>
#include <Genome.h>
#include <Ribosome.h>

Genome::Genome()
{
}

Genome::Genome(const gene_structure& genes)
: genes(genes)
{
}

unique_ptr<Genome> Genome::clone() const
{
  return make_unique<Genome>(genes);
}

void Genome::discard(const Ribosome& ribosome)
{
  if (ribosome.genome != this)
    return;

  genes.erase(ribosome.it, genes.end());
}

unique_ptr<Genome> Genome::trim(const Ribosome& ribosome)
{
  if (ribosome.genome != this)
    return make_unique<Genome>();

  gene_structure trimmed;
  genes.splice(trimmed.begin(), trimmed, ribosome.it, genes.end());

  return make_unique<Genome>(trimmed);
}

void Genome::append(const Genome& other)
{
  genes.insert(genes.end(), other.genes.begin(), other.genes.end());
}

void Genome::mutate(float percent, float window)
{
  auto size(static_cast<int>(genes.size()));
  auto count(static_cast<int>(size*percent));
  for (auto num = 0; num < count; ++num)
  {
    auto index(Ribosome::getAmount(Ribosome::newValue(), Rangei(0, size - 1)));
    auto& value(*next(genes.begin(), index));

    auto offset(Ribosome::newValue()*window - window*0.5f);
    value = (value + offset);
    while (value < 0.0f || value > 1.0f)
      value += (value < 0.0f) ? 1.0f : -1.0f;
  }
}
