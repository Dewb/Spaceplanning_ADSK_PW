#pragma once

#include <SpaceLayout.h>

class LayoutJob
{
public:
  LayoutJob();

  void generateDesigns(int count);
  const vector<unique_ptr<Design>>& getDesigns() const;

  web::json::value AsJSON() const;
  web::json::value DesignsAsJSON() const;

protected:
  virtual unique_ptr<Design> generateDesign() = 0;

private:
  vector<unique_ptr<Design>> m_designs;
  chrono::steady_clock::time_point m_timeSubmitted;
  chrono::steady_clock::time_point m_timeStarted;
  chrono::steady_clock::time_point m_timeCompleted;
  bool m_completed;
};
