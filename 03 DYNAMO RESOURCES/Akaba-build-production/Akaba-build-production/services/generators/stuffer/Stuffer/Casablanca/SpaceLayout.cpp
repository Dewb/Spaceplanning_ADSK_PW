#include <stdafx.h>

#include <SpaceLayout.h>
#include <JSONUtil.h>

using namespace web;
using namespace JSONUtil;

Space::Space()
: isCirculation(false)
{
}

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

Mesh Mesh::FromJSON(const json::object& object)
{
  Mesh result;
  extractString(object, INLINEDATA, result.inlineData);
  return result;
}

json::value Mesh::AsJSON() const
{
  json::value result = json::value::object();
  result[INLINEDATA] = json::value::string(inlineData);
  return result;
}

Design Design::FromJSON(const json::object & object)
{
  Design result;

  // Extract spaces
  auto spacesIt(object.find(SPACES));
  if (spacesIt != object.end())
  {
    for (auto& item : spacesIt->second.as_array())
    {
      if (!item.is_null())
        result.spaces.push_back(Space::FromJSON(item.as_object()));
    }
  }

  // Extract meshes
  auto meshesIt(object.find(MESHES));
  if (meshesIt != object.end())
  {
    for (auto& item : meshesIt->second.as_array())
    {
      if (!item.is_null())
        result.meshes.push_back(Mesh::FromJSON(item.as_object()));
    }
  }

  return result;
}

json::value Design::AsJSON() const
{
  json::value result = json::value::object();

  // Convert spaces
  json::value jspaces = json::value::array(spaces.size());
  int idxSpace = 0;
  for (auto& space : spaces)
    jspaces[idxSpace++] = space.AsJSON();

  result[SPACES] = jspaces;

  // Convert meshes
  json::value jmeshes = json::value::array(meshes.size());
  int idxMesh = 0;
  for (auto& mesh : meshes)
    jmeshes[idxMesh++] = mesh.AsJSON();

  result[MESHES] = jmeshes;

  return result;
}
