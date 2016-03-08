using System;
using System.Collections.Generic;
using stuffer;
using System.Diagnostics;
using EnvDTE;
using System.Runtime.InteropServices;
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
            Trace.WriteLine("Randomly Selected Cell is : " + randomIndex);
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
        public static List<int> AssignProgramCorrectToCells(List<ProgramData> progData, List<Cell> cells, List<Point2d> PolyPoints, double factor,int num = 5)
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
                selectedCell = SelectCellToPlaceProgram(ref cells, PolyPoints,factor * cells[0].DimX,random);
                selectedIndices.Add(selectedCell);
               
            }

            return selectedIndices;
        }




        //internal function


        //public functions
        public static void ShowProgramsInSite(List<ProgramData> progData, List<int> cellIndexes, List<Cell> cellList)
        {
            // calc program area

            // calc square edge

            // place square

            // get number of cell points inside square

            // recurse till area is satisfied

            // make cell index list for each program


        

        }



        //public functions
        public static void AssignProgramsToCells(List<ProgramData> progData, List<Cell> cells)
        {

            // can sort programs based on preference



            for (int i = 0; i < progData.Count; i++)
            {
                for (int j = 0; j < cells.Count; j++)
                {
                    if (cells[j].CellAvailable)
                    {
                        //DO COMPUTATION HERE
                    }
                }


            }


        }



    }
}
