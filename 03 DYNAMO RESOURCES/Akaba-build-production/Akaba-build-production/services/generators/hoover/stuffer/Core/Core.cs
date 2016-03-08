using System;
using System.Linq;
using System.Collections.Generic;

namespace stuffer
{
  public class Core
  {
    internal static Polygon2d Range2dToQuadPolygon2d(Range2d range)
    {
      List<Point2d> points = new List<Point2d>();
      points.Add(new Point2d(range.Min.X, range.Min.Y));
      points.Add(new Point2d(range.Max.X, range.Min.Y));
      points.Add(new Point2d(range.Max.X, range.Max.Y));
      points.Add(new Point2d(range.Min.X, range.Max.Y));

      return new Polygon2d(points);
    }

    public static List<Polygon2d> RectFloorPlates(Outline2d outline)
    {
      if (outline == null)
        return null;
      
      List<Line2d> lines = outline.SelfSlice();
      if (lines == null || lines.Count == 0)
        return null;
      
      List<Range2d> found = new List<Range2d>();
      foreach (Line2d l0 in lines)
      {
        Line2d c0 = new Line2d(l0);
        foreach (Line2d l1 in lines)
        {
          if (l1 == l0)
            continue;

          Line2d c1 = l1.Join(c0);
          if (c1 == null)
            continue;

          foreach (Line2d l2 in lines)
          {
            if (l2 == l0 || l2 == l1)
              continue;

            Line2d c2 = l2.Join(c1);
            if (c2 == null)
              continue;

            foreach (Line2d l3 in lines)
            {
              if (l3 == l0 || l3 == l1 || l3 == l2)
                continue;

              if (!l3.Compare(c2))
                continue;

              Range2d range = null;
              range = new Range2d(range, new Range2d(l0.StartPoint, l0.EndPoint));
              range = new Range2d(range, new Range2d(l1.StartPoint, l1.EndPoint));
              range = new Range2d(range, new Range2d(l2.StartPoint, l2.EndPoint));
              range = new Range2d(range, new Range2d(l3.StartPoint, l3.EndPoint));

              bool alreadyFound = false;
              for (int index = 0; index < found.Count; ++index)
              {
                if (found[index].Compare(range))
                {
                  alreadyFound = true;
                  break;
                }
              }

              if (!alreadyFound)
                found.Add(range);
            }
          }
        }
      }

      List<Polygon2d> plates = new List<Polygon2d>();
      foreach (Range2d range in found)
        plates.Add(Range2dToQuadPolygon2d(range));

      return plates;
    }

    public static Line2d SharedLine(Polygon2d p0, Polygon2d p1)
    {
      if (p0 == null || p1 == null)
        return null;

      foreach (Line2d l0 in p0.Lines)
        foreach (Line2d l1 in p1.Lines)
          if (l0.Compare(l1))
            return l0;

      return null;
    }

    public static List<Polygon2d> SimpleJoin(List<Polygon2d> polygons)
    {
      if (polygons.Count < 3)
        return polygons;

      if (polygons.Count == 3)
      {
        SortedDictionary<double, Range1i> candidates = new SortedDictionary<double, Range1i>();
        for (int idx0 = 0; idx0 < polygons.Count; ++idx0)
        {
          for (int idx1 = 0; idx1 < polygons.Count; ++idx1)
          {
            if (idx0 == idx1)
              continue;

            Polygon2d p0 = polygons[idx0];
            Polygon2d p1 = polygons[idx1];
            Line2d shared = SharedLine(p0, p1);
            if (shared != null)
            {
              double totalArea = p0.BBox.Area + p1.BBox.Area;
              candidates[totalArea] = new Range1i(idx0, idx1);
            }
          }
        }

        Range1i toCombine = candidates.Values.Last();
        Range2d combined = new Range2d(polygons[toCombine.Low].BBox, polygons[toCombine.High].BBox);
        polygons.RemoveAt(toCombine.High);
        polygons.RemoveAt(toCombine.Low);
        polygons.Add(Range2dToQuadPolygon2d(combined));
      }

      return polygons;
    }

    public static List<Point2d> Positions(List<Range2d> floorPlates, double maxDistance)
    {
      List<Point2d> positions = new List<Point2d>();
      foreach (Range2d plate in floorPlates)
      {
        double numCores = Math.Ceiling(plate.LongestSpan/maxDistance);
        Line2d centerline = plate.MajorCenterline;
        for (double index = 0; index < numCores; ++index)
        {
          double t = (index + 1)/(numCores + 1);
          positions.Add(centerline.PointAt(t));
        }
      }

      return positions;
    }
  }
}
