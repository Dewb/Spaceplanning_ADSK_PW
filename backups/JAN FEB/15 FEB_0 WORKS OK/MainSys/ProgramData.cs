
using System.Collections.Generic;


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
        private double _gridX;
        private double _gridY;
        private int _numCellAdded;

        public List<Cell> _progrCell;
        private int _numCells;
        private double _areaGiven;
        private bool _IsAreaSatsifed;
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
            _gridX = dimX;
            _gridY = dimY;

            _numCellAdded = 0;
            _areaGiven = 0;
            _IsAreaSatsifed = false;
            




        }

        public int NumberofCellsProgram
        {
            get { return _numCells; }
            //set { _cellAvailable = value; }
        }
        public string ProgName
        {
            get { return _progName; }
        }

        public string DeptName
        {
            get { return _progDept; }
        }

        public double UnitArea
        {
            get { return _progUnitArea; }
        }

        public double Quantity
        {
            get { return _progQuanity; }
        }

        public double AreaNeeded
        {
            get { return _progQuanity*_progUnitArea; }
        }

        public double AreaProvided
        {
            get { return _gridX*_gridY*_numCellAdded; }
        }

        public int NumberofCellsAdded
        {
            get { return _numCellAdded; }
            set { _numCellAdded = value; }
        }

        public double AreaAllocatedValue
        {
            get { return _areaGiven; }
            //set { _areaGiven = value; }
        }

      
        public bool IsAreaSatisfied
        {
            get {

                double areaNeeded = _progQuanity * _progUnitArea;
                if (_areaGiven >= areaNeeded){
                    return true;
                }else{
                    return false;
                }
            }
            set
            {
                _IsAreaSatsifed = value;
            }
            
        }
   

        public string ProgramName
        {
            get { return _progName; }
        }

        public double GridX
        {
            get { return _gridX; }
        }

        public double GridY
        {
            get { return _gridY; }
        }


        //CALC ARE GIVEN
        //internal void Cal

        //CALC THE AREA ALLOCATED
        internal void CalcAreaAllocated()
        {
            _numCellAdded += 1;
           _areaGiven = _gridX * _gridY * _numCellAdded;

        }


        //CHECK IF AREA ALLOCATED IS ENOUGH



        //CALC NUMBER OF CELLS NEEDED TO PLACE EACH PROGRAM
        internal int NumCellsNeeded()
        {
            int num = 0;
            double cellArea = _gridX * _gridY;
            num = (int)(_progUnitArea / cellArea);
            return num;
        }


 



    }
}
