/* BorderCurveData.cs
 * https://stroytekproekt.ru/
 * © Stroytekproekt, 2023
 *
 * This updater is used to create an updater capable of reacting
 * to changes in the Revit model.
 */
#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using WPF = System.Windows;

#endregion

namespace FloorsCreateIP
{
    public class BorderCurveData
    {
        public List<Curve> curves;

        //data for doors
        public Wall hostWall;
        public List<Element> doors;

        public BorderCurveData (BoundarySegment segment, Room room, Document doc, List<Element> roomDoors)
        {
            curves = new List<Curve>();
            curves.Add(segment.GetCurve());
            if (segment.LinkElementId != new ElementId(-1)) return;

            Element hostElem = doc.GetElement(segment.ElementId);
            if (!(hostElem is Wall)) return;

            doors = new List<Element>();

            hostWall = (Wall)hostElem;

            foreach (Element el in roomDoors)
            {
                if (!(el is FamilyInstance)) continue;
                if ((el as FamilyInstance).Host.Id.IntegerValue == hostWall.Id.IntegerValue) doors.Add(el);
            }
            
            //Create doors 
            if (doors.Count > 0) 
            {
                //sort along curve
                doors = doors.OrderBy(fi => (fi.Location as LocationPoint).Point.DistanceTo(curves.First().GetEndPoint(0))).ToList();

                double borderZ = curves.First().GetEndPoint(0).Z;


                foreach (Element el in doors)
                {
                    Curve borderCurve = curves.Last();
                    FamilyInstance door = (FamilyInstance)el;

                    //get door orient
                    Room fromRoom = null;
                    if (fromRoom == null)
                    {
                        XYZ testPoint = (door.Location as LocationPoint).Point;
                        testPoint = testPoint.Add(XYZ.BasisZ.Multiply(3)).Add(door.FacingOrientation.Negate().Multiply(3));
                        fromRoom = doc.GetRoomAtPoint(testPoint);
                    }

                    if (fromRoom == null || fromRoom.Id.IntegerValue != room.Id.IntegerValue) continue;

                    //get door width
                    BoundingBoxXYZ symbolBB = door.Symbol.get_BoundingBox(null);
                    double doorWith = symbolBB.Max.X - symbolBB.Min.X;

                    //create side lines
                    XYZ locPoint = (door.Location as LocationPoint).Point;
                    XYZ p1 = locPoint.Add(door.HandOrientation.Multiply(doorWith / 2));
                    XYZ p2 = locPoint.Add(door.HandOrientation.Negate().Multiply(doorWith / 2));

                    //sort points
                    if (p2.DistanceTo(borderCurve.GetEndPoint(0)) < p1.DistanceTo(borderCurve.GetEndPoint(0)))
                    {
                        XYZ p3 = p1;
                        p1 = p2;
                        p2 = p3;
                    }

                    p1 = new XYZ(p1.X, p1.Y, 0);
                    p2 = new XYZ(p2.X, p2.Y, 0);


                    //get points on border corve
                    Line l1 = Line.CreateUnbound(p1, door.FacingOrientation.Negate());
                    Line l2 = Line.CreateUnbound(p2, door.FacingOrientation.Negate());

                    Curve zeroZBorder = null;
                    XYZ zeroZP1 = new XYZ(borderCurve.GetEndPoint(0).X, borderCurve.GetEndPoint(0).Y, 0);
                    XYZ zeroZP2 = new XYZ(borderCurve.GetEndPoint(1).X, borderCurve.GetEndPoint(1).Y, 0);

                    if (borderCurve is Arc)
                    {
                        XYZ zeroZArcP = borderCurve.Evaluate(0.5, true);
                        zeroZArcP = new XYZ(zeroZArcP.X, zeroZArcP.Y, 0);
                        zeroZBorder = Arc.Create(zeroZP1, zeroZP2, zeroZArcP);
                    }
                    else
                    {
                        zeroZBorder = Line.CreateBound(zeroZP1, zeroZP2);
                    }

                        
                    IntersectionResultArray intersectDataP1 = null;
                    SetComparisonResult resultP1 = zeroZBorder.Intersect(l1, out intersectDataP1);

                    IntersectionResultArray intersectDataP2 = null;
                    SetComparisonResult resultP2 = zeroZBorder.Intersect(l2, out intersectDataP2);

                    if (resultP1 == SetComparisonResult.Overlap && resultP2 == SetComparisonResult.Overlap)
                    {
                        //create new points
                        XYZ newPoint1 = intersectDataP1.get_Item(0).XYZPoint;
                        newPoint1 = new XYZ(newPoint1.X, newPoint1.Y, borderZ);
                        XYZ newPoint2 = newPoint1.Add(door.FacingOrientation.Multiply(hostWall.Width));

                        XYZ newPoint4 = intersectDataP2.get_Item(0).XYZPoint;
                        newPoint4 = new XYZ(newPoint4.X, newPoint4.Y, borderZ);
                        XYZ newPoint3 = newPoint4.Add(door.FacingOrientation.Multiply(hostWall.Width));

                        Curve newLine1 = null;
                        Curve newLine5 = null;


                        if (borderCurve is Arc)
                        {
                            Arc borderArc = (Arc)borderCurve;
                            XYZ pointOnArc1 = borderArc.ComputeDerivatives(0.01, true).Origin;
                            XYZ pointOnArc2 = borderArc.ComputeDerivatives(0.99, true).Origin;

                            newLine1 = Arc.Create(borderArc.GetEndPoint(0), newPoint1, pointOnArc1);
                            newLine5 = Arc.Create(newPoint4, borderArc.GetEndPoint(1), pointOnArc2);
                        }
                        else
                        {
                            newLine1 = Line.CreateBound(borderCurve.GetEndPoint(0), newPoint1);
                            newLine5 = Line.CreateBound(newPoint4, borderCurve.GetEndPoint(1));
                        }

                        Curve newLine2 = Line.CreateBound(newPoint1, newPoint2);
                        Curve newLine3 = Line.CreateBound(newPoint2, newPoint3);
                        Curve newLine4 = Line.CreateBound(newPoint3, newPoint4);

                        int LastIndex = curves.Count - 1;
                        curves.RemoveAt(LastIndex);

                        curves.Add(newLine1);
                        curves.Add(newLine2);
                        curves.Add(newLine3);
                        curves.Add(newLine4);
                        curves.Add(newLine5);
                    }
                }
            }
        }
    }

    class FloorOpening
    {
        public Floor destFloor;
        public CurveArray OpeningCurves;
    }
}
