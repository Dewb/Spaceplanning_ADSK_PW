#include <stdafx.h>
#include <LayoutJob.h>
#include <JobRequest.h>
#include <Vision.h>

using namespace web;
using namespace chrono;

LayoutJob::LayoutJob(unique_ptr<Vision> vision)
: vision(move(vision)),
  timeSubmitted(system_clock::now()),
  timeStarted(system_clock::time_point::max()),
  timeCompleted(system_clock::time_point::max()),
  completed(false),
  count(vision->request().settings.designs)
{
}

void LayoutJob::startJob()
{
  timeStarted = system_clock::now();

  pplx::parallel_for(size_t(0), size_t(count), [&](size_t)
  {
    vision->execute([&](unique_ptr<Design> design) {
      completeJob(move(design));
    });
  });
}

void LayoutJob::completeJob(unique_ptr<Design> design)
{
  tasks.push_back(move(design));
  --count;

  if (count == 0)
  {
    completed = true;
    timeCompleted = steady_clock::now();
  }
}

const vector<unique_ptr<Design>>& LayoutJob::getDesigns() const
{
  return tasks;
}

namespace
{
  string_t timeToString(system_clock::time_point time)
  {
    stringstream_t ss;
    ss << time.time_since_epoch().count();
    return ss.str();
  }
}

json::value LayoutJob::AsJSON() const
{
  json::value result = json::value::object();
  result[STATUS] = json::value::string(completed ? U("completed") : U("in-progress"));
  result[DESIGNCOUNT] = json::value::number((int)tasks.size());
  result[TIMESUBMITTED] = json::value::string(timeToString(timeSubmitted) + U("Z"));
  result[TIMESTARTED] = json::value::string(timeToString(timeStarted) + U("Z"));

  if (completed)
    result[TIMECOMPLETED] = json::value::string(timeToString(timeCompleted) + U("Z"));

  return result;
}

json::value LayoutJob::DesignsAsJSON() const
{
  json::value jdesigns = json::value::array(tasks.size());
  int idx = 0;
  for (auto& design : tasks)
    jdesigns[idx++] = design->AsJSON();

  return jdesigns;
}
