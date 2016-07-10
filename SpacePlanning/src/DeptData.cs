
using stuffer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpacePlanning
{
    /// <summary>
    /// Department Data object to store information related to departments from the input program document.
    /// </summary>
    public class DeptData
    {
        // Two private variables for example purposes
        private string _deptName;
        private List<ProgramData> _progDataList;
        private int _numCellsDept;
        private double _deptAreaNeeded;
        private int _numCells;
        private int _numCellAdded;
        private double _areaGivenDept;
        private bool _IsAreaSatisfied;
        private List<Cell> _CellsAssigned;
        private double _gridX;
        private double _gridY;
        private string _deptType;
        private List<Polygon2d> _polyDepts;
        private double _deptAreaProportion;
        private double _deptAreaProportionAchieved;
        private double _cirFactor;
        private string _deptAbrv;
        private double _deptAdjacencyWeight;
        private bool _stackingOptions;

        private List<double> _flrHeightList;
        private bool _mode3D;
        private int _floorLevel;


        #region - internal constructor
        internal DeptData(string deptName, List<ProgramData> programDataList, double circulationFactor, double dimX, double dimY, bool stackingOptions)
        {
            _deptName = deptName;
            _progDataList = programDataList;
            _numCellsDept = NumCellsNeededDept();
            _cirFactor = circulationFactor;
            _deptAreaNeeded = AreaEachDept();
            _numCells = NumCellsNeededDept();
            _numCellAdded = 0;
            _IsAreaSatisfied = false;
            _CellsAssigned = new List<Cell>();
            _gridX = dimX;
            _gridY = dimY;
            _deptType = CalcDepartmentType();
            _polyDepts = null;
            _deptAreaProportion = 0;
            _deptAreaProportionAchieved = 0;
            _deptAbrv = _deptName +" = " + _deptName.Substring(0, 2).ToUpper() + " @ " + _deptType;//_deptName.Split(' ').Select(s => s[0]).ToString();
            _deptAdjacencyWeight = 0;
            _stackingOptions = stackingOptions;

            _mode3D = false;
            _flrHeightList = new List<double>();
            _flrHeightList.Add(0);
            _floorLevel = 0;

        }

        internal DeptData(DeptData other)
        {
            _deptName = other.DepartmentName;
            _progDataList = other.ProgramsInDept;
            _numCellsDept = other.NumCellsNeededDept();
            _cirFactor = other.DeptCirFactor;
            _deptAreaNeeded = other.AreaEachDept();
            _deptAreaNeeded = other.DeptAreaNeeded;
            _numCells = other.NumCellsNeededDept();
            _numCellAdded = other.NumCellsInDept;
            _IsAreaSatisfied = other.IsAreaSatisfied;
            _CellsAssigned = other.DepartmentCells;
            _gridX = other._gridX;
            _gridY = other._gridY;
            _deptType = other.DepartmentType;
            _deptAreaProportion = other.DeptAreaProportionNeeded;
            _deptAreaProportionAchieved = other.DeptAreaProportionAchieved;

            _areaGivenDept = other.DeptAreaProvided;
            _deptAbrv = other.DepartmentAbbrev;
            _deptAdjacencyWeight = other.DeptAdjacencyWeight;
            _stackingOptions = other._stackingOptions;
            _mode3D = other._mode3D;
            _flrHeightList = other._flrHeightList;
            _floorLevel = other.DeptFloorLevel;

            if (other.PolyAssignedToDept != null && other.PolyAssignedToDept.Count > 0) _polyDepts = other.PolyAssignedToDept;
            else _polyDepts = null;
        }
        #endregion



        #region - Public Properties
        /// <summary>
        /// Required area proportion for each department on site.
        /// </summary>
        public double DeptAreaProportionNeeded
        {
            get { return _deptAreaProportion; }
            set { _deptAreaProportion = value; }
        }

        /// <summary>
        /// 3d Mode of the program.
        /// </summary>
        public bool Mode3D
        {
            get { return _mode3D; }
            set { _mode3D = value; }
        }
        /// <summary>
        /// Floor Level of the department
        /// </summary>
        public int DeptFloorLevel
        {
            get { return _floorLevel; }
            set { _floorLevel = value; }
        }

        /// <summary>
        /// Floor HeightList
        /// </summary>
        public List<double> FloorHeightList
        {
            get { return _flrHeightList; }
            set { _flrHeightList = value; }
        }

        /// <summary>
        /// Department Adjacency Weight.
        /// </summary>
        public double DeptAdjacencyWeight
        {
            get { return _deptAdjacencyWeight; }
            set { _deptAdjacencyWeight = value; }
        }

        /// <summary>
        /// Department Stacking Options boolean value.
        /// </summary>
        internal bool StackingOptions
        {
            get { return _stackingOptions; }
        }

        /// <summary>
        /// Returns the area proportion achieved for each department after space plan layout is generated.
        /// </summary>
        public double DeptAreaProportionAchieved
        {
            get { return _deptAreaProportionAchieved; }
            set { _deptAreaProportionAchieved = value; }
        }

        /// <summary>
        /// Proportion of each department after it's assigned on site.
        /// </summary>
        public double AreaPercentageAchieved
        {
            get { return Math.Round(DeptAreaProvided / DeptAreaNeeded, 3); }
        }

        /// <summary>
        /// Type of Department (either KPU or Regular ).
        /// </summary>
        public string DepartmentType
        {
            get { return _deptType; }
        }

        /// <summary>
        /// Type of Department (either KPU or Regular ).
        /// </summary>
        public string DepartmentAbbrev
        {
            get { return _deptAbrv; }
        }
        /// <summary>
        /// Polygon2d assigned to each department.
        /// </summary>     
        public List<Polygon2d> PolyAssignedToDept
        {
            get { return _polyDepts; }
            set { _polyDepts = value; }
        }

        /// <summary>
        /// Cell objects assigned to each department.
        /// </summary>
        public List<Cell> DepartmentCells
        {
            get { return _CellsAssigned; }
            set { _CellsAssigned = value; }
        }

        /// <summary>
        /// Area provided to each department.
        /// </summary>
        public double DeptAreaProvided
        {
            get { return _areaGivenDept; }
            set
            {
                _areaGivenDept = value;
                _numCellAdded = (int)(_areaGivenDept / (_gridX * _gridY));

            }
        }

        /// <summary>
        /// Does provided area to the department satisfy the area needs.
        /// </summary>
        public bool IsAreaSatisfied
        {
            get
            {

                if (_areaGivenDept >= _deptAreaNeeded)
                {
                    return true;
                }
                else {
                    return false;
                }
            }
            set
            {
                _IsAreaSatisfied = value;
            }

        }

        /// <summary>
        /// Total number of cells assigned to each department.
        /// </summary>
        public int NumCellsInDept
        {
            get { return _numCellAdded; }
            set { _numCellAdded = value; }
        }

        /// <summary>
        /// Name of the Department.
        /// </summary>
        public string DepartmentName
        {
            get { return _deptName; }
        }

        /// <summary>
        /// Area needed for each department.
        /// </summary>
        public double DeptAreaNeeded
        {
            get { return _deptAreaNeeded; }
        }

        /// <summary>
        /// List of programs inside each department.
        /// </summary>
        public List<ProgramData> ProgramsInDept
        {
            get { return _progDataList; }
            set { _progDataList = value; }
        }
        #endregion

        #region - Private Properties
        //dept circulation factor
        internal double DeptCirFactor
        {
            get { return _cirFactor; }
            set { _cirFactor = value; }
        }
        #endregion


        #region - Private Methods
        //calc number of cells needed for each dept
        internal int NumCellsNeededDept()
        {
            int num = 0;
            double cellArea = _gridX * _gridY;
            double totalProgramAreas = AreaEachDept();
            num = (int)(totalProgramAreas / cellArea);
            return num;
        }

        //computes total area of each dept
        internal double AreaEachDept()
        {
            if (_progDataList == null) return 0;
            double area = 0;
            for (int i = 0; i < _progDataList.Count; i++) area += _progDataList[i].UnitArea;
            return area * _cirFactor;
        }

        //assign cells per dept
        internal void CellAssignPerItem(Cell cellItem)
        {

            _CellsAssigned.Add(cellItem);
            _numCellAdded = _CellsAssigned.Count;
            NumCellsInDept = _numCellAdded;
        }

        //calc area allocated
        internal void CalcAreaAllocated()
        {
            _numCellAdded += 1;
            _areaGivenDept = _gridX * _gridY * _numCellAdded;

        }


        //compute the type of the department
        internal string CalcDepartmentType()
        {
            //return BuildLayout.REG.ToUpper();
            if (_progDataList == null) return "";
            int count = 0;
            for(int i = 0; i < _progDataList.Count; i++)
                if (_progDataList[i].ProgramType.IndexOf(BuildLayout.KPU.ToLower()) != -1 || _progDataList[i].ProgramType.IndexOf(BuildLayout.KPU.ToUpper()) != -1) count += 1;
            int perc = count / _progDataList.Count;
            if (perc > 0.50) return BuildLayout.KPU.ToUpper();
            else return BuildLayout.REG.ToUpper();
        }

        #endregion



    }
}

