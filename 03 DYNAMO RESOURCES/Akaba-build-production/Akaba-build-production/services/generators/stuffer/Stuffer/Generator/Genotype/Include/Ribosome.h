#pragma once

#include <Genome.h>

class Ribosome
{
  friend class Genome;

public:
  static float fromBool(bool value);
  static float fromAmount(const Rangei& range, int value);

  Ribosome(Genome& genome);
  ~Ribosome();

  bool getBool();
  float getValue(float val);
  int getAmount(const Rangei& range);
  list<int> getOrder(const list<int>& original);
  Rect2i insetRect(const Rect2i& outer, const Rect2i& inner);

private:
  static default_random_engine* generator;
  static int count;
  static float newValue();

  static bool getBool(float gene);
  static int getAmount(float gene, const Rangei& pt);

  Genome* genome;
  Genome::gene_structure::iterator it;

  float next();
};
