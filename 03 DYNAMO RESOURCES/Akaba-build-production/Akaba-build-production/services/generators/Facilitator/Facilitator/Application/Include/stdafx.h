#pragma once

#ifdef _WIN32
#define TRACE(msg) wcout << msg
#else
#define TRACE(msg) cout << msg
#endif

#include <memory>
#include <string>
#include <map>
#include <list>
#include <vector>
#include <set>
#include <random>
#include <thread>
#include <chrono>

#include <cpprest/json.h>
#include <cpprest/http_listener.h>
#include <cpprest/uri.h>
#include <cpprest/asyncrt_utils.h>

using namespace std;
using string_t = utility::string_t;
using char_t = utility::char_t;
using stringstream_t = utility::stringstream_t;

#include <Point3.h>
//#include <Plane3.h>
//#include <Facet3.h>

//#include <Point2.h>
//#include <Rect2.h>
//#include <Outline2.h>

#include <Range.h>
//#include <Segment.h>

//#include <GridBasis.h>
//#include <GridValue.h>
//#include <GridRef.h>
//#include <GridPoint.h>
//#include <GridVisitor.h>
//#include <GridScanner.h>

//#include <Bag.h>
//#include <BagCache.h>

using namespace utility::details;
