using System;
using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using System.Linq;


namespace SpacePlanning
{
    public class GlareVisualization
    {

        //builds the line3d of the furnitures
        public static List<Line3d> GetLine2dFurnitures(List<NurbsCurve> nurbList, double height =0)
        {
            List<Line3d> lineList = new List<Line3d>();
            for (int i = 0; i < nurbList.Count; i++)
            {
                Point2d stPt = new Point2d(nurbList[i].StartPoint.X, nurbList[i].StartPoint.Y);
                Point2d endPt = new Point2d(nurbList[i].EndPoint.X, nurbList[i].EndPoint.Y);
                Line3d line = new Line3d(ConvertToPoint3d(stPt, height), ConvertToPoint3d(endPt, height));
                lineList.Add(line);               
            }
            return lineList;
        }


        //converts point2d to point3d element
        public static Point3d ConvertToPoint3d(Point2d ptInput, double height = 0)
        {
            return new Point3d(ptInput, height);
        }

        //converts point2d to point2d element
        public static Point2d ConvertToPoint2d(Point3d ptInput)
        {
            return new Point2d(ptInput.X,ptInput.Y);
        }


        //gets the points on the floor in point3d format
        public static List<Point3d> GetFloorPoints(List<Cell> cellList, double height = 0)
        {
            List<Point3d> ptList = new List<Point3d>();
            for(int i = 0; i < cellList.Count; i++)
            {
                Point3d pt3d = ConvertToPoint3d(cellList[i].CenterPoint, height);
                ptList.Add(pt3d);
            }
            return ptList;
        }
        //gets the mid point of the furnitures in point3d
        public static List<Point3d> GetFurniturePoints(List<Line3d> lineList)
        {
            List<Point3d> ptList = new List<Point3d>();
            double height = lineList[0].StartPoint.Z;
            for(int i = 0; i < lineList.Count; i++)
            {
                Point2d pt1 = new Point2d(lineList[i].StartPoint.X, lineList[i].StartPoint.Y);
                Point2d pt2 = new Point2d(lineList[i].EndPoint.X, lineList[i].EndPoint.Y);
                Point2d midPt = new Point2d((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
                ptList.Add(ConvertToPoint3d(midPt, height));
            }   
            return ptList;
        }


        //gets the mid point of the furnitures in point3d
        public static List<Point3d> GetLightingPoints(List<Cell> cellList, double height = 0)
        {
            List<Point3d> ptList = new List<Point3d>(), lightPtList = new List<Point3d>();
            for (int i = 0; i < cellList.Count; i++)
            {
                Point3d pt3d = ConvertToPoint3d(cellList[i].CenterPoint, height);
                ptList.Add(pt3d);
            }
          
            Random ran = new Random();
            for(int i = 0; i < ptList.Count; i++)
            {
                double num = BasicUtility.RandomBetweenNumbers(ran, 0, 5);
                if (num < 3) lightPtList.Add(ptList[i]);
            }
            return lightPtList;
        }



        //place furntiures at a set height
        public static List<Line3d> PlaceFurnitureAtHeight(List<Line3d> furnitureList, double height = 0)
        {
            List<Line3d> lineList = new List<Line3d>();
            for(int i = 0; i < furnitureList.Count; i++)
            {
                Point2d stpt = new Point2d(furnitureList[i].StartPoint.X, furnitureList[i].StartPoint.Y);
                Point2d endpt = new Point2d(furnitureList[i].EndPoint.X, furnitureList[i].EndPoint.Y);
                Point3d pt3dA = ConvertToPoint3d(stpt, height);
                Point3d pt3dB = ConvertToPoint3d(endpt, height);
                lineList.Add(new Line3d(pt3dA, pt3dB));
            }           
            return lineList;
        }


        //compute glare angle values
        public static List<List<double>> ComputerGlareValues(List<Point3d> floorPoints,List<Point3d> furniturePoints,
            List<Point3d> lightPoints, double threshDist = 10, double thresAngleLow = 50, double thresAngleHigh = 80)
        {
            List<double> angList = new List<double>();
            List < List < double >> allAngles = new List<List<double>>();
            for(int i = 0; i < floorPoints.Count; i++)
            {
                List<Point3d> selectedPts = new List<Point3d>();
                Point2d pt2FloorPt = ConvertToPoint2d(floorPoints[i]);
                for(int j=0; j < furniturePoints.Count; j++)
                {
                    Point2d pt2Furniture = ConvertToPoint2d(furniturePoints[j]);
                    double distance = GraphicsUtility.DistanceBetweenPoints(pt2Furniture, pt2FloorPt);
                    if (distance < threshDist) selectedPts.Add(furniturePoints[j]);
                }// end of j for loop
                double ang = 0;
                for ( int k = 0; k < selectedPts.Count; k++)
                {
                    //ang = 0;
                    List<Vector3d> vec3dList = new List<Vector3d>();
                    for ( int m = 0;m< lightPoints.Count; m++)
                    {
                        if(lightPoints[m].X < selectedPts[k].X)
                        {
                            Vector3d vx = new Vector3d(1, 0, 0);
                            Vector3d vy = new Vector3d(0, 1, 0);
                            Vector3d vz = new Vector3d(0, 0, 1);

                            Vector3d vec = new Vector3d(lightPoints[m], selectedPts[k]);
                            vec3dList.Add(vec);
                            double angle = VectorUtility.AngleBetween(vec, vx,false);
                            //if (angle > thresAngleLow && angle < thresAngleHigh) ang += angle;
                            ang += angle;
                        }
                    }
                }
                angList.Add((ang / selectedPts.Count));


            }// end of i for loop
            //return angList;

            List<int> indexList = new List<int>();
            for(int n = 0; n < angList.Count; n++) indexList.Add(n);

            int colorVal = 255;
            List<int> sortedIndices = BasicUtility.Quicksort(angList, indexList, 0, angList.Count-1);
            sortedIndices.Reverse();
            double maxAng = angList[sortedIndices[0]];
            List<double> angListNormalized = new List<double>();
            List<double> val2 = new List<double>();
            List<double> val3 = new List<double>();
            for (int n = 0; n < angList.Count; n++)
            {
                angListNormalized.Add(angList[n] * colorVal / maxAng);
                val2.Add(0);
                val3.Add(0);
            }
            List<List<double>> result = new List<List<double>>();
            result.Add(angListNormalized);
            result.Add(val2);
            result.Add(val3);
            return result;
            

        }




    }
}
