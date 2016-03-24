using SpacePlanning;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace stuffer
{
  [DebuggerDisplay("{m_start} - {m_end}")]
  public class Line2d
  {
    private Point2d m_start;
    private Point2d m_end;



        internal Line2d(Point2d start, Point2d end)
    {
      m_start = new Point2d(start);
      m_end = new Point2d(end);
    }




    internal Line2d(Line2d other)
    {
      m_start = new Point2d(other.StartPoint);
      m_end = new Point2d(other.EndPoint);
    }

    public Point2d StartPoint
    {
      get { return m_start; }
    }

    public Point2d EndPoint
    {
      get { return m_end; }
    }

    public Point2d PointAt(double t)
    {
      return (m_end - m_start)*t + m_start;
    }

    public double Length
    {
      get { return new Vector2d(m_start, m_end).Length; }
    }

    public bool Compare(Line2d other)
    {
      if (other.StartPoint.Compare(m_start) && other.EndPoint.Compare(m_end))
        return true;

      if (other.StartPoint.Compare(m_end) && other.EndPoint.Compare(m_start))
        return true;

      return false;
    }

    public Point2d Intersect(Line2d line, bool clip, bool allowOneFail)
    {
      Vector2d p = new Vector2d(m_start);
      Vector2d q = new Vector2d(line.StartPoint);
      Vector2d r = new Vector2d(p, m_end);
      Vector2d s = new Vector2d(q, line.EndPoint);
      
      double rxs = r.Cross(s);

      // Parallel (could be collinear)
      if (rxs.IsZero())
        return null;

      double t = q.Subtract(p).Cross(s)/rxs;
      if (clip)
      {
        bool tFail = (t.CompareLess(0) || t.CompareGreater(1));
        if (tFail && !allowOneFail)
          return null;

        double u = q.Subtract(p).Cross(r)/rxs;
        bool uFail = (u.CompareLess(0) || u.CompareGreater(1));
        if (uFail && !allowOneFail)
          return null;

        if (allowOneFail && tFail && uFail)
          return null;
      }

      return p.Add(r.Scale(t));
    }

    public Point2d Intersect(Line2d line, bool clip = false)
    {
      return Intersect(line, clip, false);
    }

    public Point2d Heal(Line2d line, bool start)
    {
      Point2d other = start ? m_start : m_end;

      if (new Vector2d(line.StartPoint, other).Length.IsZero())
        return line.StartPoint;

      if (new Vector2d(line.EndPoint, other).Length.IsZero())
        return line.EndPoint;

      return Intersect(line);
    }

    public bool Contains(Point2d point)
    {
      Vector2d vec1 = new Vector2d(point, m_start);
      if (vec1.Length.IsZero())
        return true;

      Vector2d vec2 = new Vector2d(point, m_end);
      if (vec2.Length.IsZero())
        return true;

      //double addLength = vec1.Length + vec2.Length;
      //if (addLength.CompareGreater(Length, Point1d.DotCompare))
      //  return false;

      double area = vec1.Cross(vec2)/0.5;
      if (area.CompareGreater(Length, Point1d.DotCompare))
        return false;

      return true;
    }


    

    internal Line2d Join(Line2d current)
    {
      if (m_start.Compare(current.StartPoint))
        return new Line2d(current.EndPoint, m_end);

      if (m_end.Compare(current.StartPoint))
        return new Line2d(current.EndPoint, m_start);

      if (m_start.Compare(current.EndPoint))
        return new Line2d(current.StartPoint, m_end);

      if (m_end.Compare(current.EndPoint))
        return new Line2d(current.StartPoint, m_start);

      return null;
    }


        //ADDED CODE : SD///////////////////////////////////////////////////////////////////////////////////////




        public static Line2d ByStartPointEndPoint(Point2d a, Point2d b)
        {
            return new Line2d(a, b);
        }


        internal Line2d(Point2d centerpt, double extents, double direction)
        {

            if (direction == 0)
            {
                m_start = new Point2d((centerpt.X - extents), centerpt.Y);
                m_end = new Point2d((centerpt.X + extents), centerpt.Y);

            }
            else
            {
                m_start = new Point2d(centerpt.X, (centerpt.Y - extents));
                m_end = new Point2d(centerpt.X, (centerpt.Y + extents));
            }

        }










        internal void move(double distX, double distY)
        {
            m_start = new Point2d((m_start.X+distX),(m_start.Y+distY));
            m_end = new Point2d((m_end.X + distX), (m_end.Y + distY));

        }


        internal void move(List<Point2d> poly, double distance)
        {

            Point2d midPt = this.midPt();
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(poly);
            Vector2d vecToCenter = new Vector2d(midPt, centerPoly);
            Vector2d vecToCenterN = vecToCenter.Normalize();
            Vector2d vectScaled = vecToCenter.Scale(distance);

            m_start = new Point2d((m_start.X + vectScaled.X), (m_start.Y + vectScaled.Y));
            m_end = new Point2d((m_end.X + vectScaled.X), (m_end.Y + vectScaled.Y));
        }


        internal Point2d midPt()
        {
            double x = (m_start.X + m_end.X) / 2;
            double y = (m_start.Y + m_end.Y) / 2;
            return new Point2d(x, y);
        }

        internal void move(Point2d point)
        {
            Point2d midPt = this.midPt();

            double distX = point.X - midPt.X;
            double distY = point.Y - midPt.Y;
            m_start = new Point2d((m_start.X + distX), (m_start.Y + distY));
            m_end = new Point2d((m_end.X + distX), (m_end.Y + distY));

        }

        public Point2d NudgeLineMidPt(Polygon2d poly, double scale = 0.2)
        {
            Point2d midPt = this.midPt();
            Point2d polyCenter = GraphicsUtility.CentroidInPointLists(poly.Points);

            Vector2d vecToCenter = new Vector2d(midPt, polyCenter);
            Vector2d vecNormalized = vecToCenter.Normalize();
            Vector2d vecScaled = vecNormalized.Scale(scale);
            Point2d point = new Point2d(midPt.X + vecScaled.X, midPt.Y + vecScaled.Y);
            return point;
        }


        internal void extend()
        {
            double eps = 1000;
            double extend = 100000;
            Vector2d vecLine = new Vector2d(m_start, m_end);
            Vector2d vecX = new Vector2d(m_start, Point2d.ByCoordinates(m_start.X + eps,0));
            Vector2d vecY = new Vector2d(m_start, Point2d.ByCoordinates(0, m_start.X + eps));

            double dotX = vecLine.Dot(vecX);
            double dotY = vecLine.Dot(vecY);

            if(dotX == 0)
            {
                //line is vertical
                this.move(0, extend);

            }
            else if (dotY == 0)
            {
                //line is horizontal
                this.move(extend, 0);

            }

        }


         













    /////////////////////////////////////////////////////////////////////////////////////////////


    

    public override string ToString()
    {
      return String.Format("{0} - {1}", m_start, m_end);
    }
  }
}
