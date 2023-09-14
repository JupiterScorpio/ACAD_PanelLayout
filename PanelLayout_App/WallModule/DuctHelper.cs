using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PanelLayout_App.WallModule
{
    public class DuctHelper
    {
        public static List<ObjectId> listOfDuctObjId = new List<ObjectId>();
        public static List<List<ObjectId>> listOfListOfDuctRoomLineId_With_WallName = new List<List<ObjectId>>();

    }
}
