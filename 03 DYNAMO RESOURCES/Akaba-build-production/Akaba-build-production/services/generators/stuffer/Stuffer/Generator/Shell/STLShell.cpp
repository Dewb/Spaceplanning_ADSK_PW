#include <stdafx.h>
#include <STLShell.h>

STLShell::STLShell(const string& stlData, const GridBasis& basis)
: Shell(basis)
{
  facets = STLShell::loadSTLFromASCII(stlData);
  if (facets->empty())
    return;

  Rangef zRange(facets->begin()->pt[0].z());
  for (const auto& facet : *facets)
    for (int index(0); index < 3; ++index)
      zRange.inflate(facet.pt[index].z());

  Rangei levelRange(basis.levelNum(zRange.l()), basis.levelNum(zRange.h()));
  
  calculateOutlines(levelRange);
  calculateGrids();
}

namespace
{
  void split(vector<string> &tokens, const string &text, char sep) 
  {
    size_t start(0);
    size_t end(0);
    while ((end = text.find(sep, start)) != string::npos)
    {
      tokens.push_back(text.substr(start, end - start));
      start = end + 1;
    }
    tokens.push_back(text.substr(start));
  }

  float feetToMeters(float feet)
  {
    return feet*0.3048f;
  }

  void setValueASCII(const vector<string>& parts, Point3f& value, bool isNormal)
  {
    int offset(isNormal ? 1 : 0);
    value.x() = feetToMeters(static_cast<float>(strtod(parts[1 + offset].c_str(), 0)));
    value.y() = feetToMeters(static_cast<float>(strtod(parts[2 + offset].c_str(), 0)));
    value.z() = feetToMeters(static_cast<float>(strtod(parts[3 + offset].c_str(), 0)));

    if (isNormal)
      value.normalize();
  }
}

unique_ptr<vector<Facet3f>> STLShell::loadSTLFromASCII(const string& data)
{
  unique_ptr<vector<Facet3f>> stl(new vector<Facet3f>());

  vector<string> lines;
  split(lines, data, '\n');

  Facet3f item;

  enum State
  {
    Begin,
    StartFacet,
    Normal,
    Outer,
    VertexX,
    VertexY,
    VertexZ,
    EndLoop
  } last = Begin;

  for (const auto& line : lines)
  {
    // NOTE: Making the canned STL can introduce an empty line at the beginning
    if (line == "")
      continue;

    vector<string> parts;
    split(parts, line, ' ');

    switch (last)
    {
    case Begin:
      last = StartFacet;
      break;

    case StartFacet:
      if (parts[0].compare("endsolid") == 0)
        return move(stl);

      item = Facet3f();
      setValueASCII(parts, item.n, true);
      last = Normal;
      break;
    
    case Normal:
      last = Outer;
      break;

    case Outer:
      setValueASCII(parts, item.pt[0], false);
      last = VertexX;
      break;

    case VertexX:
      setValueASCII(parts, item.pt[1], false);
      last = VertexY;
      break;

    case VertexY:
      setValueASCII(parts, item.pt[2], false);
      last = VertexZ;
      break;

    case VertexZ:
      last = EndLoop;
      break;

    case EndLoop:
      stl->push_back(item);
      last = StartFacet;
      break;
    }
  }

  return move(stl);
}

namespace
{
  pair<bool, Point3f> intersectSegmentWithPlane(const Segment3f& seg, const Plane3f& plane)
  {
    Point3f u(seg.pt[0], seg.pt[1]);
    Point3f w(plane.pos, seg.pt[0]);
    float d(plane.n.dot(u));
    float n(-plane.n.dot(w));

    float s(n / d);
    if (s < 0.0f || s > 1.0f)
      return{ false, Point3f() };

    return{ true, Point3f(seg.pt[0].x() + s*u.x(), seg.pt[0].y() + s*u.y(), seg.pt[0].z() + s*u.z()) };
  }

  Segment3f intersectFacetWithPlane(const Facet3f& facet, const Plane3f& plane)
  {
    vector<Point3f> points;
    for (int index(0); index < 3; ++index)
    {
      const auto& p0(facet.pt[index]);
      const auto& p1(facet.pt[(index + 1) % 3]);
      const auto& ret(intersectSegmentWithPlane(Segment3f(p0, p1), plane));
      if (ret.first)
        points.push_back(ret.second);
    }

    if (points.size() == 3) 
    {
      // NOTE: Check for equal points here if needed...
      assert(false);
  //    for (var index = 0; index < 3; ++index) {
  //      var p0 = points[index];
  //      var p1 = points[(index + 1) % 3];
  //      if (eqPt(p0, p1)) {
  //        points.splice(index, 1);
  //        break;
  //      }
  //    }
    }
    else if (points.size() == 1)
    {
      // NOTE: One point should be impossible...
      assert(false);
      return Segment3f();
    }
    else if (points.size() == 0)
    {
      return Segment3f();
    }

    if (points[0] == points[1])
      return Segment3f();
    
    return Segment3f(points[0], points[1]); // , triangle.normal, plane.n);
  }
}

void STLShell::calculateOutlines(const Rangei& levelRange)
{
  // NOTE: Does not add last-top grid
  outlines.reset(new vector<Outline2f>());
  for (int level = levelRange.l(); level < levelRange.h(); ++level)
    calculateOutline(basis.levelHeight(level), true);
}

void STLShell::calculateOutline(float height, bool isFloorOutline)
{
  Plane3f plane(
    Point3f(0.0f, 0.0f, height + GridBasis::epsilon*(isFloorOutline ? 1.0f : -1.0f)),
    Point3f(0.0f, 0.0f, 1.0f));

  Outline2f outline(height);
  for (const auto& facet : *facets)
  {
    Segment3f seg3(intersectFacetWithPlane(facet, plane));
    if (seg3.empty())
      continue;

    // TODO: This value will need to be tuned!!!
    float length(seg3.length());
    if (length < basis.h()/4.0f)
      continue;

    Segment2f seg2(
      Point2f(seg3.pt[0].x(), seg3.pt[0].y()),
      Point2f(seg3.pt[1].x(), seg3.pt[1].y()));

    outline.add(seg2);
  }

  outlines->push_back(outline);
}

void STLShell::calculateGrids()
{
  for (const auto& outline : *outlines)
    if (!outline.empty())
      calculateGrid(outline);
}

namespace
{
  void extend(Segment2f& seg, float amount)
  {
    Point2f dp(seg.pt[1] - seg.pt[0]);
    dp.normalize();
    dp *= amount;
    seg.pt[0] -= dp;
    seg.pt[1] += dp;
  }

  float area(const Point2f& p0, const Point2f& p1, const Point2f& p2) 
  {
    return (p0.x() - p2.x())*(p1.y() - p2.y()) - (p0.y() - p2.y())*(p1.x() - p2.x());
  }

  unique_ptr<Point2f> intersect(const Segment2f& s0, const Segment2f& s1)
  {
    auto a1(area(s0.pt[0], s0.pt[1], s1.pt[1]));
    auto a2(area(s0.pt[0], s0.pt[1], s1.pt[0]));
    auto tol(0.1f);
    if (a1*a2 <= 0.0f || abs(a1) < tol || abs(a2) < tol)
    {
      auto a3(area(s1.pt[0], s1.pt[1], s0.pt[0]));
      auto a4(a3 + a2 - a1);
      if (a3*a4 <= 0.1f || abs(a3) < tol || abs(a4) < tol)
      {
        if (abs(a1) > tol || abs(a2) > tol || abs(a3) > tol || abs(a4) > tol)
        {
          auto t(a3 / (a3 - a4));
          return make_unique<Point2f>(s0.pt[0] + (s0.pt[1] - s0.pt[0])*t);
        }
      }
    }

    return nullptr;
  }
}

void STLShell::calculateGrid(const Outline2f& outline)
{
  const auto bbox(basis.shrinkToGrid(outline.boundingBox()));

  vector<Segment2f> extended;
  for (auto segment : outline.segments())
  {
    Segment2f extSeg(segment);
    extend(extSeg, basis.h()/2.0f);
    extended.push_back(extSeg);
  }

  auto& grid(createLevel(basis.levelNum(outline.height())).getGrid());
  for (int pass = 0; pass < 2; ++pass)
  {
    string_t tag((pass == 0) ? U("x") : U("y"));
    auto minIndex((pass == 0) ? bbox.l() : bbox.t());
    auto maxIndex((pass == 0) ? bbox.r() : bbox.b());
    for (auto index = minIndex; index <= maxIndex; ++index)
    {
      Segment2f axis;
      if (pass == 0)
      {
        axis.pt[0] = basis.fromGrid(Point2i(index, (bbox.t() - 1)));
        axis.pt[1] = basis.fromGrid(Point2i(index, (bbox.b() + 1)));
      }
      else
      {
        axis.pt[0] = basis.fromGrid(Point2i((bbox.l() - 1), index));
        axis.pt[1] = basis.fromGrid(Point2i((bbox.r() + 1), index));
      }

      vector<float> points;
      for (auto segment : extended)
      {
        const auto point(intersect(axis, segment));
        if (point)
          points.push_back((pass == 0) ? point->y() : point->x());
      }

      if (points.size() < 2)
        continue;

      sort(points.begin(), points.end());

      for (auto it(points.begin()); it != points.end(); ++it)
      {
        float v0(*it/basis.h());
        ++it;
        if (it == points.end())
          break;
        float v1(*it/basis.h());

        Rangei range(
          static_cast<int>(ceil(v0)),
          static_cast<int>(floor(v1)));

        grid.addLine(tag, index, range);
      }
    }
  }
}
