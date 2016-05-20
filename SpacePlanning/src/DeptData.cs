﻿
using stuffer;
using System;
using System.Collections.Generic;


namespace SpacePlanning
{
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
        private List<Polygon2d> _polyDepts;
        private double _deptAreaProportion;
        private double _deptAreaProportionAchieved;
        private double _cirFactor;


        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        public DeptData(string deptName, List<ProgramData> programDataList, double circulationFactor, double dimX, double dimY)
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
            _polyDepts = null;
            _deptAreaProportion = 0;
            _deptAreaProportionAchieved = 0;

        }

        public DeptData(DeptData other)
        {
            _deptName = other.DepartmentName;
            _progDataList = other.ProgramsInDept;
            _numCellsDept = other.NumCellsNeededDept();
            _cirFactor = other.DeptCirFactor;
            _deptAreaNeeded = other.AreaEachDept();
            _numCells = other.NumCellsNeededDept();
            _numCellAdded = other.NumberofCellsAdded;
            _IsAreaSatisfied = other.IsAreaSatisfied;
            _CellsAssigned = other.DepartmentCells;
            _gridX = other.GridX;
            _gridY = other.GridY;
            _deptAreaProportion = other.DeptAreaProportion;
            _deptAreaProportionAchieved = other.DeptAreaProportionAchieved;

            if (other.PolyDeptAssigned != null && other.PolyDeptAssigned.Count > 0)
            {
                _polyDepts = other.PolyDeptAssigned;
            }
            else
            {
                _polyDepts = null;
            }
        }



        #region - Public Methods

        public double DeptCirFactor
        {
            get { return _cirFactor; }
            set { _cirFactor = value; }
        }

        public double DeptAreaProportionAchieved
        {
            get { return _deptAreaProportionAchieved; }
            set { _deptAreaProportionAchieved = value; }
        }

        public double DeptAreaProportion
        {
            get { return _deptAreaProportion; }
            set { _deptAreaProportion = value; }
        }

        public double AreaPercentageAchieved
        {
            get { return Math.Round(AreaProvided / DeptAreaNeeded, 3); }
        }

        public List<Polygon2d> PolyDeptAssigned
        {
            get { return _polyDepts; }
            set { _polyDepts = value; }
        }

        public double GridX
        {
            get { return _gridX; }
        }

        public double GridY
        {
            get { return _gridY; }
        }

        public List<Cell> DepartmentCells
        {
            get { return _CellsAssigned; }
            set { _CellsAssigned = value; }
        }

        public int NumberofCellsDept
        {
            get { return _numCellsDept; }
            //set { _cellAvailable = value; }
        }

        public double AreaProvided
        {
            get { return _areaGivenDept; }
            set
            {
                _areaGivenDept = value;
                _numCellAdded = (int)(_areaGivenDept / (_gridX * _gridY));

            }
        }

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

        public int NumberofCellsAdded
        {
            get { return _numCellAdded; }
            set { _numCellAdded = value; }
        }

        public string DepartmentName
        {
            get { return _deptName; }
        }

        public double DeptAreaNeeded
        {
            get { return _deptAreaNeeded; }
        }

        public List<ProgramData> ProgramsInDept
        {
            get { return _progDataList; }
            set { _progDataList = value; }
        }

        //calc number of cells assigned to each dept
        public int NumCellsNeededDept()
        {
            int num = 0;
            double cellArea = _gridX * _gridY;
            double totalProgramAreas = AreaEachDept();
            num = (int)(totalProgramAreas / cellArea);
            return num;
        }

        //calc area for each dept
        public double AreaEachDept()
        {
            double area = 0;
            for (int i = 0; i < _progDataList.Count; i++) area += _progDataList[i].UnitArea;
            return area * _cirFactor;
        }

        #endregion


        #region - Private Methods
        //assign cells
        internal void CellAssign(List<Cell> inputCellList)
        {
            _CellsAssigned = inputCellList;
            _numCellAdded = _CellsAssigned.Count;
            NumberofCellsAdded = _numCellAdded;
        }

        //assign cells per dept
        internal void CellAssignPerItem(Cell cellItem)
        {

            _CellsAssigned.Add(cellItem);
            _numCellAdded = _CellsAssigned.Count;
            NumberofCellsAdded = _numCellAdded;
        }

        //calc area allocated
        internal void CalcAreaAllocated()
        {
            _numCellAdded += 1;
            _areaGivenDept = _gridX * _gridY * _numCellAdded;

        }

        //calc area from a given poly
        internal void CalcAreaGivenFromPoly(double areaPoly)
        {
            _areaGivenDept = areaPoly;

        }
        #endregion



    }
}

