﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Ninja.Views
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class SaveViewState : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            View activeView = doc.ActiveView;
            StringBuilder sb = new StringBuilder();
            List<Element> onlyVisible = new List<Element>();
            if (activeView is ViewSheet || activeView is ViewSchedule)
            {
                doc.print("Active view must be a view not a sheet or a schedule");
                return Result.Failed;
            }
            Parameter state = activeView.LookupParameter("View State");
            if (state == null)
            {
                if (!
                    Ninja.assignParameter(
                        commandData,
                        "Ninja-Views",
                        "View State",
                        BuiltInCategory.OST_Views,
                        SpecTypeId.String.Text)
                    )
                {
                    doc.print("Please Create a new Project Parameter (Instance Parameter) for views\nParameter Name: View State\nType: Text");
                    return Result.Failed;
                }
                else
                {
                    state = activeView.LookupParameter("View State");
                    if (state == null)
                    {
                        doc.print("Please Create a new Project Parameter (Instance Parameter) for views\nParameter Name: View State\nType: Text");
                    }
                }
            }
            if (state.IsReadOnly)
            {
                doc.print("make sure the View State Parameter is editable!");
                return Result.Failed;
            }
            if (state.AsString() != null)
            {
                if (state.AsString().Trim().Length > 0)
                {
                    if (doc.YesNoMessage("View State is saved already!.\nAre you sure you want to update?") == TaskDialogResult.No)
                    {
                        doc.print("Canceled");
                        return Result.Cancelled;
                    }
                }
            }
            FilteredElementCollector collector = new FilteredElementCollector(doc, activeView.Id).WhereElementIsNotElementType();
            onlyVisible = collector.Where(x => x.CanBeHidden(activeView)).ToList();
            foreach (ElementId id in onlyVisible.Select(x => x.Id))
            {
                sb.Append(id.ToString() + ",");
            }
            //uidoc.Selection.SetElementIds(onlyVisible.Select(x => x.Id).ToList());
            using (Transaction tr = new Transaction(doc, "Save View State"))
            {
                tr.Start();
                state.Set(sb.ToString());
                tr.Commit();
                tr.Dispose();
            }
            return Result.Succeeded;
        }
    }
}
