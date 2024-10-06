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
    internal class AlignText : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            TextNote note = uidoc.Selection.GetElementIds().Select(x => doc.GetElement(x)).Where(x => x is TextNote)
                .FirstOrDefault() as TextNote;
            Leader leader = note.GetLeaders().FirstOrDefault();
            XYZ a = leader.End;
            XYZ b = leader.Elbow;
            XYZ c = leader.Anchor;

            IndependentTag tag = uidoc.Selection.GetElementIds().Select(x => doc.GetElement(x)).Where(x => x is IndependentTag)
                .Last() as IndependentTag;
            XYZ elbow = tag.GetLeaderElbow(tag.GetTaggedReferences().Last());
            XYZ end = tag.GetLeaderEnd(tag.GetTaggedReferences().Last());


            using (Transaction tr = new Transaction(doc, "Leader"))
            {
                tr.Start();
                XYZ apoint = new XYZ(leader.End.X, leader.End.Y, doc.ActiveView.CropBox.Transform.Origin.Z);
                XYZ bpoint = new XYZ(leader.Elbow.X, leader.Elbow.Y, doc.ActiveView.CropBox.Transform.Origin.Z);
                XYZ cpoint = new XYZ(leader.Anchor.X, leader.Anchor.Y, doc.ActiveView.CropBox.Transform.Origin.Z);
                XYZ zpoint = new XYZ(elbow.X, elbow.Y, doc.ActiveView.CropBox.Transform.Origin.Z);

                DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel)).SetShape(new List<GeometryObject> {
                Line.CreateBound(apoint,apoint.Add(3*doc.ActiveView.CropBox.Transform.BasisY)),
                Line.CreateBound(bpoint,bpoint.Add(3*doc.ActiveView.CropBox.Transform.BasisY)),
                Line.CreateBound(cpoint,cpoint.Add(3*doc.ActiveView.CropBox.Transform.BasisY)),
                Line.CreateBound(zpoint,zpoint.Add(3*doc.ActiveView.CropBox.Transform.BasisY)),
                Line.CreateBound(tag.TagHeadPosition,tag.TagHeadPosition.Add(3*doc.ActiveView.CropBox.Transform.BasisY)),

                Line.CreateBound(apoint,apoint.Add(3*doc.ActiveView.CropBox.Transform.BasisX)),
                Line.CreateBound(bpoint,bpoint.Add(3*doc.ActiveView.CropBox.Transform.BasisX)),
                Line.CreateBound(cpoint,cpoint.Add(3*doc.ActiveView.CropBox.Transform.BasisX)),
                Line.CreateBound(zpoint,zpoint.Add(3*doc.ActiveView.CropBox.Transform.BasisX)),
                Line.CreateBound(tag.TagHeadPosition,tag.TagHeadPosition.Add(3*doc.ActiveView.CropBox.Transform.BasisX)),
                });
                tr.Commit();

            }

            return Result.Succeeded;
        }
    }
}
