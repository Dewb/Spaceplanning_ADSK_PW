#pragma once

class Section;

class Adjacency
{
public:
  static unique_ptr<Adjacency> calculate(const Section& section1, const Section& section2);

  float getOverlapAmount() const;

private:
  static unique_ptr<Adjacency> calculate(bool horizontal, float value, const Rangef& range1, const Rangef& range2);

  bool horizontal;
  float value;
  Rangef overlap;

  Adjacency(bool horizontal, float value, const Rangef& overlap);
};
