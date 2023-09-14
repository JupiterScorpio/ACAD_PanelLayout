using System;
using System.Collections.Generic;

namespace PanelLayout_App
{
    internal class MaxNeighboursFinder
    {
        private int[][] data;
        private int fillValue;
        private Rect rec;
        private List<int[][]> result;

        public MaxNeighboursFinder(int[][] data, Rect Rec, int fillVal, List<int[][]> lst)
        {
            this.data = data;
            this.rec = Rec;
            this.fillValue = fillVal;
            this.result = lst;
        }

        internal void Find()
        {
            var modified = false;
            for (var i = 0; i < 4; ++i)
            {
                switch (i)
                {
                    case 0:
                        if (HasLeftContent())
                        {
                            FillLeftContent();
                            modified = true;
                        }
                        break;
                    case 1:
                        if (HasRightContent())
                        {
                            FillRightContent();
                            modified = true;
                        }
                        break;
                    case 2:
                        if (HasTopContent())
                        {
                            FillTopContent();
                            modified = true;
                        }
                        break;
                    case 3:
                        if (HasBottomContent())
                        {
                            FillBottomContent();
                            modified = true;
                        }
                        break;
                }
            }
            if (!modified)
            {
                result.Add(data);
            }
        }

        private int[][] getCopy(int[][] data)
        {
            var cData = new int[data.Length][];
            for(var n = 0; n < data.Length; ++n)
            {
                cData[n] = new int[data[n].Length];
                for(var m = 0; m < data[n].Length; ++m)
                {
                    cData[n][m] = data[n][m];
                }
            }
            return cData;
        }

        private void FillLeftContent()
        {
            var recNext = new Rect(rec);
            var newData = getCopy(data);
            for (var n = 0; n < rec.Height; ++n)
            {
                newData[rec.Bottom + n][rec.Left - 1] = fillValue;
            }
            --recNext.Left;
            recNext.Width++;
            CheckForNext(recNext, newData);
        }

        private void CheckForNext(Rect recNext, int[][] newData)
        {
            var obj = new MaxNeighboursFinder(newData, recNext, fillValue,this.result);
            obj.Find();
        }

        private void FillRightContent()
        {
            var recNext = new Rect(rec);
            var newData = getCopy(data);
            for (var n = 0; n < rec.Height; ++n)
            {
                newData[rec.Bottom+n][rec.Left + rec.Width] = fillValue;
            }
            recNext.Width++;
            CheckForNext(recNext, newData);
        }

        private void FillTopContent()
        {
            var recNext = new Rect(rec);
            var newData = getCopy(data);
            for (var n = 0; n < rec.Width; ++n)
            {
                newData[rec.Bottom + rec.Height][rec.Left + n] = fillValue;
            }
            recNext.Height++;
            CheckForNext(recNext, newData);
        }
        private void FillBottomContent()
        {
            var recNext = new Rect(rec);
            var newData = getCopy(data);
            for (var n = 0; n < rec.Width; ++n)
            {
                newData[rec.Bottom - 1][rec.Left + n] = fillValue;
            }
            recNext.Height++;
            recNext.Bottom--;
            CheckForNext(recNext, newData);
        }
        
        private bool HasLeftContent()
        {
            if(rec.Left == 0)
            {
                return false;
            }
            for(var n = 0; n < rec.Height; ++n)
            {
                if(data[rec.Bottom + n][rec.Left - 1] != 0 &&
                    data[rec.Bottom + n][rec.Left - 1] != fillValue)
                {
                    return false;
                }
            }
            return true;
        }
        private bool HasRightContent()
        {
            if (rec.Left + rec.Width == data[0].Length)
            {
                return false;
            }
            for (var n = 0; n < rec.Height; ++n)
            {
                if (data[rec.Bottom + n][rec.Left + rec.Width] != 0 &&
                    data[rec.Bottom + n][rec.Left + rec.Width] != fillValue)
                {
                    return false;
                }
            }
            return true;
        }
        private bool HasTopContent()
        {
            if (rec.Bottom + rec.Height == data.Length)
            {
                return false;
            }
            for (var n = 0; n < rec.Width; ++n)
            {
                if (data[rec.Bottom + rec.Height][rec.Left + n] != 0 &&
                    data[rec.Bottom + rec.Height][rec.Left + n] != fillValue)
                {
                    return false;
                }
            }
            return true;
        }
        private bool HasBottomContent()
        {
            if (rec.Bottom == 0)
            {
                return false;
            }
            for (var n = 0; n < rec.Width; ++n)
            {
                if (data[rec.Bottom - 1][rec.Left + n] != 0 &&
                    data[rec.Bottom - 1][rec.Left + n] != fillValue)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

public class Rect
{
    public int Left { get; set; }
    public int Bottom { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Rect(Rect rec)
    {
        Left = rec.Left;
        Bottom = rec.Bottom;
        Width = rec.Width;
        Height = rec.Height;
    }

    public Rect(int bottom, int left, int width, int height)
    {
        Left = left;
        Bottom = bottom;
        Width = width;
        Height = height;
    }
}