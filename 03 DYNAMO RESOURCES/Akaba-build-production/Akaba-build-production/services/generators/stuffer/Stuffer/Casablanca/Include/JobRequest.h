#pragma once

#include <SpaceLayout.h>

class SpaceRequest
{
public:
  static SpaceRequest FromJSON(const web::json::object& object);
  web::json::value AsJSON() const;

  SpaceRequest();

  string_t usage;
  bool circulation;
  int minimumArea;
  int minimumCount;
};

class Adjacency
{
public:
  static Adjacency FromJSON(const web::json::object& object);
  web::json::value AsJSON() const;

  Adjacency();

  string_t from;
  string_t to;
  int maxDistance;
};

class JobRequest
{
public:
  static JobRequest FromJSON(const web::json::object& object);
  web::json::value AsJSON() const;

  JobRequest();

  struct
  {
    map<string_t, float> code;
    vector<SpaceRequest> spaces;
    vector<Adjacency> adjacencies;
    struct
    {
      float width;
      float height;
    } site;
    web::http::uri shell;
    vector<Design> designs;
  } requirements;

  struct
  {
    string_t generatorName;
    int generatorVersion;
    int designs;
    float grid;
    float floorHeight;
    float hallwayWidth;
  } settings;
};
