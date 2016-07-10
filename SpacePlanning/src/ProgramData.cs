
using stuffer;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpacePlanning
{
    /// <summary>
    /// Program Data object to store information related to program elements from the input program document.
    /// </summary>
    public class ProgramData 
    {

        private int _progrID;
        private string _progName;
        private string _progDept;
        private string _progNameShort;
        private int _progQuanity;
        private double _progUnitArea;
        private int _progPrefValue;
        private List<string> _progAdjList;
        private double _gridX;
        private double _gridY;
        private string _progType;
        private int _numCellAdded;
        private List<Cell> _CellsAssigned;
        private List<Cell> _progrCell;
        private int _numCells;
        private double _areaGiven;
        private bool _IsAreaSatsifed;
        private List<Polygon2d> _polyProgs;
        private double _adjacencyWeight;
        private double _combinedProgramWeight;
        private bool _stackingOptions;
        private int _floorLevel;

        internal ProgramData(int programID,string programName,string programDept,
            int programQuant,double programUnitArea, int programPrefValue, List<string> programAdjList,List<Cell> programCell, double dimX, double dimY, string progType, bool stackingOptions)
        {
            _progrID = programID;
            
            _progDept = programDept;
            _progName = programID + "|" +programName + "@ " + programDept.Substring(0, 2).ToUpper();//
            _progQuanity = programQuant;
            _progUnitArea = programUnitArea;
            _progPrefValue = programPrefValue;
            _progAdjList = programAdjList;
            _progrCell = programCell;
            _numCells = NumCellsNeeded();
            _gridX = dimX;
            _gridY = dimY;
            _progType = progType;
            _stackingOptions = stackingOptions;

            _combinedProgramWeight = _progPrefValue;
            _adjacencyWeight = 0;
            _numCellAdded = 0;
            _areaGiven = 0;
            _IsAreaSatsifed = false;
            _CellsAssigned = new List<Cell>();
            _polyProgs = null;
            _progNameShort = ProgramNameShorten();
            _floorLevel = 0;
        }
        
        internal ProgramData(ProgramData other)
        {
            _progrID = other.ProgID;
            _progName = other.ProgramName;
            _progDept = other.DeptName;
            _progQuanity = other.Quantity;
            _progUnitArea = other.UnitArea;
            _progPrefValue = other.ProgPreferenceVal;
            _progAdjList = other.ProgAdjList;
            _progrCell = other.ProgCell;
            _numCells = other.NumCellsNeeded();
            _gridX = other._gridX;
            _gridY = other._gridY;
            _progType = other.ProgramType;
            _stackingOptions = other._stackingOptions;

            _numCellAdded = other.NumberofCellsAdded;
            _areaGiven = other.ProgAreaProvided;
            _IsAreaSatsifed = other.IsAreaSatisfied;
            _CellsAssigned = new List<Cell>();
            _adjacencyWeight = other.AdjacencyWeight;
            _combinedProgramWeight = other.ProgramCombinedAdjWeight;
            _progNameShort = other.ProgramNameShort;
            _floorLevel = other.ProgFloorLevel;

            if (other.PolyAssignedToProg != null) _polyProgs = other.PolyAssignedToProg;
            else _polyProgs = null;            
        }


        #region - Public properties
        /// <summary>
        /// Name of the program.
        /// </summary>
        public string ProgramName
        {
            get { return _progName; }
            set { _progName = value; }
        }
        /// <summary>
        /// Floor Level of the program.
        /// </summary>
        public int ProgFloorLevel
        {
            get { return _floorLevel; }
            set { _floorLevel = value; }
        }

        /// <summary>
        /// Name of the program.
        /// </summary>
        public string ProgramNameShort
        {
            get { return _progNameShort; }
        }
        /// <summary>
        /// Type of Program (either KPU or Regular ).
        /// </summary>
        public string ProgramType
        {
            get { return _progType; }
        }
        /// <summary>
        /// Program Stacking Options boolean value.
        /// </summary>
        internal bool StackingOptions
        {
            get { return _stackingOptions; }
        }

        /// <summary>
        /// Polygon2d's assigned to each program.
        /// </summary>
        public List<Polygon2d> PolyAssignedToProg
        {
            get { return _polyProgs; }
            set { _polyProgs = value; }
        }

        /// <summary>
        /// Number of cell objects assigned to each program.
        /// </summary>
        public int NumCellsInProg
        {
            get { return _numCells; }
        }
        /// <summary>
        /// Computed combined program weight.
        /// </summary>
        public double ProgramCombinedAdjWeight
        {
            get { return _combinedProgramWeight; }
            set { _combinedProgramWeight = value; }
        }

        /// <summary>
        /// Computed Adjacency weight value of the program.
        /// </summary>
        public double AdjacencyWeight
        {
            get { return _adjacencyWeight; }
            set { _adjacencyWeight = value; }
        }

        /// <summary>
        /// Program adjacency list.
        /// </summary>
        public List<string> ProgAdjList
        {
            get { return _progAdjList; }
        }

        /// <summary>
        /// List of cell objects in each program.
        /// </summary>
        public List<Cell> ProgCell
        {
            get { return _progrCell; }
        }

        /// <summary>
        /// Program preference value.
        /// </summary>
        public int ProgPreferenceVal
        {
            get { return _progPrefValue; }
        }

        /// <summary>
        /// Name of the Deparment to which the program is assigned to.
        /// </summary>
        public string DeptName
        {
            get { return _progDept; }
        }

        /// <summary>
        /// Area of one unit of program.
        /// </summary>
        public double UnitArea
        {
            get { return _progUnitArea; }
        }

        /// <summary>
        /// X axis dimension of the cell.
        /// </summary>
        public double DimX
        {
            get { return _gridX; }
        }


        /// <summary>
        /// Y axis dimension of the cell.
        /// </summary>
        public double DimY
        {
            get { return _gridY; }
        }


        /// <summary>
        /// Quantity of each program.
        /// </summary>
        public int Quantity
        {
            get { return _progQuanity; }
        }

        /// <summary>
        /// Area of one unit of the program.
        /// </summary>
        public double ProgAreaNeeded
        {
            get { return _progUnitArea; }            
        }


       
        /// <summary>
        /// Does the area assigned to the program satisfy the area requirements.
        /// </summary>
        public bool IsAreaSatisfied
        {
            get
            {
                //double areaNeeded = _progQuanity * _progUnitArea;
                if (CurrentAreaNeeds <= 0) return true;
                else return false;
            }
            set
            {
                _IsAreaSatsifed = value;
            }

        }

        /// <summary>
        /// Area assigned to the program.
        /// </summary>
        public double ProgAreaProvided
        {
            //get { return _gridX*_gridY*_numCellAdded; }
            get {
                //Trace.WriteLine("ProgAreaProvided = " + _areaGiven);
                //Trace.WriteLine("GridX = " + _gridX);
                //Trace.WriteLine("GridY = " + _gridY);
                return _areaGiven; }
            set {
                _areaGiven = value;
                _numCellAdded = (int)(value / (_gridX * _gridY));
            }            
        }

        #endregion



        #region - Private properties
        internal int NumberofCellsAdded
        {
            get { return _numCellAdded; }
            set { _numCellAdded = value; }
        }

        internal string ProgramNameShorten()
        {

            int number = 6;
            bool hasHash = false;
            
            if (_progName.IndexOf("#") != -1)
            {
                hasHash = true;

            }
            string[] str = _progName.Split(' ');
            string part = "";
            for (int i = 0; i < str.Length; i++)
            {
                if (i == 0) number = 7;
                else number = 4;
                if (i < 2)
                {
                    if (str[i].Length > number) part += str[i].Substring(0, number - 1) + " ";
                    else part += str[i];
                }else
                {
                    //part += str[i] + " ";
                }            
                
            }

            //part += " " + str[str.Length-2];
            part += " @" + str[str.Length-1];
            //if (hasHash) part  += " ##";
            return part;
         
        }

        public double CurrentAreaNeeds
        {
            get { return ProgAreaNeeded - ProgAreaProvided; }

        }

        internal int ProgID
        {
            get { return _progrID; }
        }
        #endregion


        #region - Private Methods
        //calc area allocated per program element
        internal void CalcAreaAllocated()
        {
            _numCellAdded += 1;
           _areaGiven = _gridX * _gridY * _numCellAdded;

        }

        //allocate cells to the program
        internal void CellAssign(List<Cell> inputCellList)
        {
            _CellsAssigned = inputCellList;
            _numCellAdded = _CellsAssigned.Count;
            NumberofCellsAdded = _numCellAdded;
        }

        //allocate cells to the program
        internal void CellAssignPerItem(Cell cellItem)
        {
            _CellsAssigned.Add(cellItem);
            _numCellAdded = _CellsAssigned.Count;
            NumberofCellsAdded = _numCellAdded;
        }

        //calc number of cells needed
        internal int NumCellsNeeded()
        {
            int num = 0;
            double cellArea = _gridX * _gridY;
            num = (int)(_progUnitArea / cellArea);
            return num;
        }

        //call area added to each prog
        internal void AddAreaToProg(double area)
        {
            _areaGiven += area;
        }
        #endregion


    }
}
