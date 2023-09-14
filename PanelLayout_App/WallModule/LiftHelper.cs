
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelLayout_App.WallModule
{
    public class LiftHelper
    {
        public static List<ObjectId> listOfLiftObjId = new List<ObjectId>();
        public static List<List<ObjectId>> listOfListOfLiftRoomLineId_With_WallName = new List<List<ObjectId>>();

    }
}
