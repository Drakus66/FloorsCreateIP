/* FloorsCreateIP
 * ExternalCommand.cs
 * https://stroytekproekt.ru/
 * © Stroytekproekt, 2023
 *
 * The external command.
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
    /// <summary>
    /// Revit external command.
    /// </summary>	
	[Transaction(TransactionMode.Manual)]
    public sealed partial class CreateFloors : IExternalCommand
    {
        class NewFloorData
        {
            public List<List<BorderCurveData>> floorCurveLoops;
            public Level level;
            public string roomNumber;
        }
        
        /// <summary>
        /// This method implements the external command within 
        /// Revit.
        /// </summary>
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Autodesk.Revit.DB.Document doc = commandData.Application.ActiveUIDocument.Document; // получаем текущий документ
            UIDocument uidoc = commandData.Application.ActiveUIDocument; // активный док

            // Тип пола по умолчанию
            string floortypename = "Пол_не_назначено";
            FloorType floorType = null;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementCategoryFilter catfilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);
            List<Element> fTypes = collector.WherePasses(catfilter).WhereElementIsElementType().ToElements().ToList();

            foreach (Element ft in fTypes)
            {
                if ((ft as FloorType) != null && ft.Name == floortypename) floorType = ft as FloorType;
            }

            if (floorType == null)
            {
                using (var TR = new Transaction(commandData.Application.ActiveUIDocument.Document, "Настройка типа"))
                {
                    TR.Start();
                    FloorType deffltype = null;
                    foreach (Element ft in fTypes)
                    {
                        if ((ft as FloorType) != null)
                        {
                            deffltype = ft as FloorType;
                        }
                    }
                    floorType = deffltype.Duplicate(floortypename) as FloorType;
                    Material finm = null;
                    try
                    {
                        finm = doc.GetElement(Material.Create(doc, "Условный пол")) as Material;
                    }
                    catch
                    {
                        FilteredElementCollector materials = new FilteredElementCollector(doc).OfClass(typeof(Material));
                        finm = materials.First(m => m.Name == "Условный пол") as Material;
                    }
                    finm.Color = new Autodesk.Revit.DB.Color(0, 128, 0);
                    finm.CutForegroundPatternColor = new Autodesk.Revit.DB.Color(0, 128, 0);
                    CompoundStructure cs = CompoundStructure.CreateSingleLayerCompoundStructure(MaterialFunctionAssignment.Substrate, 2 / 304.8, finm.Id);
                    cs.EndCap = EndCapCondition.NoEndCap;
                    floorType.SetCompoundStructure(cs);
                    floorType.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).Set("Отделка пола");
                    TR.Commit();
                }
            }

            List<Room> rooms = new List<Room>();
            //get rooms to analyze
            var selection = uidoc.Selection;
            List<ElementId> selectedIds = uidoc.Selection.GetElementIds().ToList(); // айдишники элементов, выбранных на активном виде
            foreach (ElementId eid in selectedIds)
            {
                Element selElem = doc.GetElement(eid);
                if (selElem is Room) rooms.Add(selElem as Room);
            }

            //show selct level window
            if (rooms.Count == 0)
            {
                FilteredElementCollector roomsCollector = new FilteredElementCollector(doc);
                ElementCategoryFilter roomCatfilter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);
                List<Element> docRooms = roomsCollector.WherePasses(roomCatfilter).WhereElementIsNotElementType().ToElements().ToList();



                FilteredElementCollector lvlsCollector = new FilteredElementCollector(doc);
                ElementCategoryFilter lvlCatfilter = new ElementCategoryFilter(BuiltInCategory.OST_Levels);
                List<Level> docLevels = lvlsCollector.WherePasses(lvlCatfilter)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Where(l => l is Level)
                    .Select(l => l as Level).ToList();

                LvlSelectForm selectForm = new LvlSelectForm(docLevels.Select(l => l.Name).ToList());

                selectForm.ShowDialog();

                if (!selectForm.okStart) return Result.Cancelled;

                docLevels = docLevels.Where(l => selectForm.SelectedLevels.Contains(l.Name)).ToList();

                foreach (Level selectedLvl in docLevels)
                {
                    FilteredElementCollector lvlRooms = new FilteredElementCollector(doc, docRooms.Select(r => r.Id).ToList());
                    ElementLevelFilter lvlFilter = new ElementLevelFilter(selectedLvl.Id);

                    rooms = rooms.Concat(lvlRooms.WherePasses(lvlFilter).Where(e => (e as Room).Area > 0).Select(e => e as Room).ToList()).ToList();
                }
            }

            if (rooms.Count == 0)
            {
                TaskDialog.Show("Построение полов", "Не выбрано ни одного помещения");
                return Result.Failed;
            }

            FilteredElementCollector doorsCollector = new FilteredElementCollector(doc);
            ElementCategoryFilter doorsCatfilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            List<ElementId> docDoors = doorsCollector.WherePasses(doorsCatfilter).WhereElementIsNotElementType().ToElementIds().ToList();

            
            List<NewFloorData> newFloors = new List<NewFloorData>();
            
            foreach (Room selectedRoom in rooms)
            {
                //Получение границ не через геометрию
                bool boundsGet = false;

                SpatialElementBoundaryOptions segOpt = new SpatialElementBoundaryOptions();
                segOpt.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;
                segOpt.StoreFreeBoundaryFaces = true;
                IList<IList<BoundarySegment>> roomSegments = selectedRoom.GetBoundarySegments(segOpt);

                roomSegments = roomSegments.OrderByDescending(rs => rs.Sum(c => c.GetCurve().Length)).ToList();

                //get room near doors
                BoundingBoxXYZ roomBB = selectedRoom.get_BoundingBox(null);
                Outline roomOutline = new Outline(roomBB.Min.Subtract(new XYZ(1, 1, 0)), roomBB.Max.Add(new XYZ(1, 1, 0)));
                BoundingBoxIntersectsFilter roomBBFilter = new BoundingBoxIntersectsFilter(roomOutline);

                FilteredElementCollector roomNearDoorsCollector = new FilteredElementCollector(doc, docDoors);
                List<Element> nearDoors = roomNearDoorsCollector.WherePasses(roomBBFilter).ToElements().ToList();   


                double perimetr = 0;
                if (roomSegments.Count > 0)
                {
                    List<BoundarySegment> OuterBund = roomSegments.First().ToList();
                    perimetr = OuterBund.Sum(b => b.GetCurve().Length);
                }

                NewFloorData nFloor = new NewFloorData()
                {
                    roomNumber = selectedRoom.Number,
                    level = selectedRoom.Level,
                    floorCurveLoops = new List<List<BorderCurveData>>()
                };
                


                foreach (List<BoundarySegment> boundCountur in roomSegments)
                {
                    List<Curve> curveCountur = new List<Curve>();
                    CurveLoop testCurveLoop = new CurveLoop();
                    List<BorderCurveData> counturData = new List<BorderCurveData>();

                    foreach (BoundarySegment b in boundCountur)
                    {
                        testCurveLoop.Append(b.GetCurve());
                        curveCountur.Add(b.GetCurve());
                        BorderCurveData bCurve = new BorderCurveData(b, doc, nearDoors);
                        counturData.Add(bCurve);
                    }

                    bool isCounterClock = testCurveLoop.IsCounterclockwise(XYZ.BasisZ);
                    bool IsOpen = testCurveLoop.IsOpen();
                    bool hasPlane = testCurveLoop.HasPlane();

                    nFloor.floorCurveLoops.Add(counturData);
                }

                newFloors.Add(nFloor);
            }

            List<FloorOpening> floOpenings = new List<FloorOpening>();

            using (var TR = new Transaction(commandData.Application.ActiveUIDocument.Document, "Построение полов"))
            {
                TR.Start();

                foreach(NewFloorData nFloor in newFloors)
                {
                   
                    CurveArray outerCurveArray = new CurveArray();
                    foreach (BorderCurveData bcData in nFloor.floorCurveLoops.First())
                    {
                        foreach (Curve c in bcData.curves) outerCurveArray.Append(c);
                    }
                    Floor newFloor = doc.Create.NewFloor(outerCurveArray, floorType, nFloor.level, false, XYZ.BasisZ);

                    if (nFloor.floorCurveLoops.Count > 1)
                    {
                        //Отверстия пола
                        for (int i = 1; i < nFloor.floorCurveLoops.Count; i++)
                        {
                            FloorOpening fo = new FloorOpening();
                            fo.OpeningCurves = new CurveArray();
                            fo.destFloor = newFloor;
                            CurveArray OpeningCurves = new CurveArray();
                            foreach (BorderCurveData bcData in nFloor.floorCurveLoops[i])
                            {
                                foreach (Curve c in bcData.curves) fo.OpeningCurves.Append(c);
                            }
                            floOpenings.Add(fo);
                        }
                    }
                }

                doc.Regenerate();

                foreach (FloorOpening fo in floOpenings)
                {
                    doc.Create.NewOpening(fo.destFloor, fo.OpeningCurves, true);
                }

                TR.Commit();
            }



            Result result = Result.Succeeded;
            return result;
        }
    }
}
