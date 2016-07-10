using System;
using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using System.Linq;


namespace SpacePlanning
{
    internal static class BuildLayout3D
    {

        //arranges depts on site and updates dept data object
        /// <summary>
        /// Arranges dept on site by assigning polygon2d's to each dept in the Dept Data object.
        /// Returns Dept polygon2d's, Left Over polygon2d's, Circulation polygon2d's and Updated Dept Data object.
        /// </summary>
        /// <param name="deptData">List of DeptData object.</param>
        /// <param name="buildingOutline">Building outline polygon2d geometry.</param>
        /// <param name="kpuDepthList">Depth of the main department.</param>
        /// <param name="acceptableWidth">Acceptable width in meters while allocating area and polygon2d to each dept on site.</param>
        /// <param name="minNotchDistance">Minimum distance below which an edge will be considered as a removable notch.</param>
        /// <param name="circulationFreq">Value to consider while checking frequency of cirulation computation polygon2d.</param>
        /// <param name="designSeed">Regardless of the recompute value, it is used to restart computing the node every time its value is changed.</param>
        /// <param name="noExternalWall">Boolean toggle to turn on or off requirement of external wall for KPU.</param>
        /// <param name="unlimitedKPU">Boolean toggle to turn on or off unlimied KPU placement.</param>
        /// <returns name="DeptData">Updated Dept Data object</returns>
        /// <returns name="LeftOverPolys">Polygon2d's not assigned to any department.</returns>
        /// <returns name="CirculationPolys">Polygon2d's needed to compute circulation networks.</returns>
        /// <returns name="OtherDeptMainPoly">Polygon2d for all other departments except for the primary department.</returns>
        /// <search>
        /// DeptData object, department arrangement on site
        /// </search>
        [MultiReturn(new[] { "DeptData", "LeftOverPolys" })]//"CirculationPolys", "OtherDeptMainPoly" 
        public static Dictionary<string, object> PlaceDepartments3D(List<DeptData> deptData, List<Polygon2d> buildingOutline, List<double> kpuDepthList, List<double> kpuWidthList,
            double acceptableWidth, double polyDivision = 8, int designSeed = 50, bool noExternalWall = false,
            bool unlimitedKPU = true, bool mode3D = false, double totalBuildingHeight = 60, double avgFloorHeight = 15)
        {


            if (polyDivision >= 1 && polyDivision < 30) { BuildLayout.SPACING = polyDivision; BuildLayout.SPACING2 = polyDivision; }
            double circulationFreq = 8;
            List<DeptData> deptDataInp = deptData;
            deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy
            List<double> heightList = new List<double>();
            if (mode3D == true)
            {
                int numFloors = (int)Math.Floor(totalBuildingHeight / avgFloorHeight);
                for (int i = 0; i < numFloors; i++) heightList.Add((i + 1) * avgFloorHeight);
                Trace.WriteLine("Heightlist formed");
                for (int i = 0; i < deptData.Count; i++)
                {
                    deptData[i].Mode3D = true;
                    deptData[i].FloorHeightList = heightList;
                }
            }
            Dictionary<string, object> deptArrangement = new Dictionary<string, object>();
            double count = 0, eps = 5;
            Random rand = new Random();
            bool deptPlaced = false;
            Random ran = new Random(designSeed);
            bool stackOptionsDept = deptData[0].StackingOptions;
            bool stackOptionsProg = deptData[0].ProgramsInDept[0].StackingOptions;
            while (deptPlaced == false && count < BuildLayout.MAXCOUNT)//MAXCOUNT
            {
                double parameter = BasicUtility.RandomBetweenNumbers(ran, 0.9, 0.5);
                if (!stackOptionsDept) parameter = 0;
                //parameter = 0;
                Trace.WriteLine("PLACE DEPT STARTS , Lets arrange dept again ++++++++++++++++ : " + count);
                deptArrangement = BuildLayout.DeptPlacer(deptData, buildingOutline, kpuDepthList, kpuWidthList, acceptableWidth, circulationFreq, designSeed, noExternalWall, unlimitedKPU, stackOptionsDept, stackOptionsProg, parameter);
                if (deptArrangement != null)
                {
                    List<DeptData> deptDataUpdated = (List<DeptData>)deptArrangement["DeptData"];
                    List<List<Polygon2d>> deptAllPolys = new List<List<Polygon2d>>();
                    for (int i = 0; i < deptDataUpdated.Count; i++) deptAllPolys.Add(deptDataUpdated[i].PolyAssignedToDept);
                    List<Polygon2d> deptPolysTogether = new List<Polygon2d>();
                    for (int i = 0; i < deptAllPolys.Count; i++)
                    {
                        if (ValidateObject.CheckPolyList(deptAllPolys[i])) deptPolysTogether.AddRange(deptAllPolys[i]);
                    }

                    if (deptAllPolys.Count > 0) Trace.WriteLine("dept arrangement not null, lets check further");
                    for (int i = 0; i < deptAllPolys.Count; i++)
                    {
                        List<Polygon2d> eachDeptPoly = deptAllPolys[i];
                        if (ValidateObject.CheckPolyList(eachDeptPoly)) deptPlaced = true;
                        else { deptPlaced = false; Trace.WriteLine("dept arrangement bad polys, rejected"); break; }
                        bool orthoResult = ValidateObject.CheckPolygon2dListOrtho(deptPolysTogether, eps);
                        Trace.WriteLine("The poly formed is : " + orthoResult);
                        if (orthoResult) deptPlaced = true;
                        else { deptPlaced = false; Trace.WriteLine("dept arrangement non orthogonal, rejected"); break; }
                    }
                }
                else
                {
                    deptPlaced = false;
                    designSeed += 1;
                    Trace.WriteLine("DeptPlacer returned null, rejected for: " + count);
                }
                count += 1;
                Trace.WriteLine(" EXIT PLACE DEPARTMENTS +++++++++++++++++++++++++++++++++");
            }// end of while loop
            return deptArrangement;
        }





    }
}
