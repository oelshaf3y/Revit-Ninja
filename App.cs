using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace Revit_Ninja
{
    internal class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Failed;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string assemblyName = Assembly.GetExecutingAssembly().Location;
            string asPath = System.IO.Path.GetDirectoryName(assemblyName);
            string TabName = "Ninja";
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                return Result.Cancelled;
            }
            RibbonPanel panel;
            panel = application.CreateRibbonPanel(TabName,"Revit Ninja");
            PushButtonData SaveState = new PushButtonData("Save View State", "Save State", assemblyName, "Revit_Ninja.Views.SaveViewState")
            {
                Image = Properties.Resources.captures.ToImageSource(),
                LargeImage = Properties.Resources.capture.ToImageSource(),
                ToolTip = "Save visible elements in the current view to be reset later"
            };
            PushButtonData restoreState = new PushButtonData("Reset View State", "Reset State", assemblyName, "Revit_Ninja.Views.ResetViewState")
            {
                Image = Properties.Resources.resetViewS.ToImageSource(),
                LargeImage = Properties.Resources.ResetView.ToImageSource(),
                ToolTip = "reset stored elements to be the only visible elements."
            };
            PushButtonData ResetSheets = new PushButtonData("Reset Sheets State", "Reset Sheets", assemblyName, "Revit_Ninja.Views.ResetSheet")
            {
                Image = Properties.Resources.ResetSheetsS.ToImageSource(),
                LargeImage = Properties.Resources.resetSheets.ToImageSource(),
                ToolTip = "reset stored elements to be the only visible elements in all sheets."
            };
            PushButtonData HideUnhosted = new PushButtonData("Hide Unhosted Rebar", "Hide Unhosted", assemblyName, "Revit_Ninja.Reinforcement.HideUnHostedBars")
            {
                Image = Properties.Resources.hideUnhostedS.ToImageSource(),
                LargeImage = Properties.Resources.hideUnhosted.ToImageSource(),
                ToolTip = "Hide rebar bars which are not hosted by selected element."
            };
            PushButtonData NOS = new PushButtonData("Delete Not on sheets", "Delete N.O.S", assemblyName, "Revit_Ninja.Views.NotOnSheet")
            {
                Image = Properties.Resources.NOSsmall.ToImageSource(),
                LargeImage = Properties.Resources.NOS.ToImageSource(),
                ToolTip = "Delete Unused Views which are not hosted in sheets."
            };
            PushButtonData SelectBy = new PushButtonData("Select by", "Parameter Selection", assemblyName, "Revit_Ninja.SelectBy.SelectBy")
            {
                Image = Properties.Resources.selectbys.ToImageSource(),
                LargeImage = Properties.Resources.selectbyl.ToImageSource(),
                ToolTip = "Select All Elements By A Parameter Value"
            };
            PushButtonData ToggleReb = new PushButtonData("Toggle Rebar", "Rebar On/Off", assemblyName, "Revit_Ninja.Reinforcement.ToggleRebar")
            {
                Image = Properties.Resources.Rebs.ToImageSource(),
                LargeImage = Properties.Resources.Rebl.ToImageSource(),
                ToolTip = "Hide or unhide rebar category"
            };
            PushButtonData rebarByHost = new PushButtonData("Rebar's Host", "Rebar By Host", assemblyName, "Revit_Ninja.Reinforcement.RebarByHost")
            {
                Image = Properties.Resources.HostS.ToImageSource(),
                LargeImage = Properties.Resources.HostL.ToImageSource(),
                ToolTip = "Select Rebar By Selecting Host."
            };


            //PushButtonData BatchPrint = new PushButtonData("Batch Export", "Print/Export", assemblyName, "RSCC_GEN.exportAndPrint")
            //{
            //    Image = Properties.Resources.pdfSmall.ToImageSource(),
            //    LargeImage = Properties.Resources.pdfLarge.ToImageSource(),
            //    ToolTip = "Print or export multiple sheets, views or schedules at once."
            //};

            PulldownButtonData pd = new PulldownButtonData("options", "Ninja")
            {
                Image = Properties.Resources.pdfSmall.ToImageSource(),
                LargeImage = Properties.Resources.pdfLarge.ToImageSource(),
                ToolTip= "Print or export multiple sheets, views or schedules at once."
            };
            try
            {
                //panel.AddItem(BatchPrint);
                //panel.AddSeparator();

                panel.AddItem(NOS);
                panel.AddSeparator();
                panel.AddItem(SaveState);
                panel.AddStackedItems(restoreState, ResetSheets);
                panel.AddSeparator();
                panel.AddStackedItems(HideUnhosted, ToggleReb);
                panel.AddStackedItems(SelectBy, rebarByHost);
                //panel.AddSeparator();

                //panel.AddSeparator();
            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("exception", ex.StackTrace);
                TaskDialog.Show("exception", ex.Message);

            }
            //panel.AddItem(SaveState);
            //panel.AddItem(restoreState);
            //panel.AddItem(ResetSheets);

            return Result.Succeeded;
        }
    }

}
