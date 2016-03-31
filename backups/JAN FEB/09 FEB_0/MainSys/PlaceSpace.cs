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
