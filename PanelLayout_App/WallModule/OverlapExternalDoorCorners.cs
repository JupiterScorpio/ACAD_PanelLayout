using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace PanelLayout_App.WallModule
{
    public class OverlapExternalDoorCorners
    {
        public ObjectId CornerId { get; set; }

        public List<ObjectId> TextIds { get; set; }
        public Point2d WallCornerPointOnDoor { get; set; }
        public Point3d WallCornerBasePoint { get; set; }
        public ObjectId WallCornerId { get; set; }
        public List<ObjectId> WallCornerTextId { get; set; }
    }

    public class DoorEACorner
    {
        public ObjectId CornerId { get; set; }
        public List<ObjectId> TextIds { get; set; }
        public Point3d WallCornerPointOnDoor { get; set; }

        public DoorEACorner() { }
    }
}
