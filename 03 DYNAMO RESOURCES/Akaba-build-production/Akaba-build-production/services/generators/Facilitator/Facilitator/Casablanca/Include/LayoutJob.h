#pragma once

#include <JobRequest.h>

class Vision;

class LayoutJob
{
public:
  LayoutJob(unique_ptr<Vision> vision);

  void startJob();

  const vector<unique_ptr<Design>>& getDesigns() const;

  web::json::value AsJSON() const;
  web::json::value DesignsAsJSON() const;

private:
  chrono::system_clock::time_point timeSubmitted;
  chrono::system_clock::time_point timeStarted;
  chrono::system_clock::time_point timeCompleted;
  bool completed;

  int count;
  vector<unique_ptr<Design>> tasks;

  shared_ptr<Vision> vision;

  void completeJob(unique_ptr<Design> design);
};
