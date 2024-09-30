using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace Revit_Ninja.Alignment
{
    internal class AlignElement
    {
        public Element element;
        public XYZ location, horDir, vertDir;
        public Line horLine, vertLine;
        public int intersectionCount;
        public List<Tuple<AlignElement, XYZ>> intersections;
        public AlignElement(Element element, View activeView)
        {
            this.intersectionCount = 0;
            intersections = new List<Tuple<AlignElement, XYZ>>();
            this.element = element;
            if (this.element is IndependentTag)
            {
                this.location = ((IndependentTag)this.element).TagHeadPosition;
            }
            else this.location = element.getCG();
            horDir = activeView.CropBox.Transform.BasisX;
            vertDir = activeView.CropBox.Transform.BasisY;
            this.horLine = Line.CreateUnbound(this.location, horDir);
            this.vertLine = Line.CreateUnbound(this.location, vertDir);
        }

        public void intersectsHor(AlignElement other)
        {
            if (this.element.Id == other.element.Id) return;
            IntersectionResultArray ira = new IntersectionResultArray();
            if (this.vertLine.Intersect(other.horLine, out ira) != SetComparisonResult.Disjoint)
            {
                intersections.Add(Tuple.Create(other, ira.get_Item(0).XYZPoint));
                intersectionCount++;
            }
        }
        public void intersectsVert(AlignElement other)
        {
            if (this.element.Id == other.element.Id) return;
            IntersectionResultArray ira = new IntersectionResultArray();
            if (this.horLine.Intersect(other.vertLine, out ira) != SetComparisonResult.Disjoint)
            {
                intersections.Add(Tuple.Create(other, ira.get_Item(0).XYZPoint));
                intersectionCount++;
            }
        }
    }
}
