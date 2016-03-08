#pragma once

class GridValue
{
public:
  GridValue(bool normal);
  GridValue(const GridValue& other);

  int getValue() const;
  void setValue(int other);

  int next() const;
  int prev() const;

  void inc();
  void dec();

  bool compare(const int other) const;

private:
  bool normal;
  int value;

  int step() const;
};
