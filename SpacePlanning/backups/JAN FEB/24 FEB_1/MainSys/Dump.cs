using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacePlanning.MainSys
{
    class Dump
    {
        /*
        ///////////////////////////////////////////////////////////////////////
        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "Polygons","AreaEachPoly", "PointOnPoly" })]
        public static Dictionary<string, object> RecursiveSplitPoly(Polygon2d poly, double ratio = 0.5, double extents = 2000)
        {

     
        List<Polygon2d> polyList = new List<Polygon2d>();
        List<Point2d> pointsList = new List<Point2d>();
        List<double> areaList = new List<double>();
        Stack<Polygon2d> polyRetrieved = new Stack<Polygon2d>();
        polyRetrieved.Push(poly);
            int count = 0;
        int thresh = 10;
        //double areaThreshold = GraphicsUtility.AreaPolygon2d(poly.Points)/10;
        double areaThreshold = 1000;
        int iter = 0;
        int dir = 1;
        Polygon2d currentPoly;
            while(count<thresh && polyRetrieved.Count> 0)
            {
                currentPoly = polyRetrieved.Pop();
                //List<Polygon2d> polyAfterSplit = 
                Dictionary<string, object> splitReturn = SplitPolyIntoTwoCheckNew(currentPoly, ratio, extents, dir);
        List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
        List<Point2d> pointsOnPoly = (List<Point2d>)splitReturn["PointsOnPoly"];
        double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
        double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
        polyRetrieved.Push(polyAfterSplit[0]);
                polyRetrieved.Push(polyAfterSplit[1]);
                polyList.AddRange(polyAfterSplit);
                pointsList.AddRange(pointsOnPoly);
                areaList.Add(area1);
                areaList.Add(area2);



                if (dir == 1)
                {
                    dir = 0;
                }
                else
                {
                    dir = 1;
                }
count += 1;
            }


            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "PointOnPoly", (pointsList) }
            };


        }

        ///////////////////////////////////////////////////////////////////////
        */
    }
}
