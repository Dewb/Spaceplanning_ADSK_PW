
using stuffer;
using System.Collections.Generic;

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
        private List<Cell> _CellsAssigned;

        public List<Cell> _progrCell;
        private int _numCells;
        private double _areaGiven;
        private bool _IsAreaSatsifed;
        private List<Polygon2d> _polyProgs;


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
            _CellsAssigned = new List<Cell>();
            _polyProgs = null;




        }


        public ProgramData(ProgramData other)
        {
            _progrID = other.ProgID;
            _progName = other.ProgName;
            _progDept = other.DeptName;
            _progQuanity = other.Quantity;
            _progUnitArea = other.UnitArea;
            _progPrefValue = other.ProgPrefValue;
            _progAdjList = other.ProgAdjList;
            _progrCell = other.ProgCell;
            _numCells = other.NumCellsNeeded();
            _gridX = other.GridX;
            _gridY = other.GridY;

            _numCellAdded = other.NumberofCellsAdded;
            _areaGiven = other.AreaProvided;
            _IsAreaSatsifed = other.IsAreaSatisfied;
            _CellsAssigned = new List<Cell>();

            if (other.PolyProgAssigned != null) _polyProgs = other.PolyProgAssigned;
            else _polyProgs = null;            
        }
        public List<Polygon2d> PolyProgAssigned
        {
            get { return _polyProgs; }
            set { _polyProgs = value; }
        }
        public List<Cell> ProgramCells
        {
            get { return _CellsAssigned; }
            set { _CellsAssigned = value; }
        }

        public int NumberofCellsProgram
        {
            get { return _numCells; }
            //set { _cellAvailable = value; }
        }

        public int ProgID
        {
            get { return _progrID; }
        }

        public List<string> ProgAdjList
        {
            get { return _progAdjList; }
        }

        public List<Cell> ProgCell
        {
            get { return _progrCell; }
        }

        public int ProgPrefValue
        {
            get { return _progPrefValue; }
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

        public int Quantity
        {
            get { return _progQuanity; }
        }

        public double AreaNeeded
        {
            get { return _progUnitArea; }
            
        }


        public double CurrentAreaNeeds
        {
            get { return _progUnitArea - _areaGiven; }

        }



        public double AreaProvided
        {
            get { return _gridX*_gridY*_numCellAdded; }
            set {
                _areaGiven = value;
                _numCellAdded = (int)(value / (_gridX * _gridY));
            }
            
        }

        public int NumberofCellsAdded
        {
            get { return _numCellAdded; }
            set { _numCellAdded = value; }
        }


      
        public bool IsAreaSatisfied
        {
            get {
                double areaNeeded = _progQuanity * _progUnitArea;
                if (CurrentAreaNeeds <= 0) return true;
                else return false;
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

        //CALC THE AREA ALLOCATED
        internal void CalcAreaAllocated()
        {
            _numCellAdded += 1;
           _areaGiven = _gridX * _gridY * _numCellAdded;

        }


        //ALLOCATE CELLS TO THE PROGRAM
        internal void CellAssign(List<Cell> inputCellList)
        {
            _CellsAssigned = inputCellList;
            _numCellAdded = _CellsAssigned.Count;
            NumberofCellsAdded = _numCellAdded;
        }

        //ALLOCATE CELLS TO THE PROGRAM
        internal void CellAssignPerItem(Cell cellItem)
        {
            _CellsAssigned.Add(cellItem);
            _numCellAdded = _CellsAssigned.Count;
            NumberofCellsAdded = _numCellAdded;
        }

        //CALC NUMBER OF CELLS NEEDED TO PLACE EACH PROGRAM
        public int NumCellsNeeded()
        {
            int num = 0;
            double cellArea = _gridX * _gridY;
            num = (int)(_progUnitArea / cellArea);
            return num;
        }

        internal void AddAreaToProg(double area)
        {
            _areaGiven += area;
        }

     
    }
}
