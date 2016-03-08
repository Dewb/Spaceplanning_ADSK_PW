#include "SpaceLayout.h"

Vec3f Vec3f::FromJSON(const web::json::value & value) {
    Vec3f result;
    if (!value.is_array()) 
        return result;

    result.x = (float)value.at(0).as_double();
    result.y = (float)value.at(1).as_double();
    result.z = (float)value.at(2).as_double();
    return result;
}

web::json::value Vec3f::AsJSON() const  {
    web::json::value result = web::json::value::array(3);
    result[0] = web::json::value::number(x);
    result[1] = web::json::value::number(y);
    result[2] = web::json::value::number(z);
    return result;
}

std::ostream& operator<<(std::ostream& os, const Vec3f& vec) {
    os << "(" << vec.x << "," << vec.y << "," << vec.z << ")";
    return os;
}

void Space::setPosition(float x, float y, float z) { 
    origin.x = x;
    origin.y = y;
    origin.z = z;
}


Space Space::FromJSON(const web::json::object & object) {
    Space result;
    result.usage = object.at(USAGENAME).as_string();
    result.origin = Vec3f::FromJSON(object.at(POSITION));
    result.dimensions = Vec3f::FromJSON(object.at(DIMENSIONS));
    result.isCirculation = object.at(ISCIRCULATION).as_bool();
    return result;
}

web::json::value Space::AsJSON() const {
    web::json::value result = web::json::value::object();
    result[USAGENAME] = web::json::value::string(usage);
    result[POSITION] = origin.AsJSON();
    result[DIMENSIONS] = dimensions.AsJSON();
    result[ISCIRCULATION] = web::json::value::boolean(isCirculation);
    return result;
}


std::ostream& operator<<(std::ostream& os, const Space& s) {
    os << "Usage: " << s.usage << " Position: " << s.origin << " Dimensions: " << s.dimensions;
    return os;
}


Design Design::FromJSON(const web::json::object & object) {
    Design result; 
    web::json::value jspaces = object.at(SPACES);
    for (auto& item : jspaces.as_array()) {
        if (!item.is_null()) {
            Space space;
            space = Space::FromJSON(item.as_object());
            result.spaces.push_back(space);
        }
    }
    return result;
}

web::json::value Design::AsJSON() const {
    web::json::value result = web::json::value::object();
    web::json::value jspaces = web::json::value::array(spaces.size());
    int idx = 0;
    for (auto& space : spaces) {
        jspaces[idx++] = space.AsJSON();
    }
    result[SPACES] = jspaces;
    return result;
}

