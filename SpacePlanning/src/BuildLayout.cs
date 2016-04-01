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

    

    public class BuildLayout
    {      
        public static double spacingSet = 6; //higher value makes code faster but less precise 3 was good 4 worked and 1
        public static double spacingSet2 = 1;
        internal static Random ranGenerate = new Random();
        internal static double recurse = 0;
        internal static Point2d reference = new Point2d(0,0);
        internal static int maxCount = 5;
        internal static int maxRound = 5;



        internal double spacingSetOrig
        {
            get
            {
                return spacingSet;
            }
            set
            {
                spacingSet = value;
            }
        }

        internal double spacingSetAnother
        {
            get
            {
                return spacingSet2;
            }
            set
            {
                spacingSet2 = value;
            }
        }

     

        //****DEF - USING THIS TO SPLIT PROGRAMS-------------------------------

        //make a tree to test
        [MultiReturn(new[] { "SpaceTree", "NodeList" })]
        internal static Dictionary<string,object> CreateSpaceTree(int numNodes, Point origin, double spaceX, double spaceY,double radius, double recompute = 5)
        {
            // make root node
            Node root = new Node(0, NodeType.Container, true, origin,radius);            
            List<Node> nodeList = new List<Node>();
            //nodeList.Add(root);
            Random ran = new Random();
            bool tag = true;
            for (int i = 0; i < numNodes-1; i++)
            {
                Node N;
                double val = ran.NextDouble();
                //NodeType ndType = BasicUtility.GenerateNodeType(val);
                NodeType ndType = SpaceDataTree.GenerateBalancedNodeType(tag);
                tag = !tag;
                N = new Node(i + 1, ndType);                        
                nodeList.Add(N);
            }
            //////////////////////////////////////////////////////////////////
            SpaceDataTree tree = new SpaceDataTree(root,origin,spaceX,spaceY);
            Node current = root;
            
            Node nodeAdditionResult = null;
            for (int i = 0; i < nodeList.Count; i++)
            {
              
                if (current.NodeType == NodeType.Space)
                {
                    Trace.WriteLine("Make Sure Space Nodes are childless");
                    //current = current.ParentNode.RightNode;
                    //current = current.RightNode;
                    current = current.ParentNode;
                  
                }


                nodeAdditionResult = tree.addNewNodeSide(current, nodeList[i]);
                string foo = "";
                //nodeAdditionResult = null , means node properly added
                //nodeAdditionResult = current, means, parent node of current is null
                //nodeAdditionResult = some other node means, current should be that other node to add new node
                if (nodeAdditionResult == current)
                {
                    Trace.WriteLine("Parent Node is found Null");
                    break;

                }else if (nodeAdditionResult != current && nodeAdditionResult != null)
                {
                    Trace.WriteLine("Current Should be that other Node");
                    current = nodeAdditionResult;
                }
                else
                {
                    Trace.WriteLine("Node is added properly Yay");
                    current = nodeList[i];
                  
                }
                Trace.WriteLine("+++++++++++++++++++++++++++++++++++++ \\");
                string foo1 = "";
      
    
            }
            Trace.WriteLine("Tree Constructed=====================");
            //return tree;

            return new Dictionary<string, object>
            {
                { "SpaceTree", (tree) },
                { "NodeList", (nodeList) }
            };

        }
        


        /// <summary>
        /// Thisnode places the programs in the dept polygon2d's based on the list from the program document
        /// It returns the input number multiplied by 2.
        /// </summary>
        /// <param name="a">Dept Polygon2d where programs should be placed</param>
        /// <param name="b">Program Data Object containing program information</param>
        /// <search>
        /// split, divide , partition space based on distance
        /// </search>
        public static int TestCode(int a, int b)
        {
            return a + b;
        }



        //make a tree to test
        [MultiReturn(new[] { "SpaceTree", "NodeList" })]
        internal static Dictionary<string, object> CreateSpaceTreeFromDeptData(Node root, List<Node> nodeList,
            Point origin, double spaceX, double spaceY, double radius, bool symettry = true)
        {
            // make root node
            //Node root = new Node(0, NodeType.Container, true, origin, radius);
            SpaceDataTree tree = new SpaceDataTree(root, origin, spaceX, spaceY);
            Node current = root;
            Node nodeAdditionResult = null;
            for (int i = 0; i < nodeList.Count; i++)
            {
                if (current.NodeType == NodeType.Space)
                {
                    //Trace.WriteLine("Make Sure Space Nodes are childless");
                    current = current.ParentNode;
                }
                nodeAdditionResult = tree.addNewNodeSide(current, nodeList[i]);
                //nodeAdditionResult = null , means node properly added
                //nodeAdditionResult = current, means, parent node of current is null
                //nodeAdditionResult = some other node means, current should be that other node to add new node
                if (nodeAdditionResult == current)
                {
                    //Trace.WriteLine("Parent Node is found Null");
                    break;

                }
                else if (nodeAdditionResult != current && nodeAdditionResult != null)
                {
                    //Trace.WriteLine("Current Should be that other Node");
                    current = nodeAdditionResult;
                }
                else
                {
                    //Trace.WriteLine("Node is added properly Yay");
                    current = nodeList[i];

                }
                //Trace.WriteLine("+++++++++++++++++++++++++++++++++++++ \\");

            }
            //Trace.WriteLine("Tree Constructed=====================");
            return new Dictionary<string, object>
            {
                { "SpaceTree", (tree) },
                { "NodeList", (nodeList) }
            };

        }



        //****DEF - USING THIS TO SPLIT DEPTS-------------------------------
        // SPLITS A POLY TO MAKE DEPTS
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "CentralStation", "UpdatedDeptData","SpaceDataTree" })]
        internal static Dictionary<string, object> DeptSplitRefined(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {

            List<List<Polygon2d>> AllDeptPolys = new List<List<Polygon2d>>();
            List<string> AllDepartmentNames = new List<string>();
            List<double> AllDeptAreaAdded = new List<double>();
            Stack<Polygon2d> leftOverPoly = new Stack<Polygon2d>();
            List<Node> AllNodesList = new List<Node>();
            List<Point2d> polyPts = PolygonUtility.SmoothPolygon(poly.Points, spacingSet);
            poly = new Polygon2d(polyPts, 0);

            SortedDictionary<double, DeptData> sortedD = new SortedDictionary<double, DeptData>();
            for (int i = 0; i < deptData.Count; i++) sortedD.Add(deptData[i].AreaEachDept(), deptData[i]);
           
            List<DeptData> sortedDepartmentData = new List<DeptData>();
            foreach (KeyValuePair<double, DeptData> p in sortedD) sortedDepartmentData.Add(p.Value);
            sortedDepartmentData.Reverse();

            leftOverPoly.Push(poly);
            int dir = 0;
            double count3 = 0;

            for (int i = 0; i < sortedD.Count; i++)
            {
                DeptData deptItem = sortedDepartmentData[i];
                double areaDeptNeeds = deptItem.DeptAreaNeeded;
                double areaAddedToDept = 0;
                double areaLeftOverToAdd = areaDeptNeeds - areaAddedToDept;
                double areaCurrentPoly = 0;
                double perc = 0.2;
                double limit = areaDeptNeeds * perc;

                Polygon2d currentPolyObj = poly;
                List<Polygon2d> everyDeptPoly = new List<Polygon2d>();
                List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
                double count1 = 0;
                double count2 = 0;
                double areaCheck = 0;

                Random ran = new Random();
                Node spaceNode, containerNode;

                // when dept is inpatient unit
                if (i == 0)
                {
                    while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0 && count1 < maxRound)
                    {
                        dir = BasicUtility.ToggleInputInt(dir);
                        currentPolyObj = leftOverPoly.Pop();
                        areaCurrentPoly = PolygonUtility.AreaCheckPolygon(currentPolyObj);
                        Dictionary<string,object> splitReturned = SplitByDistance(currentPolyObj, ran, offset, dir);
                        List<Polygon2d> edgeSplitted = (List<Polygon2d>)splitReturned["PolyAfterSplit"];

                        if (edgeSplitted == null) return null;
                        double areaA = PolygonUtility.AreaCheckPolygon(edgeSplitted[0]);
                        double areaB = PolygonUtility.AreaCheckPolygon(edgeSplitted[1]);
                        if (areaA < areaB)
                        {
                            everyDeptPoly.Add(edgeSplitted[0]);
                            areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                            areaCheck += areaA;
                            leftOverPoly.Push(edgeSplitted[1]);
                        }
                        else
                        {
                            everyDeptPoly.Add(edgeSplitted[1]);
                            areaLeftOverToAdd = areaLeftOverToAdd - areaB;
                            areaCheck += areaB;
                            leftOverPoly.Push(edgeSplitted[0]);
                        }
                        count1 += 1;
                    }

                    spaceNode = new Node(i, NodeType.Space);
                    containerNode = new Node(i, NodeType.Container);
                    AllNodesList.Add(spaceNode);
                    AllNodesList.Add(containerNode);
                }
                //when other depts 
                else
                {
                    Random rn = new Random();
                    while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0 && count2 < maxRound)
                    {
                        double ratio = BasicUtility.RandomBetweenNumbers(rn, 0.85, 0.15);
                        currentPolyObj = leftOverPoly.Pop();
                        areaCurrentPoly = PolygonUtility.AreaCheckPolygon(currentPolyObj);
                        dir = BasicUtility.ToggleInputInt(dir);
                        if (areaLeftOverToAdd > areaCurrentPoly)
                        {
                            everyDeptPoly.Add(currentPolyObj);
                            areaLeftOverToAdd = areaLeftOverToAdd - areaCurrentPoly;
                            areaCheck += areaCurrentPoly;
                            //Trace.WriteLine("Area left over after assigning when area is greater than current : " + areaLeftOverToAdd);
                        }
                        else
                        {
                            Dictionary<string, object> basicSplit = SplitByRatio(currentPolyObj, ratio, dir); 
                            if(basicSplit == null) return null;
                            List<Polygon2d> polyS = (List<Polygon2d>)basicSplit["PolyAfterSplit"];
                            double areaA = PolygonUtility.AreaCheckPolygon(polyS[0]);
                            double areaB = PolygonUtility.AreaCheckPolygon(polyS[1]);
                            if (areaA < areaB)
                            {
                                everyDeptPoly.Add(polyS[0]);
                                areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                                areaCheck += areaA;
                                leftOverPoly.Push(polyS[1]);
                            }
                            else
                            {
                                everyDeptPoly.Add(polyS[1]);
                                areaLeftOverToAdd = areaLeftOverToAdd - areaB;
                                areaCheck += areaB;
                                leftOverPoly.Push(polyS[0]);
                            }

                            spaceNode = new Node(i, NodeType.Space);
                            containerNode = new Node(i, NodeType.Container);
                            AllNodesList.Add(spaceNode);
                            AllNodesList.Add(containerNode);
                        }
                        count2 += 1;
                    } // end of while loop
                }

                AllDeptAreaAdded.Add(areaCheck);
                AllDeptPolys.Add(everyDeptPoly);
                AllDepartmentNames.Add(deptItem.DepartmentName);

            }// end of for loop

            Random ran2 = new Random();
            //for any left over poly
            double minArea = 10, areaMoreCheck = 0;
            if (leftOverPoly.Count > 0)
            {
                while (leftOverPoly.Count > 0 && count3 < maxRound)
                {
                    dir = BasicUtility.ToggleInputInt(dir);
                    Polygon2d currentPolyObj = leftOverPoly.Pop();
                    if (!PolygonUtility.CheckPolyDimension(currentPolyObj))
                    {
                        count3 += 1;
                        continue;
                    }
                    double areaCurrentPoly = PolygonUtility.AreaCheckPolygon(currentPolyObj);
                    Dictionary<string,object> splitReturned = SplitByDistance(currentPolyObj, ran2, offset, dir);
                    List<Polygon2d> edgeSplitted = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
                    if (edgeSplitted == null) return null;
                    if (!PolygonUtility.CheckPolyDimension(edgeSplitted[0]))
                    {
                        count3 += 1;
                        continue;
                    }
                    double areaA = PolygonUtility.AreaCheckPolygon(edgeSplitted[0]);
                    double areaB = PolygonUtility.AreaCheckPolygon(edgeSplitted[1]);                                               

                    if (areaA < areaB)
                    {
                        AllDeptPolys[0].Add(edgeSplitted[0]);
                        areaMoreCheck += areaA;
                        if (areaB > minArea) leftOverPoly.Push(edgeSplitted[1]); 
                    }
                    else
                    {
                        AllDeptPolys[0].Add(edgeSplitted[1]);
                        areaMoreCheck += areaB;
                        if (areaA > minArea) { leftOverPoly.Push(edgeSplitted[0]); }
                    }
                    count3 += 1;
                }// end of while loop
            }// end of if loop for leftover count
            AllDeptAreaAdded[0] += areaMoreCheck;     
            
            // adding the left over polys to the 2nd highest dept after inpatient
            if(leftOverPoly.Count > 0)
            {
                double areaLeftOver = 0;
                for (int i = 0; i < leftOverPoly.Count; i++)
                {
                    Polygon2d pol = leftOverPoly.Pop();
                    areaLeftOver += GraphicsUtility.AreaPolygon2d(pol.Points);                   
                    AllDeptPolys[1].Add(pol);
                }
                AllDeptAreaAdded[1] += areaLeftOver;
            }
            
            List<DeptData> UpdatedDeptData = new List<DeptData>();
            //make the sorted dept data
            for (int i = 0; i < sortedDepartmentData.Count; i++)
            {
                DeptData newDeptData = new DeptData(sortedDepartmentData[i]);
                newDeptData.AreaProvided = AllDeptAreaAdded[i];
                newDeptData.PolyDeptAssigned = AllDeptPolys[i];
                UpdatedDeptData.Add(newDeptData);
            }
            
            List<Polygon2d> AllLeftOverPolys = new List<Polygon2d>();
            AllLeftOverPolys.AddRange(leftOverPoly);

            //make the centralStation 
            Point2d centerPt = PolygonUtility.CentroidFromPoly(poly.Points);
            Dictionary<string,object>  centralPolyLists = MakeCentralStation(AllDeptPolys[0], centerPt);
            int index = (int)centralPolyLists["IndexInPatientPoly"];
            List<Polygon2d> polyReturned = (List<Polygon2d>)centralPolyLists["PolyCentral"];

            if (polyReturned.Count > 1)
            {
                AllDeptPolys[0].RemoveAt(index);
                AllDeptPolys[0].Add(polyReturned[1]); 
            }
            else AllDeptPolys[0][index] = polyReturned[0];            
            
            //create space data tree
            double spaceX = 22;
            double spaceY = 13;
            double nodeRadius = 4;
            Point origin = Point.ByCoordinates(500, 0);
            Node root = new Node(0, NodeType.Container, true, origin, nodeRadius);
            Dictionary<string, object> SpaceTreeData = CreateSpaceTreeFromDeptData(root, AllNodesList, origin, spaceX, spaceY, nodeRadius, true);
          
            Trace.WriteLine("Dept Splitting Done ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            //return polyList;
            
            return new Dictionary<string, object>
            {
                { "DeptPolys", (AllDeptPolys) },
                { "LeftOverPolys", (AllLeftOverPolys) },
                { "CentralStation", (polyReturned[0]) },
                { "UpdatedDeptData", (UpdatedDeptData)},
                { "SpaceDataTree", (SpaceTreeData) }
            };


        }

        //gets list of polygon2ds and find the most closest polygon2d to the center , to place the central stn
        internal static Dictionary<string, object> MakeCentralStation(List<Polygon2d> polygonsList, Point2d centerPt)
        {
            if (polygonsList == null) return null;
            List<Polygon2d> newPolyLists = new List<Polygon2d>();
            List<double> distanceList = new List<double>();
            double minArea = 100, ratio = 0.5, dis = 0; ; int dir = 0;
            List<int> indices = PolygonUtility.SortPolygonsFromAPoint(polygonsList, centerPt);
            Polygon2d polyToPlace = polygonsList[indices[0]];
            double area = PolygonUtility.AreaCheckPolygon(polyToPlace);
            if(area > minArea)
            {
                List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(polyToPlace.Points);
                if (spans[0] > spans[1]) {
                    dir = 1;
                    dis = spans[0] / 2;
                } else {
                    dir = 0;
                    dis = spans[1] / 2;
                }
                Random ran = new Random();
                //Dictionary<string, object> splittedPoly = BasicSplitPolyIntoTwo(polyToPlace, ratio, dir);                
                //Dictionary<string, object> splittedPoly = SplitByDistanceFromPoint(polyToPlace, dis, dir);
                Dictionary<string, object> splittedPoly = SplitByDistance(polyToPlace, ran, dis, dir);
                List<Polygon2d> polyReturnedList = (List<Polygon2d>)splittedPoly["PolyAfterSplit"];
                List<int> ind = PolygonUtility.SortPolygonsFromAPoint(polyReturnedList, centerPt);
                newPolyLists.Add(polyReturnedList[ind[0]]);
                newPolyLists.Add(polyReturnedList[ind[1]]);
            }
            else
            {
                newPolyLists.Add(polyToPlace);
            }
            return new Dictionary<string, object>
            {
                { "PolyCentral", (newPolyLists) },
                { "IndexInPatientPoly", (indices[0]) }
            };
            
        }


        //arrange depts on site, till all depts have not been satisfied
        /// <summary>
        /// It arranges the dept on the site based on input from program document
        /// Returns the dept polygon2d and dept  .
        /// </summary>
        /// <param name="Poly">Site Outline where space planning to be placed</param>
        /// <param name="DeptData">Dept Data Object from csv file</param>
        /// <param name="CellsInside">List of Cell Objects inside the site outline</param>
        /// <param name="Offset">Distance of Space Division</param>
        /// <param name="Recompute">Run the function again</param>
        /// <returns name="DeptPolys">Polys for each depts</param>
        /// <returns name="LeftOverPolys">Polys which has not been assigned to any dept</param>
        /// <returns name="CentralStation">Poly for central nurse station or lobby or foyer</param>
        /// <returns name="UpdatedDeptData">Updated Dept Data Object having </param>
        /// <returns name="SpaceDataTree">Space Data Structure - a Binary Tree </param>
        /// <search>
        /// dept data arrangement on sie
        /// </search>
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "CentralStation", "UpdatedDeptData","SpaceDataTree" })]
        public static Dictionary<string, object> DeptArrangeOnSite(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {
            Dictionary<string, object> deptArrangement = new Dictionary<string, object>();
            //deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, 1);
            double count = 0;            
            Random rand = new Random();
            bool deptPlaced = false;
            while(deptPlaced == false && count < maxCount)
            {
                Trace.WriteLine("Lets Go Again for : " + count);
                int reco = rand.Next();
                deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, reco);
                if(deptArrangement != null)
                {
                    List<List<Polygon2d>> deptAllPolys =(List<List<Polygon2d>>) deptArrangement["DeptPolys"];
                    for(int i = 0; i < deptAllPolys.Count; i++)
                    {
                        List<Polygon2d> eachDeptPoly = deptAllPolys[i];
                        if(eachDeptPoly != null)
                        {                        
                            for(int j = 0; j < eachDeptPoly.Count; j++)
                            {
                                Polygon2d polyItem = eachDeptPoly[j];
                                if(polyItem == null || polyItem.Points == null)
                                {
                                    deptPlaced = false;
                                    break;
                                } else if (polyItem.Points.Count == 0)
                                {
                                    deptPlaced = false;
                                    break;
                                } else
                                {
                                    deptPlaced = true;
                                }     
                            }
                        }
                        else
                        {
                            deptPlaced = false;
                            break;
                        }
                    }
                }
                count += 1;
            }
            return deptArrangement;
        }

  
        //dept arrange on site till it returns not a null value  
        internal static Dictionary<string, object> DeptArrangeOnSiteSingleOut(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {
            Dictionary<string, object> deptArrangement = null;
            double count = 0;
            int maxCount = 10;
            Random rand = new Random();
            while (deptArrangement == null && count < maxCount)
            {
                Trace.WriteLine("Lets Go Again for : " + count);
                int reco = rand.Next();
                deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, reco);
                count += 1;
            }
            return deptArrangement;

        }


        //splits a big poly in a single direction
        /// <summary>
        /// Thisnode places the programs in the dept polygon2d's based on the list from the program document
        /// It returns the input number multiplied by 2.
        /// </summary>
        /// <param name="PolyInputList">Dept Polygon2d where programs should be placed</param>
        /// <param name="ProgData">Program Data Object containing program information</param>
        /// <param name="distance">Distance of Program Spaces</param>
        /// <param name="recompute">Compute the function again</param>
        /// <returns name="PolyAfterSplit">Polys after splitting the dept into programs</param>
        /// <returns name="UpdatedProgramData">Updated Program Data Object</param>
        /// <search>
        /// split, divide , partition space based on distance
        /// </search>
        [MultiReturn(new[] { "PolyAfterSplit", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursiveSplitProgramsOneDirByDistance(List<Polygon2d> polyInputList, List<ProgramData> progData, double distance, int recompute = 1)
        {
            int dir = 0;
            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();

            for (int j = 0; j < progData.Count; j++)
            {
                programDataRetrieved.Push(progData[j]);
            }

            for (int i = 0; i < polyInputList.Count; i++)
            {
                Polygon2d poly = polyInputList[i];
                if (!PolygonUtility.CheckPoly(poly)) continue;
                List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);               
                double setSpan = 1000000000000;                
                if (spans[0] > spans[1])
                {
                    dir = 1;
                    setSpan = spans[0];
                }
                else
                {
                    dir = 0;
                    setSpan = spans[1];
                }

                Polygon2d currentPoly = poly;
                int count = 0;
                Random ran2 = new Random();
                while (setSpan > 0 && programDataRetrieved.Count > 0)
                {
                    ProgramData progItem = programDataRetrieved.Pop();
                    Dictionary<string, object> splitReturn = SplitByDistance(currentPoly, ran2, distance, dir);
                    List<Polygon2d> polyAfterSplitting = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    double selectedArea = 0;
                    double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                    double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[1].Points);
                    if (area1 > area2)
                    {
                        currentPoly = polyAfterSplitting[0];
                        if (polyAfterSplitting[1] == null) break;
                        polyList.Add(polyAfterSplitting[1]);
                        progItem.AreaProvided = area1;
                        areaList.Add(area2);
                        selectedArea = area2;
                    }
                    else
                    {
                        currentPoly = polyAfterSplitting[1];
                        polyList.Add(polyAfterSplitting[0]);
                        progItem.AreaProvided = area2;
                        areaList.Add(area1);
                        selectedArea = area1;
                    }
                    if (currentPoly.Points == null) break;
                    setSpan -= distance;
                    count += 1;
                }// end of while loop


            }// end of for loop
            List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count) progNew.PolyProgAssigned = polyList[i];
                else progNew.PolyProgAssigned = null;
                UpdatedProgramDataList.Add(progNew);
            }
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "UpdatedProgramData",(UpdatedProgramDataList) }
            };
        }

        //splits a poly in a single direction
        internal static Dictionary<string, object> RecursiveSplitProgramsOneDirByDistanceSingleOut(List<Polygon2d> polyInputList,
            List<ProgramData> progData, double distance, int recompute = 1)
        {
            return RecursiveSplitProgramsOneDirByDistance(polyInputList, progData, distance, recompute);

        }

        

       
        //splits a polygon into two based on ratio and dir
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints"})]
        internal static Dictionary<string, object> SplitByRatio(Polygon2d polyOutline, double ratio = 0.5, int dir = 0)
        {
            if(polyOutline == null) return null;
            if(polyOutline != null && polyOutline.Points == null) return null;

            double extents = 5000;
            double minimumLength = 2, minWidth = 10, aspectRatio = 0, eps = 0.1;
            List<Point2d> polyOrig = polyOutline.Points;          
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacingSet);
            List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly);
            double horizontalSpan = spans[0], verticalSpan = spans[1];
            Point2d polyCenter = PolygonUtility.CentroidFromPoly(poly);
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength) return null;

            if (horizontalSpan > verticalSpan) { dir = 1; aspectRatio = horizontalSpan / verticalSpan; }
            else { dir = 0; aspectRatio = verticalSpan / horizontalSpan; }         

            // adjust ratio
            if (ratio < 0.15) ratio = ratio + eps;
            if (ratio > 0.85) ratio = ratio - eps;

            if(horizontalSpan < minWidth || verticalSpan < minWidth) ratio = 0.5;
            Line2d splitLine = new Line2d(polyCenter, extents, dir);
            double shift = ratio - 0.5;
            if (dir == 0) splitLine = LineUtility.move(splitLine,0, shift * verticalSpan);
            else splitLine = LineUtility.move(splitLine,shift * horizontalSpan, 0);

            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, spacingSet);
            List<Point2d> intersectedPoints = (List<Point2d>)intersectionReturn["IntersectedPoints"];
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) }
            };

        }



        //used to split Depts into Program Spaces based on recursive poly split grid 
        // USING NOW 
        //splits a ploy into two based on dist and dir, selects the starting pt and side randomly
        /// <summary>
        /// Thisnode places the programs in the dept polygon2d's based on the list from the program document
        /// It returns the input number multiplied by 2.
        /// </summary>
        /// <param name="PolyInputList">Dept Polygon2d where programs should be placed</param>
        /// <param name="ProgData">Program Data Object containing program information</param>
        /// <returns name="AcceptableMinDim">Minimum acceptable dimension of any placed program space</param>
        /// <search>
        /// split, divide , partition space based on distance
        /// </search>
        [MultiReturn(new[] { "PolyAfterSplit", "BigPolysAfterSplit", "CirculationPolygons", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursivePlaceProgramsSeries(List<Polygon2d> PolyInputList, 
            List<ProgramData> ProgData, double AcceptableMinDim, int factor = 4, int recompute = 0)
        {
            int fac = 5;
            Random ran = new Random();
            List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();
            //push programdata into the stack
            for (int j = 0; j < ProgData.Count; j++) { programDataRetrieved.Push(ProgData[j]); }
            List<Polygon2d> polyOrganizedList = new List<Polygon2d>(), polyCoverList = new List<Polygon2d>();
            double max = AcceptableMinDim + AcceptableMinDim / fac, min = AcceptableMinDim - AcceptableMinDim / fac;
            double acceptWide = BasicUtility.RandomBetweenNumbers(ran, max, min);
            Dictionary<string,object> polySplit = SplitBigPolys(PolyInputList, acceptWide, factor);
            polyOrganizedList = (List<Polygon2d>)polySplit["PolySpaces"];
            polyCoverList = (List<Polygon2d>)polySplit["PolyForCirculation"];

            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < ProgData.Count; i++)
            {
                newProgDataList.Add(new ProgramData(ProgData[i]));
            }
            polyList = AssignPolysToProgramData(newProgDataList, polyOrganizedList);            
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "BigPolysAfterSplit", (polyOrganizedList) },
                { "CirculationPolygons", (polyCoverList) },
                { "UpdatedProgramData",(newProgDataList) }
            };


        }

        // from a list of given polys  it assigns each program a list of polys till its area is satisfied
        internal static List<List<Polygon2d>> AssignPolysToProgramData(List<ProgramData> newProgDataList, List<Polygon2d> polygonLists)
        {

          
            //reset the area provided to the input progdata
            for (int i = 0; i < newProgDataList.Count; i++)
            {
                newProgDataList[i].AreaProvided = 0;
            }
            List<List<Polygon2d>> polyEachProgramList = new List<List<Polygon2d>>();
            Stack <Polygon2d> polyStack = new Stack<Polygon2d>();
            for (int i = 0; i < polygonLists.Count; i++) { polyStack.Push(polygonLists[i]); }
         
            for (int i =0; i < newProgDataList.Count; i++)
            {
                ProgramData progItem = newProgDataList[i];
                Trace.WriteLine("Starting Porg Data : " + i + "///////////");
                bool added = false;
                //double areaProgram = progData[i].CurrentAreaNeeds;
                List<Polygon2d> polysForProg = new List<Polygon2d>();
                while (progItem.CurrentAreaNeeds>0 && polyStack.Count>0)
                {
                    Trace.WriteLine("  = = now in while = = ");
                    Polygon2d currentPoly = polyStack.Pop();
                    double currentArea = PolygonUtility.AreaCheckPolygon(currentPoly);
                    progItem.AddAreaToProg(currentArea);
                    polysForProg.Add(currentPoly);
                    Trace.WriteLine("Area Given Now is : " + progItem.AreaAllocatedValue);
                    Trace.WriteLine("Area Left over to Add :" + progItem.CurrentAreaNeeds);
                    added = true;
                }

                //dummy is just to make sure the function re reuns when slider is hit
                if(added) polyEachProgramList.Add(polysForProg);
                if (!added) Trace.WriteLine("Did not add.  PolyStack Left : " + polyStack.Count + " | Current area needs were : " + progItem.CurrentAreaNeeds);
                Trace.WriteLine("++++++++++++++++++++++++++++++");
            }
            
            return polyEachProgramList;
        }


        //used to split Depts into Program Spaces based on cell grids
        [MultiReturn(new[] { "PolyAfterSplit", "BigPolysAfterSplit", "UpdatedProgramData" })]
        internal static Dictionary<string, object> RecursivePlaceProgramsGridCells(List<Polygon2d> polyInputList,
            List<ProgramData> progData, double dimX = 3, double dimY = 3)
        {

            List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
            List<double> areaList = new List<double>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();
            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData newProgData = new ProgramData(progData[i]);
                newProgDataList.Add(newProgData);
            }
            List<Polygon2d> polyOrganizedList = new List<Polygon2d>();
            for (int i = 0; i < polyInputList.Count; i++)
            {
                Polygon2d pol = polyInputList[i];
                List<Point2d> bbox = ReadData.FromPointsGetBoundingPoly(pol.Points);
                Dictionary<string, object> cellObject = GridObject.GridPointsInsideOutline(bbox, pol.Points, dimX, dimY);
                List<Point2d> point2dList = (List<Point2d>)cellObject["PointsInsideOutline"];
                List<Polygon2d> polyGridList = GridObject.MakeCellsFromGridPoints2d(point2dList, dimX, dimY);
                polyOrganizedList.AddRange(polyGridList);
            }
            
            polyList = AssignPolysToProgramData(newProgDataList, polyOrganizedList);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "BigPolysAfterSplit", (polyOrganizedList) },
                { "UpdatedProgramData",(newProgDataList) }
            };


        }

     


        //gets a poly and recursively splits it till acceptabledimension is met and makes a polyorganized list
        internal static void MakePolysOfProportion(Polygon2d poly, List<Polygon2d> polyOrganizedList, 
            List<Polygon2d> polycoverList, double acceptableWidth, double targetArea)
        {
            recurse += 1;
            List<double> spanListXY = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);
            double spanX = spanListXY[0], spanY = spanListXY[1];
            double aspRatio = spanX / spanY;
            double lowRange = 0.3, highRange = 2;
            double maxValue = 0.70, minValue = 0.35;
            double threshDistanceX = acceptableWidth;
            double threshDistanceY = acceptableWidth;
            Random ran = new Random();
            bool square = true;
            double div;
            div = BasicUtility.RandomBetweenNumbers(ranGenerate, maxValue, minValue);
            if (spanX > threshDistanceX && spanY > threshDistanceY) square = false;
            else {
                if (aspRatio > lowRange && aspRatio < highRange) square = false;
                else square = true;
            }
            if (square) polyOrganizedList.Add(poly);
            else
            {
                Dictionary<string, object> splitResult;
                //poly is rectangle so split it into two and add
                if (spanX > spanY)
                {
                    double dis = spanY * div;
                    int dir = 1;
                    //splitResult = BasicSplitPolyIntoTwo(poly, 0.5, dir);
                    splitResult = SplitByDistance(poly, ran, dis, dir,spacingSet2);
                    //splitResult = SplitByDistanceFromPoint(poly, dis, dir);
                }
                else
                {
                    double dis = spanX * div;
                    int dir = 0;
                    //splitResult = BasicSplitPolyIntoTwo(poly, 0.5, dir);
                    splitResult = SplitByDistance(poly,ran, dis, dir,spacingSet2);
                    //splitResult = SplitByDistanceFromPoint(poly, dis, dir);
                }

                List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitResult["PolyAfterSplit"];
                double areaPolA = PolygonUtility.AreaCheckPolygon(polyAfterSplit[0]);
                double areaPolB = PolygonUtility.AreaCheckPolygon(polyAfterSplit[1]);
                if (areaPolA > targetArea) polycoverList.Add(polyAfterSplit[0]);
                if (areaPolB > targetArea) polycoverList.Add(polyAfterSplit[1]);
               
                List<double> spanA = PolygonUtility.GetSpansXYFromPolygon2d(polyAfterSplit[0].Points);
                List<double> spanB = PolygonUtility.GetSpansXYFromPolygon2d(polyAfterSplit[1].Points);
                Trace.WriteLine("Recurse is : " + recurse);
                if (recurse < 1500)
                {
                    if ((spanA[0] > 0 && spanA[1] > 0) || (spanB[0] > 0 && spanB[1] > 0))
                    {
                        if (spanA[0] > acceptableWidth && spanA[1] > acceptableWidth)
                        {
                            MakePolysOfProportion(polyAfterSplit[0], polyOrganizedList, polycoverList,acceptableWidth,targetArea);
                        }
                        else
                        {
                            polyOrganizedList.Add(polyAfterSplit[0]);
                            double areaPoly = PolygonUtility.AreaCheckPolygon(polyAfterSplit[0]);
                        }
                        //end of 1st if
                        if (spanB[0] > acceptableWidth && spanB[1] > acceptableWidth)
                        {
                            MakePolysOfProportion(polyAfterSplit[1], polyOrganizedList, polycoverList, acceptableWidth,targetArea);
                        }
                        else
                        {
                            polyOrganizedList.Add(polyAfterSplit[1]);
                        }
                        //end of 2nd if                        
                    }
                    else
                    {
                        polyOrganizedList.Add(polyAfterSplit[0]);
                        polyOrganizedList.Add(polyAfterSplit[1]);
                    }
                }
            }
        }// end of function




        //uses makepolysofproportion function to split one big poly into sub components
        internal static Dictionary<string,object> SplitBigPolys(List<Polygon2d> polyInputList, double acceptableWidth, double factor = 4)
        {
            List<Polygon2d> polyOrganizedList = new List<Polygon2d>(), polyCoverList = new List<Polygon2d>();
            int count = 0;
            for (int i = 0; i < polyInputList.Count; i++)
            {
                Polygon2d poly = polyInputList[i];
                if (poly == null || poly.Points == null || poly.Points.Count == 0)
                {
                    count += 1;
                    continue;
                }
                double targetArea = PolygonUtility.AreaCheckPolygon(poly) / factor;
                MakePolysOfProportion(poly, polyOrganizedList, polyCoverList, acceptableWidth,targetArea);
            }
            recurse = 0;
            return new Dictionary<string, object>
            {
                { "PolySpaces", (polyOrganizedList) },
                { "PolyForCirculation", (polyCoverList) }
            };
        }

        

        //splits a polygon based on distance and random direction
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints"})]
        internal static Dictionary<string, object> SplitByDistance(Polygon2d polyOutline, Random ran, double distance = 10, int dir = 0, double spacing =0)
        {       
            if (!PolygonUtility.CheckPoly(polyOutline)) return null;
            double extents = 5000, spacingProvided;
            List<Point2d> polyOrig = polyOutline.Points;
            if (spacing == 0) spacingProvided = spacingSet;
            else spacingProvided = spacing;

            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacingProvided);
            if (!PolygonUtility.CheckPointList(poly)) return null;
            Dictionary<int, object> obj = PolygonUtility.PointSelector(ran,poly);
            Point2d pt = (Point2d)obj[0];
            int orient = (int)obj[1];
            Line2d splitLine = new Line2d(pt, extents, dir);          

            // push this line right or left or up or down based on ratio
            if (dir == 0) splitLine = LineUtility.move(splitLine,0, orient * distance);
            else splitLine = LineUtility.move(splitLine,orient * distance, 0);

            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine,spacingProvided);
            List<Point2d> intersectedPoints =(List<Point2d>)intersectionReturn["IntersectedPoints"];
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];
            
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) }
            };

        }


        //makes intersections and returns the two polygon2ds after intersection
        internal static Dictionary<string,object> MakeIntersections(List<Point2d> poly,Line2d splitLine,double space)
        {
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);
            // find all points on poly which are to the left or to the right of the line
            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check) pIndexA.Add(i);
                else pIndexB.Add(i);
            }

            //organize the points to make closed poly
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);
            List<Polygon2d> splittedPoly = PolygonUtility.OptimizePolyPoints(sortedA, sortedB, true, space);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "IntersectedPoints", (intersectedPoints) }
            };

        }

     
        //splits a polygon by a line 
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine" })]
        internal static Dictionary<string, object> SplitByLine(Polygon2d polyOutline, Line2d inputLine, double distance = 5)
        {

            if (!PolygonUtility.CheckPoly(polyOutline)) return null;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacingSet);
            Line2d splitLine = new Line2d(inputLine);
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(poly);
            bool checkSide = GraphicsUtility.CheckPointSide(splitLine, centerPoly);
            int orient = GraphicsUtility.CheckLineOrient(splitLine);
            if (orient == 0)
            {
                if (!checkSide) splitLine = LineUtility.move(splitLine, 0, -1 * distance);
                else splitLine = LineUtility.move(splitLine, 0, 1 * distance);
            } else
            {
                if (checkSide) splitLine = LineUtility.move(splitLine, -1 * distance, 0);
                else splitLine = LineUtility.move(splitLine, 1 * distance, 0);
            }         

            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, spacingSet);
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) }
            };
        }


 
        //splits a polygon into two based on direction and distance from the lowest pt in the poly
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints" })]
        internal static Dictionary<string, object> SplitByDistanceFromPoint(Polygon2d polyOutline,double distance = 10, int dir = 0)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0) return null;
            double extents = 5000; 
            int threshValue = 20;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = new List<Point2d>();
            if (polyOrig.Count > threshValue) { poly = polyOrig; }
            else { poly = PolygonUtility.SmoothPolygon(polyOrig, spacingSet2); } 

            if (poly == null || poly.Count == 0) return null;
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);// THIS IS BETTER THAN THE OTHER VER
            Point2d lowPt = poly[lowInd];   
            Line2d splitLine = new Line2d(lowPt, extents, dir);
            if (dir == 0) splitLine = LineUtility.move(splitLine, 0, 1 * distance);
            else splitLine = LineUtility.move(splitLine, 1 * distance, 0);



            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, spacingSet2);
            List<Point2d> intersectedPoints = (List<Point2d>)intersectionReturn["IntersectedPoints"];
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) }
            };

        }


      

    }
}
