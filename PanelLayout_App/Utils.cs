using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PanelLayout_App
{
    public class Profiler : IDisposable
    {

        DateTime stamp;
        string msg = "";
        private bool showMessage;

        public Profiler(string msg, bool showMessage = true)
        {
            stamp = DateTime.Now;
            this.msg = msg;
            this.showMessage = showMessage;
        }
        public static double LastDelay = 0;
        public void Dispose()
        {
            var last = (DateTime.Now - stamp).TotalMilliseconds;
            LastDelay = last;

            if (showMessage)
                MessageBox.Show(msg + " : " + last.ToString() + " milliseconds");

        }
    }
    public static class Utils
    {
        public static Stream GetResourceFileStream(string fileName)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            // Get all embedded resources
            string[] arrResources = currentAssembly.GetManifestResourceNames();

            foreach (string resourceName in arrResources)
            {
                if (resourceName.Contains(fileName))
                {
                    return currentAssembly.GetManifestResourceStream(resourceName);
                }
            }

            return null;
        }

        //public static unsafe System.Drawing.Bitmap ReplaceColor(this System.Drawing.Bitmap source,
        //                            Color toReplace,
        //                            Color replacement)
        //{
        //    const int pixelSize = 4; // 32 bits per pixel

        //    System.Drawing.Bitmap target = new System.Drawing.Bitmap(
        //      source.Width,
        //      source.Height,
        //      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        //    BitmapData sourceData = null, targetData = null;

        //    try
        //    {
        //        sourceData = source.LockBits(
        //          new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
        //          ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        //        targetData = target.LockBits(
        //          new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
        //          ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        //        for (int y = 0; y < source.Height; ++y)
        //        {
        //            byte* sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
        //            byte* targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);

        //            for (int x = 0; x < source.Width; ++x)
        //            {
        //                byte b = sourceRow[x * pixelSize + 0];
        //                byte g = sourceRow[x * pixelSize + 1];
        //                byte r = sourceRow[x * pixelSize + 2];
        //                byte a = sourceRow[x * pixelSize + 3];

        //                if (toReplace.R == r && toReplace.G == g && toReplace.B == b)
        //                {
        //                    r = replacement.R;
        //                    g = replacement.G;
        //                    b = replacement.B;
        //                }

        //                targetRow[x * pixelSize + 0] = b;
        //                targetRow[x * pixelSize + 1] = g;
        //                targetRow[x * pixelSize + 2] = r;
        //                targetRow[x * pixelSize + 3] = a;
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        if (sourceData != null)
        //            source.UnlockBits(sourceData);

        //        if (targetData != null)
        //            target.UnlockBits(targetData);
        //    }

        //    return target;
        //}
        /// <summary>
        /// brush parameter is just to keep the variable for debug
        /// </summary>
        /// <param name="brush"></param>
        /// <returns></returns>
        public static Brush SwapBrushDebug(this Brush brush, Brush brush2)
        {
#if false && DEBUG
            return brush2;

#endif

            return brush;
        }

        //=============================================================================
        public static string GetPropertyName<TProperty>(Expression<Func<TProperty>> projection)
        {
            var memberExpression = (MemberExpression)projection.Body;
            return memberExpression.Member.Name;
        }
        public static void MaskNoComma(this TextBox tb)
        {
            tb.PreviewTextInput += (s, o) =>
            {
                if (o.Text == ",")
                    o.Handled = true;
            };
        }

        /// <summary>
        /// Rotates one point around another
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="centerPoint">The center point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        public static Point RotatePoint(this Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
        
        /// <summary>
        /// return arrow points for a line with two points, we must use it only with 0 / 90 / 180 / 270 degrees inclined
        /// </summary>
        /// <param name="sizeLeaderLength"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="bInside"></param>
        /// <returns></returns>
        public static Tuple<Point, Point, Point, Point> GetArrowHorizontalVerticalOnly(double sizeLeaderLength, Point pt1, Point pt2)
        {




            bool bHoriz = pt1.X != pt2.X;


            if (bHoriz)
            {
                var sign = pt1.X > pt2.X ? -1 : 1;

                var p11 = pt1.CopyShiftPoint(sign * 0.3 * sizeLeaderLength, 0.1 * sizeLeaderLength);
                var p13 = pt1.CopyShiftPoint(sign * 0.3 * sizeLeaderLength, -0.1 * sizeLeaderLength);
                var p21 = pt2.CopyShiftPoint(-sign * 0.3 * sizeLeaderLength, 0.1 * sizeLeaderLength);
                var p23 = pt2.CopyShiftPoint(-sign * 0.3 * sizeLeaderLength, -0.1 * sizeLeaderLength);

                return new Tuple<Point, Point, Point, Point>(p11, p13, p21, p23);
            }
            else
            {
                var sign = pt1.Y > pt2.Y ? -1 : 1;

                var p11 = pt1.CopyShiftPoint(0.1 * sizeLeaderLength, sign * 0.3 * sizeLeaderLength);
                var p13 = pt1.CopyShiftPoint(-0.1 * sizeLeaderLength, sign * 0.3 * sizeLeaderLength);
                var p21 = pt2.CopyShiftPoint(0.1 * sizeLeaderLength, -sign * 0.3 * sizeLeaderLength);
                var p23 = pt2.CopyShiftPoint(-0.1 * sizeLeaderLength, -sign * 0.3 * sizeLeaderLength);

                return new Tuple<Point, Point, Point, Point>(p11, p13, p21, p23);
            }
        }

        public static double GetAngleBetweenPoints(Point p1, Point p2)
        {
            double xDiff = p2.X - p1.X;
            double yDiff = p1.Y - p2.Y;

            double Angle = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);

            while (Angle < 0.0)
                Angle += 360.0;
            return Angle;
        }

        public static Point? ProjectPointOnLine(this Point myPoint, Point pt1, Point pt2)
        {
            var r = new Point(0, 0);
            if (pt1.Y == pt2.Y && pt1.X == pt2.X) pt1.Y -= 0.00001;

            var U = ((myPoint.Y - pt1.Y) * (pt2.Y - pt1.Y)) + ((myPoint.X - pt1.X) * (pt2.X - pt1.X));

            var Udenom = Math.Pow(pt2.Y - pt1.Y, 2) + Math.Pow(pt2.X - pt1.X, 2);

            U /= Udenom;

            r.Y = pt1.Y + (U * (pt2.Y - pt1.Y));
            r.X = pt1.X + (U * (pt2.X - pt1.X));



            var minx = Math.Min(pt1.Y, pt2.Y);
            var maxx = Math.Max(pt1.Y, pt2.Y);

            var miny = Math.Min(pt1.X, pt2.X);
            var maxy = Math.Max(pt1.X, pt2.X);

            var isValid = r.Y >= minx && r.Y <= maxx && r.X >= miny && r.X <= maxy;
            return isValid ? (Point?)r : null;
        }

        public static bool IsPointBetween(this Point myPoint, Point pt1, Point pt2)
        {
            var segment = (pt1 - pt2).Length;
            return (myPoint - pt1).Length < segment && (myPoint - pt2).Length < segment;
        }
              

        public static bool IsPointOnEdges(this Point myPoint, Point[] polygon)
        {
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (IsPointAligned(myPoint, polygon[i], polygon[j]))
                    return true;
                j = i;
            }

            return false;
        }

        public static bool IsPointAligned(this Point myPoint, Point pt1, Point pt2)
        {
            return (myPoint - pt1).Length + (myPoint - pt2).Length == (pt1 - pt2).Length;
        }
        /// <summary>
        /// Determines if the given point is inside the polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="myPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public static bool IsPointInsidePolygon(this Point myPoint, Point[] polygon, bool bEdges = false)
        {


            if (myPoint.IsPointOnEdges(polygon))
                return bEdges;

            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y < myPoint.Y && polygon[j].Y >= myPoint.Y || polygon[j].Y < myPoint.Y && polygon[i].Y >= myPoint.Y)
                {
                    if (polygon[i].X + (myPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < myPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }

            return result;
        }
        public static Point AlignPointToLine(this Point myPoint, Point pt1, Point pt2)
        {
            if ((pt1 - pt2).Length == 0)// same point we do nothing
                return myPoint;

            if (pt1.X == pt2.X)
            {
                var x = (myPoint.Y * (pt2.X - pt1.X) + pt1.X * pt2.Y - pt1.Y * pt2.X) / (pt2.Y - pt1.Y);
                return new Point(x, myPoint.Y);
            }
            else
            {
                var y = (myPoint.X * (pt2.Y - pt1.Y) - pt1.X * pt2.Y + pt1.Y * pt2.X) / (pt2.X - pt1.X);
                return new Point(myPoint.X, y);
            }
        }

        public static Point PointShiftByAngleAndDistance(this Point myPoint, double angleDeg, double distance)
        {
            var radians = Math.PI * angleDeg / 180;

            var x = myPoint.X + distance * Math.Cos(radians);
            var y = myPoint.Y + distance * Math.Sin(radians);

            return new Point(x, y);
        }
        public static double GetDistanceFromLine(this Point myPoint, Point pt1, Point pt2)
        {
            var pt = myPoint.ProjectPointOnLine(pt1, pt2);
            if (pt == null)
                return double.MaxValue;

            return (pt.Value - myPoint).Length;
        }
        /// <summary>
        /// Get min distance Pt1, Pt2 and the middle point
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double GetDistanceFromThreePoint(Point pt1, Point pt2, Point point)
        {
            var dist1 = (point - pt1).Length;
            var dist2 = (point - pt2).Length;
            var dist3 = (point - MiddlePoint(pt1, pt2)).Length;

            return Math.Min(dist1, Math.Min(dist2, dist3));
        }

        public static System.Drawing.Bitmap ToGrayScale(this System.Drawing.Bitmap original)
        {
            //create a blank bitmap the same size as original
            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBitmap))
            {

                //create the grayscale ColorMatrix
                //   ColorMatrix colorMatrix = new ColorMatrix(
                //      new float[][]
                //      {
                //new float[] {.3f, .3f, .3f, 0, 0},
                //new float[] {.59f, .59f, .59f, 0, 0},
                //new float[] {.11f, .11f, .11f, 0, 0},
                //new float[] {0, 0, 0, 1, 0},
                //new float[] {0, 0, 0, 0, 1}
                //      });

                ColorMatrix colorMatrix = new ColorMatrix(
                 new float[][]
                 {
             new float[] {0, 0, 0, 0, 0},
             new float[] {0, 0, 0, 0, 0},
             new float[] {0, 0, 0, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {1, 0, 0, 0, 1}
                 });



                //create some image attributes
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //set the color matrix attribute
                    attributes.SetColorMatrix(colorMatrix);

                    //draw the original image on the new image
                    //using the grayscale color matrix
                    g.DrawImage(original, new System.Drawing.Rectangle(0, 0, original.Width, original.Height),
                                0, 0, original.Width, original.Height, System.Drawing.GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;

        }

        public static System.Drawing.Bitmap ToColorize(this System.Drawing.Bitmap original, System.Drawing.Color newcolor)
        {
            //create a blank bitmap the same size as original
            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            using (System.Drawing.Graphics gfx = System.Drawing.Graphics.FromImage(newBitmap))
            {

                var r = newcolor.R / 255.0f;
                var g = newcolor.G / 255.0f;
                var b = newcolor.B / 255.0f;
                ColorMatrix colorMatrix = new ColorMatrix(
                 new float[][]
                 {
             new float[] {0, 0, 0, 0, 0},
             new float[] {0, 0, 0, 0, 0},
             new float[] {0, 0, 0, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {r, g, b, 0, 1}
                 });



                //create some image attributes
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //set the color matrix attribute
                    attributes.SetColorMatrix(colorMatrix);

                    //draw the original image on the new image
                    //using the grayscale color matrix
                    gfx.DrawImage(original, new System.Drawing.Rectangle(0, 0, original.Width, original.Height),
                                0, 0, original.Width, original.Height, System.Drawing.GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;

        }
        public static ImageSource BitmapToImagesource(this System.Drawing.Bitmap bmp)
        {
            if (bmp == null) return null;
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            var ig = new BitmapImage();
            ig.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            ig.StreamSource = ms;
            ig.EndInit();
            return ig;
        }

       
        public static void SaveElementAsImage(DrawingVisual visual, double dpi, string path, double width, double height)
        {


            var bitmap = new RenderTargetBitmap(
                (int)(width * dpi / 96), (int)(height * dpi / 96),
                dpi, dpi, PixelFormats.Default);
            bitmap.Render(visual);

            var encocer = new JpegBitmapEncoder();
            encocer.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = File.OpenWrite(path))
            {
                encocer.Save(stream);
            }
        }
      

        private static List<Brush> _brushes;
        private static void InitBrushes()
        {
            _brushes = new List<Brush>();
            var props = typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var propInfo in props)
            {
                _brushes.Add((Brush)propInfo.GetValue(null, null));
            }
        }

        private static Random _rand = new Random();
        public static Brush GetRandomBrush()
        {
            if (_brushes == null)
                InitBrushes();

            return _brushes[_rand.Next(_brushes.Count)];
        }

        public static Point MiddlePoint(Point minPoint, Point maxPoint)
        {
            return new Point(minPoint.X + (maxPoint.X - minPoint.X) / 2, minPoint.Y + (maxPoint.Y - minPoint.Y) / 2);
        }

        public static Point SwapPoint(this Point pt)
        {
            return new Point(pt.Y, pt.X);
        }

        public static Point CopyShiftPoint(this Point pt, double x, double y)
        {
            var resu = new Point(pt.X, pt.Y);
            resu.X += x;
            resu.Y += y;

            return resu;
        }
        /// <summary>
        /// if there is no shift we don't create a new point
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Point CopyShiftPoint2(this Point pt, double x, double y)
        {
            if (x == 0 && y == 0)
                return pt;
            return pt.CopyShiftPoint(x, y);
        }

        public static System.Drawing.Point ToDrawingPoint(this Point pt)
        {
            return new System.Drawing.Point(Math.Floor(pt.X).ToString().ToIntOrZero(), Math.Floor(pt.Y).ToString().ToIntOrZero());
        }

        public static Point CopyShiftPoint(this Point pt, Point pt2)
        {
            var resu = new Point(pt.X, pt.Y);
            resu.X += pt2.X;
            resu.Y += pt2.Y;

            return resu;
        }

        public static void Refresh<T>(this ObservableCollection<T> value)
        {
            CollectionViewSource.GetDefaultView(value).Refresh();
        }

        //=============================================================================
        public static bool ConvertToDouble(string str, out double result)
        {
            result = 0.0;
            if (string.IsNullOrEmpty(str))
                return false;

            string strDouble = str.Replace(",", ".");
            try
            {
                result = Convert.ToDouble(strDouble, CultureInfo.InvariantCulture);
                return true;
            }
            catch { }

            return false;
        }

        //=============================================================================
        public static double ToDoubleOrZero(this string str)
        {
            try
            {
                double result;
                if (!ConvertToDouble(str, out result))
                    return 0;

                return result;
            }
            catch
            {
                return 0;
            }
        }

        //=============================================================================
        public static int ToIntOrZero(this string str)
        {
            try
            {
                return Convert.ToInt32(str);
            }
            catch
            {
                return 0;
            }
        }

        public static bool CompareNoSpacesNoCase(this string str, string str2)
        {
            if (str == null && str2 == null)
                return true;

            return str?.ToLower().Replace(" ", "") == str2.ToLower().Replace(" ", "");
        }

        //=============================================================================
        public static double _sDefPrec = 0.000001;

        public static string GetLocalFile(params string[] path)
        {
            var lst = new List<string>();
            lst.Add(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            lst.AddRange(path);

            return Path.Combine(lst.ToArray());
        }

        public static bool FNE(double a, double b)
        {

            return a != b || !FEQ(a, b);
        }
        public static bool FEQ(double a, double b)
        {
            return a == b || ((a - _sDefPrec <= b) && (b <= a + _sDefPrec));
        }

        public static bool FGT(double a, double b)
        {
            return a > b || a - _sDefPrec > b;
        }
        public static bool FGE(double a, double b)
        {
            return a >= b || a >= b - _sDefPrec;
        }
        public static bool FLT(double a, double b)
        {
            return a < b || !FGE(a, b);
        }
        public static bool FLE(double a, double b)
        {
            return a <= b || !FGT(a, b);
        }

        //=============================================================================
        public static Point GetWholePoint(Point pnt)
        {
            Point result = pnt;
            result.X = Convert.ToInt32(Math.Truncate(result.X));
            result.Y = Convert.ToInt32(Math.Truncate(result.Y));

            return result;
        }

        //=============================================================================
        public static int GetWholeNumber(double rNumber)
        {
            return Convert.ToInt32(Math.Truncate(rNumber));
        }

        //=============================================================================
        public static Point CheckBorders(Point pnt, double min_x, double min_y, double max_x, double max_y, double margin_X, double margin_Y)
        {
            Point result = pnt;
            if (Utils.FLT(result.X, min_x + margin_X))
                result.X = min_x + margin_X;
            if (Utils.FLT(result.Y, min_y + margin_Y))
                result.Y = min_y + margin_Y;
            if (Utils.FGT(result.X, max_x - margin_X))
                result.X = max_x - margin_X;
            if (Utils.FGT(result.Y, max_y - margin_Y))
                result.Y = max_y - margin_Y;

            return result;
        }

        //=============================================================================
        public static int CheckWholeNumber(double rNumber, double minValue, double maxValue)
        {
            double rResult = rNumber;

            if (Utils.FLT(rResult, minValue))
                rResult = minValue;
            if (!double.IsInfinity(maxValue) && Utils.FGT(rResult, maxValue))
                rResult = maxValue;

            return GetWholeNumber(rResult);
        }
        public static UInt32 Check_UInt32_Number(UInt32 number, int minValue, int maxValue)
        {
            UInt32 result = number;

            if (minValue >= 0 && result < minValue)
                result = (UInt32)minValue;
            if (maxValue >= 0 && result > maxValue)
                result = (UInt32)maxValue;

            return result;
        }

        //=============================================================================
        public static int GetWholeNumberByStep(double rNumber, double step)
        {
            double rResult = rNumber;

            // 
            rResult = rResult / step;
            rResult = Utils.GetWholeNumber(rResult);
            rResult = rResult * step;

            return Utils.GetWholeNumber(rResult);
        }
        public static Typeface QuickTypeFace(string fontname, bool bold = false, bool italic = false)
        {
            if (!bold && !italic)
                return new Typeface(fontname);

            return new Typeface(new FontFamily(fontname), italic ? FontStyles.Italic : FontStyles.Normal,
                bold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal);
        }
        public static UInt32 Get_UInt32_NumberByStep(UInt32 number, UInt32 step)
        {
            try
            {
                double rResult = number;

                // 
                rResult = rResult / step;
                rResult = Utils.GetWholeNumber(rResult);
                rResult = rResult * step;

                return Convert.ToUInt32(rResult);
            }
            catch { }

            return 0;
        }

        // Dont use it for clone objects, it is too slow.
        // Use IClonable.Clone() method instead.
        // 
        ////=============================================================================
        ///// <summary>
        ///// Use IClonable.Clone() instead DeepClone.
        ///// DeepClone should be used only for seralize\deserialize drawing to the file.
        ///// It has very low performance for using in runtime operations.
        ///// </summary>
        //public static T DeepClone<T>(T obj)
        //{
        //	using (var ms = new MemoryStream())
        //	{
        //		var formatter = new BinaryFormatter();
        //		formatter.Serialize(ms, obj);
        //		ms.Position = 0;
        //
        //		return (T)formatter.Deserialize(ms);
        //	}
        //}

        //=============================================================================
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindParent<T>(this DependencyObject child)
            where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        public static string ImageToBase64(this System.Drawing.Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);

                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                return Convert.ToBase64String(imageBytes);
            }
        }

        public static System.Drawing.Image Base64ToImage(string base64String)
        {

            try
            {

                // Convert Base64 String to byte[]
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    // Convert byte[] to Image
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    return System.Drawing.Image.FromStream(ms, true);
                }
            }
            catch
            {
                return null;
            }
        }

        //=============================================================================
        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        //=============================================================================
        /// <summary>
        /// AngleBetween - the angle between 2 vectors
        /// </summary>
        /// <returns>
        /// Returns the the angle in degrees between vector1 and vector2
        /// </returns>
        /// <param name="vector1"> The first Vector </param>
        /// <param name="vector2"> The second Vector </param>
        public static double AngleBetween(Vector vector1, Vector vector2)
        {
            double sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
            double cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

            return Math.Atan2(sin, cos) * (180 / Math.PI);
        }

        //=============================================================================
        public static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        //=============================================================================
        public static double CalcScale(double imageAvailableWidth, double imageAvailableHeight, double drawingWidth, double drawingHeight)
        {
            double scaleX = imageAvailableWidth / drawingWidth;
            double scaleY = imageAvailableHeight / drawingHeight;

            if (scaleX < scaleY)
                return scaleX;

            return scaleY;
        }

      

        //=============================================================================
        //from 1.2
        /// <summary>
        ///	Check if line contains in other line
        /// </summary>
        /// <param name="sublineStart"></param>
        /// <param name="sublineEnd"></param>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <returns></returns>
        public static bool IsSubline(Point sublineStart, Point sublineEnd, Point lineStart, Point lineEnd)
        {
            bool isContains = false;
            // is line horizontal
            if (lineStart.X == lineEnd.X)
            {
                isContains = sublineStart.X == lineStart.X && sublineEnd.X == lineEnd.X && sublineStart.Y >= lineStart.Y && sublineEnd.Y <= lineEnd.Y;
            }
            else
            {
                isContains = sublineStart.Y == lineStart.Y && sublineEnd.Y == lineEnd.Y && sublineStart.X >= lineStart.X && sublineEnd.X <= lineEnd.X;
            }
            return isContains;
        }

        public static void RotateAroundYAxis(ref Point point1, ref Point point2)
        {
            var tempY2 = point2.Y;
            point2.Y = point1.Y;
            point1.Y = tempY2;
        }//from 1.2

        public static Brush ChangeAlpha(this Brush _brush, double alpha)
        {
            if (!(_brush is SolidColorBrush))
                return _brush;
            var brush = (SolidColorBrush)_brush;
            return new SolidColorBrush(Color.FromArgb(alpha.ForceFloorByte(), brush.Color.R, brush.Color.G, brush.Color.B));
        }

        public static Color ChangeAlpha(this Color _color, double alpha)
        {
            return Color.FromArgb(alpha.ForceFloorByte(), _color.R, _color.G, _color.B);
        }

        public static Pen ChangeAlpha(this Pen pen, double alpha)
        {
            return new Pen(pen.Brush.ChangeAlpha(alpha), pen.Thickness);
        }

        public static Pen ChangeAlphaConditional(this Pen pen, double alpha, bool bcond)
        {
            if (!bcond)
                return pen;
            return pen.ChangeAlpha(alpha);
        }

        public static byte ForceFloorByte(this double b)
        {
            try
            {
                return Convert.ToByte(Math.Floor(b));
            }
            catch
            {
                return 0;
            }
        }

        public static Brush ChangeAlphaConditional(this Brush _brush, byte alpha, bool bcond)
        {
            if (!bcond)
                return _brush;
            return _brush.ChangeAlpha(alpha);
        }

        public static List<string> QuickSplit(this string raw, string separator)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new List<string>();

            return raw.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static string QuickSplit(this string raw, string separator, int index)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            try
            {
                return raw.QuickSplit(separator)[index];
            }
            catch
            {
                return null;
            }
        }

        public static void ReplaceReturnTab(this KeyEventArgs o)
        {
            try
            {
                if (o.Key == Key.Enter)
                {
                    o.Handled = true;
                    KeyEventArgs eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 30, Key.Tab);
                    eInsertBack.RoutedEvent = UIElement.KeyDownEvent;
                    InputManager.Current.ProcessInput(eInsertBack);
                }
            }
            catch
            {
            }
        }

        public static void AlertInfo(string msg, string title = "Information")
        {
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static void AlertWarning(string msg, string title = "Warning")
        {
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void AlertError(string msg, string title = "Error")
        {
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static T GetValueAndCast<T>(this SerializationInfo info, string propname)
        {
            return (T)info.GetValue(propname, typeof(T));
        }
        public static bool Exist(this SerializationInfo info, string memberName)
        {
            if (info == null) return false;

            foreach (var entry in info)
            {
                if (entry.Name == memberName)
                    return true;
            }

            return false;
        }
        public static bool IsNullOrEmpty(this ICollection collection)
        {
            return collection == null || collection.Count == 0;
        }
        public static T GetValueAndCastOrDefault<T>(this SerializationInfo info, string propname)
        {
            if (!info.Exist(propname))
                return default(T);

            return (T)info.GetValue(propname, typeof(T));
        }

        public static T FirstOrDefault2<T>(this List<T> lst, Func<T, bool> expression)
        {

            foreach (var item in lst)
                if (expression(item))
                    return item;


            return default(T);

        }

        public static bool PerceivedBrightness(this Color c)
        {
            try
            {

                var iii = (int)Math.Sqrt(c.R * c.R * .241 + c.G * c.G * 0.691 + c.B * c.B * .068);

                return iii > 200;
            }
            catch
            {
                return false;
            }
        }


        public static bool PerceivedBrightness(this Brush brush)
        {
            try
            {
                var c = ((SolidColorBrush)brush).Color;
                return PerceivedBrightness(c);

            }
            catch
            {
                return false;
            }
        }

        public static void SetValueAndCastIfExists<T>(this SerializationInfo info, string propname, ref T prop)
        {
            if (!info.Exist(propname))
                return;

            prop = (T)info.GetValue(propname, typeof(T));

        }
        /// <summary>
        /// from obj to state
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <param name="obj"></param>
     
        public static bool IsGenericList(this Type tt)
        {
            if (tt == null)
                return false;
            return tt.IsGenericType && tt.GetGenericTypeDefinition() == typeof(List<>);
        }
        public static bool IsEmpty(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// A method that does nothing, only for break point purpose
        /// </summary>
        public static void Nothing()
        {

        }
        /// <summary>
        /// XNOR means that the booleans are both true or both false.
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        internal static bool XNOR(bool b1, bool b2)
        {
            if (b1 && b2)
                return true;

            if (!b1 && !b2)
                return true;


            return false;
        }

        public static bool ExtractFileFromResources(string source, string dest)
        {

            try
            {

                var si = Application.GetResourceStream(new Uri(source, UriKind.Relative));

                if (si != null)
                    using (Stream stream = si.Stream)
                    {


                        using (var file = new FileStream(Uri.UnescapeDataString(dest), FileMode.Create, FileAccess.Write))
                            stream.CopyTo(file);

                        return true;

                    }
            }
            catch
            {

            }
            return false;
        }
              
    }
}
