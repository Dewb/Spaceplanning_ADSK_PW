using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace SpacePlanning.MainSys
{
    class Utility
    {

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
