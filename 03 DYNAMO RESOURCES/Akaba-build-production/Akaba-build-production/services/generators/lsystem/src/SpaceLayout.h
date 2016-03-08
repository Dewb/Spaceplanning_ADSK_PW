#pragma once
#include "stdafx.h"

#define USAGENAME U("usageName")
#define POSITION U("position")
#define DIMENSIONS U("dimensions")
#define ISCIRCULATION U("isCirculation")

#define SPACES U("spaces")
#define DESIGNS U("designs")

#define STATUS U("status")
#define DESIGNCOUNT U("designCount")
#define TIMESUBMITTED U("timeSubmitted")
#define TIMESTARTED U("timeStarted")
#define TIMECOMPLETED U("timeCompleted")
#define MESSAGE U("message")


struct Vec3f {
    union {
        struct {
            float x;
            float y;
            float z;
        };
        float xyz[3];
    };

    Vec3f() : x(0), y(0), z(0) {}
    Vec3f(float _x, float _y) : x(_x), y(_y), z(0) {}
    Vec3f(float _x, float _y, float _z) : x(_x), y(_y), z(_z) {}

    static Vec3f FromJSON(const web::json::value & value);
    web::json::value AsJSON() const;

    const float& operator[](const int index) const { return xyz[index]; }
    float& operator[](const int index) { return xyz[index]; }
};

std::ostream& operator<<(std::ostream& os, const Vec3f& vec);


struct Space {
    std::string usage;
    Vec3f origin;
    Vec3f dimensions;
    bool isCirculation;

    // for easier migration of JS Akaba functions
    const Vec3f& getPosition() const { return origin; }
    void setPosition(float x, float y, float z = 0);
    const Vec3f& getWorldDimensions() const { return dimensions; }

    static Space FromJSON(const web::json::object & object);
    web::json::value AsJSON() const;
};

std::ostream& operator<<(std::ostream& os, const Space& s);


struct Design {
    std::vector<Space> spaces;

    static Design FromJSON(const web::json::object & object);
    web::json::value AsJSON() const;
};


