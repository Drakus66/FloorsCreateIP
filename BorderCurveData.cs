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
        public Curve curve;

        //data for doors
        public Wall hostWall;
        public List<Element> doors;

        public BorderCurveData (BoundarySegment segment, Document doc, List<Element> roomDoors)
        {
            curve = segment.GetCurve();
            if (segment.LinkElementId != new ElementId(-1)) return;

            Element hostElem = doc.GetElement(segment.ElementId);
            if (!(hostElem is Wall)) return;

            hostWall = (Wall)hostElem;

            foreach (Element el in roomDoors)
            {
                if (!(el is FamilyInstance)) continue;
                if ((el as FamilyInstance).Host.Id.IntegerValue == hostWall.Id.IntegerValue) doors.Add(el);
            }   
        }
    }
}
