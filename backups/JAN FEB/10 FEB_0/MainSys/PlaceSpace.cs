using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
using System.IO;
using stuffer;
using System.Collections;

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

        internal static int SelectCellToPlaceProgram(ref List<Cell> cells, List<Point2d> PolyPoints, double threshDistance)
        {
            
            int size = cells.Count;
            Random random = new Random();
            int randomIndex = -1;
            randomIndex = random.Next(0, size);
            while (cells[randomIndex].CellAvailable == false)
            {
                randomIndex = random.Next(0, size);
            }

            Point2d ranCenterPt = cells[randomIndex].CenterPoint;
            int closestIndex = GraphicsUtility.FindClosestPointIndex(PolyPoints, ranCenterPt);
            double testDistance = PolyPoints[closestIndex].DistanceTo(ranCenterPt);
            if (testDistance < threshDistance)
            {
                Point2d centroidCells = GraphicsUtility.CentroidInCells(cells);
                Vector2d v1 = new Vector2d(ranCenterPt, centroidCells);
                v1 = v1.Normalize();
                v1 = v1.Scale(threshDistance - testDistance);
                ranCenterPt += v1;

            }

            int cellIndexSelected = GraphicsUtility.FindClosestPointIndex(cells, ranCenterPt);

            return cellIndexSelected;
        }

        //public functions
        public static void AssignProgramsToCells(ref List<DeptData> deptData, ref List<Cell> cells, List<Point2d> PolyPoints)
        {

            // can sort depts based on preference





            for (int i = 0; i < deptData.Count; i++)
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

        //public functions
        public static void AssignProgramsToCells(ref List<ProgramData> progData, ref List<Cell> cells)
        {

            // can sort programs based on preference





            for(int i =0; i < progData.Count; i++)
            {
                for(int j=0; j < cells.Count; j++)
                {
                    if (cells[j].CellAvailable)
                    {
                        //DO COMPUTATION HERE
                    }
                }


            }
            
            
        }


        //public functions
        public static void ShowProgramsInSite(ref List<ProgramData> progData)
        {
            // compute number of cells required

        }






    }
}
