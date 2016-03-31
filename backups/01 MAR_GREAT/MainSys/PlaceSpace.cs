using System;
using System.Collections.Generic;
using stuffer;
using System.Diagnostics;
using EnvDTE;
using System.Runtime.InteropServices;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
///////////////////////////////////////////////////////////////////
/// NOTE: This project requires references to the ProtoInterface
/// and ProtoGeometry DLLs. These are found in the Dynamo install
/// directory.
///////////////////////////////////////////////////////////////////

namespace SpacePlanning
{
    public class PlaceSpace
    {

        // private variables
  



        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        internal PlaceSpace()
        {
            
        }

        /// <summary>
        /// Takes the input Sat file geometry and converts to a PolyLine Object
        /// Can Also work with a sequential list of points
        /// </summary>
        /// <param name="lines">list of lines 2d store in class.</param>
        /// <returns>A newly-constructed ZeroTouchEssentials object</returns>
        public static PlaceSpace BySiteOutline()
        {
            return new PlaceSpace();
        }

        //private functions
        internal void addSome()
        {

        }

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

        //public functions
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
           // Trace.WriteLine("Area of Each Cell is : " + cells[0].DimX * cells[0].DimY);
            return selectedIndices;
        }


        //public functions
        internal static List<int> GetProgramCentroid(List<DeptData> deptData, ref List<Cell> cells,double dimX)
        {
            int selectedCell = -1;
            List<int> selectedIndices = new List<int>();
            Random random = new Random();
            for (int i = 0; i < deptData.Count; i++)
            {
                selectedCell = SelectCellToPlaceProgram(ref cells, random);
                selectedIndices.Add(selectedCell);
            }
            //Trace.WriteLine("Area of Each Cell is : " + cells[0].DimX * cells[0].DimY);
            return selectedIndices;
        }

        //public functions
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

                    //Trace.WriteLine("Temp Pt counts are : " + tempPts.Count);
                    List<Point2d> boundingPolyPts = ReadData.FromPointsGetBoundingPoly(tempPts);
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

        //public functions
        [MultiReturn(new[] { "PolyCurves", "PointLists" })]
        public static Dictionary<string, object> MakeProgramPolygons(List<List<int>> cellProgramsList, List<Point2d> ptList)
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
                List<Point2d> boundingPolyPts = ReadData.FromPointsGetBoundingPoly(tempPts);
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


        //public functions
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






        //public functions
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
















        // 1ST MAIN COMPONENT
        public static List<DeptData> AssignDeptsToSite(List<DeptData> deptData, List<Cell> cells, int recompute = 0)
        {

            //Build new dept data list
            List<DeptData> newDeptDataList = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData newDeptData = new DeptData(deptData[i]);
                newDeptDataList.Add(newDeptData);
            }

            

            // make all cells available
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }

            // get cellIndex for Program Centroids
            List<int> selectedIndices = GetProgramCentroid(newDeptDataList, ref cells, newDeptDataList[0].GridX);



            // calc program area
            List<double> sqEdge = new List<double>();

            for (int i = 0; i < selectedIndices.Count; i++)
            {

                // calc square edge
                double side = Math.Sqrt(newDeptDataList[i].DeptAreaNeeded);
                sqEdge.Add(side);


                // place square
                Point2d center = cells[selectedIndices[i]].CenterPoint;
                Polygon2d squareCentroid = GraphicsUtility.MakeSquarePolygonFromCenterSide(center, side);
          

                // get number of cell points inside square
                for (int j = 0; j < cells.Count; j++)
                {
                    if (cells[j].CellAvailable)
                    {
                        //Trace.WriteLine(" Square Centroid is  "  + squareCentroid);
                        Point2d testPoint = cells[j].CenterPoint;
                        if (GraphicsUtility.PointInsidePolygonTest(squareCentroid, testPoint))
                        {
                            // make cell index list for each program
                            cells[j].CellAvailable = false;
                            newDeptDataList[i].CalcAreaAllocated();
                            newDeptDataList[i].CellAssignPerItem(cells[j]);
                          
                        }
                    }
                }


            }// end of for loop


            for (int i = 0; i < selectedIndices.Count; i++)
            {
                // keep adding cells till area is satisfied for the program or till any cell is available
                while (newDeptDataList[i].IsAreaSatisfied == false && AnyCellAvailable(cells) == true)
                {

                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].CellAvailable == true)
                        {
                            // make cell index list for each program
                            cells[j].CellAvailable = false;
                            newDeptDataList[i].CalcAreaAllocated();
                            newDeptDataList[i].CellAssignPerItem(cells[j]);
                            break;

                        }
                    }
                }
            }

            /*
            //make new cell object from inputcells
            List<Cell> newCellList = new List<Cell>();
            for (int i = 0; i < cells.Count; i++)
            {
                Cell aCell = new Cell(cells[i]);
                newCellList.Add(aCell);
            }
            //


            // make new list of list of cells assigned to each dept
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

                }
                

            }
            */


            cells = null;
            deptData = null;
            sqEdge = null;
            selectedIndices = null;
            return newDeptDataList;

        }


        // 2ND MAIN COMPONTNENT
        [MultiReturn(new[] { "DepartmentDataProgramsAdded", "ProgramDataUpdated" })]
        public static Dictionary<string,object> AssignProgstoDepts(List<DeptData> deptData,int recompute)
        {

            //Build new dept data list
            List<DeptData> newDeptDataList = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData newDeptData = new DeptData(deptData[i]);
                newDeptDataList.Add(newDeptData);
            }

           


            //List<List<ProgramData>> progUpdated = new List<List<ProgramData>>();
            List<ProgramData> progUpdated = new List<ProgramData>();
            for (int i = 0; i < newDeptDataList.Count; i++)
            {
                List<ProgramData> progList = newDeptDataList[i].ProgramsInDept;
                List<Cell> cellList = newDeptDataList[i].DepartmentCells;
                if (cellList.Count > 0)
                {
                    //Trace.WriteLine("CellLists obtained has length : " + cellList.Count);
                    List<ProgramData> progD = AllocateProgramsInDept(progList, cellList, 0);
                    newDeptDataList[i].ProgramsInDept = progD;
                }

                // progUpdated.Add(newDeptDataList[i].ProgramsInDept);
                progUpdated.AddRange(newDeptDataList[i].ProgramsInDept);



            }
            deptData = null;
            return new Dictionary<string, object>
            {

                { "DepartmentDataProgramsAdded", (newDeptDataList) },
                { "ProgramDataUpdated", (progUpdated) }
            };

        }


        // internal function to assign programs to the cells provided by each dept
        internal static List<ProgramData> AllocateProgramsInDept(List<ProgramData> progData, List<Cell> oldCells, int tag = 0)
        {

            List<Cell> cells = new List<Cell>();
            // make all cells available
            for (int i = 0; i < oldCells.Count; i++)
            {
                cells.Add(new Cell(oldCells[i]));
            }


            // make all cells available
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }


            //Build new prog data list
            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData newProgData = new ProgramData(progData[i]);
                newProgDataList.Add(newProgData);
            }

            //progData.Clear();
            // get cellIndex for Program Centroids
            List<int> selectedIndices = GetProgramCentroid(newProgDataList, ref cells, newProgDataList[0].GridX);

            // calc program area
            List<double> sqEdge = new List<double>();


            for (int i = 0; i < selectedIndices.Count; i++)
            {

                // calc square edge
                double side = Math.Sqrt(newProgDataList[i].UnitArea * newProgDataList[i].Quantity);
                sqEdge.Add(side);

                // place square
                Point2d center = cells[selectedIndices[i]].CenterPoint;
                Polygon2d squareCentroid = GraphicsUtility.MakeSquarePolygonFromCenterSide(center, side);

                // get number of cell points inside square
                for (int j = 0; j < cells.Count; j++)
                {
                    if (cells[j].CellAvailable)
                    {
                        //Trace.WriteLine(" Square Centroid is  "  + squareCentroid);
                        Point2d testPoint = cells[j].CenterPoint;
                        if (GraphicsUtility.PointInsidePolygonTest(squareCentroid, testPoint))
                        {
                            // make cell index list for each program
                            cells[j].CellAvailable = false;
                            newProgDataList[i].CalcAreaAllocated();
                            newProgDataList[i].CellAssignPerItem(cells[j]);
                        }
                    }
                }

                //update area allocated to each program     



            }// end of for loop


            for (int i = 0; i < selectedIndices.Count; i++)
            {
                // keep adding cells till area is satisfied for the program or till any cell is available
                while (newProgDataList[i].IsAreaSatisfied == false && AnyCellAvailable(cells) == true)
                {

                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].CellAvailable == true)
                        {
                            // make cell index list for each program
                            
                            cells[j].CellAvailable = false;
                            newProgDataList[i].CalcAreaAllocated();
                            newProgDataList[i].CellAssignPerItem(cells[j]);
                            break;

                        }
                    }
                }

            }

            /*

            //make new cell object from inputcells
            List<Cell> newCellList = new List<Cell>();
            for (int i = 0; i < cells.Count; i++)
            {
                Cell aCell = new Cell(cells[i]);
                newCellList.Add(aCell);
            }
       

            // make new list of list of cells assigned to each dept
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
                }

            }
            */
           
            oldCells.Clear();
            return newProgDataList;

        }

      
       
       

        //public functions
        [MultiReturn(new[] { "ProgramCellIndices", "ProgramData","CellsForEachProg", "Cells" })]
        public static Dictionary<string, object> PlaceProgramsInSite(List<ProgramData> progData, List<Cell> cells, List<Point2d> PolyPoints, int num = 5, int tag = 0)
        {

            // make all cells available
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }


            //Build new prog data list
            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData newProgData = new ProgramData(progData[i]);
                newProgDataList.Add(newProgData);
            }

            //progData.Clear();

            // get cellIndex for Program Centroids
            List<int> selectedIndices = GetProgramCentroid(newProgDataList, ref cells, newProgDataList[0].GridX);



            // calc program area
            List<double> sqEdge = new List<double>();
            List<List<int>> cellProgramsList = new List<List<int>>(); // to be returned


            for (int i = 0; i < selectedIndices.Count; i++)
            {

                // calc square edge
                double side = Math.Sqrt(newProgDataList[i].UnitArea * newProgDataList[i].Quantity);
                sqEdge.Add(side);
                //Trace.WriteLine("Side of Square for  Cell " + i + " : " + side);
                //Trace.WriteLine("Area of Program Unit is  : " + progData[i].UnitArea);


                // place square
                Point2d center = cells[selectedIndices[i]].CenterPoint;
                Polygon2d squareCentroid = GraphicsUtility.MakeSquarePolygonFromCenterSide(center, side);
                List<int> cellIndexPrograms = new List<int>();
                int cellAddedCount = 0;

                // get number of cell points inside square
                for (int j = 0; j < cells.Count; j++)
                {
                    if (cells[j].CellAvailable)
                    {
                        //Trace.WriteLine(" Square Centroid is  "  + squareCentroid);
                        Point2d testPoint = cells[j].CenterPoint;
                        if (GraphicsUtility.PointInsidePolygonTest(squareCentroid, testPoint))
                        {
                            // make cell index list for each program
                            cellIndexPrograms.Add(j);
                            cells[j].CellAvailable = false;
                            newProgDataList[i].CalcAreaAllocated();
                            //Trace.WriteLine(" Added Cell is : " + j);
                            cellAddedCount += 1;
                        }
                    }
                }

                //update area allocated to each program     
                cellProgramsList.Add(cellIndexPrograms);



            }// end of for loop


            for (int i = 0; i < selectedIndices.Count; i++)
            {
                // keep adding cells till area is satisfied for the program or till any cell is available
                while (newProgDataList[i].IsAreaSatisfied == false && AnyCellAvailable(cells) == true)
                {

                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].CellAvailable == true)
                        {
                            // make cell index list for each program
                            cellProgramsList[i].Add(j);
                            cells[j].CellAvailable = false;
                            newProgDataList[i].CalcAreaAllocated();
                            break;

                        }
                    }
                }

                //Trace.WriteLine("Number of Cells added for the program : " + progData[i].NumberofCellsAdded);
            }


            //make new cell object from inputcells
            List<Cell> newCellList = new List<Cell>();
            for (int i = 0; i < cells.Count; i++)
            {
                Cell aCell = new Cell(cells[i]);
                newCellList.Add(aCell);
            }
            //cells.Clear();

            List<List<Cell>> cellEachProgGets = new List<List<Cell>>();
            // make new list of list of cells assigned to each dept
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


            //return cellProgramsList;
            return new Dictionary<string, object>
            {
                { "ProgramCellIndices", (cellProgramsList) },
                { "ProgramData", (newProgDataList) },
                { "CellsForEachProg", (cellEachProgGets) },
                { "Cells", (newCellList) }
            };

        }


        //public functions
        [MultiReturn(new[] { "DeptCellIndices", "DepartmentData", "CellsForEachDept", "Cells" })]
        public static Dictionary<string, object> PlaceDeptsInSite(List<DeptData> deptData, List<Cell> cells, List<Point2d> PolyPoints, int num = 5, int tag = 0)
        {

            //Build new dept data list
            List<DeptData> newDeptDataList = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData newDeptData = new DeptData(deptData[i]);
                newDeptDataList.Add(newDeptData);
            }

            deptData.Clear();

            // make all cells available
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }

            // get cellIndex for Program Centroids
            List<int> selectedIndices = GetProgramCentroid(newDeptDataList, ref cells, newDeptDataList[0].GridX);



            // calc program area
            List<double> sqEdge = new List<double>();
            List<List<int>> cellProgramsList = new List<List<int>>(); // to be returned

            for (int i = 0; i < selectedIndices.Count; i++)
            {

                // calc square edge
                double side = Math.Sqrt(newDeptDataList[i].DeptAreaNeeded);
                sqEdge.Add(side);
                //Trace.WriteLine("Side of Square for  Cell " + i + " : " + side);
                //Trace.WriteLine("Area of Program Unit is  : " + progData[i].UnitArea);


                // place square
                Point2d center = cells[selectedIndices[i]].CenterPoint;
                Polygon2d squareCentroid = GraphicsUtility.MakeSquarePolygonFromCenterSide(center, side);
                List<int> cellIndexPrograms = new List<int>();
                int cellAddedCount = 0;

                // get number of cell points inside square
                for (int j = 0; j < cells.Count; j++)
                {
                    if (cells[j].CellAvailable)
                    {
                        //Trace.WriteLine(" Square Centroid is  "  + squareCentroid);
                        Point2d testPoint = cells[j].CenterPoint;
                        if (GraphicsUtility.PointInsidePolygonTest(squareCentroid, testPoint))
                        {
                            // make cell index list for each program
                            cellIndexPrograms.Add(j);
                            cells[j].CellAvailable = false;
                            newDeptDataList[i].CalcAreaAllocated();
                            //Trace.WriteLine(" Added Cell is : " + j);
                            cellAddedCount += 1;
                        }
                    }
                }

                //update area allocated to each program
                cellProgramsList.Add(cellIndexPrograms);



            }// end of for loop


            for (int i = 0; i < selectedIndices.Count; i++)
            {
                // keep adding cells till area is satisfied for the program or till any cell is available
                while (newDeptDataList[i].IsAreaSatisfied == false && AnyCellAvailable(cells) == true)
                {

                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].CellAvailable == true)
                        {
                            // make cell index list for each program
                            cellProgramsList[i].Add(j);
                            cells[j].CellAvailable = false;
                            newDeptDataList[i].CalcAreaAllocated();
                            break;

                        }
                    }
                }

                //Trace.WriteLine("Number of Cells added for the program : " + deptData[i].NumberofCellsAdded);
            }


            //make new cell object from inputcells
            List<Cell> newCellList = new List<Cell>();
            for(int i = 0; i < cells.Count; i++)
            {
                Cell aCell = new Cell(cells[i]);
                newCellList.Add(aCell);
            }
            cells.Clear();

            List<List<Cell>> cellEachDeptGets = new List<List<Cell>>();
            // make new list of list of cells assigned to each dept
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

                //return cellProgramsList;
                return new Dictionary<string, object>
            {
                { "DeptCellIndices", (cellProgramsList) },
                { "DepartmentData", (newDeptDataList) },
                { "CellsForEachDept", (cellEachDeptGets) },
                { "Cells", (newCellList) }
            };

        }

        internal static bool AnyCellAvailable(List<Cell> cells)
        {
            bool check = false;
            int count = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].CellAvailable)
                {
                    count += 1;
                    check = true;
                    break;
                }
            }
            //Trace.WriteLine("Number of Vacant Cells available are : " + count);
            //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            //if (count > 0) { check = true; }

            return check;
        }

      
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


        // 0B ////////////////////////////
        //TO TEST IF PROGRAM DATA OUTPUTS CORRECT STUFFS
        [MultiReturn(new[] { "ProgrName", "ProgDept" })]
        public static Dictionary<string, object> ProgramObjectCheck(List<ProgramData> progData)
        {
            List<int> progIdList = new List<int>();
            List<string> programList = new List<string>();
            List<string> deptNameList = new List<string>();
            List<string> progQuantList = new List<string>();

            for (int i = 0; i < progData.Count; i++)
            {
                // programList.Add(progData[i].ProgramName);
                deptNameList.Add(progData[i].DeptName);
            }
            return new Dictionary<string, object>
            {

                { "ProgrName", (programList) },
                { "ProgDept", (deptNameList) }
            };
        }

        
        //PROGRAM ANALYTICS
        [MultiReturn(new[] { "ProgramNames", "NumCellsTaken", "AreaSatisfied","AreaNeeded","AreaProvided","Quantity", "AvailableAndTotalCellCount" })]
        public static Dictionary<string, object> ProgramAnalytics(List<ProgramData> progData,List<Cell> cells, int tag)
        {
            List<string> progNameList = new List<string>();
            List<int> numCellsList = new List<int>();
            List<bool> areaSatisfiedList = new List<bool>();
            List<double> areaNeededList = new List<double>();
            List<double> areaProvidedList = new List<double>();
            List<double> quantList = new List<double>();
            for (int i = 0; i < progData.Count; i++)
            {
                progNameList.Add(progData[i].ProgName);
                numCellsList.Add(progData[i].NumberofCellsAdded);
                areaSatisfiedList.Add(progData[i].IsAreaSatisfied);
                areaNeededList.Add(progData[i].AreaNeeded);
                areaProvidedList.Add(progData[i].AreaProvided);
                quantList.Add(progData[i].Quantity);
            }

            List<int> cellCount = CountAvailableCells(cells);
            return new Dictionary<string, object>
            {
                { "ProgramNames", (progNameList) },
                { "NumCellsTaken", (numCellsList) },
                { "AreaSatisfied", (areaSatisfiedList) },
                { "AreaNeeded", (areaNeededList) },
                { "AreaProvided", (areaProvidedList) },
                { "Quantity", (quantList) },
                { "AvailableAndTotalCellCount", (cellCount) }
            };
        }

        //DEPT ANALYTICS
        [MultiReturn(new[] { "DepartmentNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "ProgramsInDepts" })]
        public static Dictionary<string, object> DeptAnalytics(List<DeptData> deptData)
        {
            List<string> deptNameList = new List<string>();
            List<int> numCellsList = new List<int>();
            List<bool> areaSatisfiedList = new List<bool>();
            List<List<ProgramData>> progLists = new List<List<ProgramData>>();
            List<double> areaNeededList = new List<double>();
            List<double> areaProvidedList = new List<double>();
            for (int i = 0; i < deptData.Count; i++)
            {
                deptNameList.Add(deptData[i].DepartmentName);
                numCellsList.Add(deptData[i].NumberofCellsAdded);
                areaSatisfiedList.Add(deptData[i].IsAreaSatisfied);
                progLists.Add(deptData[i].ProgramsInDept);
                areaNeededList.Add(deptData[i].DeptAreaNeeded);
                areaProvidedList.Add(deptData[i].AreaProvided);
            }

          
            return new Dictionary<string, object>
            {
                { "DepartmentNames", (deptNameList) },
                { "NumCellsTaken", (numCellsList) },
                { "AreaSatisfied", (areaSatisfiedList) },
                { "AreaNeeded", (areaNeededList) },
                { "AreaProvided", (areaProvidedList) },
                { "ProgramsInDepts", (progLists) }
            };
        }


        //DEPT ANALYTICS
        [MultiReturn(new[] { "DepartmentNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "ProgramsInDepts", "AvailableAndTotalCellCount" })]
        internal static Dictionary<string, object> DeptAnalyticsOld(List<DeptData> deptData, List<Cell> cells, int tag)
        {
            List<string> deptNameList = new List<string>();
            List<int> numCellsList = new List<int>();
            List<bool> areaSatisfiedList = new List<bool>();
            List<List<ProgramData>> progLists = new List<List<ProgramData>>();
            List<double> areaNeededList = new List<double>();
            List<double> areaProvidedList = new List<double>();
            for (int i = 0; i < deptData.Count; i++)
            {
                deptNameList.Add(deptData[i].DepartmentName);
                numCellsList.Add(deptData[i].NumberofCellsAdded);
                areaSatisfiedList.Add(deptData[i].IsAreaSatisfied);
                progLists.Add(deptData[i].ProgramsInDept);
                areaNeededList.Add(deptData[i].DeptAreaNeeded);
                areaProvidedList.Add(deptData[i].AreaProvided);
            }

            List<int> cellCount = CountAvailableCells(cells);
            return new Dictionary<string, object>
            {
                { "DepartmentNames", (deptNameList) },
                { "NumCellsTaken", (numCellsList) },
                { "AreaSatisfied", (areaSatisfiedList) },
                { "AreaNeeded", (areaNeededList) },
                { "AreaProvided", (areaProvidedList) },
                 { "ProgramsInDepts", (progLists) },
                { "AvailableAndTotalCellCount", (cellCount) }
            };
        }



        //ADDING NEW WAYS TO ASSIGN SPACES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public static List<DeptData> Do_OrganizeDepartmentsOverSite(List<DeptData> deptData, List<Cell> cells, int recompute =0)
        {
            //Build new dept data list
            List<DeptData> newDeptDataList = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData newDeptData = new DeptData(deptData[i]);
                newDeptDataList.Add(newDeptData);
            }

            deptData = null;


            return newDeptDataList;


        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    }
}
