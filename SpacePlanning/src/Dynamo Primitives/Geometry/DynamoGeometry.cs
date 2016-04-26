using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using System;
using System.Diagnostics;

namespace stuffer
{
    public static class DynamoGeometry
    {
        //////////////////////////////////////////////////////////////////////////
        // To Dynamo Geometry



        //ADDED CODE : SUBHAJIT DAS++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



            

        //convert point to point2dList
        internal static List<Point2d> PointtoPoint2D(List<Point> pointList)
        {
            List<Point2d> point2dList = new List<Point2d>();

            for (int i = 0; i < pointList.Count; i++)
            {
                Point2d p2 = Point2d.ByCoordinates(pointList[i].X, pointList[i].Y);
                point2dList.Add(p2);
            }
            pointList.Clear();
            return point2dList;
        }

        

        [MultiReturn(new[] { "aVal", "rVal", "gVal", "bVal" })]
        public static Dictionary<string, object> ARGBValues(List<List<Point2d>> ptLists)
        {

            int aa = 0, bb = 255;
            List<List<int>> aList = new List<List<int>>();
            List<List<int>> rList = new List<List<int>>();
            List<List<int>> gList = new List<List<int>>();
            List<List<int>> bList = new List<List<int>>();

            Random r1 = new Random();
            Random r2 = new Random();
            Random r3 = new Random();
            Random r4 = new Random();
            for (int i = 0; i < ptLists.Count; i++)
            {
                List<int> aL = new List<int>();
                List<int> rL = new List<int>();
                List<int> gL = new List<int>();
                List<int> bL = new List<int>();


                int r = r1.Next(aa, bb);
                int g = r2.Next(aa, bb);
                int b = r3.Next(aa, bb);

                List<Point2d> ptL = ptLists[i];
                for (int j = 0; j < ptL.Count; j++)
                {

                    aL.Add(200);
                    rL.Add(r);
                    gL.Add(g);
                    bL.Add(b);

                }
                aList.Add(aL);
                rList.Add(rL);
                gList.Add(gL);
                bList.Add(bL);

            }
            // return geomObject;

            return new Dictionary<string, object>
            {
                { "aValue", (aList) },
                { "rValue", (rList) },
                { "gValue", (gList) },
                { "bValue", (bList) }
            };

        }



        public static List<List<int>> Rvalues(List<List<Point2d>> ptLists, int tag)
        {

            int aa = 0, bb = 255;
            List<List<int>> aList = new List<List<int>>();
            List<List<int>> rList = new List<List<int>>();
            List<List<int>> gList = new List<List<int>>();
            List<List<int>> bList = new List<List<int>>();

            Random r1 = new Random();
            Random r2 = new Random();
            Random r3 = new Random();
            Random r4 = new Random();
            for (int i = 0; i < ptLists.Count; i++)
            {
                List<int> aL = new List<int>();
                List<int> rL = new List<int>();
                List<int> gL = new List<int>();
                List<int> bL = new List<int>();


                int r = r1.Next(aa, bb);
                int g = r2.Next(aa, bb);
                int b = r3.Next(aa, bb);

                List<Point2d> ptL = ptLists[i];
                for (int j = 0; j < ptL.Count; j++)
                {

                    aL.Add(200);
                    rL.Add(r);
                    gL.Add(g);
                    bL.Add(b);

                }
                aList.Add(aL);
                rList.Add(rL);
                gList.Add(gL);
                bList.Add(bL);

            }
            return rList;


        }


        public static List<Geometry> PointListsByColor(List<List<Point2d>> ptLists)
        {
            List<Geometry> geomObject = new List<Geometry>();
            for (int i = 0; i < ptLists.Count; i++)
            {
                List<Point2d> ptL = new List<Point2d>();
                for (int j = 0; j < ptL.Count; j++)
                {
                    // Autodesk.DesignScript.Geometry.Geometry
                    Point p = Point.ByCoordinates(0, 0, 0);

                }
            }
            return geomObject;

        }



        //MAKE POLYGON BASED ON POINT COORDS
        public static PolyCurve PolyCurveFromPoints(List<Point2d> pointCoordList)
        {
            PolyCurve pCurve;
            List<Point> pointList = new List<Point>();
            for (int i = 0; i < pointCoordList.Count; i++)
            {
                pointList.Add(Point.ByCoordinates(pointCoordList[i].X, pointCoordList[i].Y));
                if (i == pointCoordList.Count - 1)
                {
                    pointList.Add(Point.ByCoordinates(pointCoordList[0].X, pointCoordList[0].Y));
                }

            }

            try
            {
                pCurve = PolyCurve.ByPoints(pointList);
            }
            catch (System.Exception)
            {
                pCurve = null;
                //throw;
            }

            return pCurve;
        }

        //GET POINT GEOMETRY FROM POINT 2D LIST
        public static List<Point> pointFromPoint2dList(List<Point2d> pointList)
        {
            if (pointList == null || pointList.Count == 0)
            {
                return null;
            }
            List<Point> ptGeom = new List<Point>();
            for (int i = 0; i < pointList.Count; i++)
            {
                ptGeom.Add(Point.ByCoordinates(pointList[i].X, pointList[i].Y));
            }
            return ptGeom;

        }

        

        //ADDED CODE : SUBHAJIT DAS++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


        //makes surface patch for closed polylines
        public static List<Surface> MakeSurfaceForPolygon2d(List<Polygon2d> poly, double height = 3)
        {            
            List<Surface> surfaceList = new List<Surface>();
            for (int i = 0; i < poly.Count; i++)
            {
                if (poly[i] == null) continue;
                //List<Point> ptList = pointFromPoint2dList(poly[i].Points);
                //PolyCurve pCurve = PolyCurve.ByPoints(ptList);
                Polygon pCurve = PolygonByPolygon2d(poly[i], height);
                if (pCurve == null) continue;
                surfaceList.Add(Surface.ByPatch(pCurve));
            }
            return surfaceList;
        }





        public static List<Point> PointListByPoint2d(List<Point2d> pointList)
        {
            if (pointList == null)
                return null;

            List<Point> ptList = new List<Point>();
            for (int i = 0; i < pointList.Count; i++)
            {
                ptList.Add(Point.ByCoordinates(pointList[i].X, pointList[i].Y));
            }
            return ptList;
        }




        public static List<Point2d> Point2dFromPointList(List<Point> pointList)
        {
            if (pointList == null)
                return null;

            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < pointList.Count; i++)
            {
                ptList.Add(Point2d.ByCoordinates(pointList[i].X, pointList[i].Y));
            }
            return ptList;
        }


        public static List<Point2d> Point2dFromRegularPolygon(Polygon poly)
        {
            if (poly == null)
                return null;

            List<Point> pointList = new List<Point>();
            for (int i = 0; i < poly.Points.Length; i++)
            {
                pointList.Add(Point.ByCoordinates(poly.Points[i].X, poly.Points[i].Y));
            }

            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < pointList.Count; i++)
            {
                ptList.Add(Point2d.ByCoordinates(pointList[i].X, pointList[i].Y));
            }
            return ptList;
        }








        public static Point PointByPoint2d(Point2d point, double height = 0)
        {
            if (point == null)
                return null;

            return Point.ByCoordinates(point.X, point.Y, height);
        }

        public static Point PointByPoint3d(Point3d point)
        {
            if (point == null)
                return null;

            return Point.ByCoordinates(point.X, point.Y, point.Z);
        }

        public static Line LineByLine2d(Line2d line, double height = 0)
        {
            if (line == null)
                return null;
            if (line.StartPoint.Compare(line.EndPoint))
                return null;

            return
              Line.ByStartPointEndPoint(
                Point.ByCoordinates(line.StartPoint.X, line.StartPoint.Y, height),
                Point.ByCoordinates(line.EndPoint.X, line.EndPoint.Y, height));
        }

        public static List<Vector> VectorListByVector2dList(List<Vector2d> input)
        {
            if (input == null)
                return null;

            List<Vector> output = new List<Vector>();
            foreach (Vector2d vector in input)
            {
                output.Add(Vector.ByCoordinates(vector.X, vector.Y, 0));
            }

            return output;
        }

        public static List<Line> AxisLines(double length = 100, double height = 0)
        {
            List<Line> axis = new List<Line>();
            axis.Add(
              Line.ByStartPointEndPoint(
                Point.ByCoordinates(-length, 0, height),
                Point.ByCoordinates(length, 0, height)));
            axis.Add(
              Line.ByStartPointEndPoint(
                Point.ByCoordinates(0, -length, height),
                Point.ByCoordinates(0, length, height)));

            return axis;
        }

        public static Cuboid CuboidBySpace(Space space)
        {
            if (space == null)
                return null;

            Point2d pos2 = new Point2d(space.Position.X, space.Position.Y);
            Point2d dim2 = new Point2d(space.Dimensions.X, space.Dimensions.Y);
            Point3d p0 = new Point3d(pos2 - dim2 / 2.0, space.Position.Z);
            Point3d p1 = new Point3d(pos2 + dim2 / 2.0, space.Position.Z + space.Dimensions.Z);

            return Cuboid.ByCorners(PointByPoint3d(p0), PointByPoint3d(p1));
        }

        public static Polygon PolygonByBBox2d(Range2d bbox, double height)
        {
            if (bbox == null)
                return null;
            List<Point> points = new List<Point>();
            points.Add(Point.ByCoordinates(bbox.Min.X, bbox.Min.Y, height));
            points.Add(Point.ByCoordinates(bbox.Max.X, bbox.Min.Y, height));
            points.Add(Point.ByCoordinates(bbox.Max.X, bbox.Max.Y, height));
            points.Add(Point.ByCoordinates(bbox.Min.X, bbox.Max.Y, height));

            return Polygon.ByPoints(points);
        }

        public static Polygon PolygonByPolygon2da(Polygon2d polygon, double height)
        {
            Polygon pol;
            if (polygon == null)
            {
                //pol.Dispose();
                return null;
            }

            if (polygon.Points == null)
            {
                //pol.Dispose();
                return null;
            }

            List<Point> points = new List<Point>();
            foreach (Point2d point in polygon.Points)
            {
                points.Add(Point.ByCoordinates(point.X, point.Y, height));
            }
            try
            {
                pol = Polygon.ByPoints(points);
                return pol;
            }
            catch (Exception)
            {
                return null;
                //throw;
            }

        }

        public static Polygon PolygonByPolygon2d(Polygon2d polygon, double height)
        {
            List<Point> pt = new List<Point>();
            pt.Add(Point.ByCoordinates(0, 0));
            pt.Add(Point.ByCoordinates(0, 100));
            pt.Add(Point.ByCoordinates(100, 0));
            Polygon pol = Polygon.ByPoints(pt);
            bool polyFormed = false;
            if (polygon == null || polygon.Points == null)
            {
                return null;
            }

            List<Point> points = new List<Point>();
            foreach (Point2d point in polygon.Points)
            {
                points.Add(Point.ByCoordinates(point.X, point.Y, height));
            }
            try
            {
                pol = Polygon.ByPoints(points);
                polyFormed = true;
                //return pol;
            }
            catch (Exception)
            {
                polyFormed = false;
                //throw;
            }

            if (polyFormed) return pol;
            else { pol.Dispose(); pt.Clear(); return null; }

        }

        public static List<Polygon> PolygonsByOutline2d(Outline2d outline, double height)
    {
      if (outline == null || outline.Polygons == null)
        return null;
      List<Polygon> polygons = new List<Polygon>();
      foreach (Polygon2d polygon in outline.Polygons)
      {
        polygons.Add(PolygonByPolygon2d(polygon, height));
      }
      return polygons;
    }

    public static Autodesk.DesignScript.Geometry.Mesh MeshByFacet3dList(List<Facet3d> facets)
    {
      List<Point> points = new List<Point>();
      List<IndexGroup> groups = new List<IndexGroup>();
      uint facetNum = 0;
      foreach (Facet3d facet in facets)
      {
        uint idx = facetNum*3;
        points.Add(PointByPoint3d(facet.P0));
        points.Add(PointByPoint3d(facet.P1));
        points.Add(PointByPoint3d(facet.P2));
        groups.Add(IndexGroup.ByIndices(idx, idx + 1, idx + 2));
        ++facetNum;
      }

      return Autodesk.DesignScript.Geometry.Mesh.ByPointsFaceIndices(points, groups);
    }

    public static List<Polygon> OutlinePolygonsByShell(Shell shell, GridBasis basis)
    {
      double bottom = shell.BBox.Min.Z;
      double top = shell.BBox.Max.Z;
      Range1i levelRange = new Range1i(basis.Level(bottom), basis.Level(top));

      List<Polygon> polygons = new List<Polygon>();
      for (int level = levelRange.Low; level <= levelRange.High; ++level)
      {
        double height = basis.FloorHeight(level);
        Outline2d outline = shell.GetFloorOutline(height);
        if (outline == null)
          continue;
        if (outline.Polygons == null)
          continue;

        foreach (Polygon2d polygon in outline.Polygons)
        {
          polygons.Add(PolygonByPolygon2d(polygon, height));
        }
      }

      return polygons;
    }

    //////////////////////////////////////////////////////////////////////////
    // From Dynamo Geometry

    public static Point3d Point3dByPoint(Point point)
    {
      if (point == null)
        return null;

      return new Point3d(point.X, point.Y, point.Z);
    }

    public static Range2d Range2dByRectangle(Rectangle rectangle)
    {
      if (rectangle == null)
        return null;

      return new Range2d(
        new Point2d(rectangle.Points[0].X, rectangle.Points[0].Y),
        new Point2d(rectangle.Points[2].X, rectangle.Points[2].Y));
    }
  }

  //////////////////////////////////////////////////////////////////////////
  // Polygon2d Subtraction Extension Using Dynamo Geometry

  public static class Polygon2dExtensionMethods
  {
    public static List<Polygon2d> Trim(this Polygon2d polygon, Polygon2d other)
    {
      if (other == null)
        return null;

      Geometry[] dTrimmed = null;
      {
        Polygon dPolyThis = DynamoGeometry.PolygonByPolygon2d(polygon, 0);
        Surface dSurfaceThis = Surface.ByPatch(dPolyThis);

        Polygon dPolyOther = DynamoGeometry.PolygonByPolygon2d(other, 0);
        Surface dSurfaceOther = Surface.ByPatch(dPolyOther);
        Solid dSolidOther = dSurfaceOther.Thicken(1.0, true);

        dTrimmed = dSurfaceThis.SubtractFrom(dSolidOther);

        dPolyThis.Dispose();
        dSurfaceThis.Dispose();
        dPolyOther.Dispose();
        dSurfaceOther.Dispose();
        dSolidOther.Dispose();
      }

      List<Polygon2d> polygons = new List<Polygon2d>();
      foreach (Geometry dGeometry in dTrimmed)
      {
        List<Point2d> points = new List<Point2d>();
        Curve[] dCurves = ((Surface)dGeometry).PerimeterCurves();
        foreach (Curve dCurve in dCurves)
        {
          Point dPoint = dCurve.StartPoint;
          points.Add(new Point2d(dPoint.X, dPoint.Y));
          dPoint.Dispose();

          dCurve.Dispose();
        }

        polygons.Add(new Polygon2d(points));

        dGeometry.Dispose();
      }

      return polygons;
    }
  }
}
