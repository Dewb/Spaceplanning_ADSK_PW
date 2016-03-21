using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;


namespace SpacePlanning
{

    

    public class BuildLayout
    {
        //testing to see if node type enum works
        internal static void testEnumApplication(Node A, Node B, Node C)
        {
            NodeType nd = NodeType.Container;
           // Node nd = new Node(1, A, B, C, nd, poly, lin, dept);
        }


        private static double spacingSet = 8; // 0.5 worked great
        internal static Point2d reference = new Point2d(0,0);

        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint" })]
        public static Dictionary<string, object> RecursiveSplitPoly(Polygon2d poly,   double ratioA = 0.5, int recompute = 1)
        {

            /*PSUEDO CODE:
            stack polylist = new polylist
            polylist.push(input poly)
            while(polyList != empty )
            {

                poly = polyList[i]
                splittedpoly = splitpolyintowtwocheck(poly, ratio, extents, dir)
                if (splipoly[0].area > thresharea)
                polylist.addrange(splitpoly)
                i += 1
                count += 1

            }
                
            */
            double ratio = 0.5;

            List<Polygon2d> polyList = new List<Polygon2d>();
            List<Point2d> pointsList = new List<Point2d>();
            List<double> areaList = new List<double>();
            Stack<Polygon2d> polyRetrieved = new Stack<Polygon2d>();
            polyRetrieved.Push(poly);
            int count = 0;
            //int thresh = 10;
            //double areaThreshold = GraphicsUtility.AreaPolygon2d(poly.Points)/10;
            double areaThreshold = 1000;
            int dir = 1;
            List<Polygon2d> polyAfterSplit = null;
            Dictionary<string, object> splitReturn = null;
            Polygon2d currentPoly;
            Random rand = new Random();
            double maximum = 0.9;
            double minimum = 0.3;
            while (polyRetrieved.Count > 0)
            {
                //double mul = rand.NextDouble() * (maximum - minimum) + minimum;
                //ratio *= mul;
                ratio = rand.NextDouble() * (maximum - minimum) + minimum;
                currentPoly = polyRetrieved.Pop();
                try
                {
                    splitReturn = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                    polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                }
                catch (Exception)
                {
                    //toggle dir between 0 and 1
                    dir = BasicUtility.toggleInputInt(dir);
                    splitReturn = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                    if (splitReturn == null)
                    {
                        //Trace.WriteLine("Could Not Split due to Aspect Ration Problem : Sorry");
                        continue;
                    }
                    polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    //throw;
                }
                List<List<Point2d>> pointsOnPoly = (List<List<Point2d>>)splitReturn["EachPolyPoint"];
                double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                if (area1 > areaThreshold)
                {
                    polyRetrieved.Push(polyAfterSplit[0]);
                    polyList.Add(polyAfterSplit[0]);
                    pointsList.AddRange(pointsOnPoly[0]);
                    areaList.Add(area1);

                }
                if (area2 > areaThreshold)
                {
                    polyRetrieved.Push(polyAfterSplit[1]);
                    polyList.Add(polyAfterSplit[1]);
                    pointsList.AddRange(pointsOnPoly[1]);
                    areaList.Add(area2);

                }
                //pointsList.AddRange(pointsOnPoly);
                //polyList.AddRange(polyAfterSplit);
                //pointsList.AddRange(pointsOnPoly);
                //areaList.Add(area1);
                //areaList.Add(area2);


                //toggle dir between 0 and 1
                dir = BasicUtility.toggleInputInt(dir);
                count += 1;
            }

            //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) }
            };


        }



        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursiveSplitPolyPrograms(List<Polygon2d> polyInputList,List<ProgramData> progData, double ratioA = 0.5, int recompute = 1)
        {

            /*PSUEDO CODE:
            stack polylist = new polylist
            polylist.push(input poly)
            while(polyList != empty )
            {

                poly = polyList[i]
                splittedpoly = splitpolyintowtwocheck(poly, ratio, extents, dir)
                if (splipoly[0].area > thresharea)
                polylist.addrange(splitpoly)
                i += 1
                count += 1

            }
                
            */
            double ratio = 0.5;

            List<Polygon2d> polyList = new List<Polygon2d>();            
            List<Point2d> pointsList = new List<Point2d>();
            List<double> areaList = new List<double>();
            Stack<Polygon2d> polyRetrieved = new Stack<Polygon2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();

            for(int j = 0; j < progData.Count; j++)
            {
                programDataRetrieved.Push(progData[j]);
            }

            //////////////////////////////////////////////////////////////////////
            for(int i=0; i < polyInputList.Count; i++)
            {

           
            Polygon2d poly = polyInputList[i];
            if (poly == null || poly.Points == null || poly.Points.Count == 0)
            {
                return null;
            }


                polyRetrieved.Push(poly);
            int count = 0;
            //int thresh = 10;
            //double areaThreshold = GraphicsUtility.AreaPolygon2d(poly.Points)/10;
            double areaThreshold = 1000;
            int dir = 1;
            List<Polygon2d> polyAfterSplit = null;
            Dictionary<string, object> splitReturn = null;
            Polygon2d currentPoly;
            Random rand = new Random();
            double maximum = 0.9;
            double minimum = 0.3;
            while (polyRetrieved.Count > 0 && programDataRetrieved.Count>0)
            {
                //double mul = rand.NextDouble() * (maximum - minimum) + minimum;
                //ratio *= mul;
                ProgramData progItem = programDataRetrieved.Pop();
                ratio = rand.NextDouble() * (maximum - minimum) + minimum;
                currentPoly = polyRetrieved.Pop();
                try
                {
                    splitReturn = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                    polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                }
                catch (Exception)
                {
                    //toggle dir between 0 and 1
                    dir = BasicUtility.toggleInputInt(dir);
                    splitReturn = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                    if (splitReturn == null)
                    {
                        //Trace.WriteLine("Could Not Split due to Aspect Ration Problem : Sorry");
                        continue;
                    }
                    polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    //throw;
                }
                List<List<Point2d>> pointsOnPoly = (List<List<Point2d>>)splitReturn["EachPolyPoint"];
                double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                if (area1 > areaThreshold)
                {
                    polyRetrieved.Push(polyAfterSplit[0]);
                    polyList.Add(polyAfterSplit[0]);
                    progItem.AreaProvided = area1;
                    pointsList.AddRange(pointsOnPoly[0]);
                    areaList.Add(area1);

                }
                if (area2 > areaThreshold)
                {
                    polyRetrieved.Push(polyAfterSplit[1]);
                    polyList.Add(polyAfterSplit[1]);
                    progItem.AreaProvided = area2;
                    pointsList.AddRange(pointsOnPoly[1]);
                    areaList.Add(area2);

                }
                    //pointsList.AddRange(pointsOnPoly);
                    //polyList.AddRange(polyAfterSplit);
                    //pointsList.AddRange(pointsOnPoly);
                    //areaList.Add(area1);
                    //areaList.Add(area2);
                    
                //toggle dir between 0 and 1
                dir = BasicUtility.toggleInputInt(dir);
                count += 1;
            }// end of while loop
            }//end of for loop

            List<ProgramData> AllProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count)
                {
                    progNew.PolyProgAssigned = polyList[i];
                }
                else
                {
                    progNew.PolyProgAssigned = null;
                }
              
                AllProgramDataList.Add(progNew);
            }

            //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) },
                { "UpdatedProgramData", (AllProgramDataList) }
                
            };


        }


        //RECURSIVE SPLITS A POLY
        public static Dictionary<string, object> RecursiveSplitPolyProgramsSingleOut(List<Polygon2d> polyInputList, List<ProgramData> progData, double ratioA = 0.5, int recompute = 1)
        {

            /*PSUEDO CODE:
            stack polylist = new polylist
            polylist.push(input poly)
            while(polyList != empty )
            {

                poly = polyList[i]
                splittedpoly = splitpolyintowtwocheck(poly, ratio, extents, dir)
                if (splipoly[0].area > thresharea)
                polylist.addrange(splitpoly)
                i += 1
                count += 1

            }
                
            */
            double ratio = 0.5;

            List<Polygon2d> polyList = new List<Polygon2d>();
            List<Point2d> pointsList = new List<Point2d>();
            List<double> areaList = new List<double>();
            Stack<Polygon2d> polyRetrieved = new Stack<Polygon2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();

            for (int j = 0; j < progData.Count; j++)
            {
                programDataRetrieved.Push(progData[j]);
            }

            //////////////////////////////////////////////////////////////////////
            for (int i = 0; i < polyInputList.Count; i++)
            {


                Polygon2d poly = polyInputList[i];
                if (poly == null || poly.Points == null || poly.Points.Count == 0)
                {
                    return null;
                }


                polyRetrieved.Push(poly);
                int count = 0;
                //int thresh = 10;
                //double areaThreshold = GraphicsUtility.AreaPolygon2d(poly.Points)/10;
                double areaThreshold = 1000;
                int dir = 1;
                List<Polygon2d> polyAfterSplit = null;
                Dictionary<string, object> splitReturn = null;
                Polygon2d currentPoly;
                Random rand = new Random();
                double maximum = 0.9;
                double minimum = 0.3;
                while (polyRetrieved.Count > 0 && programDataRetrieved.Count > 0)
                {
                    //double mul = rand.NextDouble() * (maximum - minimum) + minimum;
                    //ratio *= mul;
                    ProgramData progItem = programDataRetrieved.Pop();
                    ratio = rand.NextDouble() * (maximum - minimum) + minimum;
                    currentPoly = polyRetrieved.Pop();
                    try
                    {
                        splitReturn = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                        polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    }
                    catch (Exception)
                    {
                        //toggle dir between 0 and 1
                        dir = BasicUtility.toggleInputInt(dir);
                        splitReturn = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                        if (splitReturn == null)
                        {
                            //Trace.WriteLine("Could Not Split due to Aspect Ration Problem : Sorry");
                            continue;
                        }
                        polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                        //throw;
                    }
                    List<List<Point2d>> pointsOnPoly = (List<List<Point2d>>)splitReturn["EachPolyPoint"];
                    double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                    double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                    if (area1 > areaThreshold)
                    {
                        polyRetrieved.Push(polyAfterSplit[0]);
                        polyList.Add(polyAfterSplit[0]);
                        progItem.AreaProvided = area1;
                        pointsList.AddRange(pointsOnPoly[0]);
                        areaList.Add(area1);

                    }
                    if (area2 > areaThreshold)
                    {
                        polyRetrieved.Push(polyAfterSplit[1]);
                        polyList.Add(polyAfterSplit[1]);
                        progItem.AreaProvided = area2;
                        pointsList.AddRange(pointsOnPoly[1]);
                        areaList.Add(area2);

                    }
                    //pointsList.AddRange(pointsOnPoly);
                    //polyList.AddRange(polyAfterSplit);
                    //pointsList.AddRange(pointsOnPoly);
                    //areaList.Add(area1);
                    //areaList.Add(area2);

                    //toggle dir between 0 and 1
                    dir = BasicUtility.toggleInputInt(dir);
                    count += 1;
                }// end of while loop
            }//end of for loop

            List<ProgramData> AllProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count)
                {
                    progNew.PolyProgAssigned = polyList[i];
                }
                else
                {
                    progNew.PolyProgAssigned = null;
                }

                AllProgramDataList.Add(progNew);
            }

            //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) },
                { "UpdatedProgramData", (AllProgramDataList) }

            };


        }

        //changes the center of one or both polys to ensure correct intersection line is found
        [MultiReturn(new[] { "CenterPolyA", "CenterPolyB", "PolyA", "PolyB" })]
        internal static Dictionary<string, object> ComputePolyCentersAlign(Polygon2d polyA, Polygon2d polyB)
        {
            double extents = 10000;
            //compute orig centers
            Point2d centerPolyA = GraphicsUtility.CentroidInPointLists(polyA.Points);
            Point2d centerPolyB = GraphicsUtility.CentroidInPointLists(polyB.Points);
            
            Point2d staticPoint;
            Polygon2d staticPoly;
            Point2d movingPoint;
            Polygon2d movingPoly;

            double areaPolyA = GraphicsUtility.AreaPolygon2d(polyA.Points);
            double areaPolyB = GraphicsUtility.AreaPolygon2d(polyB.Points);

            if (areaPolyA > areaPolyB)
            {
                staticPoint = centerPolyB;
                staticPoly = polyB;
                movingPoint = centerPolyA;
                movingPoly = polyA;
            }
            else
            {
                staticPoint = centerPolyA;
                staticPoly = polyA;
                movingPoint = centerPolyB;
                movingPoly = polyB;
            }

            //shift the other points

            Point2d movingPoint1 = new Point2d(staticPoint.X, movingPoint.Y);
            Point2d movingPoint2 = new Point2d(movingPoint.X, staticPoint.Y);

            bool IsMovingPoint1 = GraphicsUtility.PointInsidePolygonTest(movingPoly.Points, movingPoint1);
            bool IsMovingPoint2 = GraphicsUtility.PointInsidePolygonTest(movingPoly.Points, movingPoint2);

            if (IsMovingPoint1)
            {
                movingPoint = movingPoint1;

            }
            else if (IsMovingPoint2)
            {
                movingPoint = movingPoint2;
            }
            else
            {
                staticPoint = centerPolyA;
                staticPoly = polyA;
                movingPoint = movingPoint1;
                movingPoly = polyB;
            }

         

            return new Dictionary<string, object>
                {
                { "CenterPolyA", (staticPoint) },
                { "CenterPolyB", (movingPoint) },
                { "PolyA", (staticPoly) },
                { "PolyB", (movingPoly) }
                };
        }




        //changes the center of one or both polys to ensure correct intersection line is found
        [MultiReturn(new[] { "CenterPolyA", "CenterPolyB", "PolyA", "PolyA" })]
        internal static Dictionary<string, object> ComputePolyCenters(Polygon2d polyA, Polygon2d polyB)
        {
            double extents = 10000;
            //compute orig centers
            Point2d centerPolyA = GraphicsUtility.CentroidInPointLists(polyA.Points);
            Point2d centerPolyB = GraphicsUtility.CentroidInPointLists(polyB.Points);

            //make infinite lines via both centers 0 - horizontal line, 1 - vertical line
            Line2d lineAX = new Line2d(centerPolyA, extents, 0);
            Line2d lineAY = new Line2d(centerPolyA, extents, 1);

            Line2d lineBX = new Line2d(centerPolyB, extents, 0);
            Line2d lineBY = new Line2d(centerPolyB, extents, 1);

        
            //get line line intersection for these lines
            //AX-BY and BX-AY
            Point2d pAXBY = GraphicsUtility.LineLineIntersection(lineAX, lineBY);
            Point2d pBXAY = GraphicsUtility.LineLineIntersection(lineBX, lineAY);

            //check for point containment test for these two
            bool checkA_AXBY = GraphicsUtility.PointInsidePolygonTest(polyA.Points, pAXBY);
            bool checkA_BXAY = GraphicsUtility.PointInsidePolygonTest(polyA.Points, pBXAY);
            bool checkB_AXBY = GraphicsUtility.PointInsidePolygonTest(polyB.Points, pAXBY);            
            bool checkB_BXAY = GraphicsUtility.PointInsidePolygonTest(polyB.Points, pBXAY);
            ////////////////////////////////////////////////////
            if(checkA_BXAY ==true && checkB_AXBY ==true)
            {
                return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
            }

            if (checkA_AXBY == true && checkB_BXAY == true)
            {
                return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
            }
            ////////////////////////////////////////////////////


            if(checkA_AXBY == true)
            {
                
                //centerPolyB.X = centerPolyA.X;

            }else if(checkA_BXAY == true)
            {
                //centerPolyB.Y = centerPolyA.Y;

            }
            else if(checkB_AXBY== true)
            {

            }else if(checkB_BXAY == true)
            {

            }
            else
            {
                return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
            }





                return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
        }





        [MultiReturn(new[] { "Neighbour", "SharedEdgeA", "SharedEdgeB", "LineMoved", "CenterToCenterLine","CenterPolyPoint","CenterPolyOtherPoint" })]
        internal static Dictionary<string, object> PolygonPolygonCommonEdgeDict(Polygon2d poly, Polygon2d other)
        {
            /*
            first reduce number of pts in both polys
            find their centers
            make a vec between their center
            get horizontal comp of vec
            get vertical comp of vec
            which length is long will be our vector

            then for both polys
                check line line intersection between line between two centers and each line of the poly
                    if no intersect, no edge
                    find the line intersects 
                    find the perpendicular projection of centers on these linese

            */

            bool check = false;
            if (poly == null || other == null)
            {
                return null;
            }

            double eps = 200;
            //Polygon2d polyReg = poly;
            //Polygon2d otherReg = other;
            //reduce number of points
            Polygon2d polyReg = new Polygon2d(poly.Points);
            Polygon2d otherReg = new Polygon2d(other.Points);
            //reassign centers to each poly
            //Dictionary<string,object> UpdatedCenters  = ComputePolyCentersAlign(polyReg, otherReg);
            Dictionary<string, object> UpdatedCenters = ComputePolyCentersAlign(polyReg, otherReg);

            /*
            return new Dictionary<string, object>
                {
                { "CenterPolyA", (staticPoint) },
                { "CenterPolyB", (movingPoint) },
                { "PolyA", (staticPoly) },
                { "PolyB", (movingPoly) }
                };

            */
            Point2d centerPoly = (Point2d)UpdatedCenters["CenterPolyA"];
            Point2d centerOther = (Point2d)UpdatedCenters["CenterPolyB"];

            polyReg = (Polygon2d)UpdatedCenters["PolyA"];
            otherReg = (Polygon2d)UpdatedCenters["PolyB"];






            //find centers
            //Point2d centerPoly = GraphicsUtility.CentroidInPointLists(polyReg.Points);
            //Point2d centerOther = GraphicsUtility.CentroidInPointLists(otherReg.Points);




            //make vectors
            Vector2d centerToCen = new Vector2d(centerPoly, centerOther);
            Vector2d centerToCenX = new Vector2d(centerToCen.X, 0);
            Vector2d centerToCenY = new Vector2d(0, centerToCen.Y);





            //make centerLine
            Line2d centerLine = new Line2d(centerPoly, centerOther);
            Vector2d keyVec;
            if (centerToCenX.Length > centerToCenY.Length)
            {
                keyVec = new Vector2d(centerToCenX.X, centerToCenX.Y);
            }
            else
            {
                keyVec = new Vector2d(centerToCenY.X, centerToCenY.Y);
            }

            //check line poly intersection between centertocen vector and each polys            
            Line2d lineInPolyReg = GraphicsUtility.LinePolygonIntersectionReturnLine(polyReg.Points, centerLine,centerOther);
            Line2d lineInOtherReg = GraphicsUtility.LinePolygonIntersectionReturnLine(otherReg.Points, centerLine, centerPoly);

            //extend the line
            //lineInPolyReg.extend();
            //lineInOtherReg.extend();

            //check = GraphicsUtility.AreLinesCollinear(lineInPolyReg, lineInOtherReg);
            //check = GraphicsUtility.CheckLineCollinear(lineInPolyReg, lineInOtherReg);

            //find distance d1 and d2 from two centers to linepolyintersection line
            Point2d projectedPtOnPolyReg = GraphicsUtility.ProjectedPointOnLine(lineInPolyReg, centerPoly);
            Point2d projectedPtOnOtherReg = GraphicsUtility.ProjectedPointOnLine(lineInOtherReg, centerOther);

            double dist1 = GraphicsUtility.DistanceBetweenPoints(centerPoly, projectedPtOnPolyReg);
            double dist2 = GraphicsUtility.DistanceBetweenPoints(centerOther, projectedPtOnOtherReg);

            double totalDistance = 2 * (dist1 + dist2);
            //Line2d lineMoved = new Line2d(lineInPolyReg);
            Line2d lineMoved = new Line2d(lineInPolyReg.StartPoint,lineInPolyReg.EndPoint);
            lineMoved.move(centerPoly);
            Point2d projectedPt = GraphicsUtility.ProjectedPointOnLine(lineMoved, centerOther);
            double distance = GraphicsUtility.DistanceBetweenPoints(projectedPt, centerOther);

            bool isNeighbour = false;
            if (totalDistance - eps < distance && distance < totalDistance + eps)
            {
                isNeighbour = true;
            }
            else
            {
                isNeighbour = false;
            }

            //"Neighbour", "SharedEdgeA", "SharedEdgeB" 

            return new Dictionary<string, object>
            {
                { "Neighbour", (isNeighbour) },
                { "SharedEdgeA", (lineInPolyReg) },
                { "SharedEdgeB", (lineInOtherReg) },
                { "LineMoved", (lineMoved) },
                { "CenterToCenterLine", (centerLine) },
                { "CenterPolyPoint", (centerPoly) },
                { "CenterPolyOtherPoint", (centerOther) },
            };

        }

        //Make Dept Topology Matrix
        [MultiReturn(new[] { "DeptTopologyList", "DeptNeighborNameList", "DeptNamesFlatten", "SharedEdgeSetA", "SharedEdgeSetB" })]
        internal static Dictionary<string, object> MakeDeptTopologyChart(List<DeptData> deptData)
        {
            List<Polygon2d> polygonsAllDeptList = new List<Polygon2d>();
            List<DeptData> deptDataAllDeptList = new List<DeptData>();
            List<List<string>> deptNamesNeighbors = new List<List<string>>();
            List<List<Line2d>> lineCollection1 = new List<List<Line2d>>();
            List<List<Line2d>> lineCollection2 = new List<List<Line2d>>();
            for (int i = 0; i < deptData.Count; i++)
            {
                List<string> neighbors = new List<string>();
                List<Line2d> networkLine1 = new List<Line2d>();
                List<Line2d> networkLine2 = new List<Line2d>();
                List<Polygon2d> polyListA = deptData[i].PolyDeptAssigned;                
                for(int j = 0; j < polyListA.Count; j++)
                {
                    Polygon2d polyA = polyListA[j];
                    for (int k = i+1; k < deptData.Count; k++)
                    {
                        List<Polygon2d> polyListB = deptData[k].PolyDeptAssigned;
                        for (int l = 0; l < polyListB.Count; l++)
                        {
                            Polygon2d polyB = polyListB[l];
                            Dictionary<string, object> checkNeighbor = PolygonPolygonCommonEdgeDict(polyA, polyB);
                            if ((bool)checkNeighbor["Neighbour"] == true)
                            {
                                neighbors.Add(deptData[k].DepartmentName);
                                networkLine1.Add((Line2d)checkNeighbor["SharedEdgeA"]);
                                networkLine2.Add((Line2d)checkNeighbor["SharedEdgeB"]);
                            }
                            
                        }// 4th for loop done
                    } // 3rd for loop done
                } // 2nd for loop done
                deptNamesNeighbors.Add(neighbors);
                lineCollection1.Add(networkLine1);
                lineCollection2.Add(networkLine2);
            } // 1st for loop done

            /*
            return new Dictionary<string, object>
            {
                { "Neighbour", (isNeighbour) },
                { "SharedEdgeA", (lineInPolyReg) },
                { "SharedEdgeB", (lineInOtherReg) },
                { "LineMoved", (lineMoved) },
                { "CenterToCenterLine", (centerLine) },
                { "CenterPolyPoint", (centerPoly) },
                { "CenterPolyOtherPoint", (centerOther) },
            };
            */




            List<List<string>> deptNeighborNames = new List<List<string>>();
         
            return new Dictionary<string, object>
            {
                { "DeptTopologyList", (deptNamesNeighbors) },
                { "DeptNeighborNameList", (deptNamesNeighbors) },
                { "DeptNamesFlatten", (deptNamesNeighbors) },
                { "SharedEdgeSetA", (lineCollection1) },
                { "SharedEdgeSetB", (lineCollection2) }

            };
        }






        //Make Dept Topology Matrix can be discarded
        [MultiReturn(new[] { "IsDuplicate", "CleanLineList", "OrigLinesFlattenList","CirculationPolygons" })]
        internal static Dictionary<string, object> CirculationMake(List<DeptData> deptData,List<List<Line2d>> lineListA, List<List<Line2d>> lineListB, double distance = 8, bool tag = true)
        {

            //flatten the list
            List<Line2d> flatLineList = new List<Line2d>();
            List<bool> IsDuplicateList = new List<bool>();
            List<Line2d> flatListA = GraphicsUtility.FlattenLine2dList(lineListA);
            List<Line2d> flatListB = GraphicsUtility.FlattenLine2dList(lineListB);

            if (flatListA.Count > 0)
                flatLineList.AddRange(flatListA);
            if (flatListB.Count > 0)
                flatLineList.AddRange(flatListB);
          
            // get clean lines - remove duplicates
            List<double> slopeInterceptList = GraphicsUtility.LineSlopeIntercept(flatLineList);
            List<double> cleanIndexes = BasicUtility.DuplicateIndexes(slopeInterceptList);
            List<Line2d> cleanLineList = GraphicsUtility.RemoveDuplicateLines(slopeInterceptList, flatLineList);
            string foo = "";




            //flatten all the polys in each depts to make it one list
            List<Polygon2d> allDeptPolyList = new List<Polygon2d>();
            List<Polygon2d> circulationPolyList = new List<Polygon2d>();
            List<Polygon2d> reforemedDeptPolyList = new List<Polygon2d>();
            for(int i=0; i < deptData.Count; i++)
            {
                List<Polygon2d> deptPolyList = deptData[i].PolyDeptAssigned;
                if (deptPolyList.Count > 0)
                {
                    allDeptPolyList.AddRange(deptPolyList);
                }
            }

            for (int i = 0; i < cleanLineList.Count; i++)
            {
                Line2d splitter = cleanLineList[i];
                for (int j = 0; j < allDeptPolyList.Count; j++)
                {
                    Polygon2d deptPoly = allDeptPolyList[j];
                    Point2d midPt = splitter.midPt();
                    bool checkInside = GraphicsUtility.PointInsidePolygonTest(deptPoly, midPt);
                    if (!tag)
                    {
                        Dictionary<string, object> splitResult = SplitByLine(deptPoly, splitter, distance);
                        List<Polygon2d> polyAfterSplit = (List<Polygon2d>)(splitResult["PolyAfterSplit"]);
                        if (polyAfterSplit != null)
                        {
                            double areaA = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                            double areaB = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                            if (areaA < areaB)
                            {
                                circulationPolyList.Add(polyAfterSplit[0]);
                            }
                            else
                            {
                                circulationPolyList.Add(polyAfterSplit[1]);
                            }

                        }
                    }
                    else
                    {
                        if (checkInside)
                        {
                            Dictionary<string, object> splitResult = SplitByLine(deptPoly, splitter, distance);
                            List<Polygon2d> polyAfterSplit = (List<Polygon2d>)(splitResult["PolyAfterSplit"]);
                            if (polyAfterSplit != null)
                            {
                                double areaA = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                                double areaB = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                                if (areaA < areaB)
                                {
                                    circulationPolyList.Add(polyAfterSplit[0]);
                                }
                                else
                                {
                                    circulationPolyList.Add(polyAfterSplit[1]);
                                }

                            }

                        } // end of check inside
                    }
                 
                  
                }// end of for loop j
            }// end of for loop i


            return new Dictionary<string, object>
            {
                { "IsDuplicate", (cleanLineList) },
                { "CleanLineList", (slopeInterceptList) },
                { "OrigLinesFlattenList", (flatLineList) },
                { "CirculationPolygons", (circulationPolyList) }

            };
        }

        


        //Make Dept Topology Matrix
        [MultiReturn(new[] { "DeptTopologyList","DeptNeighborNameList", "DeptNamesFlatten", "SharedEdgeSetA", "SharedEdgeSetB" })]
        public static Dictionary<string,object> MakeDeptTopology(List<DeptData> deptData)
        {

            //List<Dictionary<int, Polygon2d>> polygonsAllDeptList = new List<Dictionary<int, Polygon2d>>();
            //List<Dictionary<int, DeptData>> deptDataAllDeptList = new List<Dictionary<int, DeptData>>();
            List<Polygon2d> polygonsAllDeptList = new List<Polygon2d>();
            List<DeptData> deptDataAllDeptList = new List<DeptData>();

            for (int i = 0; i < deptData.Count; i++)
            {
                List<Polygon2d> polyList = deptData[i].PolyDeptAssigned;               
                for (int j = 0; j < polyList.Count; j++)
                {
                    polygonsAllDeptList.Add(polyList[j]);
                    deptDataAllDeptList.Add(deptData[i]);
                }
            }






            List<List<string>> deptNeighborNames = new List<List<string>>();
            List<Line2d> sharedEdgeAList = new List<Line2d>();
            List<Line2d> sharedEdgeBList = new List<Line2d>();
            string nameDeptCurrent = "";
            string nameDeptPrev = "";
            bool changed = true;
            int iter = -1;
            for (int i = 0; i < polygonsAllDeptList.Count; i++)
            {
                nameDeptCurrent = deptDataAllDeptList[i].DepartmentName; ;
                if (nameDeptCurrent != nameDeptPrev)
                {
                    string nameDept = deptDataAllDeptList[i].DepartmentName;
                    iter += 1;
                }

               
               
                List<string> neighbors = new List<string>();
                Polygon2d polyA = polygonsAllDeptList[i];

                //for loop begins======================================================
                for (int j = i; j < polygonsAllDeptList.Count; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }
                    Polygon2d polyB = polygonsAllDeptList[j];
                    Dictionary<string, object> checkNeighbor = PolygonPolygonCommonEdgeDict(polyA, polyB);
                    if ((bool)checkNeighbor["Neighbour"] == true)
                    {
                        neighbors.Add(deptDataAllDeptList[i].DepartmentName);
                    }
                }

                // end of for loop 2nd=================================================
                if(iter == 0)
                {
                    deptNeighborNames.Add(neighbors);
                }
                else
                {
                    if (neighbors.Count > 0)
                    {
                        deptNeighborNames[iter].AddRange(neighbors);
                      
                    }
                    else
                    {

                        //deptNeighborNames[iter].Add(null);
                    }
                   
                }
                
                nameDeptPrev = deptDataAllDeptList[i].DepartmentName;
            }// end of for loop 1st







            sharedEdgeAList = null;
            sharedEdgeBList = null;
            return new Dictionary<string, object>
            {
                { "DeptTopologyList", (deptNeighborNames) },
                { "DeptNeighborNameList", (deptNeighborNames) },
                { "DeptNamesFlatten", (deptDataAllDeptList) },
                { "SharedEdgeSetA", (sharedEdgeAList) },
                { "SharedEdgeSetB", (sharedEdgeBList) }

            };
        }


        internal static Line2d PolygonPolygonCommonEdge(Polygon2d poly, Polygon2d other)
        {
            /*
            first reduce number of pts in both polys
            find their centers
            make a vec between their center
            get horizontal comp of vec
            get vertical comp of vec
            which length is long will be our vector

            then for both polys
                check line line intersection between line between two centers and each line of the poly
                    if no intersect, no edge
                    find the line intersects 
                    find the perpendicular projection of centers on these linese

            */

            bool check = false;
            if (poly == null || other == null)
            {
                return null;
            }
            double eps = 100;
            //reduce number of points
            Polygon2d polyReg = new Polygon2d(poly.Points);
            Polygon2d otherReg = new Polygon2d(other.Points);

            //find centers
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(polyReg.Points);
            Point2d centerOther = GraphicsUtility.CentroidInPointLists(otherReg.Points);

            //make vectors
            Vector2d centerToCen = new Vector2d(centerPoly, centerOther);
            Vector2d centerToCenX = new Vector2d(centerToCen.X, 0);
            Vector2d centerToCenY = new Vector2d(0, centerToCen.Y);

            //make centerLine
            Line2d centerLine = new Line2d(centerPoly, centerOther);
            Vector2d keyVec;
            if(centerToCenX.Length > centerToCenY.Length)
            {
                keyVec = new Vector2d(centerToCenX.X, centerToCenX.Y);
            }
            else
            {
                keyVec = new Vector2d(centerToCenY.X, centerToCenY.Y);
            }

            //check line poly intersection between centertocen vector and each polys            
            Line2d lineInPolyReg = GraphicsUtility.LinePolygonIntersectionReturnLine(polyReg.Points, centerLine,centerOther);
            Line2d lineInOtherReg = GraphicsUtility.LinePolygonIntersectionReturnLine(otherReg.Points, centerLine,centerPoly);

            //extend the line
            //lineInPolyReg.extend();
            //lineInOtherReg.extend();

            //check = GraphicsUtility.AreLinesCollinear(lineInPolyReg, lineInOtherReg);
            //check = GraphicsUtility.CheckLineCollinear(lineInPolyReg, lineInOtherReg);

            //find distance d1 and d2 from two centers to linepolyintersection line
            Point2d projectedPtOnPolyReg = GraphicsUtility.ProjectedPointOnLine(lineInPolyReg, centerPoly);
            Point2d projectedPtOnOtherReg = GraphicsUtility.ProjectedPointOnLine(lineInOtherReg, centerOther);

            double dist1 = GraphicsUtility.DistanceBetweenPoints(centerPoly, projectedPtOnPolyReg);
            double dist2 = GraphicsUtility.DistanceBetweenPoints(centerOther, projectedPtOnOtherReg);

            double totalDistance = 2*(dist1 + dist2);
            lineInPolyReg.move(centerPoly);
            Point2d projectedPt = GraphicsUtility.ProjectedPointOnLine(lineInPolyReg, centerOther);
            double distance = GraphicsUtility.DistanceBetweenPoints(projectedPt, centerOther);


            if (totalDistance - eps < distance && distance < totalDistance + eps)
            {
                return lineInOtherReg;
            }
            else
            {
                return null;
            }
            
        }



        internal static Line CheckLineMove(Line testLine, Point2d movePt)
        {
            Point2d pt1 = new Point2d(testLine.StartPoint.X, testLine.StartPoint.Y);
            Point2d pt2 = new Point2d(testLine.EndPoint.X, testLine.EndPoint.Y);
            Line2d line = new Line2d(pt1, pt2);
            line.move(movePt);
            Point ptA = Point.ByCoordinates(line.StartPoint.X, line.StartPoint.Y);
            Point ptB = Point.ByCoordinates(line.EndPoint.X, line.EndPoint.Y);
            Line movedLine = Line.ByStartPointEndPoint(ptA, ptB);
            return movedLine;
        }

       

        //make a tree to test
        [MultiReturn(new[] { "SpaceTree", "NodeList" })]
        public static Dictionary<string,object> CreateSpaceTree(int numNodes, Point origin, double spaceX, double spaceY,double radius, double recompute = 5)
        {
            // make root node
            Node root = new Node(0, NodeType.Container, true, origin,radius);            
            List<Node> nodeList = new List<Node>();
            //nodeList.Add(root);
            Random ran = new Random();
            bool tag = true;
            for (int i = 0; i < numNodes-1; i++)
            {
                Node N;
                double val = ran.NextDouble();
                //NodeType ndType = BasicUtility.GenerateNodeType(val);
                NodeType ndType = BasicUtility.GenerateBalancedNodeType(tag);
                tag = !tag;
                N = new Node(i + 1, ndType);                        
                nodeList.Add(N);
            }
            //////////////////////////////////////////////////////////////////
            SpaceDataTree tree = new SpaceDataTree(root,origin,spaceX,spaceY);
            Node current = root;
            
            Node nodeAdditionResult = null;
            for (int i = 0; i < nodeList.Count; i++)
            {
              
                if (current.NodeType == NodeType.Space)
                {
                    Trace.WriteLine("Make Sure Space Nodes are childless");
                    //current = current.ParentNode.RightNode;
                    //current = current.RightNode;
                    current = current.ParentNode;
                  
                }


                nodeAdditionResult = tree.addNewNodeSide(current, nodeList[i]);
                string foo = "";
                //nodeAdditionResult = null , means node properly added
                //nodeAdditionResult = current, means, parent node of current is null
                //nodeAdditionResult = some other node means, current should be that other node to add new node
                if (nodeAdditionResult == current)
                {
                    Trace.WriteLine("Parent Node is found Null");
                    break;

                }else if (nodeAdditionResult != current && nodeAdditionResult != null)
                {
                    Trace.WriteLine("Current Should be that other Node");
                    current = nodeAdditionResult;
                }
                else
                {
                    Trace.WriteLine("Node is added properly Yay");
                    current = nodeList[i];
                  
                }
                Trace.WriteLine("+++++++++++++++++++++++++++++++++++++ \\");
                string foo1 = "";
      
    
            }
            Trace.WriteLine("Tree Constructed=====================");
            //return tree;

            return new Dictionary<string, object>
            {
                { "SpaceTree", (tree) },
                { "NodeList", (nodeList) }
            };

        }


        //make a tree to test
        [MultiReturn(new[] { "SpaceTree", "NodeList" })]
        internal static Dictionary<string, object> CreateSpaceTreeFromDeptData(Node root, List<Node> nodeList,
            Point origin, double spaceX, double spaceY, double radius, bool symettry = true)
        {
            // make root node
            //Node root = new Node(0, NodeType.Container, true, origin, radius);
            SpaceDataTree tree = new SpaceDataTree(root, origin, spaceX, spaceY);
            Node current = root;
            Node nodeAdditionResult = null;
            for (int i = 0; i < nodeList.Count; i++)
            {
                if (current.NodeType == NodeType.Space)
                {
                    Trace.WriteLine("Make Sure Space Nodes are childless");
                    current = current.ParentNode;
                }
                nodeAdditionResult = tree.addNewNodeSide(current, nodeList[i]);
                //nodeAdditionResult = null , means node properly added
                //nodeAdditionResult = current, means, parent node of current is null
                //nodeAdditionResult = some other node means, current should be that other node to add new node
                if (nodeAdditionResult == current)
                {
                    Trace.WriteLine("Parent Node is found Null");
                    break;

                }
                else if (nodeAdditionResult != current && nodeAdditionResult != null)
                {
                    Trace.WriteLine("Current Should be that other Node");
                    current = nodeAdditionResult;
                }
                else
                {
                    Trace.WriteLine("Node is added properly Yay");
                    current = nodeList[i];

                }
                Trace.WriteLine("+++++++++++++++++++++++++++++++++++++ \\");

            }
            Trace.WriteLine("Tree Constructed=====================");
            return new Dictionary<string, object>
            {
                { "SpaceTree", (tree) },
                { "NodeList", (nodeList) }
            };

        }

        internal static List<Polygon2d> BasicSplitWrapper(Polygon2d currentPoly, double ratio, int dir)
        {

            Dictionary<string, object> splitReturned = null;
            List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
            try
            {
                splitReturned = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
            }
            catch (Exception)
            {
                //toggle dir between 0 and 1
                dir = BasicUtility.toggleInputInt(dir);
                splitReturned = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                if (splitReturned == null)
                {
                    //Trace.WriteLine("Split Polys did not work");
                    return null;
                }
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
                //throw;
            }

            return polyAfterSplitting;
        }


        internal static List<Polygon2d> EdgeSplitWrapper(Polygon2d currentPoly,Random ran, double distance, int dir, double dummy =0)
        {

            Dictionary<string, object> splitReturned = null;
            List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
            try
            {
                splitReturned = SplitByDistance(currentPoly, ran, distance, dir, dummy);
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
            }
            catch (Exception)
            {
                //toggle dir between 0 and 1
                dir = BasicUtility.toggleInputInt(dir);
                splitReturned = SplitByDistance(currentPoly, ran, distance, dir,dummy);
                if (splitReturned == null)
                {
                    //Trace.WriteLine("Split Polys did not work");
                    return null;
                }
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];

                if(polyAfterSplitting[0] == null && polyAfterSplitting[1] == null)
                {
                    return null;
                }
                //throw;
            }

            

            return polyAfterSplitting;
        }

        
        /*
        public void codeToMakeTree(Polygon2d polyOutline)
        {
            Node root = new Node(0, NodeType.Container, false);
            SpaceDataTree deptTree = new SpaceDataTree(root,null);
            Node newEntry1 = new Node(1, NodeType.Container);
            Node newEntry2 = new Node(1, NodeType.Container);
            root.RightNode = newEntry1;
            root.LeftNode = newEntry2;
            //deptTree.addNode(root, newEntry);

        } 
        */



        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "DepartmentNames", "UpdatedDeptData" })]
        internal static Dictionary<string, object> DeptSplitRefined(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {

            /*
            get the poly
            get the poly area
            maintain a leftoverstack
            if dept is inpatients
                split by distance from edge ( distance = 32 )
            else
                split by basicsplitwrapper
            */

            List<List<Polygon2d>> AllDeptPolys = new List<List<Polygon2d>>();
            List<string> AllDepartmentNames = new List<string>();
            List<double> AllDeptAreaAdded = new List<double>();
            Stack<Polygon2d> leftOverPoly = new Stack<Polygon2d>();


            SortedDictionary<double, DeptData> sortedD = new SortedDictionary<double, DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                double area = deptData[i].AreaEachDept();
                DeptData deptD = deptData[i];
                sortedD.Add(area, deptD);

            }



            List<DeptData> sortedDepartmentData = new List<DeptData>();
            foreach (KeyValuePair<double, DeptData> p in sortedD)
            {
                DeptData deptItem = p.Value;
                sortedDepartmentData.Add(deptItem);
            }

            //SORT THE DEPT BASED ON THE AREA
            sortedDepartmentData.Reverse();
            leftOverPoly.Push(poly);
            int dir = 0;
            int maxRound = 1000;
            double count3 = 0;

            for (int i = 0; i < sortedD.Count; i++)
            {

                DeptData deptItem = sortedDepartmentData[i];
                double areaDeptNeeds = deptItem.DeptAreaNeeded;
                double areaAddedToDept = 0;
                double areaLeftOverToAdd = areaDeptNeeds - areaAddedToDept;
                double areaCurrentPoly = 0;
                double perc = 0.2;
                double limit = areaDeptNeeds * perc;

                Polygon2d currentPolyObj = poly;
                List<Polygon2d> everyDeptPoly = new List<Polygon2d>();
                List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
                double count1 = 0;
                double count2 = 0;
                double areaCheck = 0;



                //areaCurrentPoly = Polygon2d.AreaCheckPolygon(currentPolyObj);

                Random ran = new Random();
                // when inpatient--------------------------------------------------------------------------
                if (i == 0)
                {
                    while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0 && count1 < maxRound)
                    {
                        dir = BasicUtility.toggleInputInt(dir);
                        currentPolyObj = leftOverPoly.Pop();
                        areaCurrentPoly = Polygon2d.AreaCheckPolygon(currentPolyObj);
                        List<Polygon2d> edgeSplitted = EdgeSplitWrapper(currentPolyObj, ran, offset, dir, 0.75); //////////////////////

                        if (edgeSplitted == null)
                        {
                            return null;
                            //Trace.WriteLine("Found Null");
                            int countTry = 0;
                            Random ran1 = new Random();
                            while (edgeSplitted == null && countTry < 4)
                            {

                                dir = BasicUtility.toggleInputInt(dir);
                                double percentage = BasicUtility.RandomBetweenNumbers(ran1, 0.75, 0.25);
                                double offsetNew = offset * percentage;

                                edgeSplitted = EdgeSplitWrapper(currentPolyObj, ran, offsetNew, dir, percentage);
                                //Trace.WriteLine("Trying to Split By Edge for :  " + countTry);
                                //Trace.WriteLine("Direction is :  " + dir + " | Offset is : " + offsetNew +
                                    //" | Outer While Iteration is : " + count1);
                                countTry += 1;
                            }

                            //continue;
                        }
                        double areaA = Polygon2d.AreaCheckPolygon(edgeSplitted[0]);
                        double areaB = Polygon2d.AreaCheckPolygon(edgeSplitted[1]);
                        if (areaA < areaB)
                        {
                            everyDeptPoly.Add(edgeSplitted[0]);
                            areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                            areaCheck += areaA;
                            leftOverPoly.Push(edgeSplitted[1]);
                        }
                        else
                        {
                            everyDeptPoly.Add(edgeSplitted[1]);
                            areaLeftOverToAdd = areaLeftOverToAdd - areaB;
                            areaCheck += areaB;
                            leftOverPoly.Push(edgeSplitted[0]);
                        }
                        count1 += 0;
                    }
                }
                //when other depts------------------------------------------------------------------------
                else
                {
                    Random rn = new Random();
                    while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0 && count2 < maxRound)
                    {
                        dir = BasicUtility.toggleInputInt(dir);
                        //double ratio = rn.NextDouble() * (0.85 - 0.15) + 0.15;
                        double ratio = BasicUtility.RandomBetweenNumbers(rn, 0.85, 0.15);
                        currentPolyObj = leftOverPoly.Pop();
                        areaCurrentPoly = Polygon2d.AreaCheckPolygon(currentPolyObj);
                        dir = BasicUtility.toggleInputInt(dir);
                        //dir = BasicUtility.RandomToggleInputInt();
                        //Trace.WriteLine("Area left over is : " + areaLeftOverToAdd);
                        if (areaLeftOverToAdd > areaCurrentPoly)
                        {
                            everyDeptPoly.Add(currentPolyObj);
                            areaLeftOverToAdd = areaLeftOverToAdd - areaCurrentPoly;
                            areaCheck += areaCurrentPoly;
                            //Trace.WriteLine("Area left over after assigning when area is greater than current : " + areaLeftOverToAdd);

                        }
                        else
                        {

                            Dictionary<string, object> basicSplit = BasicSplitPolyIntoTwo(currentPolyObj, ratio, dir); ///////////////////////////////
                            if(basicSplit == null)
                            {
                                return null;
                            }
                            List<Polygon2d> polyS = (List<Polygon2d>)basicSplit["PolyAfterSplit"];
                            double areaA = Polygon2d.AreaCheckPolygon(polyS[0]);
                            double areaB = Polygon2d.AreaCheckPolygon(polyS[1]);

                            if (areaA < areaB)
                            {
                                everyDeptPoly.Add(polyS[0]);
                                areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                                areaCheck += areaA;
                                leftOverPoly.Push(polyS[1]);
                            }
                            else
                            {
                                everyDeptPoly.Add(polyS[1]);
                                areaLeftOverToAdd = areaLeftOverToAdd - areaB;
                                areaCheck += areaB;
                                leftOverPoly.Push(polyS[0]);
                            }


                        }

                        //Trace.WriteLine("Poly After Splitting Length is : " + polyAfterSplitting.Count);

                        //Trace.WriteLine("\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\");
                        count2 += 1;
                    } // end of while loop
                }

                AllDeptAreaAdded.Add(areaCheck);
                AllDeptPolys.Add(everyDeptPoly);
                AllDepartmentNames.Add(deptItem.DepartmentName);

            }// end of for loop

            Random ran2 = new Random();
            if (recompute > 2)
            {
                //there is any left over poly
                double minArea = 10;
                double areaMoreCheck = 0;
                if (leftOverPoly.Count > 0)
                {

                    while (leftOverPoly.Count > 0 && count3 < maxRound)
                    {
                        dir = BasicUtility.toggleInputInt(dir);
                        Polygon2d currentPolyObj = leftOverPoly.Pop();
                        double areaCurrentPoly = Polygon2d.AreaCheckPolygon(currentPolyObj);
                        List<Polygon2d> edgeSplitted = EdgeSplitWrapper(currentPolyObj,ran2, offset, dir);
                        if (edgeSplitted == null)
                        {
                            return null;
                        }
                        double areaA = Polygon2d.AreaCheckPolygon(edgeSplitted[0]);
                        double areaB = Polygon2d.AreaCheckPolygon(edgeSplitted[1]);
                        if (areaA < areaB)
                        {
                            AllDeptPolys[0].Add(edgeSplitted[0]);
                            //areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                            areaMoreCheck += areaA;
                            if (areaB > minArea) { leftOverPoly.Push(edgeSplitted[1]); }

                        }
                        else
                        {
                            AllDeptPolys[0].Add(edgeSplitted[1]);
                            //areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                            areaMoreCheck += areaB;
                            if (areaA > minArea) { leftOverPoly.Push(edgeSplitted[0]); }
                        }
                        count3 += 1;
                    }// end of while loop



                }// end of if loop for leftover count
                AllDeptAreaAdded[0] += areaMoreCheck;
            }// end of if loop



            // adding the left over polys to the 2nd highest dept after inpatient
            if(leftOverPoly.Count > 0)
            {
                //Trace.WriteLine("There is still left over poly left :" + leftOverPoly.Count);
                double areaLeftOver = 0;
                //Trace.WriteLine("No of poly before :  " + AllDeptPolys[1].Count);
                for (int i = 0; i < leftOverPoly.Count; i++)
                {
                    Polygon2d pol = leftOverPoly.Pop();
                    areaLeftOver += GraphicsUtility.AreaPolygon2d(pol.Points);                   
                    AllDeptPolys[1].Add(pol);
                }
                AllDeptAreaAdded[1] += areaLeftOver;

                //Trace.WriteLine("Area from left over :  " + areaLeftOver);
                //AllDeptPolys[1].AddRange(leftOverPoly.ToList());
                //Trace.WriteLine("No of poly after :  " + AllDeptPolys[1].Count);
            }
            

            List<DeptData> UpdatedDeptData = new List<DeptData>();
            //make the new deptdata to output
            for (int i = 0; i < sortedDepartmentData.Count; i++)
            {

                DeptData newDeptData = new DeptData(sortedDepartmentData[i]);
                newDeptData.AreaProvided = AllDeptAreaAdded[i];
                newDeptData.PolyDeptAssigned = AllDeptPolys[i];
                UpdatedDeptData.Add(newDeptData);

            }




            List<Polygon2d> AllLeftOverPolys = new List<Polygon2d>();
            AllLeftOverPolys.AddRange(leftOverPoly);

            Trace.WriteLine("Dept Splitting Done ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            //return polyList;
            
            return new Dictionary<string, object>
            {
                { "DeptPolys", (AllDeptPolys) },
                { "LeftOverPolys", (AllLeftOverPolys) },
                { "DepartmentNames", (AllDepartmentNames) },
                { "UpdatedDeptData", (UpdatedDeptData) }
            };


        }



        //RECURSIVE SPLITS A POLY - USES EdgeSplitWrapper (spltbydistance) & BasicSplitPolyIntoTwo
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "DepartmentNames", "UpdatedDeptData" })]
        public static Dictionary<string, object> DeptArrangeOnSite(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {
            Dictionary<string, object> deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, 1);
            double count = 0;
            int maxCount = 10;
            Random rand = new Random();
            while(deptArrangement == null && count < maxCount)
            {
                Trace.WriteLine("Lets Go Again for : " + count);
                int reco = rand.Next();
                deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, reco);
                count += 1;
            }


            return deptArrangement;

        }

        ///++++++++++++++++++++++++++++++++
        //RECURSIVE SPLITS A POLY - USES EdgeSplitWrapper (spltbydistance) & BasicSplitPolyIntoTwo       
        public static Dictionary<string, object> DeptArrangeOnSiteSingleOut(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {
            Dictionary<string, object> deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, 1);
            double count = 0;
            int maxCount = 10;
            Random rand = new Random();
            while (deptArrangement == null && count < maxCount)
            {
                Trace.WriteLine("Lets Go Again for : " + count);
                int reco = rand.Next();
                deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, reco);
                count += 1;
            }


            return deptArrangement;

        }








        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, BASED ON A DIRECTION AND RATIO
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        internal static Dictionary<string, object> BasicSplitPolyIntoTwo(Polygon2d polyOutline, double ratio = 0.5, int dir = 0)
        {
            if(polyOutline == null)
            {
                //Trace.WriteLine("-----Basic Poly is Null found");
                return null;
            }
            if(polyOutline != null && polyOutline.Points == null)
            {
                //Trace.WriteLine("-----Basic Poly Points are Null found");
                return null;
            }

            double extents = 5000;
            double spacing = spacingSet;
            double minimumLength = 2;
            double minWidth = 10;
            // dir = 0 : horizontal split line
            // dir = 1 : vertical split line

            List<Point2d> polyOrig = polyOutline.Points;
            double eps = 0.1;
            //CHECKS
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //List<Point2d> poly = GraphicsUtility.AddPointsInBetween(polyOrig, 5);
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            // compute bounding box ( set of four points ) for the poly
            // find x Range, find y Range
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            //compute centroid
            Point2d polyCenter = Polygon2d.CentroidFromPoly(poly);
            //check aspect ratio
            double aspectRatio = 0;



            // check if width or length is enough to make split
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength)
            {
                return null;
            }


            //should check direction of split ( flip dir value )
            if (horizontalSpan > verticalSpan)
            {
                dir = 1;
                aspectRatio = horizontalSpan / verticalSpan;
            }
            else
            {
                dir = 0;
                aspectRatio = verticalSpan / horizontalSpan;
            }

            if (aspectRatio < 2)
            {
                //dir = BasicUtility.toggleInputInt(dir);
                //return null;
            }



            // adjust ratio
            if (ratio < 0.15)
            {
                ratio = ratio + eps;
            }
            if (ratio > 0.85)
            {
                ratio = ratio - eps;
            }

            if(horizontalSpan < minWidth || verticalSpan < minWidth)
            {
                ratio = 0.5;
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Line2d splitLine = new Line2d(polyCenter, extents, dir);


            //compute vertical or horizontal line via centroid
            double basic = 0.5;
            double shift = ratio - basic;

            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, shift * verticalSpan);
            }
            else
            {
                splitLine.move(shift * horizontalSpan, 0);
            }


            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            ////////////////////////////////////////////////////////////////////////////////////////////
            
            

            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                    //ptA.Add(poly[i]);
                }
                else
                {
                    pIndexB.Add(i);
                    //ptB.Add(poly[i]);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            //List<Point2d> sortedA = makePolyPointsStraight(poly, intersectedPoints, pIndexA);
            //List<Point2d> sortedB = makePolyPointsStraight(poly, intersectedPoints, pIndexB);
            List<Point2d> sortedA = DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = DoSortClockwise(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            polyA = new Polygon2d(twoSets[0], 0);
            polyB = new Polygon2d(twoSets[1], 0);


            List<Polygon2d> splittedPoly = new List<Polygon2d>();

            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);
            
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (twoSets) }
            };

        }







        //internal function for Recursive Split By Area to work
        internal static double DistanceEditBasedOnRatio(double distance, double areaPoly, double areaFound, double area, double setSpan,double areaDifference)
        {
            double distanceNew = 0;
            /*
            if (areaDifference < 0)
            {
                // need to split less - decrease distance
                double ratio = Math.Abs(areaDifference) / areaPoly;
                distance -= ratio * setSpan;
                Trace.WriteLine("Reducing Distance by : " + ratio * setSpan);
                //double ratio = areaFound / area;
                //distance -= distance*Math.Sqrt(ratio);


            }
            else
            {
                //need to split more -  increase distance
                double ratio = Math.Abs(areaDifference) / areaPoly;
                distance += ratio * setSpan;
                Trace.WriteLine("Increading Distance by : " + ratio * setSpan);
                //double ratio = areaFound / area;
                //distance += distance * Math.Sqrt(ratio);
            }
            */

            distanceNew = distance * (area / areaFound);
            //Trace.WriteLine("Ratio multiplied to distance is : " + (area / areaFound));
            //distanceNew = distance;
            return distanceNew;
        }

        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint" })]
        internal static Dictionary<string, object> RecursiveSplitByArea(Polygon2d poly, double area, int dir, int recompute = 1)
        {

            /*PSUEDO CODE:
            get poly's vertical and horizontal span
            based on that get the direction of split
            get polys area and compare with given area ( if area bigger than polyarea then return null )
            based on the proportion calc distance
            bigpoly 
                split that into two
                save distance
                check area of both
                get the smaller poly and compare with asked area
                if area more increase distance by that much
                if area less decrease distance by that much
                split the bigger poly again
                repeat                
            */



            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly.Points);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);
            double minimumLength = 200;
            double perc = 0.2;
            //set limit of 10%
            double limit = area*perc;

            // increase required area by 10%
            //area += area * perc/4;

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X; 
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            double setSpan = 1000000000000;
            //int dir = 0;
            if (horizontalSpan > verticalSpan)
            {
                //dir = 1;
                setSpan = horizontalSpan;

            }
            else
            {
                //dir = 0;
                setSpan = verticalSpan;

            }
            double prop = 0;
            double areaPoly = GraphicsUtility.AreaPolygon2d(poly.Points);
            double areaDifference = 200000;
            if (areaPoly < area)
            {
                return null;
            }
            else
            {
                prop = area / areaPoly;
            }

            List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
            double distance = prop * setSpan;
            Polygon2d currentPoly = poly;
            int count = 0;
            //Trace.WriteLine("Initial Distance set is : " + distance);
            //Trace.WriteLine("Set Span found is : " + setSpan);
            //Trace.WriteLine("Limit accepted is : " + limit);
            Random ran2 = new Random();
            while (Math.Abs(areaDifference) > limit && count < 300)
            {
                if (currentPoly.Points == null || distance > setSpan)
                {
                    //Trace.WriteLine("Breaking This---------------------------------");
                    break;
                }

                polyAfterSplitting = EdgeSplitWrapper(currentPoly,ran2, distance, dir);
                double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                

                areaDifference = area - area1;
                distance = DistanceEditBasedOnRatio(distance, areaPoly, area1,area, setSpan, areaDifference);
                //Trace.WriteLine("Updated Distance for 1 is : " + distance);
                //Trace.WriteLine("Area Difference found for 1 is : " + areaDifference);
                

                if (areaDifference < 0)
                {
                    //Trace.WriteLine("Reducing Distance");
                }






                //reduce number of points
                //currentPoly = new Polygon2d(currentPoly.Points);
                areaList.Add(distance);
                //Trace.WriteLine("Distance Now is : " + distance);
                //Trace.WriteLine("Iteration is : " + count);
                //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                count += 1;
            }

            polyList.AddRange(polyAfterSplitting);
            pointsList = null;
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) }
            };
        }

      


    //RECURSIVE SPLITS A POLY
    [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint","UpdatedProgramData" })]
    public static Dictionary<string, object> RecursiveSplitProgramsOneDirByDistance(List<Polygon2d> polyInputList, List<ProgramData> progData, double distance, int recompute = 1)
    {

            /*PSUEDO CODE:
            get poly's vertical and horizontal span
            based on that get the direction of split
            bigpoly 
                split that into two
                push the smaller one in a list
                take the bigger one 
                make it big poly
                repeat

            */
        List<Polygon2d> polyList = new List<Polygon2d>();
        List<double> areaList = new List<double>();
        List<Point2d> pointsList = new List<Point2d>();
        Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();

        for (int j = 0; j < progData.Count; j++)
        {
            programDataRetrieved.Push(progData[j]);
        }

        ////////////////////////////////////////////////////////////////////////////
        for (int i = 0; i < polyInputList.Count; i++)
        {

        Polygon2d poly = polyInputList[i]; 


        if (poly == null || poly.Points == null || poly.Points.Count == 0)
        {
            return null;
        }
        
        
        List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly.Points);
        Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);
        double minimumLength = 200;

        Point2d span = polyRange.Span;
        double horizontalSpan = span.X;
        double verticalSpan = span.Y;
        List<double> spans = new List<double>();
        spans.Add(horizontalSpan);
        spans.Add(verticalSpan);
        double setSpan = 1000000000000;
        int dir = 0;
        if (horizontalSpan > verticalSpan)
        {
            dir = 1;
            setSpan = horizontalSpan;

        }
        else
        {
            dir = 0;
            setSpan = verticalSpan;

        }


        Polygon2d currentPoly = poly;
        int count = 0;


        Random ran2 = new Random();
        while (setSpan > 0 && programDataRetrieved.Count>0)
        {
            ProgramData progItem = programDataRetrieved.Pop();
            List<Polygon2d> polyAfterSplitting = EdgeSplitWrapper(currentPoly, ran2, distance, dir);
            double selectedArea = 0;
            double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
            double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[1].Points);
            if (area1 > area2)
            {
                currentPoly = polyAfterSplitting[0];
                if (polyAfterSplitting[1] == null)
                {
                    break;
                }
                polyList.Add(polyAfterSplitting[1]);
                progItem.AreaProvided = area1;
                areaList.Add(area2);
                selectedArea = area2;


            }
            else
            {
                currentPoly = polyAfterSplitting[1];
                polyList.Add(polyAfterSplitting[0]);
                progItem.AreaProvided = area2;
                areaList.Add(area1);
                selectedArea = area1;

            }


            if (currentPoly.Points == null)
            {
                //Trace.WriteLine("Breaking This");
                break;
            }



            //reduce number of points
            //currentPoly = new Polygon2d(currentPoly.Points);

            setSpan -= distance;
            count += 1;
        }// end of while loop


            }// end of for loop
        List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
        for (int i = 0; i < progData.Count; i++)
        {
            ProgramData progItem = progData[i];
            ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count)
                {
                    progNew.PolyProgAssigned = polyList[i];
                }
                else
                {
                    progNew.PolyProgAssigned = null;
                }
                UpdatedProgramDataList.Add(progNew);
        }

            pointsList = null;
        //return polyList;
        return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) },
                { "UpdatedProgramData",(UpdatedProgramDataList) }
            };
    }

        //RECURSIVE SPLITS A POLY
        public static Dictionary<string, object> RecursiveSplitProgramsOneDirByDistanceSingleOut(List<Polygon2d> polyInputList, List<ProgramData> progData, double distance, int recompute = 1)
        {

            /*PSUEDO CODE:
            get poly's vertical and horizontal span
            based on that get the direction of split
            bigpoly 
                split that into two
                push the smaller one in a list
                take the bigger one 
                make it big poly
                repeat

            */
            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();

            for (int j = 0; j < progData.Count; j++)
            {
                programDataRetrieved.Push(progData[j]);
            }

            ////////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < polyInputList.Count; i++)
            {

                Polygon2d poly = polyInputList[i];


                if (poly == null || poly.Points == null || poly.Points.Count == 0)
                {
                    return null;
                }


                List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly.Points);
                Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);
                double minimumLength = 200;

                Point2d span = polyRange.Span;
                double horizontalSpan = span.X;
                double verticalSpan = span.Y;
                List<double> spans = new List<double>();
                spans.Add(horizontalSpan);
                spans.Add(verticalSpan);
                double setSpan = 1000000000000;
                int dir = 0;
                if (horizontalSpan > verticalSpan)
                {
                    dir = 1;
                    setSpan = horizontalSpan;

                }
                else
                {
                    dir = 0;
                    setSpan = verticalSpan;

                }


                Polygon2d currentPoly = poly;
                int count = 0;


                Random ran2 = new Random();
                while (setSpan > 0 && programDataRetrieved.Count > 0)
                {
                    ProgramData progItem = programDataRetrieved.Pop();
                    List<Polygon2d> polyAfterSplitting = EdgeSplitWrapper(currentPoly, ran2, distance, dir);
                    double selectedArea = 0;
                    double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                    double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[1].Points);
                    if (area1 > area2)
                    {
                        currentPoly = polyAfterSplitting[0];
                        if (polyAfterSplitting[1] == null)
                        {
                            break;
                        }
                        polyList.Add(polyAfterSplitting[1]);
                        progItem.AreaProvided = area1;
                        areaList.Add(area2);
                        selectedArea = area2;


                    }
                    else
                    {
                        currentPoly = polyAfterSplitting[1];
                        polyList.Add(polyAfterSplitting[0]);
                        progItem.AreaProvided = area2;
                        areaList.Add(area1);
                        selectedArea = area1;

                    }


                    if (currentPoly.Points == null)
                    {
                        //Trace.WriteLine("Breaking This");
                        break;
                    }



                    //reduce number of points
                    //currentPoly = new Polygon2d(currentPoly.Points);

                    setSpan -= distance;
                    count += 1;
                }// end of while loop


            }// end of for loop
            List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count)
                {
                    progNew.PolyProgAssigned = polyList[i];
                }
                else
                {
                    progNew.PolyProgAssigned = null;
                }
                UpdatedProgramDataList.Add(progNew);
            }

            pointsList = null;
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) },
                { "UpdatedProgramData",(UpdatedProgramDataList) }
            };
        }





        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint" })]
        public static Dictionary<string, object> RecursiveSplitOneDirByDistance(Polygon2d poly, double distance, int recompute = 1)
        {

            /*PSUEDO CODE:
            get poly's vertical and horizontal span
            based on that get the direction of split
            bigpoly 
                split that into two
                push the smaller one in a list
                take the bigger one 
                make it big poly
                repeat
                
            */

            if(poly == null || poly.Points ==null || poly.Points.Count ==0)
            {
                return null;
            }


            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly.Points);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);
            double minimumLength = 200;

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            double setSpan = 1000000000000;
            int dir = 0;
            if (horizontalSpan > verticalSpan)
            {
                dir = 1;
                setSpan = horizontalSpan;
        
            }
            else
            {
                dir = 0;
                setSpan = verticalSpan;
             
            }


            Polygon2d currentPoly = poly;
            int count = 0;


            Random ran2 = new Random();
            while (setSpan > 0)
            {
                
                List<Polygon2d> polyAfterSplitting = EdgeSplitWrapper(currentPoly,ran2, distance, dir);
                double selectedArea = 0;
                double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[1].Points);
                if (area1 > area2)
                {
                    currentPoly = polyAfterSplitting[0];
                    if (polyAfterSplitting[1] == null)
                    {
                       break;
                    }
                    polyList.Add(polyAfterSplitting[1]);
                    areaList.Add(area2);
                    selectedArea = area2;
                    

                }
                else
                {
                    currentPoly = polyAfterSplitting[1];
                    polyList.Add(polyAfterSplitting[0]);
                    areaList.Add(area1);
                    selectedArea = area1;
                   
                }


                if (currentPoly.Points == null)
                {
                    Trace.WriteLine("Breaking This");
                    break;
                }


               
                //reduce number of points
                //currentPoly = new Polygon2d(currentPoly.Points);
               
                setSpan -= distance;
                count += 1;
            }

            pointsList = null;
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) }
            };
        }




        internal static Dictionary<int, object> pointSelector(Random ran,List<Point2d> poly)
        {
            Dictionary<int, object> output = new Dictionary<int, object>();
 
            double num = ran.NextDouble();
            //Trace.WriteLine("Point Selector Random Found is : " + num);
            int highInd = GraphicsUtility.ReturnHighestPointFromListNew(poly);
            Point2d hiPt = poly[highInd];
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);
            Point2d lowPt = poly[lowInd];


            if (num < 0.5)
            {
                output[0] = lowPt;
                output[1] = 1;
            }
            else
            {
                output[0] = hiPt; //hiPt
                output[1] = -1; //lowPt
            }


            return output;
        }



        internal static List<double> PolySpanCheck(Polygon2d poly)
        {
            List<double> spanList = new List<double>();
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;

            //place longer span first
            if (horizontalSpan > verticalSpan)
            {
                spanList.Add(horizontalSpan);
                spanList.Add(verticalSpan);
            }
            else
            {
                spanList.Add(verticalSpan);
                spanList.Add(horizontalSpan);                
            }
            
            return spanList;
        }


        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, by a Given Line
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine" })]
        internal static Dictionary<string, object> SplitByLineMake(Polygon2d polyOutline, Line2d inputLine, double distance = 5)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0)
            {
                return null;
            }


            double spacing = spacingSet;

            List<Point2d> polyOrig = polyOutline.Points;

            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            Line2d splitLine = new Line2d(inputLine);
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(poly);
            bool checkSide = GraphicsUtility.CheckPointSide(splitLine, centerPoly);
            int orient = GraphicsUtility.CheckLineOrient(splitLine);
            if (orient == 0)
            {
                if (!checkSide)
                {
                    splitLine.move(0, -1 * distance);
                }
                else
                {
                    splitLine.move(0, 1 * distance);
                }
            }
            else
            {
                if (checkSide)
                {
                    splitLine.move(-1 * distance, 0);
                }
                else
                {
                    splitLine.move(1 * distance, 0);
                }

            }

            //splitLine.move(poly, distance);
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                }
                else
                {
                    pIndexB.Add(i);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            List<Point2d> sortedA = DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = DoSortClockwise(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            polyA = new Polygon2d(twoSets[0], 0);
            polyB = new Polygon2d(twoSets[1], 0);


            List<Polygon2d> splittedPoly = new List<Polygon2d>();

            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) }
            };

        }


        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, by a Given Line
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine"})]
        internal static Dictionary<string, object> SplitByLine(Polygon2d polyOutline, Line2d inputLine, double distance = 5)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0)
            {
                return null;
            }


            double spacing = spacingSet;
            
            List<Point2d> polyOrig = polyOutline.Points;
       
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            Line2d splitLine = new Line2d(inputLine);
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(poly);
            bool checkSide = GraphicsUtility.CheckPointSide(splitLine, centerPoly);
            int orient = GraphicsUtility.CheckLineOrient(splitLine);
            if (orient == 0)
            {
                if (checkSide)
                {
                    splitLine.move(0, -1 * distance);
                }
                else
                {
                    splitLine.move(0, 1 * distance);
                }
            }else
            {
                if (checkSide)
                {
                    splitLine.move(-1 * distance,0);
                }
                else
                {
                    splitLine.move(1 * distance, 0);
                }

            }
           
            //splitLine.move(poly, distance);
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                }
                else
                {
                    pIndexB.Add(i);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            List<Point2d> sortedA = DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = DoSortClockwise(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            polyA = new Polygon2d(twoSets[0], 0);
            polyB = new Polygon2d(twoSets[1], 0);


            List<Polygon2d> splittedPoly = new List<Polygon2d>();

            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) }
            };

        }


        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, BASED ON A DIRECTION AND DISTANCE
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        internal static Dictionary<string, object> SplitByDistance(Polygon2d polyOutline, Random ran, double distance = 10, int dir = 0, double dummy =0)
        {
            if(polyOutline ==null || polyOutline.Points ==null || polyOutline.Points.Count == 0)
            {
                return null;
            }
           
            double extents = 5000;
            double spacing = spacingSet;
            double minimumLength = 10;
            double minValue = 10;
            bool horizontalSplit = false;
            bool verticalSplit = true;
            // dir = 0 : horizontal split line
            // dir = 1 : vertical split line

            List<Point2d> polyOrig = polyOutline.Points;
            double eps = 0.1;
            //CHECKS
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //List<Point2d> poly = GraphicsUtility.AddPointsInBetween(polyOrig, 5);
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            if (poly == null || poly.Count == 0)
            {
                return null;
                if (dummy> 0.65)
                {
                    //Trace.WriteLine("Killing it as poly is null");
                    //return null;
                }  
                
            }
            // compute bounding box ( set of four points ) for the poly
            // find x Range, find y Range
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            //compute centroid
            //Point2d polyCenter = Polygon2d.CentroidFromPoly(poly);

            //compute lowest point
            // int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);
            //Point2d lowPt = poly[lowInd];
            //check aspect ratio
            double aspectRatio = 0;



            // check if width or length is enough to make split
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength)
            {
                //return null;
            }


            //should check direction of split ( flip dir value )
            if (horizontalSpan > verticalSpan)
            {
                //dir = 1;
                aspectRatio = horizontalSpan / verticalSpan;
            }
            else
            {
                //dir = 0;
                aspectRatio = verticalSpan / horizontalSpan;
            }

            if (aspectRatio > 2)
            {
                //return null;
            }

            //set split style
            if (dir == 0)
            {
                horizontalSplit = true;
            }
            else
            {
                verticalSplit = true;
            }



            // adjust distance if less than some value
            if (distance < minValue)
            {
                //distance = minValue;
            }
            // adjust distance if more than total length of split possible
            if (verticalSplit)
            {
                if (distance > verticalSpan)
                {
                    //distance = verticalSpan - minValue; //CHANGED
                }
            }

            if (horizontalSplit)
            {
                if (distance > horizontalSpan)
                {
                    //distance = horizontalSpan - minValue; //CHANGED
                }
            }


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Dictionary<int, object> obj = pointSelector(ran,poly);
            Point2d pt = (Point2d)obj[0];
            int orient = (int)obj[1];

            Line2d splitLine = new Line2d(pt, extents, dir);
            //compute vertical or horizontal line via centroid



            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, orient*distance);
            }
            else
            {
                splitLine.move(orient*distance, 0);
            }



            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            ////////////////////////////////////////////////////////////////////////////////////////////



            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                    //ptA.Add(poly[i]);
                }
                else
                {
                    pIndexB.Add(i);
                    //ptB.Add(poly[i]);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            //List<Point2d> sortedA = makePolyPointsStraight(poly, intersectedPoints, pIndexA);
            //List<Point2d> sortedB = makePolyPointsStraight(poly, intersectedPoints, pIndexB);
            List<Point2d> sortedA = DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = DoSortClockwise(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            polyA = new Polygon2d(twoSets[0], 0);
            polyB = new Polygon2d(twoSets[1], 0);


            List<Polygon2d> splittedPoly = new List<Polygon2d>();

            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);



            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (twoSets) }
            };

        }


        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, BASED ON A DIRECTION AND RATIO
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        internal static Dictionary<string, object> SplitFromEdgePolyIntoTwo(Polygon2d polyOutline, double distance = 10, int dir = 0)
        {
            double extents = 5000;
            double spacing = spacingSet;
            double minimumLength = 10;
            double minValue = 10;
            bool horizontalSplit = false;
            bool verticalSplit = true;
            // dir = 0 : horizontal split line
            // dir = 1 : vertical split line

            List<Point2d> polyOrig = polyOutline.Points;
            double eps = 0.1;
            //CHECKS
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //List<Point2d> poly = GraphicsUtility.AddPointsInBetween(polyOrig, 5);
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            // compute bounding box ( set of four points ) for the poly
            // find x Range, find y Range
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            //compute centroid
            //Point2d polyCenter = Polygon2d.CentroidFromPoly(poly);

            //compute lowest point
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);
            Point2d lowPt = poly[lowInd];
            //check aspect ratio
            double aspectRatio = 0;



            // check if width or length is enough to make split
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength)
            {
                return null;
            }


            //should check direction of split ( flip dir value )
            if (horizontalSpan > verticalSpan)
            {
                //dir = 1;
                aspectRatio = horizontalSpan / verticalSpan;
            }
            else
            {
                //dir = 0;
                aspectRatio = verticalSpan / horizontalSpan;
            }

            if (aspectRatio > 2)
            {
                //return null;
            }

            //set split style
            if(dir == 0)
            {
                horizontalSplit = true;
            }
            else
            {
                verticalSplit = true;
            }


            
            // adjust distance if less than some value
            if (distance < minValue)
            {
                distance = minValue;
            }
            // adjust distance if more than total length of split possible
            if (verticalSplit)
            {
                if(distance > verticalSpan)
                {
                    distance = verticalSpan - minValue;
                }
            }

            if (horizontalSplit)
            {
                if (distance > horizontalSpan)
                {
                    distance = horizontalSpan - minValue;
                }
            }


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Line2d splitLine = new Line2d(lowPt, extents, dir);
            //compute vertical or horizontal line via centroid

         

            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, distance);
            }
            else
            {
                splitLine.move(distance, 0);
            }



            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            ////////////////////////////////////////////////////////////////////////////////////////////



            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                    //ptA.Add(poly[i]);
                }
                else
                {
                    pIndexB.Add(i);
                    //ptB.Add(poly[i]);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            //List<Point2d> sortedA = makePolyPointsStraight(poly, intersectedPoints, pIndexA);
            //List<Point2d> sortedB = makePolyPointsStraight(poly, intersectedPoints, pIndexB);
            List<Point2d> sortedA = DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = DoSortClockwise(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            polyA = new Polygon2d(twoSets[0], 0);
            polyB = new Polygon2d(twoSets[1], 0);


            List<Polygon2d> splittedPoly = new List<Polygon2d>();

            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);



            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (twoSets) }
            };

        }



        //checker function - can be discarded later
        internal static List<Point2d> CheckLowest_HighestPoint(Polygon2d poly)
        {
            List<Point2d> returnPts = new List<Point2d>();
            List<Point2d> ptList = poly.Points;
            int highPtInd = GraphicsUtility.ReturnHighestPointFromListNew(ptList);
            int  lowPtInd = GraphicsUtility.ReturnLowestPointFromListNew(ptList);
            returnPts.Add(ptList[lowPtInd]);
            returnPts.Add(ptList[highPtInd]);

            return returnPts;

        }


        // not using now can be discarded
        internal static List<Point2d> organizePointToMakePoly(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {
            List<Point2d> sortedPoint = new List<Point2d>();
            List<Point2d> unsortedPt = new List<Point2d>();
            // make two unsorted point lists
            for (int i = 0; i < pIndex.Count; i++)
            {
                unsortedPt.Add(poly[pIndex[i]]);
            }
            unsortedPt.AddRange(intersectedPoints);
            //compute lowest and highest pts
            Point2d lowPt = unsortedPt[GraphicsUtility.ReturnLowestPointFromListNew(unsortedPt)];
            Point2d hiPt = unsortedPt[GraphicsUtility.ReturnHighestPointFromListNew(unsortedPt)];
            //form a line2d between them
            Line2d lineHiLo = new Line2d(lowPt, hiPt);

            //make left and right points based on the line
            List<Point2d> ptOnA = new List<Point2d>();
            List<Point2d> ptOnB = new List<Point2d>();
            for (int i = 0; i < unsortedPt.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(lineHiLo, unsortedPt[i]);
                if (check)
                {
                    //pIndexA.Add(i);
                    ptOnA.Add(unsortedPt[i]);
                }
                else
                {
                    //pIndexB.Add(i);
                    ptOnB.Add(unsortedPt[i]);
                }
            }

            //sort ptOnA and ptOnB individually
            List<Point2d> SortedPtA = GraphicsUtility.SortPointsByDistanceFromPoint(ptOnA,
                         lowPt);
            List<Point2d> SortedPtB = GraphicsUtility.SortPointsByDistanceFromPoint(ptOnB,
                         lowPt);
            SortedPtB.Reverse();
            //add the sorted ptOnA and ptOnB
            sortedPoint.AddRange(SortedPtA);
            sortedPoint.AddRange(SortedPtB);
            return sortedPoint;
        }

        //can be discarded cleans duplicate points and returns updated list
        internal static List<Point2d> CleanDuplicatePoint2d(List<Point2d> ptListUnclean)
        {
            List<Point2d> cleanList = new List<Point2d>();
            List<double> dummyList = new List<double>();
            List<bool> isDuplicate = new List<bool>();
            double eps = 1;
            bool duplicate = false;
           


            for (int i = 0; i < ptListUnclean.Count; i++)
            {
                duplicate = false;
                double count = 0;
                for (int j = 0; j < ptListUnclean.Count; j++)
                {
                    
                    if (j == i)
                    {
                        continue;
                    }

                    if(ptListUnclean[i].X - eps < ptListUnclean[j].X  && ptListUnclean[j].X < ptListUnclean[i].X + eps)
                    {
                        if (ptListUnclean[i].Y - eps < ptListUnclean[j].Y  && ptListUnclean[j].Y < ptListUnclean[i].Y + eps)
                        {
                            count += 1;
                        }
                    }

                    if(count > 1)
                    {
                        duplicate = true;
                        
                        //continue;
                    }


                }

                dummyList.Add(count);
                if (!duplicate)
                {
                    //cleanList.Add(ptListUnclean[i]);
                }


              
            }
            for (int i = 0; i < ptListUnclean.Count; i++)
            {
                //Trace.WriteLine("count here is : " + dummyList[i]);
                if (dummyList[i] < 2)
                {
                    cleanList.Add(ptListUnclean[i]);
                }

            }
            /*
            List<double> itemX = new List<double>();
            List<double> itemY = new List<double>();
            for (int i = 0; i < ptListUnclean.Count; i++)
            {
                itemX.Add(ptListUnclean[i].X);
                itemX.Add(ptListUnclean[i].Y);
            }

            var duplicateIndexesX = itemX.Select((item, index) => new { item, index })
                        .GroupBy(g => g.item)
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g.Skip(1), (g, item) => item.index);

            var duplicateIndexesY = itemY.Select((item, index) => new { item, index })
                        .GroupBy(g => g.item)
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g.Skip(1), (g, item) => item.index);


            itemX = (List<double>)itemX.Where((item, index) => (!duplicateIndexesX.Contains(index) && !duplicateIndexesY.Contains(index)));
            itemY = (List<double>)itemY.Where((item, index) => (!duplicateIndexesX.Contains(index) && !duplicateIndexesY.Contains(index)));

            for (int i = 0; i < itemX.Count; i++)
            {
                cleanList.Add(new Point2d(itemX[i], itemY[i]));
            }
            */
            return cleanList;
        }


        //cleans duplicate points and returns updated list
        internal static List<Point2d> CleanDuplicatePoint2dNew(List<Point2d> ptListUnclean)
        {
            List<Point2d> cleanList = new List<Point2d>();
            List<double> exprList = new List<double>();
            double a = 45, b = 65;
            for(int i = 0; i < ptListUnclean.Count; i++)
            {
                double expr = a * ptListUnclean[i].X + b * ptListUnclean[i].Y;
                exprList.Add(expr);
            }

            var dups = exprList.GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

            List<double> distinct = exprList.Distinct().ToList();
            for(int i = 0; i < distinct.Count; i++)
            {
                double dis = distinct[i];
                for(int j = 0; j < exprList.Count; j++)
                {
                    if(dis == exprList[j])
                    {
                        cleanList.Add(ptListUnclean[j]);
                        break;
                    }
                }
            }
            return cleanList;

        }
        //trying new way to sort points clockwise
        internal static List<Point2d> DoSortClockwise(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {
            
            if (intersectedPoints.Count > 2)
            {
                
                //Trace.WriteLine("Wow found  " + intersectedPoints.Count + " intersection points!!!!!!!!!!!!!!!");
                List<Point2d> cleanedPtList = CleanDuplicatePoint2dNew(intersectedPoints);
                //Trace.WriteLine("After Cleaning found  " + cleanedPtList.Count + " intersection points!!!!!!!!!!!!!!!");
                //intersectedPoints = GraphicsUtility.CleanDuplicatePoint2d(intersectedPoints);
                return null;
                
            }
            /*

            if (intersectedPoints.Count < 2)
            {
                Trace.WriteLine("Returning null as less than 1 points");
                //return null;
            }
            List<Point2d> pointList = new List<Point2d>();
            List<Point2d> mergedPoints = new List<Point2d>();
            
            for (int i = 0; i < pIndex.Count; i++)
            {
                mergedPoints.Add(poly[pIndex[i]]);
            }
            reference = GraphicsUtility.CentroidInPointLists(mergedPoints);
            mergedPoints.AddRange(intersectedPoints);

            //mergedPoints.Sort((a, b) => GraphicsUtility.Angle(a, reference).CompareTo(GraphicsUtility.Angle(b, reference)));
            //mergedPoints.Sort(new Comparison<Point2d>(GraphicsUtility.SortCornersClockwise));
            //return mergedPoints;
            */
            return makePolyPointsStraight(poly, intersectedPoints, pIndex);
        }

        internal static List<Point2d> makePolyPointsStraight(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {

            if (intersectedPoints.Count < 2)
            {
                return null;
            }

            //List<Point2d> intersectedPoints = GraphicsUtility.PointUniqueChecker(intersectedPointsUnclean);
            //Trace.WriteLine("Intersected Points Length are : " + intersectedPoints.Count);
            //bool isUnique = intersectedPoints.Distinct().Count() == intersectedPoints.Count();
            //Trace.WriteLine("Intersected Point List is unique ?   " + isUnique);
            List<Point2d> pt = new List<Point2d>();
            bool added = false;

            int a = 0;
            int b = 1;
            if (intersectedPoints.Count > 2)
            {
                //Trace.WriteLine("Intersected pnts are more than one " + intersectedPoints.Count);
                //a = 0;
                //b = intersectedPoints.Count - 1;
            }

            //Trace.WriteLine("Intersected Points Length are : " + intersectedPoints.Count);
            //Trace.WriteLine("a and b are : " + a + "  ,  " + b );
            //Trace.WriteLine("Index Point length is :  " + pIndex.Count);
            for (int i = 0; i < pIndex.Count - 1; i++)
            {
                pt.Add(poly[pIndex[i]]);
                //enter only when indices difference are more than one and intersected points are not added yet
                if (Math.Abs(pIndex[i] - pIndex[i + 1]) > 1 && added == false)
                {
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                        poly[pIndex[i]]);
                    pt.Add(intersNewList[a]);
                    pt.Add(intersNewList[b]);
                    //Trace.WriteLine("Added Intersect Before for PtA");
                    added = true;
                }

                if (i == (pIndex.Count - 2) && added == false)
                {
                    pt.Add(poly[pIndex[i + 1]]);
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                             poly[pIndex[i + 1]]);
                    pt.Add(intersNewList[a]);
                    pt.Add(intersNewList[b]);
                    added = true;
                }

                else if (i == (pIndex.Count - 2) && added == true)
                {
                    pt.Add(poly[pIndex[i + 1]]);
                }
            }
            //Trace.WriteLine("Point Returned Length : " + pt.Count);
            //Trace.WriteLine("I++++++++++++++++++++++++++++++++++++++++++");
            return pt;
        }


     



    }
}
