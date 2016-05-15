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


        //gets the mid point of the furnitures in point3d
        public static List<Point3d> GetLightingPointsNAmount(List<Cell> cellList, double height = 0, double num = 15)
        {
            List<Point3d> ptList = new List<Point3d>(), lightPtList = new List<Point3d>();
            for (int i = 0; i < cellList.Count; i++)
            {
                Point3d pt3d = ConvertToPoint3d(cellList[i].CenterPoint, height);
                ptList.Add(pt3d);
            }
            Random ran = new Random();
            for (int i = 0; i < ptList.Count; i++) if (i < num) lightPtList.Add(ptList[i]);
            {
               
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

        //make cells inside the floor
        public static List<Cell> MakeCellsVizualization(List<Point2d> outLinePts, double dim =1)
        {
            List<Cell> cellList = GridObject.CellsInsidePoly(outLinePts, dim);
            List<Cell> cellNewList = new List<Cell>();
            for(int i = 0; i < cellList.Count; i++)
            {
                Point2d center = cellList[i].CenterPoint;
                Point2d centerNew = new Point2d(center.X + (dim / 2), center.Y + (dim / 2));
                Cell cl = new Cell(centerNew, dim, dim, true);
                cellNewList.Add(cl);
            }
            return cellNewList;
        }



        //computes the UGR value
        public static double ComputeUGRValue(List<Point3d> lightPts, Point3d observer, double lightSize)
        {
            double backLum = 10;
            double lumin = 15;
            double angleEach = 0;
            double guthPos = 2*lightSize;
            double value = 0;
            double summation = 0;
            for(int i = 0; i < lightPts.Count; i++)
            {
                //if (lightPts[i].Y > observer.Y)
                //{
                    Point3d lightPt1 = new Point3d((lightPts[i].X - lightSize), (lightPts[i].Y - lightSize), lightPts[i].Z);
                    Point3d lightPt2 = new Point3d((lightPts[i].X + lightSize), (lightPts[i].Y + lightSize), lightPts[i].Z);
                    Vector3d vec1 = new Vector3d(observer, lightPt1);
                    Vector3d vec2 = new Vector3d(observer, lightPt2);
                    angleEach = VectorUtility.AngleBetween(vec1, vec2);
                    summation += (lumin * lumin * angleEach) / (guthPos * guthPos);
                //}           
            }
            value = (0.25 / backLum) * summation;
            if (value == 0) return 255;
            double ugr = 8 * Math.Log10(value);
            return ugr;
        }


        //compute glare angle values
        public static List<List<double>> ComputerGlareValues(List<Point3d> floorPoints,List<Point3d> furniturePoints,
            List<Point3d> lightPoints, double threshDist = 10, double thresAngleLow = 50, double thresAngleHigh = 80, double lightSize = 3)
        {
            List<double> angList = new List<double>();
            List<double> ugrList = new List<double>();
            int count = 0;
            for(int i = 0; i < floorPoints.Count; i++)
            {
                /*
                List<Point3d> selectedPts = new List<Point3d>();
                Point2d pt2FloorPt = ConvertToPoint2d(floorPoints[i]);
                for(int j=0; j < furniturePoints.Count; j++)
                {
                    Point2d pt2Furniture = ConvertToPoint2d(furniturePoints[j]);
                    double distance = GraphicsUtility.DistanceBetweenPoints(pt2Furniture, pt2FloorPt);
                    if (distance < threshDist) selectedPts.Add(furniturePoints[j]);
                }// end of j for loop
                */
                double ugrValue = ComputeUGRValue(lightPoints, floorPoints[i], lightSize);
                ugrList.Add(ugrValue);
                count += 1;
            }

            List<int> indexList = new List<int>();
            for(int n = 0; n < ugrList.Count; n++) indexList.Add(n);

            int colorVal = 255;
            List<int> sortedIndices = BasicUtility.Quicksort(ugrList, indexList, 0, ugrList.Count-1);
            sortedIndices.Reverse();
            double maxAng = ugrList[sortedIndices[0]];
            List<double> ugrListNormalized = new List<double>();
            List<double> val2 = new List<double>();
            List<double> val3 = new List<double>();
            for (int n = 0; n < ugrList.Count; n++)
            {
                ugrListNormalized.Add(ugrList[n] * colorVal / maxAng);
                val2.Add(0);
                val3.Add(0);
            }
            List<List<double>> result = new List<List<double>>();
            result.Add(ugrListNormalized);
            result.Add(val2);
            result.Add(val3);
            result.Add(angList);
            result.Add(new List<double>{ count});
            result.Add(ugrList);
            return result;
            

        }


        //compute glare angle values
        public static List<List<double>> ComputerGlareValuesOld(List<Point3d> floorPoints, List<Point3d> furniturePoints,
            List<Point3d> lightPoints, double threshDist = 10, double thresAngleLow = 50, double thresAngleHigh = 80)
        {
            List<double> angList = new List<double>();
            List<double> allAngles = new List<double>();
            int count = 0;
            for (int i = 0; i < floorPoints.Count; i++)
            {
                List<Point3d> selectedPts = new List<Point3d>();
                Point2d pt2FloorPt = ConvertToPoint2d(floorPoints[i]);
                for (int j = 0; j < furniturePoints.Count; j++)
                {
                    Point2d pt2Furniture = ConvertToPoint2d(furniturePoints[j]);
                    double distance = GraphicsUtility.DistanceBetweenPoints(pt2Furniture, pt2FloorPt);
                    if (distance < threshDist) selectedPts.Add(furniturePoints[j]);
                }// end of j for loop
                double ang = 0;
                for (int k = 0; k < selectedPts.Count; k++)
                {
                    //ang = 0;
                    List<Vector3d> vec3dList = new List<Vector3d>();
                    for (int m = 0; m < lightPoints.Count; m++)
                    {
                        if (lightPoints[m].X < selectedPts[k].X)
                        {
                            Vector3d vx = new Vector3d(1, 0, 0);
                            Vector3d vy = new Vector3d(0, 1, 0);
                            Vector3d vz = new Vector3d(0, 0, 1);

                            Vector3d vec = new Vector3d(lightPoints[m], selectedPts[k]);
                            vec3dList.Add(vec);
                            double angle = VectorUtility.AngleBetween(vec, vx);
                            Trace.WriteLine("Angle value is : " + angle);
                            if (angle > thresAngleLow && angle < thresAngleHigh) ang += angle;
                            else { angle = 0; }
                            //ang += angle;
                            allAngles.Add(angle);
                        }
                        count += 1;
                    }
                }
                if (ang == 0) angList.Add(-1);
                else angList.Add((ang / selectedPts.Count));


            }// end of i for loop
            //return angList;

            List<int> indexList = new List<int>();
            for (int n = 0; n < angList.Count; n++) indexList.Add(n);

            int colorVal = 255;
            List<int> sortedIndices = BasicUtility.Quicksort(angList, indexList, 0, angList.Count - 1);
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
            result.Add(angList);
            result.Add(new List<double> { count });
            result.Add(allAngles);
            return result;


        }



    }
}
