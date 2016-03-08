#pragma once

#define USAGENAME U("usageName")
#define POSITION U("position")
#define DIMENSIONS U("dimensions")
#define ISCIRCULATION U("isCirculation")

#define INLINEDATA U("inlineData")

#define SPACES U("spaces")
#define MESHES U("meshes")
#define DESIGNS U("designs")

#define STATUS U("status")
#define DESIGNCOUNT U("designCount")
#define TIMESUBMITTED U("timeSubmitted")
#define TIMESTARTED U("timeStarted")
#define TIMECOMPLETED U("timeCompleted")
#define MESSAGE U("message")

class Space
{
public:
  Space();

  string_t usage;
  Point3f origin;
  Point3f dimensions;
  bool isCirculation;

  // for easier migration of JS Akaba functions
  const Point3f& getPosition() const { return origin; }
  void setPosition(float x, float y, float z = 0);
  const Point3f& getWorldDimensions() const { return dimensions; }

  static Space FromJSON(const web::json::object & object);
  web::json::value AsJSON() const;
};

class Mesh
{
public:
  string_t inlineData;

  static Mesh FromJSON(const web::json::object & object);
  web::json::value AsJSON() const;
};

class Design
{
public:
  vector<Space> spaces;
  vector<Mesh> meshes;

  static Design FromJSON(const web::json::object & object);
  web::json::value AsJSON() const;
};
