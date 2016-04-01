using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;


namespace SpacePlanning
{
    public class SpaceAnalysis
    {
        
        //provides information related to dept data
        [MultiReturn(new[] { "DepartmentNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "ProgramsInDepts", "PolyAssignedDepts" })]
        public static Dictionary<string, object> DeptAnalytics(List<DeptData> deptData)
        {

            if (deptData == null) return null;
            List<string> deptNameList = new List<string>();
            List<int> numCellsList = new List<int>();
            List<bool> areaSatisfiedList = new List<bool>();
            List<List<ProgramData>> progLists = new List<List<ProgramData>>();
            List<double> areaNeededList = new List<double>();
            List<double> areaProvidedList = new List<double>();
            List<List<Polygon2d>> polyAssignedList = new List<List<Polygon2d>>();
            for (int i = 0; i < deptData.Count; i++)
            {
                deptNameList.Add(deptData[i].DepartmentName);
                numCellsList.Add(deptData[i].NumberofCellsAdded);
                areaSatisfiedList.Add(deptData[i].IsAreaSatisfied);
                progLists.Add(deptData[i].ProgramsInDept);
                areaNeededList.Add(deptData[i].DeptAreaNeeded);
                areaProvidedList.Add(deptData[i].AreaProvided);
                polyAssignedList.Add(deptData[i].PolyDeptAssigned);
            }
            return new Dictionary<string, object>
            {
                { "DepartmentNames", (deptNameList) },
                { "NumCellsTaken", (numCellsList) },
                { "AreaSatisfied", (areaSatisfiedList) },
                { "AreaNeeded", (areaNeededList) },
                { "AreaProvided", (areaProvidedList) },
                { "ProgramsInDepts", (progLists) },
                { "PolyAssignedDepts", (polyAssignedList) }
            };
        }

        //Pprovides information related to program data
        [MultiReturn(new[] { "ProgramNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "Quantity", "PolyAssignedProgs" })]
        public static Dictionary<string, object> ProgramAnalytics(List<ProgramData> progData)
        {
            List<string> progNameList = new List<string>();
            List<int> numCellsList = new List<int>();
            List<bool> areaSatisfiedList = new List<bool>();
            List<double> areaNeededList = new List<double>();
            List<double> areaProvidedList = new List<double>();
            List<double> quantList = new List<double>();
            List<Polygon2d> polyProgList = new List<Polygon2d>();
            for (int i = 0; i < progData.Count; i++)
            {
                progNameList.Add(progData[i].ProgName);
                numCellsList.Add(progData[i].NumberofCellsAdded);
                areaSatisfiedList.Add(progData[i].IsAreaSatisfied);
                areaNeededList.Add(progData[i].AreaNeeded);
                areaProvidedList.Add(progData[i].AreaProvided);
                quantList.Add(progData[i].Quantity);
                polyProgList.Add(progData[i].PolyProgAssigned);
            }


            return new Dictionary<string, object>
            {
                { "ProgramNames", (progNameList) },
                { "NumCellsTaken", (numCellsList) },
                { "AreaSatisfied", (areaSatisfiedList) },
                { "AreaNeeded", (areaNeededList) },
                { "AreaProvided", (areaProvidedList) },
                { "Quantity", (quantList) },
                { "PolyAssignedProgs", (polyProgList) }

            };
        }


    }
}
