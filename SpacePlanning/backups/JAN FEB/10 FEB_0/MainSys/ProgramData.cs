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

///////////////////////////////////////////////////////////////////
/// NOTE: This project requires references to the ProtoInterface
/// and ProtoGeometry DLLs. These are found in the Dynamo install
/// directory.
///////////////////////////////////////////////////////////////////

namespace SpacePlanning
{
    public class ProgramData
    {

        // Two private variables for example purposes
        private int _progrID;
        private string _progName;
        private string _progDept;
        private int _progQuanity;
        private double _progUnitArea;
        private int _progPrefValue;
        private List<string> _progAdjList;
        private double gridX;
        private double gridY;

        public List<Cell> _progrCell;
        private int _numCells;

        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        public ProgramData(int programID,string programName,string programDept,
            int programQuant,double programUnitArea, int programPrefValue, List<string> programAdjList,List<Cell> programCell, double dimX, double dimY)
        {
            _progrID = programID;
            _progName = programName;
            _progDept = programDept;
            _progQuanity = programQuant;
            _progUnitArea = programUnitArea;
            _progPrefValue = programPrefValue;
            _progAdjList = programAdjList;
            _progrCell = programCell;
            _numCells = NumCellsNeeded();


        }

        public int NumberofCellsProgram
        {
            get { return _numCells; }
            //set { _cellAvailable = value; }
        }

        public string DeptName
        {
            get { return _progDept; }
        }

        public string ProgramName
        {
            get { return _progName; }
        }

        public double GridX
        {
            get { return gridX; }
        }

        public double GridY
        {
            get { return gridY; }
        }


        //CALC THE TOTAL AREA AND NUMBER OF CELLS NEEDED TO PLACE EACH DEPARTMENT


        //CALC NUMBER OF CELLS NEEDED TO PLACE EACH PROGRAM
        internal int NumCellsNeeded()
        {
            int num = 0;
            double cellArea = gridX * gridY;
            num = (int)(_progUnitArea / cellArea);
            return num;
        }



    }
}
