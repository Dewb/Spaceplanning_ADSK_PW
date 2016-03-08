#pragma once

class Vision;

class GeneratorGraphicsAPI
{
public:
  virtual ~GeneratorGraphicsAPI() = default;

  virtual void setTargetIndex(int targetIndex) = 0;
  virtual void releaseResources() = 0;

  virtual int getDisplayLevel() const = 0;
  virtual void setDisplayLevel(int displayLevel) = 0;

  virtual void displayPlanState(shared_ptr<Vision> vision) = 0;
};
