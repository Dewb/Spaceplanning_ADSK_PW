cmake_minimum_required (VERSION 2.6)
project (AkabaGeneratorService CXX)

add_executable (AkabaGeneratorService
	src/main.cpp
	src/GeneratorListener.cpp
	src/BuildingGenerator.cpp
	src/SpaceLayout.cpp
	lib/libplsys/src/LSystem.cpp
	lib/libplsys/src/LSystemRulesEngine.cpp
	lib/libplsys/src/ProductionRule.cpp
	lib/libplsys/src/RuleString.cpp
	lib/libplsys/src/RuleToken.cpp
)
include_directories (
	lib/libplsys/include
    lib/casablanca/include
)

# Boost
find_package(Boost COMPONENTS system thread chrono program_options date_time REQUIRED)
target_link_libraries (AkabaGeneratorService
    ${Boost_LIBRARIES}
)
include_directories(
    ${Boost_INCLUDE_DIRS}
)

# Casablanca
find_library(
    CPPREST
    NAMES cpprest libcpprest.2.4.dylib
    PATHS 
        /usr/lib
        /usr/local/lib
        /usr/lib/i386-linux-gnu 
        lib/casablanca/lib/osx
)
target_link_libraries (AkabaGeneratorService
    ${CPPREST}
)

# Compile Options
# XXX: this requires CMake 3.1.0
#      http://stackoverflow.com/questions/10984442/how-to-detect-c11-support-of-a-compiler-with-cmake/20165220#20165220
#set_property(TARGET AkabaGeneratorService PROPERTY CXX_STANDARD 11)
#set_property(TARGET AkabaGeneratorService PROPERTY CXX_STANDARD_REQUIRED ON)

set(CMAKE_CXX_FLAGS "-std=c++11")
set(CMAKE_BUILD_TYPE "RelWithDebInfo")

install(TARGETS AkabaGeneratorService DESTINATION bin/)
