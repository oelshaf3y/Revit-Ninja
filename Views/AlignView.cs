using Autodesk.Revit.Attributes;
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
    internal class AlignView : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            BoundingBoxXYZ cropbox = doc.ActiveView.CropBox;
            DetailLine l = doc.GetElement(uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "pick Line")) as DetailLine;
            Line line = l.GeometryCurve as Line;
            using (Transaction tr = new Transaction(doc, "align view"))
            {
                tr.Start();
                //doc.ActiveView.CropBox.Transform.;
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }
}
