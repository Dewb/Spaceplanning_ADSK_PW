
#include "BuildingGenerator.h"

#define DEG_TO_RAD M_PI/180.0
#define GRID_SPACING 0.5

namespace {

    struct ConnectionResult {
        ConnectionResult(bool v) : connected(v) {}
        Vec3f connectionPoint;
        Vec3f overlapEndpoint1;
        Vec3f overlapEndpoint2;
        bool connected;
    };

    float roundToGrid(float value, float grid) {
        float x = value / grid;
        return round(x) * grid;
    }
    
    ConnectionResult whereDoSpacesConnect(const Space& space1, const Space& space2, float minimumDoorWidth = 1.5) {

        // todo: check levels
        
        // Do the two spaces touch, with enough overlap to fit a door?
        float d = minimumDoorWidth;
        float x = fabs(space1.getPosition()[0] - space2.getPosition()[0]) - 0.5 * (space1.getWorldDimensions()[0] + space2.getWorldDimensions()[0]);
        float y = fabs(space1.getPosition()[1] - space2.getPosition()[1]) - 0.5 * (space1.getWorldDimensions()[1] + space2.getWorldDimensions()[1]);
        
        // If not, no overlap
        if (!(x == 0 && y < -d) &&
            !(y == 0 && x < -d))
            return ConnectionResult(false);
        
        // Where do they touch (midpoint of overlap)?
        Vec3f connectionPoint;
        int matchedAxis = (x == 0 ? 0 : 1);
        int unmatchedAxis = (matchedAxis == 1 ? 0 : 1);
        connectionPoint[matchedAxis] = (space1.getPosition()[matchedAxis] < space2.getPosition()[matchedAxis] ? -0.5 : 0.5) *
        space2.getWorldDimensions()[matchedAxis] + space2.getPosition()[matchedAxis];
        
        // Place most doors at the centerpoint between spaces for now, but prefer placing stairwell doors directly in front of a run
        float positionOfDoor = 0.5;
//        if (space1.data.type == "Stairs") {
//            positionOfDoor = 0.75;
//        } else if (space2.data.type == "Stairs") {
//            positionOfDoor = 0.25;
//        }
        
        float overlapEnd1, overlapEnd2;
        overlapEnd1 = max(space1.getPosition()[unmatchedAxis] - space1.getWorldDimensions()[unmatchedAxis] / 2,
                          space2.getPosition()[unmatchedAxis] - space2.getWorldDimensions()[unmatchedAxis] / 2);
        overlapEnd2 = min(space1.getPosition()[unmatchedAxis] + space1.getWorldDimensions()[unmatchedAxis] / 2,
                          space2.getPosition()[unmatchedAxis] + space2.getWorldDimensions()[unmatchedAxis] / 2);
        connectionPoint[unmatchedAxis] = overlapEnd1 + (overlapEnd2 - overlapEnd1) * positionOfDoor;
        
        // for stairs only
//        if (!entersOnValidFace(space1, connectionPoint) || !entersOnValidFace(space2, connectionPoint)) {
//            return ConnectionResult(false);
//        }
        
        Vec3f overlapPoint1(0, 0);
        Vec3f overlapPoint2(0, 0);
        overlapPoint1[matchedAxis] = connectionPoint[matchedAxis];
        overlapPoint1[unmatchedAxis] = overlapEnd1;
        overlapPoint2[matchedAxis] = connectionPoint[matchedAxis];
        overlapPoint2[unmatchedAxis] = overlapEnd2;
        
        ConnectionResult result(true);
        result.connectionPoint = connectionPoint;
        result.overlapEndpoint1 = overlapPoint1;
        result.overlapEndpoint2 = overlapPoint2;
        return result;
    }
    
    float snapScalar(float val) {
        // Round to nearest half unit in each dimension
        return round(val * 2) / 2;
    }
    
    bool doSpacesOverlap(const Space& space1, const Space& space2) {
        return
            (fabs(space1.getPosition()[0] - space2.getPosition()[0]) * 2
                < (space1.getWorldDimensions()[0] + space2.getWorldDimensions()[0])) &&
            (fabs(space1.getPosition()[1] - space2.getPosition()[1]) * 2
                < (space1.getWorldDimensions()[1] + space2.getWorldDimensions()[1]));
    }
    
    void positionSpaceRelativeToOtherSpace(Space& space, Space& otherSpace,
                                           const Vec3f& moveVec, const Vec3f& previousMoveVec)  {
        float minX = (otherSpace.getWorldDimensions()[0] + space.getWorldDimensions()[0]) / 2;
        float minY = (otherSpace.getWorldDimensions()[1] + space.getWorldDimensions()[1]) / 2;
        float tx = fabs(minX / moveVec[0]);
        float ty = fabs(minY / moveVec[1]);
        float dx = moveVec[0] * min(tx, ty);
        float dy = moveVec[1] * min(tx, ty);
        
        space.setPosition(roundToGrid(otherSpace.getPosition()[0] + dx, GRID_SPACING), 
                          roundToGrid(otherSpace.getPosition()[1] + dy, GRID_SPACING));
       
        if ((otherSpace.usage == "Hall" && space.usage == "Hall") &&
            (otherSpace.getWorldDimensions()[0] == space.getWorldDimensions()[1] ||
             otherSpace.getWorldDimensions()[1] == space.getWorldDimensions()[0])) {
                
            if (fabs(moveVec[0]) > fabs(moveVec[1])) {
                dy += 0.5 * (otherSpace.getWorldDimensions()[1] - space.getWorldDimensions()[1])
                        * (previousMoveVec[1] < 0 ? -1 : 1);
            } else {
                dx += 0.5 * (otherSpace.getWorldDimensions()[0] - space.getWorldDimensions()[0])
                        * (previousMoveVec[0] < 0 ? -1 : 1);
            }

            space.setPosition(roundToGrid(otherSpace.getPosition()[0] + dx, GRID_SPACING),
                              roundToGrid(otherSpace.getPosition()[1] + dy, GRID_SPACING));
        }
        
        if (doSpacesOverlap(space, otherSpace)) {
            ucout << "Placement failed to prevent overlap";
            ucout << "space " << space;
            ucout << "other " << otherSpace;
            ucout << "(" << moveVec << ") dx: " << dx << " dy: " << dy;
        }
    }
    
    void addSpace(BuildingGeneratorState& state, Space& space) {
        space.setPosition(state.position.x, state.position.y);
        if (state.results) {
            
            if (state.results->size()) {
                Vec3f headingVec(cos(state.heading * DEG_TO_RAD), sin(state.heading * DEG_TO_RAD));
                Vec3f previousHeadingVec(cos(state.previousHeading * DEG_TO_RAD), sin(state.previousHeading * DEG_TO_RAD));
                positionSpaceRelativeToOtherSpace(space, state.results->at(state.lastSpaceIndex),
                                                  headingVec, previousHeadingVec);
            }

            state.results->push_back(space);
            state.lastSpaceIndex = state.results->size() - 1;
        }
        
        state.position = space.origin;
    }
    
    void place_entry(BuildingGeneratorState& state, FloatParams& params) {
        Space space;
        space.usage = "Entry";
        space.dimensions.x = 20;
        space.dimensions.y = 8;
        space.dimensions.z = 3.5;
        space.isCirculation = true;
        addSpace(state, space);
    }

    void place_horizontal_circulation(BuildingGeneratorState& state, FloatParams& params) {
        float length = 8;

        Space space;
        space.usage = "Hall";
        Vec3f headingVec(cos(state.heading * DEG_TO_RAD), sin(state.heading * DEG_TO_RAD));
        Vec3f headingNormal(headingVec.y, -headingVec.x);
        space.dimensions.x = roundToGrid(fabs(headingVec[0] * length) + fabs(headingNormal[0] * state.circulationWidth), GRID_SPACING);
        space.dimensions.y = roundToGrid(fabs(headingVec[1] * length) + fabs(headingNormal[1] * state.circulationWidth), GRID_SPACING);
        space.dimensions.z = 3.5;

        space.isCirculation = true;
        
        addSpace(state, space);
    }

    void place_vertical_circulation(BuildingGeneratorState& state, FloatParams& params) {
        Space space;
        space.usage = "Stairs";
        space.dimensions.x = 3;
        space.dimensions.y = 5;
        space.dimensions.z = 35;
        space.isCirculation = true;
        addSpace(state, space);
    }

    void place_functional_space(BuildingGeneratorState& state, FloatParams& params) {
        Space space;
        space.usage = "Occupiable";
        space.dimensions.x = 8;
        space.dimensions.y = 8;
        space.dimensions.z = 3.5;
        space.isCirculation = false;
        addSpace(state, space);
    }
    
    void place_services(BuildingGeneratorState& state, FloatParams& params) {
        
    }
    
    void rotate_cw(BuildingGeneratorState& state, FloatParams& params) {
        state.previousHeading = state.heading;
        state.heading = fmod(state.heading + state.angle, 360.0f);
    }
    
    void rotate_ccw(BuildingGeneratorState& state, FloatParams& params) {
        state.previousHeading = state.heading;
        state.heading = fmod(state.heading - state.angle, 360.0f);
    }
    
}

BuildingGeneratorState::BuildingGeneratorState()
: angle(90)
, heading(0)
, position(0, 0)
, lastSpaceIndex(0)
, circulationWidth(2)
{
}

BuildingGenerator::BuildingGenerator() {
    add(Symbol('E').action(place_entry));
    add(Symbol('C').action(place_horizontal_circulation));
    add(Symbol('V').action(place_vertical_circulation));
    add(Symbol('F').action(place_functional_space));
    add(Symbol('S').action(place_services));
    add(Symbol('+').action(rotate_ccw));
    add(Symbol('-').action(rotate_cw));
    add(Symbol('[').startsGroup());
    add(Symbol(']').endsGroup());
}

void BuildingGenerator::begin(BuildingGeneratorState& state) {
    
}

void BuildingGenerator::end(BuildingGeneratorState& state) {

}


