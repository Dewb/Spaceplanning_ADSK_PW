#include <stdafx.h>

#include <JobRequest.h>
#include <JSONUtil.h>

using namespace web;
using namespace JSONUtil;

#define REQUIREMENTS U("requirements")
  #define CODE U("code")
  #define SPACES U("spaces")
    #define USAGE U("usage")
    #define CIRCULATION U("circulation")
    #define MINIMUMAREA U("minimumArea")
    #define MINIMUMCOUNT U("minimumCount")
  #define ADJACENCIES U("adjacencies")
    #define FROM U("from")
    #define TO U("to")
    #define MAXDISTANCE U("maxDistance")
  #define SITE U("site")
    #define WIDTH U("width")
    #define HEIGHT U("height")
  #define SHELL U("shell")
  #define EXISTINGDESIGNS U("existingDesigns")
#define SETTINGS U("settings")
  #define GENERATORNAME U("generatorName")
  #define GENERATORVERSION U("generatorVersion")
  #define DESIGNS U("designs")
  #define GRID U("grid")
  #define FLOORHEIGHT U("floorheight")
  #define HALLWAYWIDTH U("hallwayWidth")

SpaceRequest::SpaceRequest()
: usage(U("Unknown")),
  circulation(false),
  minimumArea(0),
  minimumCount(0)
{
}

SpaceRequest SpaceRequest::FromJSON(const json::object& object)
{
  SpaceRequest result;

  extractString(object, USAGE, result.usage);
  extractBool(object, CIRCULATION, result.circulation);
  extractInt(object, MINIMUMAREA, result.minimumArea);
  extractInt(object, MINIMUMCOUNT, result.minimumCount);

  return result;
}

json::value SpaceRequest::AsJSON() const
{
  json::value result = json::value::object();
  result[USAGE] = json::value::string(usage);
  result[CIRCULATION] = json::value::boolean(circulation);
  result[MINIMUMAREA] = json::value::number(minimumArea);
  result[MINIMUMCOUNT] = json::value::number(minimumCount);

  return result;
}

Adjacency::Adjacency()
: from(U("Unknown")),
  to(U("Unknown")),
  maxDistance(-1)
{
}

Adjacency Adjacency::FromJSON(const json::object& object)
{
  Adjacency result;

  extractString(object, FROM, result.from);
  extractString(object, TO, result.to);
  extractInt(object, MAXDISTANCE, result.maxDistance);

  return result;
}

json::value Adjacency::AsJSON() const
{
  json::value result = json::value::object();
  result[FROM] = json::value::string(from);
  result[TO] = json::value::string(to);
  result[MAXDISTANCE] = json::value::number(maxDistance);

  return result;
}

JobRequest::JobRequest()
{
  requirements.site = { 0.0f, 0.0f };
  settings = { U("Unknown"), 0, 1, 1.0f, 3.5f, 4.0f };
}

JobRequest JobRequest::FromJSON(const json::object& object)
{
  JobRequest result;

  const auto& requirements(extractObject(object, REQUIREMENTS));
  if (requirements)
  {
    const auto& code(extractObject(*requirements, CODE));
    if (code)
      for (const auto& it : *code)
        if (!it.second.is_null() && it.second.is_number())
          result.requirements.code[it.first] = static_cast<float>(it.second.as_number().to_double());

    const auto& spaces(extractArray(*requirements, SPACES));
    if (spaces)
      for (const auto& it : *spaces)
        if (!it.is_null() && it.is_object())
          result.requirements.spaces.push_back(SpaceRequest::FromJSON(it.as_object()));

    const auto& adjacencies(extractArray(*requirements, ADJACENCIES));
    if (adjacencies)
      for (const auto& it : *adjacencies)
        if (!it.is_null() && it.is_object())
          result.requirements.adjacencies.push_back(Adjacency::FromJSON(it.as_object()));

    const auto& site(extractObject(*requirements, SITE));
    if (site)
    {
      extractFloat(*site, WIDTH, result.requirements.site.width);
      extractFloat(*site, HEIGHT, result.requirements.site.height);
    }

    string_t shellUrl;
    extractString(*requirements, SHELL, shellUrl);
    if (!shellUrl.empty())
      result.requirements.shell = shellUrl;

    const auto& designs(extractArray(*requirements, EXISTINGDESIGNS));
    if (designs)
      for (const auto& it : *designs)
        if (!it.is_null() && it.is_object())
          result.requirements.designs.push_back(Design::FromJSON(it.as_object()));
  }

  const auto& settings(extractObject(object, SETTINGS));
  if (settings)
  {
    extractString(*settings, GENERATORNAME, result.settings.generatorName);
    extractInt(*settings, GENERATORVERSION, result.settings.generatorVersion);
    extractInt(*settings, DESIGNS, result.settings.designs);
    extractFloat(*settings, GRID, result.settings.grid);
    extractFloat(*settings, FLOORHEIGHT, result.settings.floorHeight);
    extractFloat(*settings, HALLWAYWIDTH, result.settings.hallwayWidth);
  }

  return result;
}

json::value JobRequest::AsJSON() const
{
  auto result(json::value::object());
  int idx = 0;

  /////////////////////////////////////////////////////////////////////
  // Requirements

  json::value jrequirements = json::value::object();

  //map<string_t, float> code;
  json::value jcode = json::value::object();
  for (auto& codeItem : requirements.code)
    jcode[codeItem.first] = codeItem.second;
  jrequirements[CODE] = jcode;

  //vector<SpaceRequest> spaces;
  json::value jspaces = json::value::array(requirements.spaces.size());
  idx = 0;
  for (auto& space : requirements.spaces)
    jspaces[idx++] = space.AsJSON();
  jrequirements[SPACES] = jspaces;

  //vector<Adjacency> adjacencies;
  json::value jadjacencies = json::value::array(requirements.adjacencies.size());
  idx = 0;
  for (auto& adjacency : requirements.adjacencies)
    jadjacencies[idx++] = adjacency.AsJSON();
  jrequirements[ADJACENCIES] = jadjacencies;

  // site { float width, float height }
  json::value jsite = json::value::object();
  jsite[WIDTH] = json::value::number(requirements.site.width);
  jsite[HEIGHT] = json::value::number(requirements.site.height);
  jrequirements[SITE] = jsite;

  //web::http::uri shell;
  jrequirements[SHELL] = json::value::string(requirements.shell.to_string());

  //vector<Design> designs;
  json::value jdesigns = json::value::array(requirements.designs.size());
  idx = 0;
  for (auto& design : requirements.designs)
    jdesigns[idx++] = design.AsJSON();
  jrequirements[DESIGNS] = jdesigns;

  /////////////////////////////////////////////////////////////////////
  // Settings

  json::value jsettings = json::value::object();

  //string_t generatorName;
  jsettings[GENERATORNAME] = json::value::string(settings.generatorName);

  //int generatorVersion;
  jsettings[GENERATORVERSION] = json::value::number(settings.generatorVersion);

  //int designs;
  jsettings[DESIGNS] = json::value::number(settings.designs);

  //float grid;
  jsettings[GRID] = json::value::number(settings.grid);

  //float floorHeight;
  jsettings[FLOORHEIGHT] = json::value::number(settings.floorHeight);

  //float hallwayWidth;
  jsettings[HALLWAYWIDTH] = json::value::number(settings.hallwayWidth);

  /////////////////////////////////////////////////////////////////////////
  // Assemble return value

  result[REQUIREMENTS] = jrequirements;
  result[SETTINGS] = jsettings;

  return result;
}
