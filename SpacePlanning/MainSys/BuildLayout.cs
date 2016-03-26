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
        

        internal static double spacingSet = 1; //higher value makes code faster but less precise 0.2 was good
        internal static double spacingSet2 = 5; // used for SplitByDistancePoly
        internal static double recurse = 0;
        internal static Point2d reference = new Point2d(0,0);
             

        //****DEF - USING THIS TO SPLIT PROGRAMS--------------------------------
        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursiveSplitPolyPrograms(List<Polygon2d> polyInputList,List<ProgramData> progData, double ratioA = 0.5, int recompute = 1)
        {
            double ratio = 0.5;
            List<Polygon2d> polyList = new List<Polygon2d>();            
            List<Point2d> pointsList = new List<Point2d>();
            List<double> areaList = new List<double>();
            Stack<Polygon2d> polyRetrieved = new Stack<Polygon2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();

            for(int j = 0; j < progData.Count; j++) { programDataRetrieved.Push(progData[j]); }

            for(int i=0; i < polyInputList.Count; i++)
            {
                Polygon2d currentPoly, poly = polyInputList[i];
                if (poly == null || poly.Points == null || poly.Points.Count == 0) return null;
         
                polyRetrieved.Push(poly);
                int count = 0, dir = 1;
                double areaThreshold = 1000, maximum = 0.9, minimum = 0.3;

                List<Polygon2d> polyAfterSplit = null;
                Dictionary<string, object> splitReturn = null;
                Random rand = new Random();
                while (polyRetrieved.Count > 0 && programDataRetrieved.Count>0)
                {
                    ProgramData progItem = programDataRetrieved.Pop();
                    ratio = rand.NextDouble() * (maximum - minimum) + minimum;
                    currentPoly = polyRetrieved.Pop();
                    try
                    {
                        splitReturn = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                        polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    }
                    catch (Exception)
                    {
                        dir = BasicUtility.toggleInputInt(dir);
                        splitReturn = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                        if (splitReturn == null) { Trace.WriteLine("Could Not Split"); continue; }
                        polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                        //throw;
                    }
                    List<List<Point2d>> pointsOnPoly = (List<List<Point2d>>)splitReturn["EachPolyPoint"];
                    double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                    double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                    if (area1 > areaThreshold)
                    {
                        polyRetrieved.Push(polyAfterSplit[0]);
                        polyList.Add(polyAfterSplit[0]);
                        progItem.AreaProvided = area1;
                        pointsList.AddRange(pointsOnPoly[0]);
                        areaList.Add(area1);
                    }
                    if (area2 > areaThreshold)
                    {
                        polyRetrieved.Push(polyAfterSplit[1]);
                        polyList.Add(polyAfterSplit[1]);
                        progItem.AreaProvided = area2;
                        pointsList.AddRange(pointsOnPoly[1]);
                        areaList.Add(area2);
                    }
                    dir = BasicUtility.toggleInputInt(dir);
                    count += 1;
                }// end of while loop
            }//end of for loop

            List<ProgramData> AllProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count) progNew.PolyProgAssigned = polyList[i];
                else progNew.PolyProgAssigned = null;              
                AllProgramDataList.Add(progNew);
            }
            //polyList = Polygon2d.PolyReducePoints(polyList);
            //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) },
                { "UpdatedProgramData", (AllProgramDataList) }                
            };


        }


        //RECURSIVE SPLITS A POLY
        public static Dictionary<string, object> RecursiveSplitPolyProgramsSingleOut(List<Polygon2d> polyInputList, 
            List<ProgramData> progData, double ratioA = 0.5, int recompute = 1)
        {
            return RecursiveSplitPolyPrograms(polyInputList, progData, ratioA, recompute);            
        }


        //****DEF - USING THIS TO SPLIT PROGRAMS-------------------------------

        //make a tree to test
        [MultiReturn(new[] { "SpaceTree", "NodeList" })]
        public static Dictionary<string,object> CreateSpaceTree(int numNodes, Point origin, double spaceX, double spaceY,double radius, double recompute = 5)
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
                NodeType ndType = BasicUtility.GenerateBalancedNodeType(tag);
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

        internal static List<Polygon2d> BasicSplitWrapper(Polygon2d currentPoly, double ratio, int dir)
        {

            Dictionary<string, object> splitReturned = null;
            List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
            try
            {
                splitReturned = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
            }
            catch (Exception)
            {
                //toggle dir between 0 and 1
                dir = BasicUtility.toggleInputInt(dir);
                splitReturned = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                if (splitReturned == null)
                {
                    //Trace.WriteLine("Split Polys did not work");
                    return null;
                }
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
                //throw;
            }

            return polyAfterSplitting;
        }


        internal static List<Polygon2d> EdgeSplitWrapper(Polygon2d currentPoly,Random ran, double distance, int dir, double dummy =0)
        {

            Dictionary<string, object> splitReturned = null;
            List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
            try
            {
                splitReturned = SplitByDistance(currentPoly, ran, distance, dir, dummy);
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
            }
            catch (Exception)
            {
                //toggle dir between 0 and 1
                dir = BasicUtility.toggleInputInt(dir);
                splitReturned = SplitByDistance(currentPoly, ran, distance, dir,dummy);
                if (splitReturned == null)
                {
                    //Trace.WriteLine("Split Polys did not work");
                    return null;
                }
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];

                if(polyAfterSplitting[0] == null && polyAfterSplitting[1] == null)
                {
                    return null;
                }
                //throw;
            }

            

            return polyAfterSplitting;
        }





        //****DEF - USING THIS TO SPLIT DEPTS-------------------------------
        // SPLITS A POLY TO MAKE DEPTS
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "DepartmentNames", "UpdatedDeptData","SpaceDataTree" })]
        internal static Dictionary<string, object> DeptSplitRefined(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {

            List<List<Polygon2d>> AllDeptPolys = new List<List<Polygon2d>>();
            List<string> AllDepartmentNames = new List<string>();
            List<double> AllDeptAreaAdded = new List<double>();
            Stack<Polygon2d> leftOverPoly = new Stack<Polygon2d>();
            List<Node> AllNodesList = new List<Node>();

            SortedDictionary<double, DeptData> sortedD = new SortedDictionary<double, DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                double area = deptData[i].AreaEachDept();
                DeptData deptD = deptData[i];
                sortedD.Add(area, deptD);

            }



            List<DeptData> sortedDepartmentData = new List<DeptData>();
            foreach (KeyValuePair<double, DeptData> p in sortedD)
            {
                DeptData deptItem = p.Value;
                sortedDepartmentData.Add(deptItem);
            }

            //SORT THE DEPT BASED ON THE AREA
            sortedDepartmentData.Reverse();
            leftOverPoly.Push(poly);
            int dir = 0;
            int maxRound = 1000;
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
                // when inpatient--------------------------------------------------------------------------
                if (i == 0)
                {
                    while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0 && count1 < maxRound)
                    {
                        dir = BasicUtility.toggleInputInt(dir);
                        currentPolyObj = leftOverPoly.Pop();
                        areaCurrentPoly = Polygon2d.AreaCheckPolygon(currentPolyObj);
                        List<Polygon2d> edgeSplitted = EdgeSplitWrapper(currentPolyObj, ran, offset, dir, 0.75); //////////////////////

                        if (edgeSplitted == null)
                        {
                            return null;
                            //Trace.WriteLine("Found Null");
                            int countTry = 0;
                            Random ran1 = new Random();
                            while (edgeSplitted == null && countTry < 4)
                            {

                                dir = BasicUtility.toggleInputInt(dir);
                                double percentage = BasicUtility.RandomBetweenNumbers(ran1, 0.75, 0.25);
                                double offsetNew = offset * percentage;

                                edgeSplitted = EdgeSplitWrapper(currentPolyObj, ran, offsetNew, dir, percentage);
                                countTry += 1;
                            }

                            //continue;
                        }
                        double areaA = Polygon2d.AreaCheckPolygon(edgeSplitted[0]);
                        double areaB = Polygon2d.AreaCheckPolygon(edgeSplitted[1]);
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
                        count1 += 0;
                    }

                    spaceNode = new Node(i, NodeType.Space);
                    containerNode = new Node(i, NodeType.Container);
                    AllNodesList.Add(spaceNode);
                    AllNodesList.Add(containerNode);
                }
                //when other depts------------------------------------------------------------------------
                else
                {
                    Random rn = new Random();
                    while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0 && count2 < maxRound)
                    {
                        dir = BasicUtility.toggleInputInt(dir);
                        //double ratio = rn.NextDouble() * (0.85 - 0.15) + 0.15;
                        double ratio = BasicUtility.RandomBetweenNumbers(rn, 0.85, 0.15);
                        currentPolyObj = leftOverPoly.Pop();
                        areaCurrentPoly = Polygon2d.AreaCheckPolygon(currentPolyObj);
                        dir = BasicUtility.toggleInputInt(dir);
                        //dir = BasicUtility.RandomToggleInputInt();
                        //Trace.WriteLine("Area left over is : " + areaLeftOverToAdd);
                        if (areaLeftOverToAdd > areaCurrentPoly)
                        {
                            everyDeptPoly.Add(currentPolyObj);
                            areaLeftOverToAdd = areaLeftOverToAdd - areaCurrentPoly;
                            areaCheck += areaCurrentPoly;
                            //Trace.WriteLine("Area left over after assigning when area is greater than current : " + areaLeftOverToAdd);

                        }
                        else
                        {

                            Dictionary<string, object> basicSplit = BasicSplitPolyIntoTwo(currentPolyObj, ratio, dir); ///////////////////////////////
                            if(basicSplit == null)
                            {
                                return null;
                            }
                            List<Polygon2d> polyS = (List<Polygon2d>)basicSplit["PolyAfterSplit"];
                            double areaA = Polygon2d.AreaCheckPolygon(polyS[0]);
                            double areaB = Polygon2d.AreaCheckPolygon(polyS[1]);

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

                        //Trace.WriteLine("Poly After Splitting Length is : " + polyAfterSplitting.Count);

                        //Trace.WriteLine("\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\");
                        count2 += 1;
                    } // end of while loop
                }

                AllDeptAreaAdded.Add(areaCheck);
                AllDeptPolys.Add(everyDeptPoly);
                AllDepartmentNames.Add(deptItem.DepartmentName);

            }// end of for loop

            Random ran2 = new Random();
            if (recompute > 2)
            {
                //there is any left over poly
                double minArea = 10;
                double areaMoreCheck = 0;
                if (leftOverPoly.Count > 0)
                {

                    while (leftOverPoly.Count > 0 && count3 < maxRound)
                    {
                        dir = BasicUtility.toggleInputInt(dir);
                        Polygon2d currentPolyObj = leftOverPoly.Pop();
                        double areaCurrentPoly = Polygon2d.AreaCheckPolygon(currentPolyObj);
                        List<Polygon2d> edgeSplitted = EdgeSplitWrapper(currentPolyObj,ran2, offset, dir);
                        if (edgeSplitted == null)
                        {
                            return null;
                        }
                        double areaA = Polygon2d.AreaCheckPolygon(edgeSplitted[0]);
                        double areaB = Polygon2d.AreaCheckPolygon(edgeSplitted[1]);
                        if (areaA < areaB)
                        {
                            AllDeptPolys[0].Add(edgeSplitted[0]);
                            //areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                            areaMoreCheck += areaA;
                            if (areaB > minArea) { leftOverPoly.Push(edgeSplitted[1]); }

                        }
                        else
                        {
                            AllDeptPolys[0].Add(edgeSplitted[1]);
                            //areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                            areaMoreCheck += areaB;
                            if (areaA > minArea) { leftOverPoly.Push(edgeSplitted[0]); }
                        }
                        count3 += 1;
                    }// end of while loop



                }// end of if loop for leftover count
                AllDeptAreaAdded[0] += areaMoreCheck;
            }// end of if loop



            // adding the left over polys to the 2nd highest dept after inpatient
            if(leftOverPoly.Count > 0)
            {
                //Trace.WriteLine("There is still left over poly left :" + leftOverPoly.Count);
                double areaLeftOver = 0;
                //Trace.WriteLine("No of poly before :  " + AllDeptPolys[1].Count);
                for (int i = 0; i < leftOverPoly.Count; i++)
                {
                    Polygon2d pol = leftOverPoly.Pop();
                    areaLeftOver += GraphicsUtility.AreaPolygon2d(pol.Points);                   
                    AllDeptPolys[1].Add(pol);
                }
                AllDeptAreaAdded[1] += areaLeftOver;

                //Trace.WriteLine("Area from left over :  " + areaLeftOver);
                //AllDeptPolys[1].AddRange(leftOverPoly.ToList());
                //Trace.WriteLine("No of poly after :  " + AllDeptPolys[1].Count);
            }
            

            List<DeptData> UpdatedDeptData = new List<DeptData>();
            //make the new deptdata to output
            for (int i = 0; i < sortedDepartmentData.Count; i++)
            {

                DeptData newDeptData = new DeptData(sortedDepartmentData[i]);
                newDeptData.AreaProvided = AllDeptAreaAdded[i];
                newDeptData.PolyDeptAssigned = AllDeptPolys[i];
                UpdatedDeptData.Add(newDeptData);

            }




            List<Polygon2d> AllLeftOverPolys = new List<Polygon2d>();
            AllLeftOverPolys.AddRange(leftOverPoly);

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
                { "DepartmentNames", (AllDepartmentNames) },
                { "UpdatedDeptData", (UpdatedDeptData) },
                { "SpaceDataTree", (SpaceTreeData) }
            };


        }



        //RECURSIVE SPLITS A POLY - USES EdgeSplitWrapper (spltbydistance) & BasicSplitPolyIntoTwo
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "DepartmentNames", "UpdatedDeptData","SpaceDataTree" })]
        public static Dictionary<string, object> DeptArrangeOnSite(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {
            Dictionary<string, object> deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, 1);
            double count = 0;
            int maxCount = 10;
            Random rand = new Random();
            while(deptArrangement == null && count < maxCount)
            {
                Trace.WriteLine("Lets Go Again for : " + count);
                int reco = rand.Next();
                deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, reco);
                count += 1;
            }


            return deptArrangement;

        }

        ///++++++++++++++++++++++++++++++++
        //RECURSIVE SPLITS A POLY - USES EdgeSplitWrapper (spltbydistance) & BasicSplitPolyIntoTwo       
        public static Dictionary<string, object> DeptArrangeOnSiteSingleOut(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {
            Dictionary<string, object> deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, 1);
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


        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursiveSplitProgramsOneDirByDistance(List<Polygon2d> polyInputList, List<ProgramData> progData, double distance, int recompute = 1)
        {

            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();

            for (int j = 0; j < progData.Count; j++)
            {
                programDataRetrieved.Push(progData[j]);
            }

            ////////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < polyInputList.Count; i++)
            {

                Polygon2d poly = polyInputList[i];


                if (poly == null || poly.Points == null || poly.Points.Count == 0)
                {
                    continue;
                }


                List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly.Points);
                Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);
                double minimumLength = 200;

                Point2d span = polyRange.Span;
                double horizontalSpan = span.X;
                double verticalSpan = span.Y;
                List<double> spans = new List<double>();
                spans.Add(horizontalSpan);
                spans.Add(verticalSpan);
                double setSpan = 1000000000000;
                int dir = 0;
                if (horizontalSpan > verticalSpan)
                {
                    dir = 1;
                    setSpan = horizontalSpan;

                }
                else
                {
                    dir = 0;
                    setSpan = verticalSpan;

                }


                Polygon2d currentPoly = poly;
                int count = 0;


                Random ran2 = new Random();
                while (setSpan > 0 && programDataRetrieved.Count > 0)
                {
                    ProgramData progItem = programDataRetrieved.Pop();
                    List<Polygon2d> polyAfterSplitting = EdgeSplitWrapper(currentPoly, ran2, distance, dir);
                    double selectedArea = 0;
                    double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                    double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[1].Points);
                    if (area1 > area2)
                    {
                        currentPoly = polyAfterSplitting[0];
                        if (polyAfterSplitting[1] == null)
                        {
                            break;
                        }
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


                    if (currentPoly.Points == null)
                    {
                        //Trace.WriteLine("Breaking This");
                        break;
                    }



                    //reduce number of points
                    //currentPoly = new Polygon2d(currentPoly.Points);

                    setSpan -= distance;
                    count += 1;
                }// end of while loop


            }// end of for loop
            List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count)
                {
                    progNew.PolyProgAssigned = polyList[i];
                }
                else
                {
                    progNew.PolyProgAssigned = null;
                }
                UpdatedProgramDataList.Add(progNew);
            }

            pointsList = null;
            //polyList = Polygon2d.PolyReducePoints(polyList);
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) },
                { "UpdatedProgramData",(UpdatedProgramDataList) }
            };
        }

        //RECURSIVE SPLITS A POLY
        public static Dictionary<string, object> RecursiveSplitProgramsOneDirByDistanceSingleOut(List<Polygon2d> polyInputList,
            List<ProgramData> progData, double distance, int recompute = 1)
        {
            return RecursiveSplitProgramsOneDirByDistance(polyInputList, progData, distance, recompute);

        }





        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint" })]
        public static Dictionary<string, object> RecursiveSplitOneDirByDistance(Polygon2d poly, double distance, int recompute = 1)
        {

            /*PSUEDO CODE:
            get poly's vertical and horizontal span
            based on that get the direction of split
            bigpoly 
                split that into two
                push the smaller one in a list
                take the bigger one 
                make it big poly
                repeat
                
            */

            if (poly == null || poly.Points == null || poly.Points.Count == 0)
            {
                return null;
            }


            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly.Points);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);
            double minimumLength = 200;

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            double setSpan = 1000000000000;
            int dir = 0;
            if (horizontalSpan > verticalSpan)
            {
                dir = 1;
                setSpan = horizontalSpan;

            }
            else
            {
                dir = 0;
                setSpan = verticalSpan;

            }


            Polygon2d currentPoly = poly;
            int count = 0;


            Random ran2 = new Random();
            while (setSpan > 0)
            {

                List<Polygon2d> polyAfterSplitting = EdgeSplitWrapper(currentPoly, ran2, distance, dir);
                double selectedArea = 0;
                double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[1].Points);
                if (area1 > area2)
                {
                    currentPoly = polyAfterSplitting[0];
                    if (polyAfterSplitting[1] == null)
                    {
                        break;
                    }
                    polyList.Add(polyAfterSplitting[1]);
                    areaList.Add(area2);
                    selectedArea = area2;


                }
                else
                {
                    currentPoly = polyAfterSplitting[1];
                    polyList.Add(polyAfterSplitting[0]);
                    areaList.Add(area1);
                    selectedArea = area1;

                }


                if (currentPoly.Points == null)
                {
                    Trace.WriteLine("Breaking This");
                    break;
                }



                //reduce number of points
                //currentPoly = new Polygon2d(currentPoly.Points);

                setSpan -= distance;
                count += 1;
            }

            pointsList = null;
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) }
            };
        }


        

        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, BASED ON A DIRECTION AND RATIO
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        public static Dictionary<string, object> BasicSplitPolyIntoTwo(Polygon2d polyOutline, double ratio = 0.5, int dir = 0)
        {
            if(polyOutline == null) return null;
            if(polyOutline != null && polyOutline.Points == null) return null;

            double extents = 5000;
            double minimumLength = 2, minWidth = 10, aspectRatio = 0, eps = 0.1;

            List<Point2d> polyOrig = polyOutline.Points;          
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacingSet);
            List<double> spans = Polygon2d.GetSpansXYFromPolygon2d(poly);
            double horizontalSpan = spans[0], verticalSpan = spans[1];
            Point2d polyCenter = Polygon2d.CentroidFromPoly(poly);
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength) return null;

            if (horizontalSpan > verticalSpan) { dir = 1; aspectRatio = horizontalSpan / verticalSpan; }
            else { dir = 0; aspectRatio = verticalSpan / horizontalSpan; }
          

            // adjust ratio
            if (ratio < 0.15) ratio = ratio + eps;
            if (ratio > 0.85) ratio = ratio - eps;

            if(horizontalSpan < minWidth || verticalSpan < minWidth) ratio = 0.5;
            Line2d splitLine = new Line2d(polyCenter, extents, dir);
            double basic = 0.5;
            double shift = ratio - basic;

            // push this line right or left or up or down based on ratio
            if (dir == 0) splitLine.move(0, shift * verticalSpan);
            else splitLine.move(shift * horizontalSpan, 0);
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            List<int> pIndexA = new List<int>(), pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check) pIndexA.Add(i);
                else pIndexB.Add(i);
            }

            //organize the points to make closed poly            
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA), 
                sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            twoSets.Add(sortedA); twoSets.Add(sortedB);        
            List<Polygon2d>  splittedPoly =  PolygonUtility.OptimizePolyPoints(sortedA, sortedB);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (twoSets) }
            };

        }
       
        
        //****DEF - USING THIS TO SPLIT PROGRAMS-------------------------------

    
       
      ////////////////////////////////////////////////////////////////////////////////////////////////////////// TEST CODE BELOW /////////////////

        //used to split Depts into Program Spaces
        [MultiReturn(new[] { "PolyAfterSplit", "BigPolysAfterSplit", "EachPolyPoint", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursivePlaceProgramsSeries(List<Polygon2d> polyInputList, 
            List<ProgramData> progData, double acceptableWidth, int minWidthAllowed = 8)
        {
           
            List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();
            double minWidth = minWidthAllowed;
            for (int j = 0; j < progData.Count; j++) { programDataRetrieved.Push(progData[j]); }
            List<Polygon2d> polyOrganizedList = new List<Polygon2d>();
            polyOrganizedList = SplitBigPolys(polyInputList, acceptableWidth, 5);
            //polyOrganizedList = Polygon2d.PolyReducePoints(polyOrganizedList);

            polyList = AssignPolysToProgramData(progData, polyOrganizedList);
        
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "BigPolysAfterSplit", (polyOrganizedList) },
                { "EachPolyPoint", (pointsList) },
                { "UpdatedProgramData",(null) }
            };


        }

        // from a list of given polys  it assigns each program a list of polys till its area is satisfied
        internal static List<List<Polygon2d>> AssignPolysToProgramData(List<ProgramData> progData, List<Polygon2d> polygonLists)
        {
            List<List<Polygon2d>> polyEachProgramList = new List<List<Polygon2d>>();
            Stack <Polygon2d> polyStack = new Stack<Polygon2d>();
            for (int i = 0; i < polygonLists.Count; i++) { polyStack.Push(polygonLists[i]); }
         
            for (int i =0; i < progData.Count; i++)
            {
                //double areaProgram = progData[i].CurrentAreaNeeds;
                List<Polygon2d> polysForProg = new List<Polygon2d>();
                while (progData[i].IsAreaSatisfied == false && polyStack.Count>0)
                {
                    Polygon2d currentPoly = polyStack.Pop();
                    double currentArea = Polygon2d.AreaCheckPolygon(currentPoly);
                    progData[i].AddAreaToProg(currentArea);
                    polysForProg.Add(currentPoly);
                }
                polyEachProgramList.Add(polysForProg);
            }
            return polyEachProgramList;
        }




        //used to split Depts into Program Spaces
        [MultiReturn(new[] { "PolyAfterSplit", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursivePlaceProgramsSeriesNew(List<Polygon2d> polyInputList,
            List<ProgramData> progData,double minWidth = 5)
        {

            if (polyInputList == null || polyInputList.Count == 0) return null;
            Stack<Polygon2d> polyContainerList = new Stack<Polygon2d>();
            List<Polygon2d> polyList = new List<Polygon2d>();
            for (int i = 0; i < polyInputList.Count; i++)
            {
                if (polyInputList[i] == null || polyInputList[i].Points == null || polyInputList[i].Points.Count == 0) continue;
                polyContainerList.Push(polyInputList[i]);
            }

            for (int i = 0; i < progData.Count; i++)
            {
                if (polyContainerList.Count > 0)
                {
                  while(progData[i].IsAreaSatisfied == false && polyContainerList.Count > 0)
                    {

                        double areaProg = progData[i].AreaNeeded;
                        Polygon2d currentPoly = polyContainerList.Pop();
                        double areaPoly = Polygon2d.AreaCheckPolygon(currentPoly);
                        Dictionary<string, object> splitResult;
                        if (areaPoly < areaProg)
                        {
                            polyList.Add(currentPoly);
                            //currentPoly = polyContainerList.Pop();
                        }
                        else
                        {
                            double dist = 0; int dir = 1;
                            double ratio = areaPoly / areaProg;
                            
                       
                            List<double> spans = Polygon2d.GetSpansXYFromPolygon2d(currentPoly.Points);
                            double spanX = spans[0], spanY = spans[1];
                            if (spanX > spanY)
                            {
                                if(spanY < minWidth)
                                {
                                    continue;
                                }
                                dist = spanX / ratio; dir = 1;
                                splitResult = SplitByDistanceFromPoint(currentPoly, dist, dir);
                            }
                            else
                            {
                                if (spanX < minWidth)
                                {
                                    continue;
                                }
                                dist = spanY / ratio; dir = 0;
                                splitResult = SplitByDistanceFromPoint(currentPoly, dist, dir);
                            }

                            List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitResult["PolyAfterSplit"];
                            double areaA = Polygon2d.AreaCheckPolygon(polyAfterSplit[0]);
                            double areaB = Polygon2d.AreaCheckPolygon(polyAfterSplit[1]);
                            Polygon2d space, container;
                            double areaSpace;
                            if (areaA < areaB)
                            {
                                space = polyAfterSplit[0];
                                container = polyAfterSplit[1];
                                areaSpace = areaA;                                
                            }
                            else
                            {
                                space = polyAfterSplit[1];
                                container = polyAfterSplit[0];
                                areaSpace = areaB;
                            }
                            double areaPolyAfterSplit = Polygon2d.AreaCheckPolygon(polyAfterSplit[0]);
                            progData[i].AreaProvided += areaSpace;
                            polyList.Add(space);
                            polyContainerList.Push(container);

                        }
                    }//end of while loop

                }
            }// end of for loop
            

                return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "UpdatedProgramData",(null) }
            };


        }


        


        //gets a poly and recursively splits it till acceptablewidth is met and makes a polyorganized list
        internal static void MakePolysOfProportion(Polygon2d poly, List<Polygon2d> polyOrganizedList, double acceptableWidth)
        {
            recurse += 1;
            // Trace.WriteLine("Recurse doing : " + recurse);
            List<double> spanListXY = Polygon2d.GetSpansXYFromPolygon2d(poly.Points);
            double spanX = spanListXY[0];
            double spanY = spanListXY[1];
            double aspRatio = spanX / spanY;
            double lowRange = 0.3;
            double highRange = 2;
            double threshDistanceX = acceptableWidth;
            double threshDistanceY = acceptableWidth;
            bool square = true;

            //double acceptableWidth = 30;
            Random ran = new Random();

            double val = ran.NextDouble();



            if (spanX > threshDistanceX && spanY > threshDistanceY)
            {
                //Trace.WriteLine("Square is false 1");
                square = false;
            }
            else {
                if (aspRatio > lowRange && aspRatio < highRange)
                {
                    //Trace.WriteLine("Square is false 2");
                    square = false;
                }
                else
                {
                    //Trace.WriteLine("Square is true");
                    square = true;

                }
            }

            if (square)
            {
                //poly can be considered as squrish
                //Trace.WriteLine("Normal Rect found");
                polyOrganizedList.Add(poly);
            }
            else
            {
                Dictionary<string, object> splitResult;
                //poly is rectangle so split it into two and add
                if (spanX > spanY)
                {
                    double dis = spanY / 2;
                    int dir = 0;
                    if (val > 0.5)
                    {
                        //dir = BasicUtility.toggleInputInt(dir);
                        //Trace.WriteLine("Long Rect found and Toggled Dir : " + val);
                    }
                    dir = BasicUtility.toggleInputInt(dir);
                    //SplitByDistanceFromPoint(poly, dis, dir);
                    splitResult = SplitByDistanceFromPoint(poly, dis, dir);
                    //Trace.WriteLine("Splitted because it was not square 1");
                }
                else
                {
                    double dis = spanX / 2;
                    int dir = 1;
                    if (val > 0.5)
                    {
                        //dir = BasicUtility.toggleInputInt(dir);
                        //Trace.WriteLine("Long Rect found and Toggled Dir Again : " + val);
                    }
                    dir = BasicUtility.toggleInputInt(dir);
                    splitResult = SplitByDistanceFromPoint(poly, dis, dir);
                    //Trace.WriteLine("Splitted because it was not square 2");
                }

                List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitResult["PolyAfterSplit"];


                List<double> spanA = Polygon2d.GetSpansXYFromPolygon2d(polyAfterSplit[0].Points);
                List<double> spanB = Polygon2d.GetSpansXYFromPolygon2d(polyAfterSplit[1].Points);

                Trace.WriteLine("Recurse is : " + recurse);

                if (recurse < 3000)
                {

                    if ((spanA[0] > 0 && spanA[1] > 0) || (spanB[0] > 0 && spanB[1] > 0))
                    {
                        //Trace.WriteLine("Recurse is  : " + recurse);
                        if (spanA[0] > acceptableWidth && spanA[1] > acceptableWidth)
                        {
                            //Trace.WriteLine("SpanA Dimension : " + spanA[0]);
                            MakePolysOfProportion(polyAfterSplit[0], polyOrganizedList, acceptableWidth);
                        }
                        else
                        {
                            polyOrganizedList.Add(polyAfterSplit[0]);
                            //Trace.WriteLine("No Need to recurse again 1");
                        }
                        //end of 1st if
                        if (spanB[0] > acceptableWidth && spanB[1] > acceptableWidth)
                        {
                            //Trace.WriteLine("SpanB Dimension : " + spanB[0]);
                            MakePolysOfProportion(polyAfterSplit[1], polyOrganizedList, acceptableWidth);

                        }
                        else
                        {
                            polyOrganizedList.Add(polyAfterSplit[1]);
                            //Trace.WriteLine("No Need to recurse again 2");
                        }
                        //end of 2nd if



                    }
                    else
                    {
                        polyOrganizedList.Add(polyAfterSplit[0]);
                        polyOrganizedList.Add(polyAfterSplit[1]);
                        //Trace.WriteLine("No Need to recurse again 3");
                    }

                }


            }
            //Trace.WriteLine("End of function +++++++++++++++++++++++++++++++++++++++++++++++++++++ ");
        }// end of function




        //uses makepolysofproportion function to split one big poly into sub components
        public static List<Polygon2d> SplitBigPolys(List<Polygon2d> polyInputList, double acceptableWidth, int recompute = 1)
        {
            Trace.WriteLine("Split Big Poly recurse is : " + recurse);
            Trace.WriteLine("polyInputList count is : " + polyInputList.Count);
            List<Polygon2d> polyOrganizedList = new List<Polygon2d>();
            int count = 0;
            for (int i = 0; i < polyInputList.Count; i++)
            {

                Polygon2d poly = polyInputList[i];
                if (poly == null || poly.Points == null || poly.Points.Count == 0)
                {
                    Trace.WriteLine("null found : " + count);
                    count += 1;
                    continue;
                    //polyOrganizedList.Add(poly);
                }

                MakePolysOfProportion(poly, polyOrganizedList, acceptableWidth);




            }

            recurse = 0;
            return polyOrganizedList;
        }



        // need to get back to this later, needs some bugs to be fixed
        //used to split Depts into Program Spaces
        [MultiReturn(new[] { "PolyAfterSplit", "BigPolysAfterSplit", "EachPolyPoint", "UpdatedProgramData",
            "SplitLines","LowestPoint","CurrentPolygons", "IntersectionPoints" })]
        public static Dictionary<string, object> RecursivePlaceProgramsSeriesTest(List<Polygon2d> polyInputList,
            List<ProgramData> progData, double acceptableWidth, int minWidthAllowed = 8)
        {


            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            List<Line2d> splitLineList = new List<Line2d>();
            List<Point2d> lowPtList = new List<Point2d>();
            List<Polygon2d> currentPolyList = new List<Polygon2d>();
            List<List<Point2d>> intersectPtList = new List<List<Point2d>>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();


            double minWidth = minWidthAllowed;
            for (int j = 0; j < progData.Count; j++)
            {
                programDataRetrieved.Push(progData[j]);
                Trace.WriteLine("Itering to push progdata" + j);
            }


            List<Polygon2d> polyOrganizedList = new List<Polygon2d>();
            polyOrganizedList = SplitBigPolys(polyInputList, acceptableWidth, 5);
            
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            Trace.WriteLine("No of Programs which needs Addition : " + programDataRetrieved.Count);
            for (int i = 0; i < polyOrganizedList.Count; i++)
            {

                Trace.WriteLine("Starting Poly Object : " + i + " | Out of : + " + polyOrganizedList.Count + " +++++++++++++");
                Trace.WriteLine("Programs left to add: " + programDataRetrieved.Count);
                Polygon2d poly = polyOrganizedList[i];


                if (poly == null || poly.Points == null || poly.Points.Count == 0)
                {
                    Trace.WriteLine("Found Null Poly as input");
                    continue;
                }

                List<double> spans = Polygon2d.GetSpansXYFromPolygon2d(poly.Points);
                Trace.WriteLine("Current Poly horizontal span is : " + spans[0]);
                Trace.WriteLine("Current Poly vertical span is : " + spans[1]);
                double highSpan = 1000000000000;
                double lowSpan = 0;
                int dir = 0;
                if (spans[0] > spans[1])
                {
                    dir = 1;
                    highSpan = spans[0]; // horizontal span
                    lowSpan = spans[1]; // vertical span

                }
                else
                {
                    dir = 0;
                    highSpan = spans[1]; // vertical span
                    lowSpan = spans[0]; // horizontal span
                }

                Polygon2d currentPoly = poly;
                int count = 0;


                Random ran2 = new Random();
                while (highSpan > 0 && programDataRetrieved.Count > 0)
                {
                    ProgramData progItem = programDataRetrieved.Pop();


                    double areaProg = progItem.AreaNeeded - progItem.AreaProvided;
                    if (areaProg <= 0)
                    {
                        break;
                    }
                    double dist = areaProg / lowSpan;
                    if (dist < minWidth)
                    {
                        dist = minWidth;
                        //Trace.WriteLine("Dist Increased : " + dist);
                    }
                    //Trace.WriteLine("HighSpan is : " + highSpan + " | Dist is : " + dist);
                    if ((highSpan - dist) < minWidth)       
                    {
                        //add extra material left in the dist to consume all space
                        dist += (highSpan - dist);
                        //Trace.WriteLine("Dist Increased");
                    }
                    string foo = "";
                    
                    if (dist >= highSpan*0.9)
                    {
                        polyList.Add(poly);
                        splitLineList.Add(null);
                        lowPtList.Add(null);
                        currentPolyList.Add(null);
                        intersectPtList.Add(null);
                        progItem.AreaProvided = dist * lowSpan;
                        programDataRetrieved.Push(progItem);
                        break;                        
                    }

                    Trace.WriteLine("Dist computed is : " + dist);
                    //List<Polygon2d> polyAfterSplitting = EdgeSplitWrapper(currentPoly, ran2, dist, dir);
                    Dictionary<string, object> splitReturn = SplitByDistancePoly(currentPoly, dist, dir);
                    List<Polygon2d> polyAfterSplitting = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    Line2d splitLine = (Line2d)splitReturn["SplitLine"];
                    Point2d lowPoint = (Point2d)splitReturn["LowestPoint"];
                    List < Point2d > intersectedPts = (List<Point2d>)splitReturn["IntersectedPoints"];



                    if (polyAfterSplitting[0] == null || polyAfterSplitting[0].Points == null ||
                        polyAfterSplitting[0].Points.Count == 0)
                    {
                        Trace.WriteLine("Null Poly : " + i);
                        programDataRetrieved.Push(progItem);
                        //dir = BasicUtility.toggleInputInt(dir);
                        break;
                    }
                    if (polyAfterSplitting[1] == null || polyAfterSplitting[1].Points == null ||
                        polyAfterSplitting[1].Points.Count == 0)
                    {
                        Trace.WriteLine("Null Poly : " + i);
                        programDataRetrieved.Push(progItem);
                        //dir = BasicUtility.toggleInputInt(dir);
                        break;
                    }
                    splitLineList.Add(splitLine);
                    lowPtList.Add(lowPoint);
                    currentPolyList.Add(currentPoly);
                    intersectPtList.Add(intersectedPts);
                    double selectedArea = 0;
                    double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                    double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[1].Points);
                    currentPoly = polyAfterSplitting[1];
                    polyList.Add(polyAfterSplitting[0]);
                    progItem.AreaProvided = area1;
                    areaList.Add(area2);
                    selectedArea = area2;
                    Trace.WriteLine("Poly Added : " + i);

                    highSpan -= dist;
                    count += 1;
                }// end of while loop


            }// end of for loop



            List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count)
                {
                    progNew.PolyProgAssigned = polyList[i];
                }
                else
                {
                    progNew.PolyProgAssigned = null;
                }
                UpdatedProgramDataList.Add(progNew);
            }

            pointsList = null;
            //return polyList;

            polyList = Polygon2d.PolyReducePoints(polyList);
            polyOrganizedList = Polygon2d.PolyReducePoints(polyOrganizedList);
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "BigPolysAfterSplit", (polyOrganizedList) },
                { "EachPolyPoint", (pointsList) },
                { "UpdatedProgramData",(UpdatedProgramDataList) },
                { "SplitLines",(splitLineList) },
                { "LowestPoint", (lowPtList) },
                { "CurrentPolygons",(currentPolyList) },
                { "IntersectionPoints", (intersectPtList) }
            };
        }
        
    



        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, BASED ON A DIRECTION AND DISTANCE
        /// <summary>
        /// This is an example node demonstrating how to use the Zero Touch import mechanism.
        /// It returns the input number multiplied by 2.
        /// </summary>
        /// <param name="polyOutline">Outline of the Polygon2d to split</param>
        /// <param name="distance">Distance from the lower left point</param>
        /// <returns name="Dictionary">An Dictionary containing the polyAfterSplit & SplitLine</param>
        /// <search>
        /// split, divide , partition space based on distance
        /// </search>
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        public static Dictionary<string, object> SplitByDistance(Polygon2d polyOutline, Random ran, double distance = 10, int dir = 0, double dummy =0)
        {
            if(polyOutline ==null || polyOutline.Points ==null || polyOutline.Points.Count == 0)
            {
                return null;
            }
           
            double extents = 5000;
            double spacing = spacingSet;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            if (poly == null || poly.Count == 0)
            {
                return null; 
            }
            List<double> spans = Polygon2d.GetSpansXYFromPolygon2d(poly);
            Dictionary<int, object> obj = PolygonUtility.pointSelector(ran,poly);
            Point2d pt = (Point2d)obj[0];
            int orient = (int)obj[1];

            Line2d splitLine = new Line2d(pt, extents, dir);
          

            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, orient*distance);
            }
            else
            {
                splitLine.move(orient*distance, 0);
            }

            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;
            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                }
                else
                {
                    pIndexB.Add(i);
                }
            }

            //organize the points to make closed poly
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);

            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);          

            List<Polygon2d> splittedPoly = PolygonUtility.OptimizePolyPoints(sortedA, sortedB);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (twoSets) }
            };

        }

        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "LowestPoint", "EachPolyPoint" })]
        public static Dictionary<string, object> SplitByDistancePoly(Polygon2d polyOutline, double distance = 10, int dir = 0, double dummy = 0)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0)
            {
                return null;
            }
            double fac = 0.999; // 0.999 default
            double extents = 5000;
            double spacing = spacingSet;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            if (poly == null || poly.Count == 0)
            {
                return null;
            }
            List<double> spans = Polygon2d.GetSpansXYFromPolygon2d(poly);
            int orient = 1;
            //int ind = GraphicsUtility.ReturnLowestPointFromListNew(poly);
            int ind = GraphicsUtility.ReturnLowestPointFromList(poly);
            Point2d pt = poly[ind];
            Line2d splitLine = new Line2d(pt, extents, dir);

            if (dir == 1)
            {
                if (distance > spans[0])
                {
                    distance = fac * spans[0];
                }

            }
            else
            {
                if (distance > spans[1])
                {
                    distance = fac* spans[1];
                }

            }
            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, orient * distance);
            }
            else
            {
                splitLine.move(orient * distance, 0);
            }

            //List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersectionInd(poly, splitLine);
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                }
                else
                {
                    pIndexB.Add(i);
                }
            }

            //organize the points to make closed poly
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);

            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);

            List<Polygon2d> splittedPoly = PolygonUtility.OptimizePolyPoints(sortedA, sortedB,true);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "LowestPoint", (pt) },
                { "EachPolyPoint", (twoSets) }
            };

        }




        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, by a Given Line
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine" })]
        internal static Dictionary<string, object> SplitByLineMake(Polygon2d polyOutline, Line2d inputLine, double distance = 5)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0)
            {
                return null;
            }


            double spacing = spacingSet;

            List<Point2d> polyOrig = polyOutline.Points;

            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            Line2d splitLine = new Line2d(inputLine);
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(poly);
            bool checkSide = GraphicsUtility.CheckPointSide(splitLine, centerPoly);
            int orient = GraphicsUtility.CheckLineOrient(splitLine);
            if (orient == 0)
            {
                if (!checkSide)
                {
                    splitLine.move(0, -1 * distance);
                }
                else
                {
                    splitLine.move(0, 1 * distance);
                }
            }
            else
            {
                if (checkSide)
                {
                    splitLine.move(-1 * distance, 0);
                }
                else
                {
                    splitLine.move(1 * distance, 0);
                }

            }

            //splitLine.move(poly, distance);
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                }
                else
                {
                    pIndexB.Add(i);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            polyA = new Polygon2d(twoSets[0], 0);
            polyB = new Polygon2d(twoSets[1], 0);


            List<Polygon2d> splittedPoly = new List<Polygon2d>();

            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) }
            };

        }



        // USING NOW  // dir = 0 : horizontal split line dir = 1 : vertical split line
        //splits a polygon into two based on direction and distance
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints" })]
        public static Dictionary<string, object> SplitByDistanceFromPoint(Polygon2d polyOutline,double distance = 10, int dir = 0)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0) return null;           double extents = 5000; double spacing = spacingSet;
            
            List<Point2d> polyOrig = polyOutline.Points;       
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            if (poly == null || poly.Count == 0) return null;

            //compute lowest point & split line
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);// THIS IS BETTER THAN THE OTHER VER
            Point2d lowPt = poly[lowInd];   
            Line2d splitLine = new Line2d(lowPt, extents, dir);
                        
            // push this line right or left or up or down based on ratio
            if (dir == 0) splitLine.move(0, 1 * distance);
            else splitLine.move(1 * distance, 0);   
                    
            //List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersectionInd(poly, splitLine);
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
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            twoSets.Add(sortedA); twoSets.Add(sortedB);
            List<Polygon2d> splittedPoly = PolygonUtility.OptimizePolyPoints(sortedA, sortedB, true);
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) }
            };

        }


        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, BASED ON A DIRECTION AND RATIO
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        internal static Dictionary<string, object> SplitFromEdgePolyIntoTwo(Polygon2d polyOutline, double distance = 10, int dir = 0)
        {
            double extents = 5000;
            double spacing = spacingSet;
            double minimumLength = 10;
            double minValue = 10;
            bool horizontalSplit = false;
            bool verticalSplit = true;
            // dir = 0 : horizontal split line
            // dir = 1 : vertical split line

            List<Point2d> polyOrig = polyOutline.Points;
            double eps = 0.1;
            //CHECKS
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //List<Point2d> poly = GraphicsUtility.AddPointsInBetween(polyOrig, 5);
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            // compute bounding box ( set of four points ) for the poly
            // find x Range, find y Range
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            //compute centroid
            //Point2d polyCenter = Polygon2d.CentroidFromPoly(poly);

            //compute lowest point
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);
            Point2d lowPt = poly[lowInd];
            //check aspect ratio
            double aspectRatio = 0;



            // check if width or length is enough to make split
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength)
            {
                return null;
            }


            //should check direction of split ( flip dir value )
            if (horizontalSpan > verticalSpan)
            {
                //dir = 1;
                aspectRatio = horizontalSpan / verticalSpan;
            }
            else
            {
                //dir = 0;
                aspectRatio = verticalSpan / horizontalSpan;
            }

            if (aspectRatio > 2)
            {
                //return null;
            }

            //set split style
            if(dir == 0)
            {
                horizontalSplit = true;
            }
            else
            {
                verticalSplit = true;
            }


            
            // adjust distance if less than some value
            if (distance < minValue)
            {
                distance = minValue;
            }
            // adjust distance if more than total length of split possible
            if (verticalSplit)
            {
                if(distance > verticalSpan)
                {
                    distance = verticalSpan - minValue;
                }
            }

            if (horizontalSplit)
            {
                if (distance > horizontalSpan)
                {
                    distance = horizontalSpan - minValue;
                }
            }


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Line2d splitLine = new Line2d(lowPt, extents, dir);
            //compute vertical or horizontal line via centroid

         

            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, distance);
            }
            else
            {
                splitLine.move(distance, 0);
            }



            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            ////////////////////////////////////////////////////////////////////////////////////////////



            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                    //ptA.Add(poly[i]);
                }
                else
                {
                    pIndexB.Add(i);
                    //ptB.Add(poly[i]);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);        
            List<Polygon2d> splittedPoly = PolygonUtility.OptimizePolyPoints(sortedA, sortedB);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (twoSets) }
            };

        }




        ////////////////

        //changes the center of one or both polys to ensure correct intersection line is found
        [MultiReturn(new[] { "CenterPolyA", "CenterPolyB", "PolyA", "PolyB" })]
        internal static Dictionary<string, object> ComputePolyCentersAlign(Polygon2d polyA, Polygon2d polyB)
        {
            double extents = 10000;
            //compute orig centers
            Point2d centerPolyA = GraphicsUtility.CentroidInPointLists(polyA.Points);
            Point2d centerPolyB = GraphicsUtility.CentroidInPointLists(polyB.Points);

            Point2d staticPoint;
            Polygon2d staticPoly;
            Point2d movingPoint;
            Polygon2d movingPoly;

            double areaPolyA = GraphicsUtility.AreaPolygon2d(polyA.Points);
            double areaPolyB = GraphicsUtility.AreaPolygon2d(polyB.Points);

            if (areaPolyA > areaPolyB)
            {
                staticPoint = centerPolyB;
                staticPoly = polyB;
                movingPoint = centerPolyA;
                movingPoly = polyA;
            }
            else
            {
                staticPoint = centerPolyA;
                staticPoly = polyA;
                movingPoint = centerPolyB;
                movingPoly = polyB;
            }

            //shift the other points

            Point2d movingPoint1 = new Point2d(staticPoint.X, movingPoint.Y);
            Point2d movingPoint2 = new Point2d(movingPoint.X, staticPoint.Y);

            bool IsMovingPoint1 = GraphicsUtility.PointInsidePolygonTest(movingPoly.Points, movingPoint1);
            bool IsMovingPoint2 = GraphicsUtility.PointInsidePolygonTest(movingPoly.Points, movingPoint2);

            if (IsMovingPoint1)
            {
                movingPoint = movingPoint1;

            }
            else if (IsMovingPoint2)
            {
                movingPoint = movingPoint2;
            }
            else
            {
                staticPoint = centerPolyA;
                staticPoly = polyA;
                movingPoint = movingPoint1;
                movingPoly = polyB;
            }



            return new Dictionary<string, object>
                {
                { "CenterPolyA", (staticPoint) },
                { "CenterPolyB", (movingPoint) },
                { "PolyA", (staticPoly) },
                { "PolyB", (movingPoly) }
                };
        }




        //changes the center of one or both polys to ensure correct intersection line is found
        [MultiReturn(new[] { "CenterPolyA", "CenterPolyB", "PolyA", "PolyA" })]
        internal static Dictionary<string, object> ComputePolyCenters(Polygon2d polyA, Polygon2d polyB)
        {
            double extents = 10000;
            //compute orig centers
            Point2d centerPolyA = GraphicsUtility.CentroidInPointLists(polyA.Points);
            Point2d centerPolyB = GraphicsUtility.CentroidInPointLists(polyB.Points);

            //make infinite lines via both centers 0 - horizontal line, 1 - vertical line
            Line2d lineAX = new Line2d(centerPolyA, extents, 0);
            Line2d lineAY = new Line2d(centerPolyA, extents, 1);

            Line2d lineBX = new Line2d(centerPolyB, extents, 0);
            Line2d lineBY = new Line2d(centerPolyB, extents, 1);


            //get line line intersection for these lines
            //AX-BY and BX-AY
            Point2d pAXBY = GraphicsUtility.LineLineIntersection(lineAX, lineBY);
            Point2d pBXAY = GraphicsUtility.LineLineIntersection(lineBX, lineAY);

            //check for point containment test for these two
            bool checkA_AXBY = GraphicsUtility.PointInsidePolygonTest(polyA.Points, pAXBY);
            bool checkA_BXAY = GraphicsUtility.PointInsidePolygonTest(polyA.Points, pBXAY);
            bool checkB_AXBY = GraphicsUtility.PointInsidePolygonTest(polyB.Points, pAXBY);
            bool checkB_BXAY = GraphicsUtility.PointInsidePolygonTest(polyB.Points, pBXAY);
            ////////////////////////////////////////////////////
            if (checkA_BXAY == true && checkB_AXBY == true)
            {
                return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
            }

            if (checkA_AXBY == true && checkB_BXAY == true)
            {
                return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
            }
            ////////////////////////////////////////////////////


            if (checkA_AXBY == true)
            {

                //centerPolyB.X = centerPolyA.X;

            }
            else if (checkA_BXAY == true)
            {
                //centerPolyB.Y = centerPolyA.Y;

            }
            else if (checkB_AXBY == true)
            {

            }
            else if (checkB_BXAY == true)
            {

            }
            else
            {
                return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
            }





            return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
        }





        [MultiReturn(new[] { "Neighbour", "SharedEdgeA", "SharedEdgeB", "LineMoved", "CenterToCenterLine", "CenterPolyPoint", "CenterPolyOtherPoint" })]
        internal static Dictionary<string, object> PolygonPolygonCommonEdgeDict(Polygon2d poly, Polygon2d other)
        {
            /*
            first reduce number of pts in both polys
            find their centers
            make a vec between their center
            get horizontal comp of vec
            get vertical comp of vec
            which length is long will be our vector

            then for both polys
                check line line intersection between line between two centers and each line of the poly
                    if no intersect, no edge
                    find the line intersects 
                    find the perpendicular projection of centers on these linese

            */

            bool check = false;
            if (poly == null || other == null)
            {
                return null;
            }

            double eps = 200;
            //Polygon2d polyReg = poly;
            //Polygon2d otherReg = other;
            //reduce number of points
            Polygon2d polyReg = new Polygon2d(poly.Points);
            Polygon2d otherReg = new Polygon2d(other.Points);
            //reassign centers to each poly
            //Dictionary<string,object> UpdatedCenters  = ComputePolyCentersAlign(polyReg, otherReg);
            Dictionary<string, object> UpdatedCenters = ComputePolyCentersAlign(polyReg, otherReg);

            Point2d centerPoly = (Point2d)UpdatedCenters["CenterPolyA"];
            Point2d centerOther = (Point2d)UpdatedCenters["CenterPolyB"];

            polyReg = (Polygon2d)UpdatedCenters["PolyA"];
            otherReg = (Polygon2d)UpdatedCenters["PolyB"];

            //make vectors
            Vector2d centerToCen = new Vector2d(centerPoly, centerOther);
            Vector2d centerToCenX = new Vector2d(centerToCen.X, 0);
            Vector2d centerToCenY = new Vector2d(0, centerToCen.Y);





            //make centerLine
            Line2d centerLine = new Line2d(centerPoly, centerOther);
            Vector2d keyVec;
            if (centerToCenX.Length > centerToCenY.Length)
            {
                keyVec = new Vector2d(centerToCenX.X, centerToCenX.Y);
            }
            else
            {
                keyVec = new Vector2d(centerToCenY.X, centerToCenY.Y);
            }

            //check line poly intersection between centertocen vector and each polys            
            Line2d lineInPolyReg = GraphicsUtility.LinePolygonIntersectionReturnLine(polyReg.Points, centerLine, centerOther);
            Line2d lineInOtherReg = GraphicsUtility.LinePolygonIntersectionReturnLine(otherReg.Points, centerLine, centerPoly);


            //find distance d1 and d2 from two centers to linepolyintersection line
            Point2d projectedPtOnPolyReg = GraphicsUtility.ProjectedPointOnLine(lineInPolyReg, centerPoly);
            Point2d projectedPtOnOtherReg = GraphicsUtility.ProjectedPointOnLine(lineInOtherReg, centerOther);

            double dist1 = GraphicsUtility.DistanceBetweenPoints(centerPoly, projectedPtOnPolyReg);
            double dist2 = GraphicsUtility.DistanceBetweenPoints(centerOther, projectedPtOnOtherReg);

            double totalDistance = 2 * (dist1 + dist2);
            //Line2d lineMoved = new Line2d(lineInPolyReg);
            Line2d lineMoved = new Line2d(lineInPolyReg.StartPoint, lineInPolyReg.EndPoint);
            lineMoved.move(centerPoly);
            Point2d projectedPt = GraphicsUtility.ProjectedPointOnLine(lineMoved, centerOther);
            double distance = GraphicsUtility.DistanceBetweenPoints(projectedPt, centerOther);

            bool isNeighbour = false;
            if (totalDistance - eps < distance && distance < totalDistance + eps)
            {
                isNeighbour = true;
            }
            else
            {
                isNeighbour = false;
            }

            //"Neighbour", "SharedEdgeA", "SharedEdgeB" 

            return new Dictionary<string, object>
            {
                { "Neighbour", (isNeighbour) },
                { "SharedEdgeA", (lineInPolyReg) },
                { "SharedEdgeB", (lineInOtherReg) },
                { "LineMoved", (lineMoved) },
                { "CenterToCenterLine", (centerLine) },
                { "CenterPolyPoint", (centerPoly) },
                { "CenterPolyOtherPoint", (centerOther) },
            };

        }




        internal static Line2d PolygonPolygonCommonEdge(Polygon2d poly, Polygon2d other)
        {
            /*
            first reduce number of pts in both polys
            find their centers
            make a vec between their center
            get horizontal comp of vec
            get vertical comp of vec
            which length is long will be our vector

            then for both polys
                check line line intersection between line between two centers and each line of the poly
                    if no intersect, no edge
                    find the line intersects 
                    find the perpendicular projection of centers on these linese

            */

            bool check = false;
            if (poly == null || other == null)
            {
                return null;
            }
            double eps = 100;
            //reduce number of points
            Polygon2d polyReg = new Polygon2d(poly.Points);
            Polygon2d otherReg = new Polygon2d(other.Points);

            //find centers
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(polyReg.Points);
            Point2d centerOther = GraphicsUtility.CentroidInPointLists(otherReg.Points);

            //make vectors
            Vector2d centerToCen = new Vector2d(centerPoly, centerOther);
            Vector2d centerToCenX = new Vector2d(centerToCen.X, 0);
            Vector2d centerToCenY = new Vector2d(0, centerToCen.Y);

            //make centerLine
            Line2d centerLine = new Line2d(centerPoly, centerOther);
            Vector2d keyVec;
            if (centerToCenX.Length > centerToCenY.Length)
            {
                keyVec = new Vector2d(centerToCenX.X, centerToCenX.Y);
            }
            else
            {
                keyVec = new Vector2d(centerToCenY.X, centerToCenY.Y);
            }

            //check line poly intersection between centertocen vector and each polys            
            Line2d lineInPolyReg = GraphicsUtility.LinePolygonIntersectionReturnLine(polyReg.Points, centerLine, centerOther);
            Line2d lineInOtherReg = GraphicsUtility.LinePolygonIntersectionReturnLine(otherReg.Points, centerLine, centerPoly);

            Point2d projectedPtOnPolyReg = GraphicsUtility.ProjectedPointOnLine(lineInPolyReg, centerPoly);
            Point2d projectedPtOnOtherReg = GraphicsUtility.ProjectedPointOnLine(lineInOtherReg, centerOther);

            double dist1 = GraphicsUtility.DistanceBetweenPoints(centerPoly, projectedPtOnPolyReg);
            double dist2 = GraphicsUtility.DistanceBetweenPoints(centerOther, projectedPtOnOtherReg);

            double totalDistance = 2 * (dist1 + dist2);
            lineInPolyReg.move(centerPoly);
            Point2d projectedPt = GraphicsUtility.ProjectedPointOnLine(lineInPolyReg, centerOther);
            double distance = GraphicsUtility.DistanceBetweenPoints(projectedPt, centerOther);


            if (totalDistance - eps < distance && distance < totalDistance + eps)
            {
                return lineInOtherReg;
            }
            else
            {
                return null;
            }

        }



        internal static Line CheckLineMove(Line testLine, Point2d movePt)
        {
            Point2d pt1 = new Point2d(testLine.StartPoint.X, testLine.StartPoint.Y);
            Point2d pt2 = new Point2d(testLine.EndPoint.X, testLine.EndPoint.Y);
            Line2d line = new Line2d(pt1, pt2);
            line.move(movePt);
            Point ptA = Point.ByCoordinates(line.StartPoint.X, line.StartPoint.Y);
            Point ptB = Point.ByCoordinates(line.EndPoint.X, line.EndPoint.Y);
            Line movedLine = Line.ByStartPointEndPoint(ptA, ptB);
            return movedLine;
        }

    }
}
