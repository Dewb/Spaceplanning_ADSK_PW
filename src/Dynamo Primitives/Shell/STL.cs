using System;
using System.Collections.Generic;

namespace stuffer
{
  public class STL : STLx
  {
    Range3d m_bbox;
    List<Facet3d> m_facets;

    internal STL()
    {
      m_bbox = null;
      m_facets = null;
    }

    internal STL(List<Facet3d> facets)
    {
      m_bbox = null;
      foreach (Facet3d facet in facets)
      {
        m_bbox = new Range3d(m_bbox, facet.P0);
        m_bbox = new Range3d(m_bbox, facet.P1);
        m_bbox = new Range3d(m_bbox, facet.P2);
      }

      m_facets = facets;
    }

    public static STL ByFacets(List<Facet3d> facets)
    {
      return new STL(facets);
    }

    public static STL ByASCII(string data)
    {
      STL stl = new STL();
      stl.ParseASCII(data);
      return stl;
    }

    enum State
    {
      Begin,
      StartFacet,
      Normal,
      Outer,
      Vertex0,
      Vertex1,
      Vertex2,
      EndLoop
    };

    internal void processPoint(string[] parts, ref Point3d point)
    {
      point = new Point3d(
        Convert.ToDouble(parts[1]),
        Convert.ToDouble(parts[2]),
        Convert.ToDouble(parts[3]));
      m_bbox = new Range3d(m_bbox, point);
    }

    internal void ParseASCII(string data)
    {
      m_facets = new List<Facet3d>();

      IEnumerable<string> rawLines = data.GetLines(true);
      if (rawLines == null)
        return;
      IEnumerator<string> enumerator = rawLines.GetEnumerator();
      if (!enumerator.MoveNext())
        return;
      if (!enumerator.Current.StartsWith("solid"))
        return;
      
      Point3d p0 = null;
      Point3d p1 = null;
      Point3d p2 = null;
      char[] delimiter = { ' ' };

      State last = State.Begin;
      foreach (string rawLine in rawLines)
      {
        string line = rawLine.Trim();
        string[] parts = line.Split(delimiter);

        switch (last)
        {
          case State.Begin:
            last = State.StartFacet;
            break;

          case State.StartFacet:
            if (parts[0].Equals("endsolid", StringComparison.Ordinal))
              return;

            // TODO: Store normal data here (Once it is used by Dynamo)
            last = State.Normal;
            break;

          case State.Normal:
            last = State.Outer;
            break;

          case State.Outer:
            processPoint(parts, ref p0);
            last = State.Vertex0;
            break;

          case State.Vertex0:
            processPoint(parts, ref p1);
            last = State.Vertex1;
            break;

          case State.Vertex1:
            processPoint(parts, ref p2);
            last = State.Vertex2;
            break;

          case State.Vertex2:
            last = State.EndLoop;
            break;

          case State.EndLoop:
            m_facets.Add(new Facet3d(p0, p1, p2));
            last = State.StartFacet;
            break;
        }
      }
    }

    public List<Facet3d> Facets
    {
      get { return m_facets; }
    }

    public Range3d BBox
    {
      get { return m_bbox; }
    }

    internal void OutputFacet(ref string text, Facet3d facet)
    {
      if (facet == null)
        return;

      // NOTE: STL usage does not use normals, (0, -1, 0) is what FormIt currently exports
      string tag = "facet";
      OutputHeader(ref text, tag, "normal 0 -1 0");
      IndentIn();

      OutputTextLine(ref text, "outer loop");
      IndentIn();

      OutputPoint3d(ref text, facet.P0);
      OutputPoint3d(ref text, facet.P1);
      OutputPoint3d(ref text, facet.P2);

      IndentOut();
      OutputTextLine(ref text, "endloop");

      IndentOut();
      OutputFooter(ref text, tag);
    }

    public string ToASCII()
    {
      return ToString();
    }

    public override string ToString()
    {
      string text = "";

      OutputHeader(ref text, "solid", m_name);
      IndentIn();

      foreach (Facet3d facet in m_facets)
        OutputFacet(ref text, facet);

      IndentOut();
      OutputFooter(ref text, "solid", m_name);

      return text;
    }
  }
}
