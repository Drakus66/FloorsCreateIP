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

namespace STP.FloorsCreateIP
{
    /// <summary>
    /// Revit external command.
    /// </summary>	
	[Transaction(TransactionMode.Manual)]
    public sealed partial class CreateFloors : IExternalCommand
    {
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
            FloorType flotype = null;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementCategoryFilter catfilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);
            IList<Element> fTypes = collector.WherePasses(catfilter).WhereElementIsElementType().ToElements();

            foreach (Element ft in fTypes)
            {
                if ((ft as FloorType) != null && ft.Name == floortypename) flotype = ft as FloorType;
            }

            if (flotype == null)
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
                    flotype = deffltype.Duplicate(floortypename) as FloorType;
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
                    flotype.SetCompoundStructure(cs);
                    flotype.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).Set("Отделка пола");
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

            }




            Result result = Result.Succeeded;
            return result;
        }
    }
}
