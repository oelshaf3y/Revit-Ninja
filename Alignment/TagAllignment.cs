using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Ninja.Alignment
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class TagAllignment : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            List<IndependentTag> tags = uidoc.Selection.PickObjects(ObjectType.Element, new TagSelectionFilter(), "Select Tags")
                .Select(x => doc.GetElement(x)).Cast<IndependentTag>().ToList();

            XYZ point = uidoc.Selection.PickPoint("Select alignment point.");
            double offset = Ninja.mmToFeet(500);
            using (Transaction tr = new Transaction(doc, "Align Tags"))
            {
                tr.Start();
                foreach (IndependentTag tag in tags)
                {
                    XYZ temp = tag.TagHeadPosition;
                    tag.TagHeadPosition = new XYZ(point.X, temp.Y, temp.Z);
                    temp = new XYZ(point.X, temp.Y, temp.Z);
                    tag.LeaderEndCondition = LeaderEndCondition.Free;
                    XYZ end = tag.GetLeaderEnd(tag.GetTaggedReferences().First());
                    tag.SetLeaderElbow(tag.GetTaggedReferences().First(), new XYZ(end.X, temp.Y, end.Z));
                    tag.LeaderEndCondition = LeaderEndCondition.Attached;
                }
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }

    internal class TagSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is IndependentTag;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
