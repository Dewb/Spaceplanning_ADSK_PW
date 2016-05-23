
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using stuffer;
using Autodesk.DesignScript.Runtime;

namespace SpacePlanning
{
    /// <summary>
    /// Class to build a bubble chart, representing programs with area, quantity and adjacency
    /// </summary>
    public static class BubbleChart
    {
        #region - Public Methods
        //makes point grid for bubbles
        /// <summary>
        /// Builds pointgrid for bubble chart. Each Point represents each program.
        /// </summary>
        /// <param name="dataStack">Department data object</param>
        /// <returns name="pointList">List of point2d's.</returns>
        /// <search>
        /// point grid for bubble chart, bubble chart points
        /// </search>
        public static List<Point2d> ProgramPointGridFromData(List<List<string>> dataStack)
        {
            int lenPointLists = dataStack[0].Count, count = 0;
            double x = 0, y = 0;
            double addX = 10, addY = 10;
            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < lenPointLists; i++)
            {
                count += 1;
                if (count > 5)
                {
                    count = 0;
                    x = 0;
                    y += addY;
                }
                ptList.Add(Point2d.ByCoordinates(x, y));
                x += addX;
            }
            return ptList;

        }

        //computes area of programs, then normalizes it to make the bubbles
        /// <summary>
        /// Computes the radius of the bubbles based on the area of the programs
        /// </summary>
        /// <param name="dataStack">Department data object.</param>
        /// <param name="scale">Custom multiplier for the bubble radius.</param>
        /// <param name="tag">Boolean value to toggle including program quantity for area calculation.</param>
        /// <returns name="areaList">List of doubles representing radius of each bubble.</returns>
        /// <search>
        /// BubbleRadius
        /// </search>
        public static List<double> BubbleRadiusFromNormalizedArea(List<List<string>> dataStack, double scale, Boolean tag = true)
        {
            List<Circle> cirList = new List<Circle>();
            List<string> progAreaList = dataStack[3];
            List<string> progQuantList = dataStack[4];

            List<double> progTotalList = new List<double>();
            for (int i = 0; i < progAreaList.Count; i++)
            {
                double totArea = 0;
                if (tag) totArea = Convert.ToDouble(progAreaList[i]) * Convert.ToDouble(progQuantList[i]);
                else totArea = Convert.ToDouble(progAreaList[i]);
                progTotalList.Add(totArea);
            }

            double maxArea = progTotalList.Max();
            List<double> progNormalizedTotArea = new List<double>();
            for (int i = 0; i < progTotalList.Count; i++)
            {
                progNormalizedTotArea.Add(progTotalList[i] * scale / maxArea);
            }
            return progNormalizedTotArea;

        }

        //circle packing algo to make bubble chart
        /// <summary>
        /// Implements circle packing algorithm to build a bubble chart for programs.
        /// </summary>
        /// <param name="pointList">List of point2d for bubble location.</param>
        /// <param name="radiusList">Double input as radius for the bubbles.</param>
        /// <param name="iteration">Number of times to run the circle packing algorithm. Higher value leads to better accuracy.</param>
        /// <returns name="BubbleList">List of circles representing bubbles.</returns>
        /// <returns name="PointList">List of point2d representing bubble location.</returns>
        /// <search>
        /// Circle packing algorithm, bubble chart
        /// </search>
        [MultiReturn(new[] { "BubbleList", "PointList" })]
        public static Dictionary<string, object> PackBubbles(List<Point2d> pointList, List<double> radiusList, int iteration = 100)
        {
            List<Circle> cirList = new List<Circle>();
            List<double> velXList = new List<double>();
            List<double> velYList = new List<double>();

            double maxSpeed = 0.05, pushFactor = 0.05, pullFactor = 0.01;
            Random rn = new Random();
            for (int i = 0; i < pointList.Count; i++)
            {
                double ran = Convert.ToDouble(rn.Next(2, 10));
            }
            for (int i = 0; i < iteration; i++)
            {
                velXList.Clear();
                velYList.Clear();
                for (int v = 0; v < pointList.Count; v++)
                {
                    velXList.Add(pointList[v].X * -1 * pullFactor);
                    velYList.Add(pointList[v].Y * -1 * pullFactor);
                }

                for (int j = 0; j < pointList.Count; j++)
                {
                    for (int k = j + 1; k < pointList.Count; k++)
                    {
                        double rad1 = radiusList[j];
                        double rad2 = radiusList[k];
                        double diam = rad1 + rad2;
                        Vector vect = Vector.ByTwoPoints(Point.ByCoordinates(pointList[j].X, pointList[j].Y), Point.ByCoordinates(pointList[k].X, pointList[k].Y));
                        double dist = vect.Length;
                        if (dist < diam)
                        {
                            velXList[j] += vect.X * -1 * pushFactor;
                            velYList[j] += vect.Y * -1 * pushFactor;
                            velXList[k] += vect.X * 1 * pushFactor;
                            velYList[k] += vect.Y * 1 * pushFactor;
                        }
                    }// end of 'k' for loop
                }// end of 'j' for loop
                
                for (int p = 0; p < pointList.Count; p++)
                {
                    if (velXList[p] > maxSpeed) velXList[p] = maxSpeed;
                    if (velYList[p] > maxSpeed) velYList[p] = maxSpeed;
                    Vector vc = Vector.ByTwoPoints(Point.ByCoordinates(0, 0), Point.ByCoordinates(velXList[p], velYList[p]));
                    Point pn = Point.ByCoordinates(pointList[p].X, pointList[p].Y).Add(vc);
                    pointList.RemoveAt(p);
                    pointList.Insert(p, Point2d.ByCoordinates(pn.X,pn.Y));
                    pn.Dispose();                 
                }
            }// end of 'i' for loop

            for (int n = 0; n < pointList.Count; n++)
            {
                Circle cir = Circle.ByCenterPointRadius(Point.ByCoordinates(pointList[n].X, pointList[n].Y), radiusList[n]);
                cirList.Add(cir);
            }


            return new Dictionary<string, object>
            {
                { "BubbleList", cirList },
                { "PointList", pointList }
            };

        }

        //Makes adjacency network lines
        /// <summary>
        /// Builds adjacency network of line geometry between program requirements to be next to each other
        /// </summary>
        /// <param name="pointList">List of point2d representing center of the bubbles.</param>
        /// <param name="dataStack">Department data object.</param>
        /// <returns name="AdjacencyList">List of indices representing program, showing adjacency list.</returns>
        /// <returns name="ConnectionList">List of line2d's representing adjacency network.</returns>
        /// <search>
        /// Adjacency network
        /// </search>        
        [MultiReturn(new[] { "AdjacencyList", "ConnectionList" })]
        public static Dictionary<string, object> MakeAdjacencyNetwork(List<Point2d> pointList, List<List<string>> dataStack)
        {
            List<int> idList = new List<int>();
            List<string> adjList = dataStack[6];
            List<List<int>> adjListPlace = new List<List<int>>();
            Random rnd1 = new Random();
            Random rnd2 = new Random();
            for (int i = 0; i < adjList.Count; i++)
            {
                int randomNum = rnd1.Next(0, 7);
                List<int> adjAdd = new List<int>();

                for (int j = 0; j < randomNum; j++)
                {
                    int num = rnd2.Next(0, 15);
                    adjAdd.Add(num);
                }
                adjListPlace.Add(adjAdd);
            }

            List<List<Line>> allLinList = new List<List<Line>>();
            for (int i = 0; i < adjList.Count; i++)
            {
                List<int> adjAdd = new List<int>();
                adjAdd = adjListPlace[i];
                List<Line> linList = new List<Line>();
                for (int j = 0; j < adjAdd.Count; j++)
                {
                    if (pointList[i].X == pointList[j].X && pointList[i].Y == pointList[j].Y) continue;
                    Line ln = Line.ByStartPointEndPoint(Point.ByCoordinates(pointList[i].X, pointList[i].Y), Point.ByCoordinates(pointList[j].X, pointList[j].Y));
                    linList.Add(ln);
                }
                allLinList.Add(linList);
            }
            return new Dictionary<string, object>
            {
                { "AdjacencyList", adjListPlace },
                { "ConnectionList", allLinList }
            };

        }
        #endregion
    }
}