
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

        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        public DeptData(string deptName, List<ProgramData> programDataList)
        {
            _deptName = deptName;
            _progDataList = programDataList;
            _numCellsDept = NumCellsNeededDept();
            _deptAreaNeeded = AreaEachDept();
            _numCells = NumCellsNeededDept();
            _numCellAdded = 0;
            _IsAreaSatisfied = false;



        }

        public DeptData(DeptData other)
        {
            _deptName = other.DepartmentName;
            _progDataList = other.ProgramsInDept;
            _numCellsDept = other.NumCellsNeededDept();
            _deptAreaNeeded = other.AreaEachDept();
            _numCells = other.NumCellsNeededDept();
            _numCellAdded = other.NumberofCellsAdded;
            _IsAreaSatisfied = other.IsAreaSatisfied;



        }

        public int NumberofCellsDept
        {
            get { return _numCellsDept; }
            //set { _cellAvailable = value; }
        }
        public double AreaProvided
        {
            get { return _areaGivenDept; }
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
        }


        //CALC THE TOTAL AREA
        public double AreaEachDept()
        {
            double area = 0;
            for(int i=0;i< _progDataList.Count; i++)
            {
                area += (_progDataList[i].Quantity * _progDataList[i].UnitArea);
            }
            return area;
        }


        //CALC THE AREA ALLOCATED
        internal void CalcAreaAllocated()
        {
            _numCellAdded += 1;
            _areaGivenDept = _progDataList[0].GridX *_progDataList[0].GridY * _numCellAdded;

        }


        //CALC NUMBER OF CELLS NEEDED TO PLACE EACH PROGRAM
        public int NumCellsNeededDept()
        {
            int num = 0;
            double cellArea = _progDataList[0].GridX * _progDataList[0].GridY;
            double totalProgramAreas = AreaEachDept();            
            num = (int)(totalProgramAreas / cellArea);
            return num;
        }
    }
}

