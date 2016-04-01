using System;
using System.Collections.Generic;
using stuffer;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace SpacePlanning
{
    internal class PlaceSpace
    {

        // assign dept objects on the site
        public static List<DeptData> AssignDeptsToSite(List<DeptData> deptData, List<Cell> cells, int recompute = 0)
        {
            //Build new dept data list
            List<DeptData> newDeptDataList = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData newDeptData = new DeptData(deptData[i]);
                newDeptDataList.Add(newDeptData);
            }
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }

            // get cellIndex for Program Centroids
            List<int> selectedIndices = GetDeptCentroid(newDeptDataList, ref cells, newDeptDataList[0].GridX);
            List<double> sqEdge = new List<double>();
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                double side = Math.Sqrt(newDeptDataList[i].DeptAreaNeeded);
                sqEdge.Add(side);
                Point2d center = cells[selectedIndices[i]].CenterPoint;
                Polygon2d squareCentroid = GraphicsUtility.MakeSquarePolygon2dFromCenterSide(center, side);
                for (int j = 0; j < cells.Count; j++)
                {
                    if (cells[j].CellAvailable)
                    {
                        Point2d testPoint = cells[j].CenterPoint;
                        if (GraphicsUtility.PointInsidePolygonTest(squareCentroid, testPoint))
                        {
                            cells[j].CellAvailable = false;
                            newDeptDataList[i].CalcAreaAllocated();
                            newDeptDataList[i].CellAssignPerItem(cells[j]);
                        }
                    }
                }
            }// end of for loop

            for (int i = 0; i < selectedIndices.Count; i++)
            {
                while (newDeptDataList[i].IsAreaSatisfied == false && AnyCellAvailable(cells) == true)
                {
                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].CellAvailable == true)
                        {
                            cells[j].CellAvailable = false;
                            newDeptDataList[i].CalcAreaAllocated();
                            newDeptDataList[i].CellAssignPerItem(cells[j]);
                            break;

                        }
                    }
                }
            }
            cells = null;
            deptData = null;
            sqEdge = null;
            selectedIndices = null;
            return newDeptDataList;

        }


        // assign program objects to each dept
        [MultiReturn(new[] { "DepartmentDataProgramsAdded", "ProgramDataUpdated" })]
        internal static Dictionary<string, object> AssignProgstoDepts(List<DeptData> deptData, int recompute)
        {
            List<DeptData> newDeptDataList = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData newDeptData = new DeptData(deptData[i]);
                newDeptDataList.Add(newDeptData);
            }
            List<ProgramData> progUpdated = new List<ProgramData>();
            for (int i = 0; i < newDeptDataList.Count; i++)
            {
                List<ProgramData> progList = newDeptDataList[i].ProgramsInDept;
                List<Cell> cellList = newDeptDataList[i].DepartmentCells;
                if (cellList.Count > 0)
                {
                    List<ProgramData> progD = AllocatePrograms(progList, cellList, 0);
                    newDeptDataList[i].ProgramsInDept = progD;
                }
                progUpdated.AddRange(newDeptDataList[i].ProgramsInDept);
                
            }
            deptData = null;
            return new Dictionary<string, object>
            {
                { "DepartmentDataProgramsAdded", (newDeptDataList) },
                { "ProgramDataUpdated", (progUpdated) }
            };

        }


        // assign programs to the cells provided by each dept
        internal static List<ProgramData> AllocatePrograms(List<ProgramData> progData, List<Cell> oldCells, int tag = 0)
        {

            List<Cell> cells = new List<Cell>();
            for (int i = 0; i < oldCells.Count; i++)
            {
                cells.Add(new Cell(oldCells[i]));
            }
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }
            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData newProgData = new ProgramData(progData[i]);
                newProgDataList.Add(newProgData);
            }
            List<int> selectedIndices = GetProgramCentroid(newProgDataList, ref cells, newProgDataList[0].GridX);
            List<double> sqEdge = new List<double>();


            for (int i = 0; i < selectedIndices.Count; i++)
            {
                double side = Math.Sqrt(newProgDataList[i].UnitArea * newProgDataList[i].Quantity);
                sqEdge.Add(side);
                Point2d center = cells[selectedIndices[i]].CenterPoint;
                Polygon2d squareCentroid = GraphicsUtility.MakeSquarePolygon2dFromCenterSide(center, side);
                for (int j = 0; j < cells.Count; j++)
                {
                    if (cells[j].CellAvailable)
                    {
                        Point2d testPoint = cells[j].CenterPoint;
                        if (GraphicsUtility.PointInsidePolygonTest(squareCentroid, testPoint))
                        {
                            cells[j].CellAvailable = false;
                            newProgDataList[i].CalcAreaAllocated();
                            newProgDataList[i].CellAssignPerItem(cells[j]);
                        }
                    }
                }   
            }// end of for loop
            
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                while (newProgDataList[i].IsAreaSatisfied == false && AnyCellAvailable(cells) == true)
                {

                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].CellAvailable == true)
                        {
                            cells[j].CellAvailable = false;
                            newProgDataList[i].CalcAreaAllocated();
                            newProgDataList[i].CellAssignPerItem(cells[j]);
                            break;

                        }
                    }
                }

            }
            oldCells.Clear();
            return newProgDataList;

        }

        //places the programs from prog data on the site
        [MultiReturn(new[] { "ProgramCellIndices", "ProgramData", "CellsForEachProg", "Cells" })]
        public static Dictionary<string, object> PlaceProgramsInSite(List<ProgramData> progData, List<Cell> cells, List<Point2d> PolyPoints, int num = 5, int tag = 0)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }
            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData newProgData = new ProgramData(progData[i]);
                newProgDataList.Add(newProgData);
            }
            List<int> selectedIndices = GetProgramCentroid(newProgDataList, ref cells, newProgDataList[0].GridX);
            List<double> sqEdge = new List<double>();
            List<List<int>> cellProgramsList = new List<List<int>>();


            for (int i = 0; i < selectedIndices.Count; i++)
            {
                double side = Math.Sqrt(newProgDataList[i].UnitArea * newProgDataList[i].Quantity);
                sqEdge.Add(side);
                Point2d center = cells[selectedIndices[i]].CenterPoint;
                Polygon2d squareCentroid = GraphicsUtility.MakeSquarePolygon2dFromCenterSide(center, side);
                List<int> cellIndexPrograms = new List<int>();
                int cellAddedCount = 0;
                for (int j = 0; j < cells.Count; j++)
                {
                    if (cells[j].CellAvailable)
                    {
                        Point2d testPoint = cells[j].CenterPoint;
                        if (GraphicsUtility.PointInsidePolygonTest(squareCentroid, testPoint))
                        {
                            cellIndexPrograms.Add(j);
                            cells[j].CellAvailable = false;
                            newProgDataList[i].CalcAreaAllocated();
                            cellAddedCount += 1;
                        }
                    }
                }   
                cellProgramsList.Add(cellIndexPrograms);



            }// end of for loop
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                while (newProgDataList[i].IsAreaSatisfied == false && AnyCellAvailable(cells) == true)
                {

                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].CellAvailable == true)
                        {
                            cellProgramsList[i].Add(j);
                            cells[j].CellAvailable = false;
                            newProgDataList[i].CalcAreaAllocated();
                            break;

                        }
                    }
                }
            }
            List<Cell> newCellList = new List<Cell>();
            for (int i = 0; i < cells.Count; i++)
            {
                Cell aCell = new Cell(cells[i]);
                newCellList.Add(aCell);
            }

            List<List<Cell>> cellEachProgGets = new List<List<Cell>>();
            for (int i = 0; i < newProgDataList.Count; i++)
            {
                List<Cell> eachProgCellList = new List<Cell>();
                if (cellProgramsList[i].Count > 0)
                {
                    for (int j = 0; j < cellProgramsList[i].Count; j++)
                    {
                        eachProgCellList.Add(newCellList[cellProgramsList[i][j]]);
                    }
                    newProgDataList[i].CellAssign(eachProgCellList);
                    cellEachProgGets.Add(eachProgCellList);
                }

            }
            return new Dictionary<string, object>
            {
                { "ProgramCellIndices", (cellProgramsList) },
                { "ProgramData", (newProgDataList) },
                { "CellsForEachProg", (cellEachProgGets) },
                { "Cells", (newCellList) }
            };

        }

        //places the departments from dept data on the site
        [MultiReturn(new[] { "DeptCellIndices", "DepartmentData", "CellsForEachDept", "Cells" })]
        internal static Dictionary<string, object> PlaceDeptsInSite(List<DeptData> deptData, List<Cell> cells, List<Point2d> PolyPoints, int num = 5, int tag = 0)
        {

            //Build new dept data list
            List<DeptData> newDeptDataList = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData newDeptData = new DeptData(deptData[i]);
                newDeptDataList.Add(newDeptData);
            }

            deptData.Clear();
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }
            List<int> selectedIndices = GetDeptCentroid(newDeptDataList, ref cells, newDeptDataList[0].GridX);
            List<double> sqEdge = new List<double>();
            List<List<int>> cellProgramsList = new List<List<int>>(); 

            for (int i = 0; i < selectedIndices.Count; i++)
            {
                double side = Math.Sqrt(newDeptDataList[i].DeptAreaNeeded);
                sqEdge.Add(side);
                Point2d center = cells[selectedIndices[i]].CenterPoint;
                Polygon2d squareCentroid = GraphicsUtility.MakeSquarePolygon2dFromCenterSide(center, side);
                List<int> cellIndexPrograms = new List<int>();
                int cellAddedCount = 0;
                for (int j = 0; j < cells.Count; j++)
                {
                    if (cells[j].CellAvailable)
                    {
                        Point2d testPoint = cells[j].CenterPoint;
                        if (GraphicsUtility.PointInsidePolygonTest(squareCentroid, testPoint))
                        {
                            cellIndexPrograms.Add(j);
                            cells[j].CellAvailable = false;
                            newDeptDataList[i].CalcAreaAllocated();
                            cellAddedCount += 1;
                        }
                    }
                }
                cellProgramsList.Add(cellIndexPrograms);
            }// end of for loop
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                while (newDeptDataList[i].IsAreaSatisfied == false && AnyCellAvailable(cells) == true)
                {

                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].CellAvailable == true)
                        {
                            cellProgramsList[i].Add(j);
                            cells[j].CellAvailable = false;
                            newDeptDataList[i].CalcAreaAllocated();
                            break;

                        }
                    }
                }
            }
            List<Cell> newCellList = new List<Cell>();
            for (int i = 0; i < cells.Count; i++)
            {
                Cell aCell = new Cell(cells[i]);
                newCellList.Add(aCell);
            }
            cells.Clear();

            List<List<Cell>> cellEachDeptGets = new List<List<Cell>>();
            for (int i = 0; i < newDeptDataList.Count; i++)
            {
                List<Cell> eachDeptCellList = new List<Cell>();
                if (cellProgramsList[i].Count > 0)
                {
                    for (int j = 0; j < cellProgramsList[i].Count; j++)
                    {
                        eachDeptCellList.Add(newCellList[cellProgramsList[i][j]]);
                    }
                    newDeptDataList[i].CellAssign(eachDeptCellList);
                    cellEachDeptGets.Add(eachDeptCellList);
                }

            }
            return new Dictionary<string, object>
            {
                { "DeptCellIndices", (cellProgramsList) },
                { "DepartmentData", (newDeptDataList) },
                { "CellsForEachDept", (cellEachDeptGets) },
                { "Cells", (newCellList) }
            };

        }

        //get the cells to place the program
        internal static int SelectCellToPlaceProgram(ref List<Cell> cells, Random random)
        {          

            int size = cells.Count;
            int randomIndex = -1;
            randomIndex = random.Next(0, size);
            int attempt = 0;
            while (cells[randomIndex].CellAvailable == false && attempt < 3)
            {
                randomIndex = random.Next(0, size);
                attempt += 1;
            }
            cells[randomIndex].CellAvailable = false;        
            return randomIndex;
            }

        //get the centroid point for a list of program
        internal static List<int> GetProgramCentroid(List<ProgramData> progData, ref List<Cell> cells,double dimX)
        {

            int selectedCell = -1;
            List<int> selectedIndices = new List<int>();
            Random random = new Random();
            for (int i = 0; i < progData.Count; i++)
            {
                selectedCell = SelectCellToPlaceProgram(ref cells,random);
                selectedIndices.Add(selectedCell);
               
            }
            return selectedIndices;
        }

        //get the centroid point for a list of dept
        internal static List<int> GetDeptCentroid(List<DeptData> deptData, ref List<Cell> cells,double dimX)
        {
            int selectedCell = -1;
            List<int> selectedIndices = new List<int>();
            Random random = new Random();
            for (int i = 0; i < deptData.Count; i++)
            {
                selectedCell = SelectCellToPlaceProgram(ref cells, random);
                selectedIndices.Add(selectedCell);
            }
            return selectedIndices;
        }

        //make polygons from cell objects
        [MultiReturn(new[] { "PolyCurves", "PointLists" })]
        public static Dictionary<string, object> MakePolygonsFromCells(List<List<Cell>> cellProgramsList)
        {
            List<PolyCurve> polyList = new List<PolyCurve>();
            List<List<Point2d>> ptAll = new List<List<Point2d>>();




            for (int i = 0; i < cellProgramsList.Count; i++)
            {
                List<Cell> cellList = cellProgramsList[i];
                List<Point2d> tempPts = new List<Point2d>();
                if (cellList.Count > 0)
                {
                    for (int j = 0; j < cellList.Count; j++)
                    {
                        tempPts.Add(cellList[j].CenterPoint);
                    }


                }
                ptAll.Add(tempPts);
                if (tempPts.Count > 2)
                {
                    List<Point2d> boundingPolyPts = GraphicsUtility.FromPointsGetBoundingPoly(tempPts);
                    PolyCurve poly = DynamoGeometry.PolyCurveFromPoints(boundingPolyPts);
                    polyList.Add(poly);
                }
                else
                {
                    polyList.Add(null);
                }
            }

            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyCurves", (polyList) },
                { "PointLists", (ptAll) }
            };


        }

        //make polygons for programs
        [MultiReturn(new[] { "PolyCurves", "PointLists" })]
        internal static Dictionary<string, object> MakeProgramPolygons(List<List<int>> cellProgramsList, List<Point2d> ptList)
        {
            List<PolyCurve> polyList = new List<PolyCurve>();
            List<List<Point2d>> ptAll = new List<List<Point2d>>();

            for (int i = 0; i < cellProgramsList.Count; i++)
            {
                List<int> cellList = cellProgramsList[i];
                List<Point2d> tempPts = new List<Point2d>();
                if (cellList.Count > 0)
                {
                    for(int j = 0; j < cellList.Count; j++)
                    {
                        tempPts.Add(ptList[cellList[j]]);
                    }

                    
                }
                ptAll.Add(tempPts);
                if (tempPts.Count > 2)
                {
                    
                //Trace.WriteLine("Temp Pt counts are : " + tempPts.Count);
                List<Point2d> boundingPolyPts = GraphicsUtility.FromPointsGetBoundingPoly(tempPts);
                PolyCurve poly = DynamoGeometry.PolyCurveFromPoints(boundingPolyPts);
                polyList.Add(poly);
                }
                else
                {
                polyList.Add(null);
                }
            }

            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyCurves", (polyList) },
                { "PointLists", (ptAll) }
            };


        }
        
        //make convex hulls from cell objects
        [MultiReturn(new[] { "PolyCurves", "PointLists" })]
        public static Dictionary<string, object> MakeConvexHullsFromCells(List<List<Cell>> cellProgramsList)
        {
            List<Polygon> polyList = new List<Polygon>();
            List<List<Point2d>> ptAllnew = new List<List<Point2d>>();

            for (int i = 0; i < cellProgramsList.Count; i++)
            {
                List<Cell> cellList = cellProgramsList[i];
                List<Point2d> tempPts = new List<Point2d>();
                List<Point2d> convexHullPts = new List<Point2d>();
                if (cellList.Count > 0)
                {
                    for (int j = 0; j < cellList.Count; j++)
                    {
                        tempPts.Add(cellList[j].CenterPoint);
                    }


                }
                if (tempPts.Count > 2)
                {
                    ptAllnew.Add(tempPts);
                    convexHullPts = GridObject.ConvexHullFromPoint2D(tempPts);
                    List<Point> ptAll = DynamoGeometry.pointFromPoint2dList(convexHullPts);
                    if (ptAll.Count > 2)
                    {
                        Polygon poly = Polygon.ByPoints(ptAll);
                        polyList.Add(poly);
                        //Trace.WriteLine("Convex Hull Pt counts are : " + tempPts.Count);
                    }
                    else
                    {
                        polyList.Add(null);
                    }

                }
                else
                {
                    polyList.Add(null);
                }


            }

            return new Dictionary<string, object>
            {
                { "PolyCurves", (polyList) },
                { "PointLists", (ptAllnew) }
            };


        }
              
        //make convex hulls for programs
        [MultiReturn(new[] { "PolyCurves", "PointLists" })]
        public static Dictionary<string, object> MakeProgramConvexHulls(List<List<int>> cellProgramsList, List<Point2d> ptList)
        {
            List<Polygon> polyList = new List<Polygon>();
            List<List<Point2d>> ptAllnew = new List<List<Point2d>>();

            for (int i = 0; i < cellProgramsList.Count; i++)
            {
                List<int> cellList = cellProgramsList[i];
                List<Point2d> tempPts = new List<Point2d>();
                List<Point2d> convexHullPts = new List<Point2d>();
                if (cellList.Count > 0)
                {
                    for (int j = 0; j < cellList.Count; j++)
                    {
                        tempPts.Add(ptList[cellList[j]]);
                    }


                }
                if (tempPts.Count > 2)
                {
                    ptAllnew.Add(tempPts);
                    convexHullPts = GridObject.ConvexHullFromPoint2D(tempPts);
                    List<Point> ptAll = DynamoGeometry.pointFromPoint2dList(convexHullPts);
                    if(ptAll.Count > 2)
                    {
                        Polygon poly = Polygon.ByPoints(ptAll);
                        polyList.Add(poly);
                        //Trace.WriteLine("Convex Hull Pt counts are : " + tempPts.Count);
                    }
                    else
                    {
                        polyList.Add(null);
                    }
                  
                }
                else
                {
                    polyList.Add(null);
                }
             
              
            }
            
            return new Dictionary<string, object>
            {
                { "PolyCurves", (polyList) },
                { "PointLists", (ptAllnew) }
            };


        }

        //checks to see if any cell is available or not
        internal static bool AnyCellAvailable(List<Cell> cells)
        {
            bool check = false;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].CellAvailable)
                {
                    check = true;
                    break;
                }
            }
            return check;
        }
        
        //count number of available cells
        internal static List<int> CountAvailableCells(List<Cell> cells)
        {
            List<int> cellInfo = new List<int>();
            int cellCount = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].CellAvailable)
                {
                    cellCount += 1;
                    
                }
            }
            cellInfo.Add(cellCount);
            cellInfo.Add(cells.Count);
            if(cells.Count > 0)
            {
                cellInfo.Add(cellCount * 100 / cells.Count);
            }
            else
            {
                cellInfo.Add(0);
            }
           
            return cellInfo;
        }



    }
}
