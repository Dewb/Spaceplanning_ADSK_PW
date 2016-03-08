
#pragma once

//#define USE_DIALOG
#ifdef USE_DIALOG

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN
#endif

#include <targetver.h>

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS
#define _AFX_ALL_WARNINGS

#include <afxwin.h>
#include <afxext.h>

#ifndef _AFX_NO_OLE_SUPPORT
#include <afxdtctl.h>
#endif
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>
#endif

#include <afxcontrolbars.h>

#else
#ifdef _WIN32
#define TRACE(msg) wcout << msg
#else
#define TRACE(msg) cout << msg
#endif
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
#include <regex>

#include <cpprest/json.h>
#include <cpprest/http_listener.h>
#include <cpprest/uri.h>
#include <cpprest/asyncrt_utils.h>

using namespace std;
using string_t = utility::string_t;
using char_t = utility::char_t;
using stringstream_t = utility::stringstream_t;
#ifdef _WIN32
using smatch_t = wsmatch;
using regex_t = wregex;
#else
using smatch_t = smatch;
using regex_t = regex;
#endif

#include <Point3.h>
#include <Plane3.h>
#include <Facet3.h>

#include <Point2.h>
#include <Rect2.h>
#include <Outline2.h>

#include <Range.h>
#include <Segment.h>

#include <GridBasis.h>
#include <GridValue.h>
#include <GridRef.h>
#include <GridPoint.h>
#include <GridVisitor.h>
#include <GridScanner.h>

#include <Bag.h>
#include <BagCache.h>

using namespace utility::details;
