
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using stuffer;

///////////////////////////////////////////////////////////////////
/// NOTE: This project requires references to the ProtoInterface
/// and ProtoGeometry DLLs. These are found in the Dynamo install
/// directory.
///////////////////////////////////////////////////////////////////

namespace SpacePlanning
{
    public class BubbleChart
    {
        // Two private variables for example purposes
        private double _a;
        private double _b;



        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        internal BubbleChart(double a, double b)
        {
            _a = a;
            _b = b;
        }



        /// <summary>
        /// An example of how to construct an object via a static method.
        /// This is needed as Dynamo lacks a "new" keyword to construct a 
        /// new object
        /// </summary>
        /// <param name="a">1st number. This will be stored in the Class.</param>
        /// <param name="b">2nd number. This will be stored in the Class</param>
        /// <returns>A newly-constructed ZeroTouchEssentials object</returns>
        public static BubbleChart ByTests(double a, double b)
        {
            return new BubbleChart(a, b);
        }

        /// <summary>
        /// Example property returning the value _a inside the object
        /// </summary>
        private double _A
        {
            get { return _a; }
        }


        // CODE FOR BUBBLE CHART BELOW

        //0B    /////////////////////
        // MAKE POINT GRID FOR BUBBLES
        public static List<Point2d> ProgramPointGridFromData(List<List<string>> dataStack)
        {

            int lenPointLists = dataStack[0].Count;
            int count = 0;
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

        //1B        ///////////////////////
        //COMPUTES THE AREA, THEN NORMALIZES IT, WHICH IS THEN USED TO GET THE RADIUS FOR THE BUBBLES
        public static List<double> BubbleRadiusFromNormalizedArea(List<List<string>> dataStack, double scale, Boolean tag = true)
        {
            List<Circle> cirList = new List<Circle>();
            List<string> progAreaList = dataStack[3];
            List<string> progQuantList = dataStack[4];

            List<double> progTotalList = new List<double>();
            for (int i = 0; i < progAreaList.Count; i++)
            {
                double totArea = 0;
                if (tag)
                {
                    totArea = Convert.ToDouble(progAreaList[i]) * Convert.ToDouble(progQuantList[i]);
                }
                else
                {
                    totArea = Convert.ToDouble(progAreaList[i]);
                }

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


        //2B        ////////////////////////
        //IMPLEMENTING CIRCLE PACKING STRATEGY
        public static Dictionary<string, object> PackBubbles(List<Point2d> ptList, List<double> radiusList, int iteration = 100)
        {
            // List<Point> ptList = new List<Point>(ptListIn);
            List<Circle> cirList = new List<Circle>();
            List<double> velXList = new List<double>();
            List<double> velYList = new List<double>();
            //List<double> radiusList = new List<double>();

            double maxSpeed = 0.05;
            double pushFactor = 0.05;
            double pullFactor = 0.01;


            //make the radius list
            Random rn = new Random();
            for (int i = 0; i < ptList.Count; i++)
            {
                double ran = Convert.ToDouble(rn.Next(2, 10));
                //radiusList.Add(ran);
            }


            //main iteration loop
            for (int i = 0; i < iteration; i++)
            {

                velXList.Clear();
                velYList.Clear();

                //make the velocity list
                for (int v = 0; v < ptList.Count; v++)
                {
                    velXList.Add(ptList[v].X * -1 * pullFactor);
                    velYList.Add(ptList[v].Y * -1 * pullFactor);
                }

                for (int j = 0; j < ptList.Count; j++)
                {
                    for (int k = j + 1; k < ptList.Count; k++)
                    {

                        double rad1 = radiusList[j];
                        double rad2 = radiusList[k];
                        double diam = rad1 + rad2;
                        if (ptList[j].Equals(ptList[k]))
                        {
                            //continue;
                        }
                        Vector vect = Vector.ByTwoPoints(Point.ByCoordinates(ptList[j].X,ptList[j].Y), Point.ByCoordinates(ptList[k].X, ptList[k].Y));
                        double dist = vect.Length;
                        //double dist = ptList[j].DistanceTo(ptList[k]);
                        //double dist = Math.Sqrt(Math.Pow((ptList[j].X - ptList[k].X), 2) + Math.Pow((ptList[j].Y - ptList[k].Y), 2));
                        if (dist < diam)
                        {
                            velXList[j] += vect.X * -1 * pushFactor;
                            velYList[j] += vect.Y * -1 * pushFactor;
                            velXList[k] += vect.X * 1 * pushFactor;
                            velYList[k] += vect.Y * 1 * pushFactor;

                        }
                    }// end of 'k' for loop
                }// end of 'j' for loop


                for (int p = 0; p < ptList.Count; p++)
                {
                    if (velXList[p] > maxSpeed)
                    {
                        velXList[p] = maxSpeed;
                    }

                    if (velYList[p] > maxSpeed)
                    {
                        velYList[p] = maxSpeed;
                    }


                    Vector vc = Vector.ByTwoPoints(Point.ByCoordinates(0, 0), Point.ByCoordinates(velXList[p], velYList[p]));
                    Point pn = Point.ByCoordinates(ptList[p].X,ptList[p].Y).Add(vc);
                    ptList.RemoveAt(p);
                    ptList.Insert(p, Point2d.ByCoordinates(pn.X,pn.Y));
                    pn.Dispose();
                 
                }



            }// end of 'i' for loop

            for (int n = 0; n < ptList.Count; n++)
            {
                Circle cir = Circle.ByCenterPointRadius(Point.ByCoordinates(ptList[n].X,ptList[n].Y), radiusList[n]);
                cirList.Add(cir);
            }


            return new Dictionary<string, object>
            {
                { "BubbleList", cirList },
                { "PointList", ptList }
            };

        }



        //3B        /////////////////////////////
        //MAKING ADJACENCY LIST AND LINE CONNECTION BETWEEN POINTS
        public static Dictionary<string, object> MakeAdjacencyNetwork(List<Point2d> ptList, List<List<string>> dataStack)
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
                    if (ptList[i].X == ptList[j].X && ptList[i].Y == ptList[j].Y)
                    {
                        continue;
                    }
                    Line ln = Line.ByStartPointEndPoint(Point.ByCoordinates(ptList[i].X,ptList[i].Y), Point.ByCoordinates(ptList[j].X, ptList[j].Y));
                    linList.Add(ln);
                    //ln.Dispose();
                }
                allLinList.Add(linList);

            }


            return new Dictionary<string, object>
            {
                { "AdjacencyList", adjListPlace },
                { "ConnectionList", allLinList }
            };

        }

    }
}