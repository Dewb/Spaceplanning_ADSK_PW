#include <stdafx.h>

#include <SpaceLayout.h>
#include <JSONUtil.h>

using namespace web;
using namespace JSONUtil;

void Space::setPosition(float x, float y, float z) 
{ 
  origin.x() = x;
  origin.y() = y;
  origin.z() = z;
}

Space Space::FromJSON(const json::object& object) 
{
  Space result;
  extractString(object, USAGENAME, result.usage);
  result.origin = Point3fFromJSON(object.at(POSITION));
  result.dimensions = Point3fFromJSON(object.at(DIMENSIONS));
  extractBool(object, ISCIRCULATION, result.isCirculation);
  return result;
}

json::value Space::AsJSON() const 
{
  json::value result = json::value::object();
  result[USAGENAME] = json::value::string(usage);
  result[POSITION] = Point3fAsJSON(origin);
  result[DIMENSIONS] = Point3fAsJSON(dimensions);
  result[ISCIRCULATION] = json::value::boolean(isCirculation);
  return result;
}

Design Design::FromJSON(const json::object & object) 
{
  Design result; 
  json::value jspaces = object.at(SPACES);
  for (auto& item : jspaces.as_array()) 
  {
    if (!item.is_null()) 
    {
      Space space;
      space = Space::FromJSON(item.as_object());
      result.spaces.push_back(space);
    }
  }

  return result;
}

json::value Design::AsJSON() const 
{
  json::value result = json::value::object();
  json::value jspaces = json::value::array(spaces.size());
  int idx = 0;
  for (auto& space : spaces)
      jspaces[idx++] = space.AsJSON();

  result[SPACES] = jspaces;

  return result;
}
