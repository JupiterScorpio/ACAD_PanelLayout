using System;
using System.Linq;
using System.Collections.Generic;

namespace PanelLayout_App
{
    internal class MaxRectFinder
    {
        private int[][] data;
        Dictionary<int, List<Tuple<int, int>>> dic = new Dictionary<int, List<Tuple<int, int>>>();
        private double[][] dataA;
        private Dictionary<int, List<int[][]>> lstResult;
        private int curValue;

        public MaxRectFinder(int[][] data, double[][] dataA, int curValue , Dictionary<int, List<int[][]>> lstResult)
        {
            this.data = data;
            this.dataA = dataA;
            this.lstResult = lstResult;
            this.curValue = curValue;
        }

        internal void Find()
        {
            FindSeeds();

            FindPossibleOptions();
        }

        private void FindPossibleOptions()
        {
            if(dic.Count == 0)
            {
                return;
            }
            var max = dic.Keys.Max();
            var lst = new List<int[][]>();
            foreach (var item in dic[max])
            {
                var rec = new Rect(item.Item1, item.Item2, 1, 1);
                var nData = getCopy(data);
                nData[item.Item1][item.Item2] = curValue;
                
                var obj = new MaxNeighboursFinder(nData, rec, curValue, lst);
                obj.Find();
            }
            Dictionary<string, int[][]> dic1 = new Dictionary<string, int[][]>();
            foreach(var s in lst)
            {
                dic1[ToString(s)] = s;
            }
            foreach (var s in dic1.Values)
            {
                var maxC = HasData(s);
                if (maxC != -1)
                {
                    if (!lstResult.ContainsKey(maxC))
                    {
                        lstResult[maxC] = new List<int[][]>();
                    }
                    lstResult[maxC].Add(s);
                }
                var mrf = new MaxRectFinder(s, dataA, curValue + 1,lstResult);
                mrf.Find();
            }

            // var dicAreaToData = SortByArea(dic1.Values, 1);

            //   for

            var ls = lst;
        }
        

        private string ToString(int[][] data)
        {
            List<string> lst = new List<string>();
            for (var i = 0; i < data.Length; ++i)
            {
                var str1 = string.Join(",", data[i]);

                lst.Add(str1);
            }
            return string.Join("$", lst);
        }
        private int HasData(int[][] data)
        {
            var max = -1;
            List<string> lst = new List<string>();
            for (var i = 0; i < data.Length; ++i)
            {
                for(var j = 0; j < data[i].Length; ++j)
                {
                    if(data[i][j]== 0)
                    {
                        return -1;
                    }
                    max = Math.Max(data[i][j], max);
                }
            }
            return max;
        }

        private int[][] getCopy(int[][] data)
        {
            var cData = new int[data.Length][];
            for (var n = 0; n < data.Length; ++n)
            {
                cData[n] = new int[data[n].Length];
                for (var m = 0; m < data[n].Length; ++m)
                {
                    cData[n][m] = data[n][m];
                }
            }
            return cData;
        }

        private void FindSeeds()
        {
            for(var i = 0; i < data.Length; ++i)
            {
                for(var j = 0; j < data[i].Length; ++j)
                {
                    if(data[i][j] != 0)
                    {
                        continue;
                    }
                    var cnt = getNeighbourCount(i, j);
                    if (!dic.ContainsKey(cnt))
                    {
                        dic[cnt] = new List<Tuple<int, int>>();
                    }
                    dic[cnt].Add(new Tuple<int, int>(i, j));
                }
            }
        }

        private int getNeighbourCount(int i, int j)
        {
            var cnt = 0;
            if(i > 0)
            {
                var rIndex = i - 1;
                cnt += data[rIndex][j] != -1 ? 1 : 0;
            }
            if (i < data.Length - 1)
            {
                var rIndex = i + 1;
                cnt += data[rIndex][j] != -1 ? 1 : 0;
            }
            if (j > 0)
            {
                var cIndex = j - 1;
                cnt += data[i][cIndex] != -1 ? 1 : 0;
            }
            if (j < data[i].Length - 1)
            {
                var cIndex = j + 1;
                cnt += data[i][cIndex] != -1 ? 1 : 0;
            }
            return cnt;
        }
    }
    public class DoubleComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y)
        {
            double t = 1e-6;
            return Math.Abs(x - y) < t;
        }

        public int GetHashCode(double obj)
        {
            string str = getString(obj);
            return str.GetHashCode();
        }

        private static string getString(double o)
        {
            var str = o.ToString("0.000000");
            return str;
        }
    }
}