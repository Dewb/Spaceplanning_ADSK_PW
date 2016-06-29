﻿using System;
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
    /// <summary>
    /// Class to compute and generate circulation networks in space plans.
    /// </summary>
    public static class Circulation
    {
        #region - Public Methods
        //Builds Dept Topology Matrix , finds all the shared edges between dept polys, and updates department polygon2d's.
        /// <summary>
        /// Builds the department topology matrix internally and finds circulation network lines between department polygon2d's. 
        /// </summary>
        /// <param name="deptData">DeptData Object</param>
        /// <param name="leftOverPoly">Polygon2d not assigned to any department.</param>
        /// <param name="limit">Maximum distance allowed to be considered as a neighbor of a department.</param>
        /// <returns name="CirculationNetwork">List of line2d geometry representing circulation network between department polygon2d's.</returns>
        /// <search>
        /// Department Circulation Network, Shared Edges between departments
        /// </search>
        [MultiReturn(new[] { "CirculationNetworkLines", "ExtraNetworkLines", "PolyKPU" })]
        public static Dictionary<string,object> FindDeptCirculationNetwork(List<DeptData> deptData, Polygon2d leftOverPoly = null, double limit = 0, bool noExternalWall = false)
        {
            if (deptData == null || deptData.Count == 0) return null;
            List<Polygon2d> polygonsAllDeptList = new List<Polygon2d>();
            List<DeptData> deptDataAllDeptList = new List<DeptData>();
            List<List<string>> deptNamesNeighbors = new List<List<string>>();
            List<List<Line2d>> lineCollection = new List<List<Line2d>>();            
            //make flattened list of all dept data and dept polys
            for (int i = 0; i < deptData.Count; i++)
            {
                List<Polygon2d> polyList = deptData[i].PolyAssignedToDept;
                if (!ValidateObject.CheckPolyList(polyList)) continue;
                for (int j = 0; j < polyList.Count; j++)
                {
                    polygonsAllDeptList.Add(polyList[j]);
                    deptDataAllDeptList.Add(deptData[i]);
                }
            }
            if (leftOverPoly != null) polygonsAllDeptList.Add(leftOverPoly);
            //else Trace.WriteLine("leftover poly found null, so not added");
            List<Line2d> networkLine = new List<Line2d>();
            for (int i = 0; i < polygonsAllDeptList.Count; i++)
            {
                Polygon2d polyA = polygonsAllDeptList[i];
                for(int j = i + 1; j < polygonsAllDeptList.Count; j++)
                {
                    Polygon2d polyB = polygonsAllDeptList[j];
                    Dictionary<string, object> checkNeighbor = PolygonUtility.FindPolyAdjacentEdge(polyA, polyB,limit);
                    if(checkNeighbor != null) if ((bool)checkNeighbor["Neighbour"] == true) networkLine.Add((Line2d)checkNeighbor["SharedEdge"]);     
                }
            }

            // if externalWalls not necessary
            List<Line2d> extraLines = new List<Line2d>();
            List<Polygon2d> polyKeyList = new List<Polygon2d>();
            if (noExternalWall)
            {
                polyKeyList= deptData[0].PolyAssignedToDept;
                for (int i = 0; i < polyKeyList.Count; i++)
                {
                    Polygon2d polyA = polyKeyList[i];
                    for (int j = i + 1; j < polyKeyList.Count; j++)
                    {
                        Polygon2d polyB = polyKeyList[j];
                        Dictionary<string, object> checkNeighbor = PolygonUtility.FindPolyAdjacentEdgeEdit(polyA, polyB, 0.05);
                        if (checkNeighbor != null) if ((bool)checkNeighbor["Neighbour"] == true)
                            {
                                networkLine.Add((Line2d)checkNeighbor["SharedEdge"]);
                                extraLines.Add((Line2d)checkNeighbor["SharedEdge"]);
                            }
                    }
                }
            }
            List<Line2d> cleanNetworkLines = LineUtility.RemoveDuplicateLines(networkLine);
            // extend the lines found
            for(int i = 0; i < cleanNetworkLines.Count; i++) cleanNetworkLines[i] = LineUtility.ExtendLine(cleanNetworkLines[i], 2000);

            //return cleanNetworkLines;
            return new Dictionary<string, object>
            {
                { "CirculationNetworkLines", (cleanNetworkLines) },
                { "ExtraNetworkLines", (extraLines) },
                { "PolyKPU", (polyKeyList) },

            };
        }

        //Make circulation Polygons2d's between departments
        /// <summary>
        /// Builds cirulation polygon2d's between departments.
        /// </summary>
        /// <param name="deptData">Dept Data object.</param>
        /// <param name="circulationNetwork">List of line2d's representing circulation network between departments.</param>
        /// <param name="circulationWidth">Width in metres for circulation corridors between departments.</param>
        /// <returns name="CirculationPolygons">Polygon2d's representing circulation areas between departments.</returns>
        /// <returns name="UpdatedDeptPolygons">Updated polygon2d's representing departments.</returns>
        /// <search>
        /// Department Circulation Network, Shared Edges between departments
        /// </search>
        [MultiReturn(new[] { "CirculationPolygons", "UpdatedDeptPolys" })]
        public static Dictionary<string, object> MakeDeptCirculationPolys(List<DeptData> deptData, List<List<Line2d>> circulationNetwork, double circulationWidth = 8)
        {
            if (deptData == null || deptData.Count == 0 || circulationNetwork == null || circulationNetwork.Count == 0) return null;
            List<Line2d> cleanLineList = LineUtility.FlattenLine2dList(circulationNetwork);
            List<Polygon2d> allDeptPolyList = new List<Polygon2d>();
            List<Polygon2d> circulationPolyList = new List<Polygon2d>();
            List<Polygon2d> updatedDeptPolyList = new List<Polygon2d>();
            List<int> deptIdList = new List<int>();
            for (int i = 0; i < deptData.Count; i++)
            {
                List<Polygon2d> deptPolyList = deptData[i].PolyAssignedToDept;
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
                        Dictionary<string, object> splitResult = SplitObject.SplitByLine(deptPoly, splitter, circulationWidth);
                        List<Polygon2d> polyAfterSplit = (List<Polygon2d>)(splitResult["PolyAfterSplit"]);
                        if (ValidateObject.CheckPolyList(polyAfterSplit))
                        {
                            double areaA = PolygonUtility.AreaPolygon(polyAfterSplit[0]);
                            double areaB = PolygonUtility.AreaPolygon(polyAfterSplit[1]);
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
                { "UpdatedDeptPolys", (deptPolyBranchedInList) }

            };
        }

        //Builds Prog Topology Matrix , finds all the shared edges between program polys, and updates program polygon2d's.
        /// <summary>
        /// Builds the program topology matrix internally and finds circulation network lines between program polygon2d's. 
        /// </summary>
        /// <param name="deptData">List of department data object.</param>
        /// <param name="buildingOutline">Polygon2d of building outline.</param>
        /// <param name="leftOverPoly">Polygon2d for programs of department A</param>
        /// <returns name="CirculationPolygons">Polygon2d's representing circulation areas between programs.</returns>
        /// <returns name="PolygonsForAllPrograms">All program element's polygon2d geometry in one list.</returns>
        [MultiReturn(new[] { "CirculationNetwork", "PolygonsForAllPrograms" })]
        public static Dictionary<string, object> FindProgCirculationNetwork(List<DeptData> deptData, Polygon2d buildingOutline, List<Polygon2d> leftOverPoly = null)
        {
            if (!ValidateObject.CheckPoly(buildingOutline)) return null;
            if (deptData == null) return null;
            List<Polygon2d> polygonsAllProgList = new List<Polygon2d>();
            List<DeptData> deptDataAllDeptList = new List<DeptData>();
            List<List<Line2d>> lineCollection = new List<List<Line2d>>();

            for(int i = 0; i < deptData.Count; i++)
            {
                if (i == 0) continue;
                polygonsAllProgList.AddRange(deptData[i].PolyAssignedToDept);
            }
            if (leftOverPoly != null) polygonsAllProgList.AddRange(leftOverPoly);
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
            List<Line2d> cleanNetworkLines = LineUtility.RemoveDuplicateLines(networkLine);
            cleanNetworkLines = GraphicsUtility.RemoveDuplicateslinesWithPoly(buildingOutline, cleanNetworkLines);
            List<List<string>> deptNeighborNames = new List<List<string>>();

            List<Line2d> onlyOrthoLineList = new List<Line2d>();
            for (int i = 0; i < cleanNetworkLines.Count; i++)
            {
                bool checkOrtho = ValidateObject.CheckLineOrthogonal(cleanNetworkLines[i]);
                if (checkOrtho == true) onlyOrthoLineList.Add(cleanNetworkLines[i]);
            }
            return new Dictionary<string, object>
            {
                { "CirculationNetwork", (onlyOrthoLineList) },
                { "PolygonsForAllPrograms", (polygonsAllProgList) }
               
            };
        }

        //Make circulation Polygons2d's between programs
        /// <summary>
        /// Builds circulation polygon2d's between programs.
        /// </summary>
        /// <param name="polyProgList">Polygon2d's of all programs in every department and of any left over space.</param>
        /// <param name="circulationNetwork">List of line2d's representing circulation network between programs.</param>
        /// <param name="circulationWidth">Width in metres for circulation corridors between programs.</param>
        /// <param name="allowedCircRatio">Allowed aspect ratio of generated circulation polygon2d's.</param>
        /// <param name="frequencyCorridor">Allowed frequncy of circulation spaces. Higher value allows more spaces for circulation network.</param>
        /// <returns></returns>
        [MultiReturn(new[] { "CirculationPolygons", "UpdatedProgPolygons" })]
        public static Dictionary<string, object> MakeProgCirculationPolys(List<Line2d> circulationNetwork, List<Polygon2d> polyProgList, double circulationWidth = 8, double allowedCircRatio = 3, double frequencyCorridor = 0.5)
        {
            if (!ValidateObject.CheckPolyList(polyProgList)) return null;
            if (circulationNetwork == null || circulationNetwork.Count == 0) return null;
            List<Line2d> flatLineList = new List<Line2d>();
            List<bool> IsDuplicateList = new List<bool>();

            polyProgList = PolygonUtility.SmoothPolygonList(polyProgList, 5);

            //flatten all the polys in each depts to make it one list
            List<Polygon2d> circulationPolyList = new List<Polygon2d>();
            List<Polygon2d> updatedProgPolyList = new List<Polygon2d>();
            List<int> deptIdList = new List<int>();
            double num = allowedCircRatio;
            List<double> areaProgPolyList = new List<double>();
            for (int i = 0; i < polyProgList.Count; i++) areaProgPolyList.Add(PolygonUtility.AreaPolygon(polyProgList[i]));

            double maxArea = areaProgPolyList.Max();
            areaProgPolyList.Sort();
            int value = (int)(areaProgPolyList.Count / 3);
            double areaThresh = areaProgPolyList[value];
            Random ran = new Random();

            for (int i = 0; i < circulationNetwork.Count; i++)
            {
                Line2d splitter = circulationNetwork[i];
                double someNumber = ran.NextDouble();
                if (someNumber > frequencyCorridor) continue;
                for (int j = 0; j < polyProgList.Count; j++)
                {
                    Polygon2d progPoly = polyProgList[j];
                    double areaPoly = PolygonUtility.AreaPolygon(progPoly);

                    Point2d midPt = LineUtility.LineMidPoint(splitter);
                    Point2d nudgedMidPt = LineUtility.NudgeLineMidPt(splitter,progPoly, 0.5);
                    bool checkInside = GraphicsUtility.PointInsidePolygonTest(progPoly, nudgedMidPt);

                    if (checkInside)
                    {
                        Dictionary<string, object> splitResult = SplitObject.SplitByLine(progPoly, splitter, circulationWidth);
                        List<Polygon2d> polyAfterSplit = (List<Polygon2d>)(splitResult["PolyAfterSplit"]);
                        if (ValidateObject.CheckPolyList(polyAfterSplit))
                        {
                            double areaA = PolygonUtility.AreaPolygon(polyAfterSplit[0]);
                            double areaB = PolygonUtility.AreaPolygon(polyAfterSplit[1]);
                            if (areaA < areaB)
                            {
                                if (polyAfterSplit[0].Points != null)
                                {
                                    bool check = ValidateObject.CheckPolyBBox(polyAfterSplit[0], num);
                                    if (check) circulationPolyList.Add(polyAfterSplit[0]);
                                }
                                updatedProgPolyList.Add(polyAfterSplit[1]);
                            }
                            else
                            {
                                if (polyAfterSplit[1].Points != null)
                                {
                                    bool check = ValidateObject.CheckPolyBBox(polyAfterSplit[1], num);
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
