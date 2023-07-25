#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using adWin = Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using WPF = System.Windows;

#endregion

namespace FloorsCreateIP
{
    public sealed partial class StartClass : IExternalApplication
    {
        Result IExternalApplication.OnStartup(UIControlledApplication uic_app)
        {

            string assemblyName = Assembly.GetExecutingAssembly().Location;

            bool tabexist = false;
            //Вкладка Стройтэкпроект
            adWin.RibbonTab stptab = null;
            foreach (Autodesk.Windows.RibbonTab tab in Autodesk.Windows.ComponentManager.Ribbon.Tabs)
            {
                if (tab.Title == "ИнвестПроект")
                {
                    tab.IsVisible = true;
                    tabexist = true;
                    stptab = tab;
                }
            }

            if (!tabexist) uic_app.CreateRibbonTab("ИнвестПроект");

            Autodesk.Revit.UI.RibbonPanel workPanel = null;
            foreach (Autodesk.Revit.UI.RibbonPanel rp in uic_app.GetRibbonPanels("ИнвестПроект"))
            {
                if (rp.Name == "Отделка")
                {
                    workPanel = rp;
                }
            }

            if (workPanel == null) workPanel = uic_app.CreateRibbonPanel("ИнвестПроект", "Отделка");

            var pushButton = new PushButtonData("FloorCreate", "Пол\nПостроение", assemblyName, "FloorsCreateIP.CreateFloors")
            {
                ToolTip = "Построить условный пол по выбранным помещениям",
            };

            workPanel.AddItem(pushButton);

            if (!Domain()) workPanel.Enabled = false;

            return Result.Succeeded;
        }

        static public bool Domain()
        {
            if (File.Exists("\\\\pamir.local\\fs\\Инвестпроект\\Revit server\\revitkey.txt")) return true;
            //if (File.Exists("\\\\hyperv\\Revit2019\\АРХИВ\\11.txt")) return true;
            else
            {
#if DEBUG                
                try
                {
                    File.OpenRead("\\\\pamir.local\\fs\\Инвестпроект\\Revit server\\revitkey.txt");
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Ошибка", ex.Message);
                }
#endif

                return false;
            }
           
        }


        Result IExternalApplication.OnShutdown(UIControlledApplication uic_app)
        {

            Result result = Result.Succeeded;
            return result;
        }
    }
}
