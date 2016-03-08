#include <stdafx.h>
#include <Adjacency.h>
#include <Section.h>

unique_ptr<Adjacency> Adjacency::calculate(const Section& section1, const Section& section2)
{
  const Rect2f& rect1(section1.getRect());
  const Rect2f& rect2(section2.getRect());

  if (rect1.t() == rect2.b())
    return calculate(true, rect1.t(), Rangef(rect1.l(), rect1.r()), Rangef(rect2.l(), rect2.r()));

  if (rect1.b() == rect2.t())
    return calculate(true, rect1.b(), Rangef(rect1.l(), rect1.r()), Rangef(rect2.l(), rect2.r()));

  if (rect1.l() == rect2.r())
    return calculate(false, rect1.l(), Rangef(rect1.t(), rect1.b()), Rangef(rect2.t(), rect2.b()));

  if (rect1.r() == rect2.l())
    return calculate(false, rect1.r(), Rangef(rect1.t(), rect1.b()), Rangef(rect2.t(), rect2.b()));

  return nullptr;
}

unique_ptr<Adjacency> Adjacency::calculate(bool horizontal, float value, const Rangef& range1, const Rangef& range2)
{
  if (range1.h() < range2.l())
    return nullptr;

  if (range1.l() > range2.h())
    return nullptr;

  Rangef overlap(max(range1.l(), range2.l()), min(range1.h(), range2.h()));

  return unique_ptr<Adjacency>(new Adjacency(horizontal, value, overlap));
}

Adjacency::Adjacency(bool horizontal, float value, const Rangef& overlap)
: horizontal(horizontal),
  value(value),
  overlap(overlap)
{
}

float Adjacency::getOverlapAmount() const
{
  return overlap.h() - overlap.l();
}
