#pragma once

class Gene;
class Ribosome;

class Genome
{
  friend class Ribosome;

public:
  using gene_structure = list<float>;

  Genome();
  Genome(const gene_structure& genes);

  unique_ptr<Genome> clone() const;

  void discard(const Ribosome& ribosome);
  unique_ptr<Genome> trim(const Ribosome& ribosome);

  void append(const Genome& other);

  void mutate(float percent, float window);

private:
  gene_structure genes;
};
