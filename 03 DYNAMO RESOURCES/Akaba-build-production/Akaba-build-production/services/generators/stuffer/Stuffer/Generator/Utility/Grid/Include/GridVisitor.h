#pragma once

class GridVisitor
{
public:
  virtual ~GridVisitor() = default;

  virtual void outerLoopReset() {};
  virtual void outerLoopComplete() {};
  virtual void innerLoopReset() {};
  virtual void innerLoopComplete() {};
};
