using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelLayout_App.WallModule
{
    public class StairCaseHelper
    {
        public static List<ObjectId> listOfStairCaseObjId = new List<ObjectId>();
        public static List<List<ObjectId>> listOfListOfStaircaseRoomLineId_With_WallName = new List<List<ObjectId>>();
    }
}
