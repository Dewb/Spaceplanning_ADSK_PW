
using System.Collections.Generic;


namespace SpacePlanning
{
    public class DeptData
    {

        // Two private variables for example purposes
        private string _deptName;
        private List<ProgramData> _progData;
        private int _numCellsDept;

        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        public DeptData(string programDept, List<ProgramData> programDataList)
        {
            _deptName = programDept;
            _progData = programDataList;
            _numCellsDept = NumCellsNeededDept();
           


        }

        public int NumberofCellsDept
        {
            get { return _numCellsDept; }
            //set { _cellAvailable = value; }
        }

        public string DepartmentName
        {
            get { return _deptName; }
        }

        public List<ProgramData> ProgramsInDept
        {
            get { return _progData; }
        }


        //CALC THE TOTAL AREA AND NUMBER OF CELLS NEEDED TO PLACE EACH DEPARTMENT


        //CALC NUMBER OF CELLS NEEDED TO PLACE EACH PROGRAM
        internal int NumCellsNeededDept()
        {
            int num = 0;
            double cellArea = _progData[0].GridX * _progData[0].GridY;
            double totalProgramAreas = 0;
            for (int i = 0; i < _progData.Count; i++)
            {
                totalProgramAreas += _progData[i].NumberofCellsProgram;
            }
            num = (int)(totalProgramAreas / cellArea);
            return num;
        }
    }
}

