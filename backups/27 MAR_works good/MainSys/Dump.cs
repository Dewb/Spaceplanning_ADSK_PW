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














        internal static List<List<Point2d>> sequencePointLists(List<Point2d> poly , List<Point2d> intersectedPoints, List<int> pIndexA, List<int> pIndexB)
        {
            List<List<Point2d>> twoSetsPoints = new List<List<Point2d>>();
            List<Point2d> ptA = new List<Point2d>();
            List<Point2d> ptB = new List<Point2d>();
            //Trace.WriteLine("Number of Points on the ptA : " + pIndexA.Count);
            //Trace.WriteLine("Number of Points on the ptB : " + pIndexB.Count);
            bool added = false;
            for (int i = 0; i < pIndexA.Count - 1; i++)
            {
                ptA.Add(poly[pIndexA[i]]);
                if (Math.Abs(pIndexA[i] - pIndexA[i + 1]) > 1 && added == false)
                {
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                        poly[pIndexA[i]]);
                    ptA.Add(intersNewList[1]);
                    ptA.Add(intersNewList[0]);
                    //Trace.WriteLine("Added Intersect Before for PtA");
                    added = true;
                }

                if (i == (pIndexA.Count - 1))
                {
                    ptA.Add(poly[pIndexA[i + 1]]);
                }
            }

            if (added == false)
            {
                //Trace.WriteLine("Second Time Added Intersect Before for PtA");
                List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                         ptA[ptA.Count - 1]);
                ptA.Add(intersNewList[1]);
                ptA.Add(intersNewList[0]);
                added = true;
            }
            ////////////////////////////////////////////////////////////////////

            bool added2 = false;
            for (int i = 0; i < pIndexB.Count - 1; i++)
            {
                ptB.Add(poly[pIndexB[i]]);
                if (Math.Abs(pIndexB[i] - pIndexB[i + 1]) > 1 && added2 == false)
                {
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                           poly[pIndexA[i]]);
                    ptB.Add(intersNewList[0]);
                    ptB.Add(intersNewList[1]);
                    added2 = true;
                    //Trace.WriteLine("Added Intersect Before for PtB");
                }

                if (i == (pIndexB.Count - 1))
                {
                    ptB.Add(poly[pIndexB[i + 1]]);
                }
            }

            if (added2 == false)
            {
                //Trace.WriteLine("Second Time Added Intersect Before for PtB");
                List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                        ptB[ptB.Count - 1]);
                ptB.Add(intersNewList[0]);
                ptB.Add(intersNewList[1]);
                added2 = true;
            }

            twoSetsPoints.Add(ptA);
            twoSetsPoints.Add(ptB);
            return twoSetsPoints;

        }




        ///////////////////////////////////////////////////////////////////////
        */
    }
}
