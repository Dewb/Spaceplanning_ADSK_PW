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


        #region- Public Methods
        //builds the line3d of the furnitures
        public static List<Line3d> GetLine2dFurnitures(List<NurbsCurve> nurbList, double height = 0)
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


        //gets the points on the floor in point3d format
        public static List<Point3d> GetFloorPoints(List<Cell> cellList, double height = 0)
        {
            List<Point3d> ptList = new List<Point3d>();
            for (int i = 0; i < cellList.Count; i++) ptList.Add(ConvertToPoint3d(cellList[i].CenterPoint, height));
            return ptList;
        }

        //gets the mid point of the furnitures in point3d
        public static List<Point3d> GetFurniturePoints(List<Line3d> lineList)
        {
            List<Point3d> ptList = new List<Point3d>();
            double height = lineList[0].StartPoint.Z;
            for (int i = 0; i < lineList.Count; i++)
            {
                Point2d pt1 = new Point2d(lineList[i].StartPoint.X, lineList[i].StartPoint.Y);
                Point2d pt2 = new Point2d(lineList[i].EndPoint.X, lineList[i].EndPoint.Y);
                Point2d midPt = new Point2d((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
                ptList.Add(ConvertToPoint3d(midPt, height));
            }
            return ptList;
        }

        //gets the mid point of the furnitures in point3d
        public static List<Point3d> GetLightingPointsRandomly(List<Cell> cellList, Random ran, double height = 0, double maxNum = 20, double recompute = 5)
        {
            double count = 0;
            List<Point3d> ptList = new List<Point3d>(), lightPtList = new List<Point3d>();
            for (int i = 0; i < cellList.Count; i++)
            {
                Point3d pt3d = ConvertToPoint3d(cellList[i].CenterPoint, height);
                ptList.Add(pt3d);
            }

            //Random ran = new Random();
            for (int i = 0; i < ptList.Count; i++)
            {
                count += 1;
                if (count < maxNum)
                {
                    double num = BasicUtility.RandomBetweenNumbers(ran, 0, 5);
                    if (num < 3) lightPtList.Add(ptList[i]);
                }
            }
            return lightPtList;
        }


        //gets the mid point of the furnitures in point3d
        public static List<Point3d> GetLightingPointsNAmount(List<Cell> cellList, double height = 0, double startPt = 0, double num = 15)
        {
            List<Point3d> ptList = new List<Point3d>(), lightPtList = new List<Point3d>();
            for (int i = 0; i < cellList.Count; i++)
            {
                Point3d pt3d = ConvertToPoint3d(cellList[i].CenterPoint, height);
                ptList.Add(pt3d);
            }
            for (int i = 0; i < ptList.Count; i++) if (i > startPt && i < num + startPt) lightPtList.Add(ptList[i]);
            return lightPtList;
        }


        //gets the mid point of the furnitures in point3d
        public static List<Point3d> GetLightingPointsEveryXTime(List<Cell> cellList, double height = 0, double div = 15)
        {
            List<Point3d> ptList = new List<Point3d>(), lightPtList = new List<Point3d>();
            for (int i = 0; i < cellList.Count; i++)
            {
                Point3d pt3d = ConvertToPoint3d(cellList[i].CenterPoint, height);
                ptList.Add(pt3d);
            }
            for (int i = 0; i < ptList.Count; i++) if (i % div == 0) lightPtList.Add(ptList[i]);
            return lightPtList;
        }

        //place furntiures at a set height
        public static List<Line3d> PlaceFurnitureAtHeight(List<Line3d> furnitureList, double height = 0)
        {
            List<Line3d> lineList = new List<Line3d>();
            for (int i = 0; i < furnitureList.Count; i++)
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
        public static List<Cell> PlaceCellsInsideOutline(List<Point2d> outLinePts, double dim = 1)
        {
            List<Cell> cellList = GridObject.CellsInsidePoly(outLinePts, dim);
            List<Cell> cellNewList = new List<Cell>();
            for (int i = 0; i < cellList.Count; i++)
            {
                Point2d center = cellList[i].CenterPoint;
                Point2d centerNew = new Point2d(center.X + (dim / 2), center.Y + (dim / 2));
                Cell cl = new Cell(centerNew, dim, dim, true);
                cellNewList.Add(cl);
            }
            return cellNewList;
        }


        //visualize glare lines
        public static List<Line3d> VisualizeGlareLines(List<Point3d> floorPoints, List<Point3d> furniturePoints,
            List<Point3d> lightPoints, double threshDist = 10, int index = 0)
        {
            for (int i = 0; i < floorPoints.Count; i++)
            {
                List<Point3d> selectedPts = new List<Point3d>();
                Point2d pt2FloorPt = ConvertToPoint2d(floorPoints[i]);
                for (int j = 0; j < furniturePoints.Count; j++)
                {
                    Point2d pt2Furniture = ConvertToPoint2d(furniturePoints[j]);
                    double distance = PointUtility.DistanceBetweenPoints(pt2Furniture, pt2FloorPt);
                    if (distance < threshDist) selectedPts.Add(furniturePoints[j]);
                }// end of j for loop   
                if (selectedPts.Count > 1) lightPoints.AddRange(selectedPts);
            }
            List<Line3d> glareLines = new List<Line3d>();
            for (int i = 0; i < lightPoints.Count; i++)
            {
                if (lightPoints[i].X > floorPoints[index].X)
                    glareLines.Add(new Line3d(floorPoints[index], lightPoints[i]));
            }
            return glareLines;
        }


        //compute glare  values
        public static List<List<double>> ComputeGlareValues(List<Point3d> floorPoints, List<Point3d> furniturePoints,
            List<Point3d> lightPoints, double threshDist = 10, double lightSize = 3, double numSpecialLights = 2, double recompute = 1)
        {
            int pos = 0;
            List<double> posList = new List<double>();
            List<double> distList = new List<double>();
            List<double> ugrList = new List<double>();
            int count = 0;
            double numD = BasicUtility.RandomBetweenNumbers(new Random(), 0.1, 0.35);
            for (int i = 0; i < numSpecialLights; i++) posList.Add(lightPoints.Count * (i + 1) * numD);

            pos = (int)(lightPoints.Count * 0.20);
            for (int i = 0; i < floorPoints.Count; i++)
            {
                List<Point3d> selectedPts = new List<Point3d>();
                Point2d pt2FloorPt = ConvertToPoint2d(floorPoints[i]);
                for (int j = 0; j < furniturePoints.Count; j++)
                {
                    Point2d pt2Furniture = ConvertToPoint2d(furniturePoints[j]);
                    double distance = PointUtility.DistanceBetweenPoints(pt2Furniture, pt2FloorPt);
                    distList.Add(distance);
                    if (distance < threshDist) selectedPts.Add(furniturePoints[j]);
                }// end of j for loop   
                if (selectedPts.Count > 0) lightPoints.AddRange(selectedPts);
                double ugrValue = CalculateUGR(lightPoints, floorPoints[i], posList, lightSize, recompute);
                ugrList.Add(ugrValue);
                count += 1;
            }
            List<double> val2 = new List<double>(), val3 = new List<double>();
            for (int n = 0; n < ugrList.Count; n++) { val2.Add(0); val3.Add(0); }
            List<double> ugrListNormalized = BasicUtility.NormalizeList(ugrList, 0, 255);
            List<List<double>> result = new List<List<double>>();
            result.Add(ugrListNormalized);
            result.Add(val2);
            result.Add(val3);
            result.Add(distList);
            result.Add(new List<double> { count });
            result.Add(ugrList);
            result.Add(new List<double> { lightSize });
            result.Add(posList);
            return result;
        }

        #endregion



        #region - Private Methods
        //converts point2d to point3d element
        internal static Point3d ConvertToPoint3d(Point2d ptInput, double height = 0)
        {
            return new Point3d(ptInput, height);
        }

        //converts point2d to point2d element
        internal static Point2d ConvertToPoint2d(Point3d ptInput)
        {
            return new Point2d(ptInput.X, ptInput.Y);
        }

        //computes the UGR value
        internal static double CalculateUGR(List<Point3d> lightPts, Point3d observer, List<double> posList, double lightSize = 2, double recompute = 1)
        {
            double backLum = 10;
            double lumin = 10;
            double angleEach = 0;
            double guthPos = 2;
            double value = 0;
            double summation = 0;
            int pos = 0, m = 0;

            for (int i = 0; i < lightPts.Count; i++)
            {
                if (m < posList.Count && i == (int)posList[m]) { lumin = recompute * lumin; m += 1; }
                else lumin = 10;
                if (lightPts[i].X > observer.X)
                {
                    Point2d ptLight2d = ConvertToPoint2d(lightPts[i]);
                    Point2d ptObserver2d = ConvertToPoint2d(observer);
                    double distance = PointUtility.DistanceBetweenPoints(ptLight2d, ptObserver2d);
                    guthPos = distance;

                    Point3d lightPt1 = new Point3d((lightPts[i].X - lightSize), (lightPts[i].Y - lightSize), lightPts[i].Z);
                    Point3d lightPt2 = new Point3d((lightPts[i].X + lightSize), (lightPts[i].Y + lightSize), lightPts[i].Z);
                    Vector3d vec1 = new Vector3d(observer, lightPt1);
                    Vector3d vec2 = new Vector3d(observer, lightPt2);
                    angleEach = VectorUtility.AngleBetweenVec3d(vec1, vec2);
                    summation += (lumin * lumin * angleEach) / (guthPos * guthPos);
                }
            }
            value = (0.25 / backLum) * summation;
            if (value == 0) return 0;
            double ugr = 8 * Math.Log10(value);
            if (ugr < 0) ugr = 0;
            return ugr;
        }


        #endregion




    }
}
