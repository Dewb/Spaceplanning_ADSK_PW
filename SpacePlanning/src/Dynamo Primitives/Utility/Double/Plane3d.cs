namespace stuffer
{
  internal class Plane3d
  {
    private Point3d m_origin;
    private Vector3d m_normal;

    internal Plane3d(Point3d origin, Vector3d normal)
    {
      m_origin = origin;
      m_normal = normal;
    }

    public Point3d Origin
    {
      get { return m_origin; }
    }

    public Vector3d Normal
    {
      get { return m_normal; }
    }

    public Point3d Intersect(Line3d line, bool clip)
    {
      Vector3d u = new Vector3d(line.StartPoint, line.EndPoint);
      double d = m_normal.Dot(u);
      if (d == 0.0)
        return null;

      Vector3d w = new Vector3d(m_origin, line.EndPoint);
      double n = m_normal.Dot(w);
      double s = n/d;

      if (clip && (s < 0.0 || s > 1.0))
        return null;

      return new Vector3d(u.Scale(s), line.EndPoint);
    }

    public Line3d Intersect(Facet3d facet, double minLineLength)
    {
      Point3d pp01 = Intersect(new Line3d(facet.P0, facet.P1), true);
      Point3d pp12 = Intersect(new Line3d(facet.P1, facet.P2), true);
      Point3d pp20 = Intersect(new Line3d(facet.P2, facet.P0), true);

      Point3d lp0 = null;
      Point3d lp1 = null;

      // Check if first point is invalid
      if (pp01 == null)
      {
        // The second and third points must be valid for intersection
        if (pp12 == null || pp20 == null)
          return null;

        // Return the line defined by the second and third points
        lp0 = pp12;
        lp1 = pp20;
      }
      else
      {
        // First point is valid, check is second point is invalid
        if (pp12 == null)
        {
          // Third point must be valid for intersection
          if (pp20 == null)
            return null;

          // Return the line defined by the first and third points
          lp0 = pp20;
          lp1 = pp01;
        }
        else
        {
          // First and second points are valid, third point must be invalid for intersection
          if (pp20 == null)
          {
            lp0 = pp01;
            lp1 = pp12;
          }
          else
          {
            // All three points are valid, face is co-planar with the plane
            // We are only interested in orthogonal triangles
            return null;
          }
        }
      }

      Vector3d distanceCheck = new Vector3d(lp0, lp1);
      if (distanceCheck.Length < minLineLength)
        return null;

      return new Line3d(lp0, lp1);
    }
  }
}
