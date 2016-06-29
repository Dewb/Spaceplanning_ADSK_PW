
using stuffer;
using System;
using System.Collections.Generic;


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

        const string KPU = "kpu";
        const string REGULAR = "regular";

        #region - internal constructor
        internal DeptData(string deptName, List<ProgramData> programDataList, double circulationFactor, double dimX, double dimY)
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

        }

        internal DeptData(DeptData other)
        {
            _deptName = other.DepartmentName;
            _progDataList = other.ProgramsInDept;
            _numCellsDept = other.NumCellsNeededDept();
            _cirFactor = other.DeptCirFactor;
            _deptAreaNeeded = other.AreaEachDept();
            _numCells = other.NumCellsNeededDept();
            _numCellAdded = other.NumCellsInDept;
            _IsAreaSatisfied = other.IsAreaSatisfied;
            _CellsAssigned = other.DepartmentCells;
            _gridX = other._gridX;
            _gridY = other._gridY;
            _deptType = other.DepartmentType;
            _deptAreaProportion = other.DeptAreaProportionNeeded;
            _deptAreaProportionAchieved = other.DeptAreaProportionAchieved;

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
            get { return Math.Round(AreaProvided / DeptAreaNeeded, 3); }
        }

        /// <summary>
        /// Type of Department (either KPU or Regular ).
        /// </summary>
        public string DepartmentType
        {
            get { return _deptType; }
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
        public double AreaProvided
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
            if (_progDataList == null) return "";
            int count = 0;
            for(int i = 0; i < _progDataList.Count; i++)
                if (_progDataList[i].ProgramType.IndexOf(KPU.ToLower()) != -1 || _progDataList[i].ProgramType.IndexOf(KPU.ToUpper()) != -1) count += 1;
            int perc = count / _progDataList.Count;
            if (perc > 0.50) return KPU.ToUpper();
            else return REGULAR.ToUpper();
        }

        #endregion



    }
}

