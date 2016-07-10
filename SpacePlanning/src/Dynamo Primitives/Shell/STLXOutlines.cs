using System;
using System.Collections.Generic;

namespace stuffer
{
  internal class STLxOutlines : STLx
  {
    List<double> m_heights;
    List<Outline2d> m_outlines;

    internal STLxOutlines(List<double> heights,  List<Outline2d> outlines)
    {
      m_heights = heights;
      m_outlines = outlines;
    }

    public static STLxOutlines ByHeightAndOutline2d(List<double> heights, List<Outline2d> outlines)
    {
      return new STLxOutlines(heights, outlines);
    }

    internal void OutputPolygon(ref string text, Polygon2d polygon)
    {
      if (polygon == null || polygon.Points == null)
        return;

      string tag = "polygon";
      OutputHeader(ref text, tag);
      IndentIn();

      foreach (Point2d point in polygon.Points)
        OutputPoint2d(ref text, point);

      IndentOut();
      OutputFooter(ref text, tag);
    }

    internal void OutputOutline(ref string text, double height, Outline2d outline)
    {
      if (outline == null || outline.Polygons == null)
        return;

      string tag = "outline";
      OutputHeader(ref text, tag, "height " + EncodeDouble(height));
      IndentIn();

      foreach (Polygon2d polygon in outline.Polygons)
        OutputPolygon(ref text, polygon);

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
      if (m_heights.Count == m_outlines.Count)
      {
        OutputHeader(ref text, "outlines", m_name);
        IndentIn();

        int count = m_heights.Count;
        for (int index = 0; index < count; ++index)
          OutputOutline(ref text, m_heights[index], m_outlines[index]);

        IndentOut();
        OutputFooter(ref text, "outlines", m_name);
      }

      return text;
    }
  }
}
