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

        internal static int SelectCellToPlaceProgram(ref List<Cell> cells, List<Point2d> PolyPoints, double threshDistance, Random random)
        {
           
            int size = cells.Count;            
            int randomIndex = -1;
            randomIndex = random.Next(0, size);
            //Trace.WriteLine("Randomly Selected Cell is : " + randomIndex);
            cells[randomIndex].CellAvailable = false;
            return randomIndex;

            //--------------------------------------------- will try later the below ----------------------------------------
            /*
            while (cells[randomIndex].CellAvailable == false)
            {
                randomIndex = random.Next(0, size);
                //Trace.WriteLine("cell available is false");
            }

            Point2d ranCenterPt = cells[randomIndex].CenterPoint;
            Trace.WriteLine("Random Center Point Before is : " + ranCenterPt);
            int closestIndex = GraphicsUtility.FindClosestPointIndex(PolyPoints, ranCenterPt);
            double testDistance = PolyPoints[closestIndex].DistanceTo(ranCenterPt);
            //double testDistance = GraphicsUtility.DistanceBetweenPoints(PolyPoints[closestIndex], ranCenterPt);
            if (testDistance < threshDistance)
            {
                Point2d centroidCells = GraphicsUtility.CentroidInCells(cells);
                Trace.WriteLine("Cell Centroid is : " + centroidCells);
                Vector2d v1 = new Vector2d(ranCenterPt, centroidCells);
                v1 = v1.Normalize();
                v1 = v1.Scale(threshDistance - testDistance);
                ranCenterPt = GraphicsUtility.PointAddVector2D(ranCenterPt, v1);
                Trace.WriteLine("Random Center Point After is : " + ranCenterPt);

            }

            int cellIndexSelected = GraphicsUtility.FindClosestPointIndex(cells, ranCenterPt);
            Trace.WriteLine("Cell Selected Index is : " + cellIndexSelected);
            Trace.WriteLine("----------------------------------------------------------------");
            Trace.WriteLine("----------------------------------------------------------------");
            cells[cellIndexSelected].CellAvailable = false;

            return cellIndexSelected;
            */
            //--------------------------------------------- will try later the below ----------------------------------------
        }

        //public functions
        internal static List<int> GetProgramCentroid(List<ProgramData> progData, ref List<Cell> cells, List<Point2d> PolyPoints,int num = 5)
        {

            //did not work----------------------------------------------------------------------
            EnvDTE.DTE ide = (EnvDTE.DTE)Marshal.GetActiveObject("VisualStudio.DTE.14.0");
            if (ide != null)
            {
                ide.ExecuteCommand("Edit.ClearOutputWindow", "");
                Marshal.ReleaseComObject(ide);
            }
            //-----------------------------------------------------------------------------------
            int selectedCell = -1;
            List<int> selectedIndices = new List<int>();
            Random random = new Random();
            for (int i = 0; i < progData.Count; i++)
            {
                if (i > num)
                {
                    break;
                }

                
                selectedCell = SelectCellToPlaceProgram(ref cells, PolyPoints,cells[0].DimX,random);
                selectedIndices.Add(selectedCell);
               
            }
            Trace.WriteLine("Area of Each Cell is : " + cells[0].DimX * cells[0].DimY);
            return selectedIndices;
        }


        //public functions
        internal static List<int> GetProgramCentroid(List<DeptData> deptData, ref List<Cell> cells, List<Point2d> PolyPoints, int num = 5)
        {

        
            int selectedCell = -1;
            List<int> selectedIndices = new List<int>();
            Random random = new Random();
            for (int i = 0; i < deptData.Count; i++)
            {
                if (i > num)
                {
                    break;
                }

                selectedCell = SelectCellToPlaceProgram(ref cells, PolyPoints, cells[0].DimX, random);
                selectedIndices.Add(selectedCell);

            }
            //Trace.WriteLine("Area of Each Cell is : " + cells[0].DimX * cells[0].DimY);
            return selectedIndices;
        }

        //public functions
       




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
        //public functions
        [MultiReturn(new[] { "ProgramCellIndices", "ProgramData","Cells" })]
        public static Dictionary<string, object> PlaceProgramsInSite(List<ProgramData> progData, List<Cell> cells, List<Point2d> PolyPoints,  int num = 5, int tag = 0)
        {

            // make all cells available
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }

            // get cellIndex for Program Centroids
            List<int> selectedIndices = GetProgramCentroid(progData, ref cells, PolyPoints, num);
         


            // calc program area
            List<double> sqEdge = new List<double>();
            List<List<int>> cellProgramsList = new List<List<int>>(); // to be returned

            for (int i = 0; i < selectedIndices.Count; i++)
            {

                // calc square edge
                double side = Math.Sqrt(progData[i].UnitArea  * progData[i].Quantity);
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
                            progData[i].CalcAreaAllocated();
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
                while (progData[i].IsAreaSatisfied == false && AnyCellAvailable(cells) == true)
                {

                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].CellAvailable==true)
                        {
                            // make cell index list for each program
                            cellProgramsList[i].Add(j);
                            cells[j].CellAvailable = false;
                            progData[i].CalcAreaAllocated();
                            break;

                        }
                    }
                }

                Trace.WriteLine("Number of Cells added for the program : " + progData[i].NumberofCellsAdded);
            }


            Trace.WriteLine("End of Assignment--------------------------------------");
            Trace.WriteLine("End of Assignment--------------------------------------");
            Trace.WriteLine("-------------------------------------------------------");

            //return cellProgramsList;
            return new Dictionary<string, object>
            {
                { "ProgramCellIndices", (cellProgramsList) },
                { "ProgramData", (progData) },
                { "Cells", (cells) }
            };

        }


        //public functions
        [MultiReturn(new[] { "DeptCellIndices", "DepartmentData", "Cells" })]
        public static Dictionary<string, object> PlaceDeptsInSite(List<DeptData> deptData, List<Cell> cells, List<Point2d> PolyPoints, int num = 5, int tag = 0)
        {

            // make all cells available
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }

            // get cellIndex for Program Centroids
            List<int> selectedIndices = GetProgramCentroid(deptData, ref cells, PolyPoints, num);



            // calc program area
            List<double> sqEdge = new List<double>();
            List<List<int>> cellProgramsList = new List<List<int>>(); // to be returned

            for (int i = 0; i < selectedIndices.Count; i++)
            {

                // calc square edge
                double side = Math.Sqrt(deptData[i].DeptAreaNeeded);
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
                            deptData[i].CalcAreaAllocated();
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
                while (deptData[i].IsAreaSatisfied == false && AnyCellAvailable(cells) == true)
                {

                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (cells[j].CellAvailable == true)
                        {
                            // make cell index list for each program
                            cellProgramsList[i].Add(j);
                            cells[j].CellAvailable = false;
                            deptData[i].CalcAreaAllocated();
                            break;

                        }
                    }
                }

                Trace.WriteLine("Number of Cells added for the program : " + deptData[i].NumberofCellsAdded);
            }


            Trace.WriteLine("End of Assignment--------------------------------------");
            Trace.WriteLine("End of Assignment--------------------------------------");
            Trace.WriteLine("-------------------------------------------------------");

            //return cellProgramsList;
            return new Dictionary<string, object>
            {
                { "DeptCellIndices", (cellProgramsList) },
                { "DepartmentData", (deptData) },
                { "Cells", (cells) }
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
            cellInfo.Add(cellCount * 100 / cells.Count);
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
        [MultiReturn(new[] { "DepartmentNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "ProgramsInDepts", "AvailableAndTotalCellCount" })]
        public static Dictionary<string, object> DeptAnalytics(List<DeptData> deptData, List<Cell> cells, int tag)
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









        //--------------------------------------------------------------------
        //public functions
        public static List<int> ShowProgramsInSite(List<ProgramData> progData, List<Cell> cells, List<Point2d> PolyPoints, int num = 5)
        {
            // make all cells available
            for(int i = 0; i < cells.Count; i++)
            {
                cells[i].CellAvailable = true;
            }
            // get cellIndex for Program Centroids
            List<int> selectedIndices = new List<int>();
            selectedIndices = GetProgramCentroid(progData, ref cells, PolyPoints, num);


            // calc program area

           
            List<double> sqEdge = new List<double>();
            List<int> cellIndexPrograms = new List<int>();
            for(int i = 0; i < selectedIndices.Count; i++)
            {
                // calc square edge
                double side = Math.Sqrt(progData[i].UnitArea);
                sqEdge.Add(side);
                //Trace.WriteLine("Side of Square for  Cell " + i + " : " + side);
                //Trace.WriteLine("Area of Program Unit is  : " + progData[i].UnitArea);

                // place square
                Point2d center = cells[selectedIndices[i]].CenterPoint;
                Polygon2d squareCentroid = GraphicsUtility.MakeSquarePolygonFromCenterSide(center, side);

                // get number of cell points inside square
                for(int j=0; j < cells.Count; j++)
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
                        //Trace.WriteLine(" Added Cell is : " + j);
                    }
                    }
                }


               
            }



            // recurse till area is satisfied
            Trace.WriteLine("----------------------------------------------------");
            return cellIndexPrograms;

        }






    }
}
