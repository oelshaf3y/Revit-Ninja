using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Ninja.Alignment
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class AlignVertically : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            XYZ dir = doc.ActiveView.CropBox.Transform.BasisX;
            List<Element> selectedElements = uidoc.Selection.GetElementIds().Select(x => doc.GetElement(x)).ToList();
            List<AlignElement> alignElements = selectedElements.Select(x => new AlignElement(x, doc.ActiveView)).ToList();
            foreach (AlignElement alignElement in alignElements)
            {
                foreach (AlignElement other in alignElements)
                {
                    alignElement.intersectsHor(other);
                }
            }
            AlignElement source = alignElements.OrderByDescending(x => x.intersectionCount).FirstOrDefault();
            using (Transaction tr = new Transaction(doc, "Align Horizontally"))
            {
                //doc.print(source.intersectionCount);
                tr.Start();
                foreach (Tuple<AlignElement, XYZ> intersection in source.intersections)
                {
                    if (intersection.Item1.location != intersection.Item2)
                    {
                        if (intersection.Item1.element is IndependentTag)
                        {
                            IndependentTag tag = intersection.Item1.element as IndependentTag;
                            tag.TagHeadPosition = intersection.Item2;
                        }
                        else ((LocationPoint)intersection.Item1.element.Location).Point = intersection.Item2;
                    }
                }
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }
}
