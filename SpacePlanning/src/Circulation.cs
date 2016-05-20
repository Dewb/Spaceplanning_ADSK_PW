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
        #region - Public Methods
        /// <summary>
        /// It arranges the dept on the site based on input from program document
        /// Returns the dept polygon2d and dept  .
        /// </summary>
        /// <param name="DeptData">DeptData Object</param>
        /// <param name="Poly">Polygon2d representing departments in the site</param>
        /// <param name="Limit">Distance allowed to be considered as a neighbor of a department</param>
        /// <returns name="ProgramDataObject">Program Data Object containing information from the embedded .csv file</param>
        /// <returns name="DepartmentTopologyList">List of list showing who are the neighboring departments for each department</param>
        /// <returns name="DepartmentTopologyList">List of list showing who are the neighboring departments for each department</param>
        /// <search>
        /// make data stack, embedded data
        /// </search>
        //Make Dept Topology Matrix , finds all the shared edges between dept polys, and based on that  makes dept neighbors
        [MultiReturn(new[] { "DeptTopologyList", "CirculationNetwork", "DeptAllPolygons", "NonRedundantCirculationNetwork" })]
        public static Dictionary<string, object> MakeDeptTopology(List<DeptData> deptData, Polygon2d poly, Polygon2d leftOverPoly = null, double limit = 0)
        {
            if (deptData == null || deptData.Count == 0 || !PolygonUtility.CheckPoly(poly))
            {
                Trace.WriteLine("Found dept or poly null, so returning");
                return null;
            }
            List<Polygon2d> polygonsAllDeptList = new List<Polygon2d>();
            List<DeptData> deptDataAllDeptList = new List<DeptData>();
            List<List<string>> deptNamesNeighbors = new List<List<string>>();
            List<List<Line2d>> lineCollection = new List<List<Line2d>>();            
            //make flattened list of all dept data and dept polys
            for (int i = 0; i < deptData.Count; i++)
            {
                List<Polygon2d> polyList = deptData[i].PolyDeptAssigned;
                for (int j = 0; j < polyList.Count; j++)
                {
                    polygonsAllDeptList.Add(polyList[j]);
                    deptDataAllDeptList.Add(deptData[i]);
                }
            }
            if (leftOverPoly != null)
            {
                Trace.WriteLine("leftover added");
                polygonsAllDeptList.Add(leftOverPoly);
            }
            else
            {
                Trace.WriteLine("leftover poly found null, so not added");
            }
            List<Line2d> networkLine = new List<Line2d>();
            for (int i = 0; i < polygonsAllDeptList.Count; i++)
            {
                Polygon2d polyA = polygonsAllDeptList[i];
                for(int j = i + 1; j < polygonsAllDeptList.Count; j++)
                {
                    Polygon2d polyB = polygonsAllDeptList[j];
                    Dictionary<string, object> checkNeighbor = PolygonUtility.FindPolyAdjacentEdge(polyA, polyB,limit);
                    if(checkNeighbor != null)
                    {
                        if ((bool)checkNeighbor["Neighbour"] == true)
                        {
                            networkLine.Add((Line2d)checkNeighbor["SharedEdge"]);
                        }
                    }       
                }
            }
            List<Line2d> cleanNetworkLines = GraphicsUtility.RemoveDuplicateLines(networkLine);
            cleanNetworkLines = GraphicsUtility.RemoveDuplicateslinesWithPoly(poly, cleanNetworkLines);
            List<List<string>> deptNeighborNames = new List<List<string>>();
            return new Dictionary<string, object>
            {
                { "DeptTopologyList", (deptNamesNeighbors) },
                { "CirculationNetwork", (networkLine) },
                { "DeptAllPolygons", (polygonsAllDeptList) },
                { "NonRedundantCirculationNetwork", (cleanNetworkLines) }

            };
        }

        //Make Circulation Polygons between depts
        [MultiReturn(new[] { "CirculationPolygons", "UpdatedDeptPolygons" })]
        public static Dictionary<string, object> MakeDeptCirculation(List<DeptData> deptData, List<List<Line2d>> lineList, double width = 8)
        {
            if (deptData == null || deptData.Count == 0 || lineList == null || lineList.Count == null) return null;
            List<Line2d> cleanLineList = GraphicsUtility.FlattenLine2dList(lineList);
            List<Polygon2d> allDeptPolyList = new List<Polygon2d>();
            List<Polygon2d> circulationPolyList = new List<Polygon2d>();
            List<Polygon2d> updatedDeptPolyList = new List<Polygon2d>();
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
                    Point2d midPt = LineUtility.LineMidPoint(splitter);
                    Point2d nudgedMidPt = LineUtility.NudgeLineMidPt(splitter, deptPoly, 0.5);
                    if (GraphicsUtility.PointInsidePolygonTest(deptPoly, nudgedMidPt))
                    {
                        Dictionary<string, object> splitResult = SplitObject.SplitByLine(deptPoly, splitter, width);
                        List<Polygon2d> polyAfterSplit = (List<Polygon2d>)(splitResult["PolyAfterSplit"]);
                        if (PolygonUtility.CheckPolyList(polyAfterSplit))
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
                        if (j < updatedDeptPolyList.Count) polyForDeptBranch.Add(updatedDeptPolyList[j]);
                }
                deptPolyBranchedInList.Add(polyForDeptBranch);
            }
            return new Dictionary<string, object>
            {
                { "CirculationPolygons", (circulationPolyList) },
                { "UpdatedDeptPolygons", (deptPolyBranchedInList) }

            };
        }
        
        //Make Dept Topology Matrix
        [MultiReturn(new[] { "ProgTopologyList", "ProgNeighborNameList", "ProgAllPolygons", "SharedEdge" })]
        public static Dictionary<string, object> MakeProgramTopology(Polygon2d polyOutline, List<Polygon2d> polyA = null,  List<Polygon2d> polyB = null, List<Polygon2d> polyC = null, List<Polygon2d> polyD = null)
        {
            if (!PolygonUtility.CheckPoly(polyOutline)) return null;
            List<Polygon2d> polygonsAllProgList = new List<Polygon2d>();
            List<DeptData> deptDataAllDeptList = new List<DeptData>();
            List<List<Line2d>> lineCollection = new List<List<Line2d>>();

            if (polyA != null) polygonsAllProgList.AddRange(polyA);
            if (polyB != null) polygonsAllProgList.AddRange(polyB);
            if (polyC != null) polygonsAllProgList.AddRange(polyC);
            if (polyD != null) polygonsAllProgList.AddRange(polyD);

            for(int i = 0; i < polygonsAllProgList.Count; i++) polygonsAllProgList[i] = new Polygon2d(polygonsAllProgList[i].Points);

            List<Line2d> networkLine = new List<Line2d>();
            for (int i = 0; i < polygonsAllProgList.Count; i++)
            {
                Polygon2d poly1 = polygonsAllProgList[i];
                for (int j = i + 1; j < polygonsAllProgList.Count; j++)
                {
                    Polygon2d poly2 = polygonsAllProgList[j];
                    Dictionary<string, object> checkNeighbor = PolygonUtility.FindPolyAdjacentEdge(poly1, poly2);
                    if ((bool)checkNeighbor["Neighbour"] == true) networkLine.Add((Line2d)checkNeighbor["SharedEdge"]);
                }
            }
            List<Line2d> cleanNetworkLines = GraphicsUtility.RemoveDuplicateLines(networkLine);
            cleanNetworkLines = GraphicsUtility.RemoveDuplicateslinesWithPoly(polyOutline, cleanNetworkLines);
            List<List<string>> deptNeighborNames = new List<List<string>>();

            List<Line2d> onlyOrthoLineList = new List<Line2d>();
            for (int i = 0; i < cleanNetworkLines.Count; i++)
            {
                bool checkOrtho = GraphicsUtility.IsLineOrthogonal(cleanNetworkLines[i]);
                if (checkOrtho == true) onlyOrthoLineList.Add(cleanNetworkLines[i]);
            }
            return new Dictionary<string, object>
            {
                { "ProgTopologyList", (null) },
                { "ProgNeighborNameList", (networkLine) },
                { "ProgAllPolygons", (polygonsAllProgList) },
                { "SharedEdge", (onlyOrthoLineList) }

            };
        }
        
        //Make Circulation Polygons between depts
        [MultiReturn(new[] { "CirculationPolygons", "UpdatedProgPolygons" })]
        public static Dictionary<string, object> MakeProgramCirculation(List<Polygon2d> polyProgList, List<Line2d> lineList, double width = 8, double allowedCircRatio = 3, double frequencyCorridor = 0.5)
        {
            if (!PolygonUtility.CheckPolyList(polyProgList)) return null;
            if (lineList == null || lineList.Count == 0) return null;
            List<Line2d> flatLineList = new List<Line2d>();
            List<bool> IsDuplicateList = new List<bool>();

            polyProgList = PolygonUtility.SmoothPolygon(polyProgList, 5);

            //flatten all the polys in each depts to make it one list
            List<Polygon2d> circulationPolyList = new List<Polygon2d>();
            List<Polygon2d> updatedProgPolyList = new List<Polygon2d>();
            List<int> deptIdList = new List<int>();
            double num = allowedCircRatio;
            List<double> areaProgPolyList = new List<double>();
            for (int i = 0; i < polyProgList.Count; i++) areaProgPolyList.Add(GraphicsUtility.AreaPolygon2d(polyProgList[i].Points));

            double maxArea = areaProgPolyList.Max();
            areaProgPolyList.Sort();
            int value = (int)(areaProgPolyList.Count / 3);
            double areaThresh = areaProgPolyList[value];
            Random ran = new Random();

            for (int i = 0; i < lineList.Count; i++)
            {
                Line2d splitter = lineList[i];
                double someNumber = ran.NextDouble();
                if (someNumber > frequencyCorridor) continue;
                for (int j = 0; j < polyProgList.Count; j++)
                {
                    Polygon2d progPoly = polyProgList[j];
                    double areaPoly = GraphicsUtility.AreaPolygon2d(progPoly.Points);

                    Point2d midPt = LineUtility.LineMidPoint(splitter);
                    Point2d nudgedMidPt = LineUtility.NudgeLineMidPt(splitter,progPoly, 0.5);
                    bool checkInside = GraphicsUtility.PointInsidePolygonTest(progPoly, nudgedMidPt);

                    if (checkInside)
                    {
                        Dictionary<string, object> splitResult = SplitObject.SplitByLine(progPoly, splitter, width);
                        List<Polygon2d> polyAfterSplit = (List<Polygon2d>)(splitResult["PolyAfterSplit"]);
                        if (PolygonUtility.CheckPolyList(polyAfterSplit))
                        {
                            double areaA = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                            double areaB = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                            if (areaA < areaB)
                            {
                                if (polyAfterSplit[0].Points != null)
                                {
                                    bool check = PolygonUtility.CheckPolyBBox(polyAfterSplit[0], num);
                                    if (check) circulationPolyList.Add(polyAfterSplit[0]);
                                }
                                updatedProgPolyList.Add(polyAfterSplit[1]);
                            }
                            else
                            {
                                if (polyAfterSplit[1].Points != null)
                                {
                                    bool check = PolygonUtility.CheckPolyBBox(polyAfterSplit[1], num);
                                    if (check) circulationPolyList.Add(polyAfterSplit[1]);
                                }
                                updatedProgPolyList.Add(polyAfterSplit[0]);
                            }



                        }// end of if loop checking polylist
                    } // end of check inside


                }// end of for loop j
            }// end of for loop i

            return new Dictionary<string, object>
            {
               
                { "CirculationPolygons", (circulationPolyList) },
                { "UpdatedProgPolygons", (updatedProgPolyList) }

            };
        }

        #endregion

        
    }
}
