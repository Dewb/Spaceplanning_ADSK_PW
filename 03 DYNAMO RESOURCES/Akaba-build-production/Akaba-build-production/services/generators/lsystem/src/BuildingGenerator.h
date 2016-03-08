#pragma once

#include "LSystemGenerator.h"
#include "SpaceLayout.h"


class BuildingGeneratorState {
public:
    BuildingGeneratorState();
    
    float angle;
    
    float heading;
    float previousHeading;
    Vec3f position;
    Vec3f previousPosition;
    
    int lastSpaceIndex;
    vector<Space> *results;

    float circulationWidth;
};

class BuildingGenerator : public Generator<BuildingGeneratorState> {
public:
    BuildingGenerator();
    virtual void begin(BuildingGeneratorState& state);
    virtual void end(BuildingGeneratorState& state);
};

