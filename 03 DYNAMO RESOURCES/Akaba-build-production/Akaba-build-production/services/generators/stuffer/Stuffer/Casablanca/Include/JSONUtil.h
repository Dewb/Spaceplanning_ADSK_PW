#pragma once

#include <SpaceLayout.h>
#include <Point3.h>

namespace JSONUtil
{
  ////////////////////////////////////////////////////////////////////////////////////////
  // Extraction Helpers

  bool extractBool(const web::json::object& object, string_t tag, bool& value);
  bool extractInt(const web::json::object& object, string_t tag, int& value);
  bool extractFloat(const web::json::object& object, string_t tag, float& value);
  bool extractString(const web::json::object& object, string_t tag, string_t& value);
  const web::json::object* extractObject(const web::json::object& object, string_t tag);
  const web::json::array* extractArray(const web::json::object& object, string_t tag);

  ////////////////////////////////////////////////////////////////////////////////////////
  // Point Helpers

  Point3f Point3fFromJSON(const web::json::value& value);
  web::json::value Point3fAsJSON(const Point3f& pt);
}
