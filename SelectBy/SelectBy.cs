using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Revit_Ninja.SelectBy
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class SelectBy : IExternalCommand
    {
        public Document doc { get; set; }
        public UIDocument uidoc { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            List<ElementId> ids = new List<ElementId>();
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            FilteredElementCollector AllElements = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType();
            Parameters.AddRange(AllElements.SelectMany(x => x.GetOrderedParameters()).Select(x => x.Definition.Name).ToList());
            //FilteredElementCollector allElements = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType();
            SelectByForm form = new SelectByForm();
            Parameters.Sort();
            form.comboBox1.Items.AddRange(Parameters.Distinct().ToArray());
            form.ShowDialog();
            if (form.DialogResult == DialogResult.Cancel) return Result.Cancelled;
            try
            {

                string val = form.textBox1.Text;
                string parName = form.comboBox1.Text;
                foreach (Element element in AllElements)
                {
                    if (element.LookupParameter(parName) != null)
                    {
                        if (element.LookupParameter(parName).AsString() != null)
                            if (element.LookupParameter(parName).AsString().ToLower().Contains(val.ToLower()))
                            {
                                if (element.Id != null) ids.Add(element.Id);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                doc.print(ex.Message);
            }
            uidoc.Selection.SetElementIds(ids);

            return Result.Succeeded;
        }
    }
}
