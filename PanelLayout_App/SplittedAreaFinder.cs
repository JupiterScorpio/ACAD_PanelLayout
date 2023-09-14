using System;
using System.Collections.Generic;
using System.Linq;

namespace PanelLayout_App
{
    public enum MaxAreaExistingSide { None, Left, Right, Bottom, Top };
    internal class SplittedAreaFinder
    {
        private int[][] data;
        private double[][] areas;

        private int mkey;
        Dictionary<int, MaxAreaExistingSide> dicRectIdToRectSharingSide = new Dictionary<int, MaxAreaExistingSide>();
        private List<int[][]> lstValidDataBasedOnArea;

        // Cuong - 20/12/2022 - DeckPanel issue
        public List<int[][]> lstInvalidDataBasedOnArea;

        private int[][] finalResult;

        public SplittedAreaFinder(int[][] data, double[][] dataA)
        {
            this.data = data;
            this.areas = dataA;
        }

        internal int maxKey()
        {
            return mkey;
        }

        internal int[][] Result
        {
            get
            {
                return finalResult;
            }
        }

        internal Dictionary<int, MaxAreaExistingSide> Relation
        {
            get
            {
                return dicRectIdToRectSharingSide;
            }
        }

        internal bool Find()
        {
            Dictionary<int, List<int[][]>> dicKeyToResult = new Dictionary<int, List<int[][]>>();
            var obj = new MaxRectFinder(data, areas, 1, dicKeyToResult);
            obj.Find();

            if (dicKeyToResult.Count == 0)
            {
                return false;
            }

            mkey = dicKeyToResult.Keys.Min();

            List<int[][]> lsR = new List<int[][]>();
            lsR.AddRange(dicKeyToResult[mkey]);

            var dicAreaToMatchingList = SortByArea(lsR, 1);

            // Cuong - 20/12/2022 - DeckPanel issue - START
            //if (!FindValidData(dicAreaToMatchingList))
            //{
            //    return false;
            //}
            if (!FindValidData(dicAreaToMatchingList))
            {
                FindInvalidData(dicAreaToMatchingList);
                return false;
            }

            FindInvalidData(dicAreaToMatchingList);
            // Cuong - 20/12/2022 - DeckPanel issue - END

            if (!GetMinMinArea())
            {
                return false;
            }
            FindRelation();
            return true;
        }

        private void FindRelation()
        {
            for (var k = 2; k <= mkey; ++k)
            {
                var side = FindNearestSideToMajorArea(finalResult, k);
                dicRectIdToRectSharingSide[k] = side;
            }
            dicRectIdToRectSharingSide[1] = MaxAreaExistingSide.None;
        }

        private bool FindValidData(Dictionary<double, List<int[][]>> dicAreaToMatchingList)
        {
            var areas = dicAreaToMatchingList.Keys.ToList();
            areas.Sort();
            areas.Reverse();
            foreach (var area in areas)
            {
                var lstData = dicAreaToMatchingList[area];
                var lstValidData = new List<int[][]>();
                foreach (var curData in lstData)
                {
                    if (hasAreaIsolatedFromMajor(curData, mkey))
                    {
                        continue;
                    }
                    lstValidData.Add(curData);
                }
                if (!lstValidData.Any())
                {
                    continue;
                }
                lstValidDataBasedOnArea = lstValidData;
                return true;
            }
            return false;
        }

        // Cuong - 20/12/2022 - DeckPanel issue - START
        private void FindInvalidData(Dictionary<double, List<int[][]>> dicAreaToMatchingList)
        {
            var areas = dicAreaToMatchingList.Keys.ToList();
            areas.Sort();
            areas.Reverse();
            lstInvalidDataBasedOnArea = new List<int[][]>();

            foreach (var area in areas)
            {
                var lstData = dicAreaToMatchingList[area];
                foreach (var curData in lstData)
                {
                    if (hasAreaIsolatedFromMajor(curData, mkey))
                    {
                        lstInvalidDataBasedOnArea.Add(curData);
                    }
                }
            }
        }
        // Cuong - 20/12/2022 - DeckPanel issue - END

        private bool GetMinMinArea()
        {
            var maxArea = -1.0;
            foreach (var curData in lstValidDataBasedOnArea)
            {
                var curMaxArea = GetMinArea(curData);
                if (maxArea == -1 || maxArea < curMaxArea)
                {
                    finalResult = curData;
                    maxArea = curMaxArea;
                }
            }
            return finalResult != null;
        }

        private double GetMinArea(int[][] curData)
        {
            Dictionary<int, double> dic = new Dictionary<int, double>();
            for (var n = 0; n < curData.Length; ++n)
            {
                for (var m = 0; m < curData[n].Length; ++m)
                {
                    if (curData[n][m] == -1)
                    {
                        continue;
                    }
                    var val = curData[n][m];
                    if (!dic.ContainsKey(val))
                    {
                        dic[val] = 0;
                    }
                    dic[val] += areas[n][m];
                }
            }
            return dic.Values.Min();
        }

        private bool hasAreaIsolatedFromMajor(int[][] curData, int max)
        {
            for (var k = 2; k <= max; ++k)
            {
                var isNearer = FindNearestSideToMajorArea(curData, k);
                if (isNearer == MaxAreaExistingSide.None)
                {
                    return true;
                }
            }
            return false;
        }

        private MaxAreaExistingSide FindNearestSideToMajorArea(int[][] curData, int k)
        {
            for (var n = 0; n < curData.Length; ++n)
            {
                for (var m = 0; m < curData[n].Length; ++m)
                {
                    if (curData[n][m] == k)
                    {
                        var side = isNearerToMajor(n, m, curData);
                        if (side != MaxAreaExistingSide.None)
                        {
                            return side;
                        }
                    }
                }
            }

            return MaxAreaExistingSide.None;
        }

        private static MaxAreaExistingSide isNearerToMajor(int n, int m, int[][] curData)
        {
            if (n > 0)
            {
                if (curData[n - 1][m] == 1)
                {
                    return MaxAreaExistingSide.Bottom;
                }
            }
            if (n < curData.Length - 1)
            {
                if (curData[n + 1][m] == 1)
                {
                    return MaxAreaExistingSide.Top;
                }
            }
            if (m < curData[n].Length - 1)
            {
                if (curData[n][m + 1] == 1)
                {
                    return MaxAreaExistingSide.Right;
                }
            }
            if (m > 0)
            {
                if (curData[n][m - 1] == 1)
                {
                    return MaxAreaExistingSide.Left;
                }
            }
            return MaxAreaExistingSide.None;
        }

        private Dictionary<double, List<int[][]>> SortByArea(List<int[][]> values, int v)
        {
            Dictionary<double, List<int[][]>> dicAreaToList = new Dictionary<double, List<int[][]>>(new DoubleComparer());
            foreach (var item in values)
            {
                double area = 0;
                for (var n = 0; n < item.Length; ++n)
                {
                    for (var m = 0; m < item[n].Length; ++m)
                    {
                        if (item[n][m] == v)
                        {
                            area += areas[n][m];
                        }
                    }
                }
                if (!dicAreaToList.ContainsKey(area))
                {
                    dicAreaToList[area] = new List<int[][]>();
                }
                dicAreaToList[area].Add(item);
            }
            return dicAreaToList;
        }
    }
}