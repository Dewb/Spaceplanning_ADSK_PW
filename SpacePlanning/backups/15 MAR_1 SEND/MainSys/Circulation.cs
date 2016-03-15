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
    public class Circulation
    {

        //find if two polys are adjacent, and if yes, then returns the common edge between them
        [MultiReturn(new[] { "Neighbour", "SharedEdge"})]
        public static Dictionary<string, object> FindPolyAdjacentEdge(Polygon2d polyA, Polygon2d polyB)
        {
            /*
                check line by line the adjacent line between two polys
                then join the adjacent line to make one line
                return the adjacent line
            */

            bool check = false;
            if (polyA == null || polyB == null)
            {
                return null;
            }
            Line2d joinedLine = null;
            bool isNeighbour = false;
            double eps = 200;
            Polygon2d polyAReg = new Polygon2d(polyA.Points);
            Polygon2d polyBReg = new Polygon2d(polyB.Points);


            for(int i = 0; i < polyAReg.Points.Count; i++)
            {
                int a = i+1;
                if (i == polyAReg.Points.Count - 1)
                {
                    a = 0;
                }
                Line2d lineA = new Line2d(polyAReg.Points[i], polyAReg.Points[a]);
                for(int j = 0; j < polyBReg.Points.Count; j++)
                {
                    int b = j + 1;
                    if (j == polyBReg.Points.Count - 1)
                    {
                        b = 0;
                    }
                    Line2d lineB = new Line2d(polyBReg.Points[j], polyBReg.Points[b]);
                    bool checkAdj = GraphicsUtility.LineAdjacencyCheck(lineA, lineB);
                    if (checkAdj)
                    {
                        joinedLine = GraphicsUtility.JoinCollinearLines(lineA, lineB);
                        isNeighbour = true;
                        break;
                    }


                }
            }

            //"Neighbour", "SharedEdgeA", "SharedEdgeB" 

            return new Dictionary<string, object>
            {
                { "Neighbour", (isNeighbour) },
                { "SharedEdge", (joinedLine) }
            };

        }


        //Make Dept Topology Matrix - Using Now
        [MultiReturn(new[] { "DeptTopologyList", "DeptNeighborNameList", "DeptAllPolygons", "SharedEdge" })]
        public static Dictionary<string, object> MakeDeptTopology(List<DeptData> deptData, Polygon2d poly)
        {
            List<Polygon2d> polygonsAllDeptList = new List<Polygon2d>();
            List<DeptData> deptDataAllDeptList = new List<DeptData>();
            List<List<string>> deptNamesNeighbors = new List<List<string>>();
            List<List<Line2d>> lineCollection = new List<List<Line2d>>();

            

            for (int i = 0; i < deptData.Count; i++)
            {
                List<Polygon2d> polyList = deptData[i].PolyDeptAssigned;
                for (int j = 0; j < polyList.Count; j++)
                {
                    polygonsAllDeptList.Add(polyList[j]);
                    deptDataAllDeptList.Add(deptData[i]);
                }
            }

            List<Line2d> networkLine = new List<Line2d>();
            for (int i = 0; i < polygonsAllDeptList.Count; i++)
            {
                Polygon2d polyA = polygonsAllDeptList[i];
                for(int j = i + 1; j < polygonsAllDeptList.Count; j++)
                {
                    Polygon2d polyB = polygonsAllDeptList[j];
                    Dictionary<string, object> checkNeighbor = FindPolyAdjacentEdge(polyA, polyB);

                    if ((bool)checkNeighbor["Neighbour"] == true)
                    {
                        //neighbors.Add(deptData[k].DepartmentName);
                        networkLine.Add((Line2d)checkNeighbor["SharedEdge"]);
                    }

                }
            }



            //remove duplicate lines from the found common lines
            List<Line2d> cleanNetworkLines = RemoveDuplicateLines(networkLine);
            //remove any lines falling on the border
            cleanNetworkLines = RemoveDuplicateslinesWithPoly(poly, cleanNetworkLines);
            List<List<string>> deptNeighborNames = new List<List<string>>();

            return new Dictionary<string, object>
            {
                { "DeptTopologyList", (deptNamesNeighbors) },
                { "DeptNeighborNameList", (networkLine) },
                { "DeptAllPolygons", (polygonsAllDeptList) },
                { "SharedEdge", (cleanNetworkLines) }

            };
        }



        //Make Circulation Polygons between depts
        [MultiReturn(new[] { "CirculationPolygons", "UpdatedDeptPolygons" })]
        public static Dictionary<string, object> MakeDeptCirculation(List<DeptData> deptData, List<List<Line2d>> lineList, double width = 8)
        {

            //flatten the list
            List<Line2d> flatLineList = new List<Line2d>();
            List<bool> IsDuplicateList = new List<bool>();
            List<Line2d> cleanLineList = GraphicsUtility.FlattenLine2dList(lineList);


            //flatten all the polys in each depts to make it one list
            List<Polygon2d> allDeptPolyList = new List<Polygon2d>();
            List<Polygon2d> circulationPolyList = new List<Polygon2d>();
            List<Polygon2d> updatedDeptPolyList = new List<Polygon2d>();
            List<Polygon2d> reforemedDeptPolyList = new List<Polygon2d>();
            List<int> deptIdList = new List<int>();
            for (int i = 0; i < deptData.Count; i++)
            {
                List<Polygon2d> deptPolyList = deptData[i].PolyDeptAssigned;

                for (int j = 0; j < deptPolyList.Count; j++)
                {
                    deptIdList.Add(i);
                    allDeptPolyList.Add(deptPolyList[j]);
                }
            }

            for (int i = 0; i < cleanLineList.Count; i++)
            {
                Line2d splitter = cleanLineList[i];
                for (int j = 0; j < allDeptPolyList.Count; j++)
                {
                    Polygon2d deptPoly = allDeptPolyList[j];
                    Point2d midPt = splitter.midPt();
                    Point2d nudgedMidPt = splitter.NudgeLineMidPt(deptPoly, 0.5);
                    bool checkInside = GraphicsUtility.PointInsidePolygonTest(deptPoly, nudgedMidPt);

                    if (checkInside)
                    {
                        Dictionary<string, object> splitResult = BuildLayout.SplitByLineMake(deptPoly, splitter, width);
                        List<Polygon2d> polyAfterSplit = (List<Polygon2d>)(splitResult["PolyAfterSplit"]);
                        if (polyAfterSplit != null)
                        {
                            double areaA = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                            double areaB = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                            if (areaA < areaB)
                            {
                                circulationPolyList.Add(polyAfterSplit[0]);
                                updatedDeptPolyList.Add(polyAfterSplit[1]);
                            }
                            else
                            {
                                circulationPolyList.Add(polyAfterSplit[1]);
                                updatedDeptPolyList.Add(polyAfterSplit[0]);
                            }

                        }

                    } // end of check inside


                }// end of for loop j
            }// end of for loop i


            List<List<Polygon2d>> deptPolyBranchedInList = new List<List<Polygon2d>>();

            List<int> distinctIdList = deptIdList.Distinct().ToList();
            for (int i = 0; i < distinctIdList.Count; i++)
            {
                List<Polygon2d> polyForDeptBranch = new List<Polygon2d>();
                for (int j = 0; j < deptIdList.Count; j++)
                {
                    if (deptIdList[j] == i)
                    {
                        if (j < updatedDeptPolyList.Count)
                        {
                            polyForDeptBranch.Add(updatedDeptPolyList[j]);
                        }

                    }
                }
                deptPolyBranchedInList.Add(polyForDeptBranch);
            }





            return new Dictionary<string, object>
            {
                { "CirculationPolygons", (circulationPolyList) },
                { "UpdatedDeptPolygons", (deptPolyBranchedInList) }

            };
        }



        // Removes the lines which are on the poly lines
        internal static List<Line2d> RemoveDuplicateslinesWithPoly(Polygon2d poly, List<Line2d> lineList)
        {
            List<Line2d> cleanLineList = new List<Line2d>();
            List<bool> duplicateList = new List<bool>();
            for(int i =0; i < lineList.Count; i++)
            {
                Line2d line = new Line2d(lineList[i].StartPoint, lineList[i].EndPoint);
                cleanLineList.Add(line);
                duplicateList.Add(false);
            }

            for(int i = 0; i < poly.Points.Count; i++)
            {
                int b = i + 1;
                if(i==poly.Points.Count - 1)
                {
                    b = 0;
                }
                Line2d lineA = new Line2d(poly.Points[i], poly.Points[b]);
                for(int j = 0; j < lineList.Count; j++)
                {
                    Line2d lineB = lineList[j];
                    bool checkAdj = GraphicsUtility.LineAdjacencyCheck(lineA, lineB);
                    if (checkAdj)
                    {
                        duplicateList[j] = true;
                        break;
                    }// end of if loop
                } // end of 2nd for loop

            
            }// end of 1st for loop

            int count = 0;
            for (int i = 0; i < duplicateList.Count; i++)
            {
                if (duplicateList[i] == true)
                {
                    cleanLineList.RemoveAt(i - count);
                    count += 1;
                }
                else
                {

                }
            }
            return cleanLineList;
        }

        // Removes duplicate lines in a list of line
        internal static List<Line2d> RemoveDuplicateLines(List<Line2d> networkLine)
        {
            List<Line2d> dummyLineList = new List<Line2d>();
            List<bool> duplicateList = new List<bool>();
            for(int i = 0; i < networkLine.Count; i++)
            {
                //Line2d line = new Line2d(networkLine[i]);
                Line2d line = new Line2d(networkLine[i].StartPoint, networkLine[i].EndPoint);
                dummyLineList.Add(line);
                duplicateList.Add(false);
            }
            List<Line2d> cleanLines = new List<Line2d>();
            for(int i = 0; i < networkLine.Count; i++)
            {
                Line2d lineA = networkLine[i];
                for (int j = i + 1; j < networkLine.Count; j++)
                {
                    Line2d lineB = networkLine[j];
                    bool checkDuplicacy = GraphicsUtility.LineAdjacencyCheck(lineA, lineB); ;
                    if (checkDuplicacy)
                    {
                        
                        double lenA = lineA.Length;
                        double lenB = lineB.Length;
                        if(lenA > lenB)
                        {
                            duplicateList[j] = true;
                            //cleanLines.Add(lineA);
                        }
                        else
                        {
                            duplicateList[i] = true;
                            //cleanLines.Add(lineB);
                        }
                    }
                }
            }

            int count = 0;
            for(int i = 0; i < duplicateList.Count; i++)
            {
                if(duplicateList[i] == true)
                {
                    dummyLineList.RemoveAt(i - count);
                    count += 1;
                }
                else
                {

                }
            }
            return dummyLineList;
        }


        
        //Make Circulation Polygons between depts
        [MultiReturn(new[] { "CirculationPolygons", "UpdatedProgPolygons" })]
        public static Dictionary<string, object> MakeProgramCirculation(List<Polygon2d> polyProgList, List<Line2d> lineList, double width = 8, double allowedCircRatio = 3, double frequencyCorridor = 0.5)
        {

            //flatten the list
            List<Line2d> flatLineList = new List<Line2d>();
            List<bool> IsDuplicateList = new List<bool>();




            //flatten all the polys in each depts to make it one list
            List<Polygon2d> circulationPolyList = new List<Polygon2d>();
            List<Polygon2d> updatedProgPolyList = new List<Polygon2d>();
            List<int> deptIdList = new List<int>();
            double num = allowedCircRatio;
            List<double> areaProgPolyList = new List<double>();
            for (int i = 0; i < polyProgList.Count; i++)
            {
                double area = GraphicsUtility.AreaPolygon2d(polyProgList[i].Points);
                areaProgPolyList.Add(area);
            }

            double maxArea = areaProgPolyList.Max();
            areaProgPolyList.Sort();
            int value = (int)(areaProgPolyList.Count / 3);
            double areaThresh = areaProgPolyList[value];
            Random ran = new Random();

            for (int i = 0; i < lineList.Count; i++)
            {
                Line2d splitter = lineList[i];
                double someNumber = ran.NextDouble();
                if (someNumber > frequencyCorridor)
                {
                    //Trace.WriteLine("Not doing it , lets continue");
                    continue;
                }
                for (int j = 0; j < polyProgList.Count; j++)
                {
                    Polygon2d progPoly = polyProgList[j];
                    double areaPoly = GraphicsUtility.AreaPolygon2d(progPoly.Points);
                    if (areaPoly < areaThresh)
                    {
                        //continue;
                    }

                    //List<Point2d> progSmoothPolyPts = Polygon2d.SmoothPolygon(progPoly.Points, 3);
                    //Polygon2d progSmoothPoly = Polygon2d.ByPoints(progSmoothPolyPts);
                    //progPoly = progSmoothPoly;

                    Point2d midPt = splitter.midPt();
                    Point2d nudgedMidPt = splitter.NudgeLineMidPt(progPoly, 0.5);
                    bool checkInside = GraphicsUtility.PointInsidePolygonTest(progPoly, nudgedMidPt);

                    if (checkInside)
                    {
                        Dictionary<string, object> splitResult = BuildLayout.SplitByLineMake(progPoly, splitter, width);
                        List<Polygon2d> polyAfterSplit = (List<Polygon2d>)(splitResult["PolyAfterSplit"]);
                        if (polyAfterSplit != null)
                        {
                            double areaA = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                            double areaB = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                            if (areaA < areaB)
                            {
                                if (polyAfterSplit[0].Points != null)
                                {
                                    bool check = CheckPolyBBox(polyAfterSplit[0], num);
                                    if (check)
                                    {
                                        circulationPolyList.Add(polyAfterSplit[0]);
                                    }
                                }
                                updatedProgPolyList.Add(polyAfterSplit[1]);
                            }
                            else
                            {
                                if (polyAfterSplit[1].Points != null)
                                {
                                    bool check = CheckPolyBBox(polyAfterSplit[1], num);
                                    if (check)
                                    {
                                        circulationPolyList.Add(polyAfterSplit[1]);
                                    }
                                }
                                updatedProgPolyList.Add(polyAfterSplit[0]);
                            }



                        }
                    } // end of check inside


                }// end of for loop j
            }// end of for loop i

            string foo = "";
            /*
            List<List<Polygon2d>> deptPolyBranchedInList = new List<List<Polygon2d>>();

            List<int> distinctIdList = deptIdList.Distinct().ToList();
            for (int i = 0; i < distinctIdList.Count; i++)
            {
                List<Polygon2d> polyForDeptBranch = new List<Polygon2d>();
                for (int j = 0; j < deptIdList.Count; j++)
                {
                    if (deptIdList[j] == i)
                    {
                        if (j < updatedDeptPolyList.Count)
                        {
                            polyForDeptBranch.Add(updatedDeptPolyList[j]);
                        }

                    }
                }
                deptPolyBranchedInList.Add(polyForDeptBranch);
            }
            */





            return new Dictionary<string, object>
            {
               
                { "CirculationPolygons", (circulationPolyList) },
                { "UpdatedProgPolygons", (updatedProgPolyList) }

            };
        }

        //checks the ratio of the dimension of a poly bbox of certain proportion or not
        internal static bool CheckPolyBBox(Polygon2d poly, double num = 3)
        {
            bool check = false;
            Range2d range = poly.BBox;
            double X = range.Xrange.Span;
            double Y = range.Yrange.Span;

            //Trace.WriteLine("X is : " + X);
            //Trace.WriteLine("Y is : " + Y);

            if(Y < X)
            {
                double div1 = X / Y;
                if (div1 > num)
                {
                    check = true;
                }
            }
            else
            {
                double div1 = Y / X;
                if (div1 > num)
                {
                    check = true;
                }

            }
            
        
          
            //Trace.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++");
            return check;
        }

        //Make Dept Topology Matrix - Using Now
        [MultiReturn(new[] { "ProgTopologyList", "ProgNeighborNameList", "ProgAllPolygons", "SharedEdge" })]
        public static Dictionary<string, object> MakeCirculationTopology(Polygon2d polyOutline, List<Polygon2d> polyA = null, List<Polygon2d> polyB = null, List<Polygon2d> polyC = null)
        {
            List<Polygon2d> polygonsAllProgList = new List<Polygon2d>();
            List<DeptData> deptDataAllDeptList = new List<DeptData>();
            List<List<Line2d>> lineCollection = new List<List<Line2d>>();

            if (polyA != null)
            {
                polygonsAllProgList.AddRange(polyA);
            }

            if (polyB != null)
            {
                polygonsAllProgList.AddRange(polyB);
            }

            if (polyC != null)
            {
                polygonsAllProgList.AddRange(polyC);
            }



            List<Line2d> networkLine = new List<Line2d>();
            for (int i = 0; i < polygonsAllProgList.Count; i++)
            {
                Polygon2d poly1 = polygonsAllProgList[i];
                for (int j = i + 1; j < polygonsAllProgList.Count; j++)
                {
                    Polygon2d poly2 = polygonsAllProgList[j];
                    Dictionary<string, object> checkNeighbor = FindPolyAdjacentEdge(poly1, poly2);

                    if ((bool)checkNeighbor["Neighbour"] == true)
                    {
                        networkLine.Add((Line2d)checkNeighbor["SharedEdge"]);
                    }

                }
            }



            //remove duplicate lines from the found common lines
            List<Line2d> cleanNetworkLines = RemoveDuplicateLines(networkLine);
            //remove any lines falling on the border
            cleanNetworkLines = RemoveDuplicateslinesWithPoly(polyOutline, cleanNetworkLines);
            List<List<string>> deptNeighborNames = new List<List<string>>();

            List<Line2d> onlyOrthoLineList = new List<Line2d>();
            for (int i = 0; i < cleanNetworkLines.Count; i++)
            {
                bool checkOrtho = GraphicsUtility.IsLineOrthogonalCheck(cleanNetworkLines[i]);
                if (checkOrtho == true)
                {
                    onlyOrthoLineList.Add(cleanNetworkLines[i]);
                }
            }
            return new Dictionary<string, object>
            {
                { "ProgTopologyList", (null) },
                { "ProgNeighborNameList", (networkLine) },
                { "ProgAllPolygons", (polygonsAllProgList) },
                { "SharedEdge", (onlyOrthoLineList) }

            };
        }



    }
}
