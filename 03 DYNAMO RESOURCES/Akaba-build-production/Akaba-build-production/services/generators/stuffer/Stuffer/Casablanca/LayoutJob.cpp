#include <stdafx.h>
#include <LayoutJob.h>

using namespace web;
using namespace chrono;

LayoutJob::LayoutJob()
: m_timeSubmitted(steady_clock::now())
{
}

void LayoutJob::generateDesigns(int count)
{
  m_timeStarted = steady_clock::now();
  while (count-- > 0) 
    m_designs.push_back(generateDesign());
  m_completed = true;
  m_timeCompleted = steady_clock::now();
}

const vector<unique_ptr<Design>>& LayoutJob::getDesigns() const
{
  return m_designs;
}

namespace
{
  string_t timeToString(steady_clock::time_point time)
  {
    stringstream_t ss;
    ss << time.time_since_epoch().count();
    return ss.str();
  }
}

json::value LayoutJob::AsJSON() const
{
  json::value result = json::value::object();
  result[STATUS] = json::value::string(m_completed ? U("completed") : U("in-progress"));
  result[DESIGNCOUNT] = json::value::number((int)m_designs.size());
  result[TIMESUBMITTED] = json::value::string(timeToString(m_timeSubmitted) + U("Z"));
  result[TIMESTARTED] = json::value::string(timeToString(m_timeStarted) + U("Z"));

  if (m_completed)
    result[TIMECOMPLETED] = json::value::string(timeToString(m_timeCompleted) + U("Z"));

  return result;
}

json::value LayoutJob::DesignsAsJSON() const
{
  json::value jdesigns = json::value::array(m_designs.size());
  int idx = 0;
  for (auto& design : m_designs)
    jdesigns[idx++] = design->AsJSON();

  return jdesigns;
}
