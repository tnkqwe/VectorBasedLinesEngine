using System;
using System.Collections.Generic;
using System.Linq;

namespace VBLEDrawing
{
    /// <summary> A pair of two integers. The two ints are public, so they can be changed at any time in any way. Use with caution. The class is used internally in the namespace just because Tuple[T, T] does not allow direct changing of the two variable, without re-constructing the object.</summary>
    public class IntPair : IComparable
    {
        /// <summary> Integer A. </summary>
        public int a;
        /// <summary> Integer B. </summary>
        public int b;
        /// <summary> Constructor - sets the initial values to 0. </summary>
        public IntPair() { a = 0; b = 0; }
        /// <summary> Constructor - sets the initial values </summary>
        /// <param name="a">Integer A</param>
        /// <param name="b">Integer B</param>
        public IntPair(int a, int b) { this.a = a; this.b = b; }
        /// <summary> Constructor - sets the initial values (pretty useless, though). </summary>
        /// <param name="a">Integer A</param>
        /// <param name="b">Integer B</param>
        public IntPair(double a, double b) { this.a = (int)a; this.b = (int)b; }
        /// <summary> Copy constructor. </summary>
        /// <param name="ip">Original IntPair object</param>
        public IntPair(IntPair ip) { a = ip.a; b = ip.b; }
        /// <summary> Convert IntPair to Double pair. </summary>
        /// <param name="ip">IntPair for converting</param>
        /// <returns>DoublePair object</returns>
        public static implicit operator DoublePair(IntPair ip) { return new DoublePair(ip.a, ip.b); }
        /// <summary> Convert IntPair to VBLEDrawing.Point. </summary>
        /// <param name="ip">IntPair for converting</param>
        /// <returns>VBLEDrawing.Point object</returns>
        public static implicit operator Point(IntPair ip) { return new Point(ip.a, ip.b); }
        /// <summary> Convert IntPair to System.Drawing.Point. Added for convinience. </summary>
        /// <param name="ip">IntPair for converting</param>
        /// <returns>System.Drawing.Point object</returns>
        public static implicit operator System.Drawing.Point(IntPair ip) { return new System.Drawing.Point(ip.a, ip.b); }
        /// <summary> Convert IntPair to Tuple[int, int]. Added for convinience. </summary>
        /// <param name="ip">IntPair for converting</param>
        /// <returns>Tuple[int, int] object</returns>
        public static implicit operator IntPair(Tuple<int, int> t) { return new IntPair(t.Item1, t.Item2); }
        public int CompareTo(object obj)
        {
            if (obj as IntPair == null || obj == null)
                throw new ArgumentException("Invalid argument: must be non null IntPair.");
            IntPair ip = obj as IntPair;
            if (this.a != ip.a && this.b != ip.b)
                return 1;
            if (this.a == ip.a && this.b == ip.b)
                return 0;
            else
                return -1;
        }
    }
    /// <summary> A pair of two double variables. The two doubles are public, so they can be changed at any time in any way. Use with caution. The class is used internally in the namespace just because Tuple[T, T] does not allow direct changing of the two variables without re-constructing the object.</summary>
    public class DoublePair : IComparable
    {
        /// <summary> Double variable A. </summary>
        public double a;
        /// <summary> Double variable B. </summary>
        public double b;
        /// <summary> Constructor - sets the initial values to 0. </summary>
        public DoublePair() { a = 0; b = 0; }
        /// <summary> Constructor - sets the initial values.</summary>
        /// <param name="a">Double variable A.</param>
        /// <param name="b">Double variable B.</param>
        public DoublePair(double a, double b) { this.a = a; this.b = b; }
        /// <summary> Copy constructor. </summary>
        /// <param name="dp">Original DoublePair object</param>
        public DoublePair(DoublePair dp) { a = dp.a; b = dp.b; }
        /// <summary> Convert DoublePair to VBLEDrawing.Point. </summary>
        /// <param name="dp">DoublePair for convertion</param>
        /// <returns>Point object</returns>
        public static implicit operator Point(DoublePair dp) { return new Point(dp.a, dp.b); }
        /// <summary> Convert DoublePair to System.Drawing.Point. Added for convenience. </summary>
        /// <param name="dp">DoublePair for convertion</param>
        /// <returns>System.Drawing.Point object</returns>
        public static implicit operator System.Drawing.Point(DoublePair dp) { return new System.Drawing.Point((int)Math.Round(dp.a), (int)Math.Round(dp.b)); }
        /// <summary> Convert DoublePair to Tuple[double double]. Added for convinience. </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static implicit operator DoublePair(Tuple<double, double> t) { return new DoublePair(t.Item1, t.Item2); }
        /// <summary> From IComparable interface. Added because in the namespace, I use SorterSet objects to store DoublePair objects. 1 - the respected variables are different. 0 - the respected variables are the same. -1 - only one of the respected values are equal, the others are different.</summary>
        /// <param name="obj">DoublePair object to be compared</param>
        /// <returns>-1, 0 or 1</returns>
        public int CompareTo(object obj)
        {
            DoublePair ip = obj as DoublePair;
            if (ip == null || obj == null)
                throw new ArgumentException("Invalid argument: must be non null DoublePair.");
            if (this.a != ip.a && this.b != ip.b)
                return 1;
            if (this.a == ip.a && this.b == ip.b)
                return 0;
            else
                return -1;
        }
    }
    /// <summary> Contains the screen's width and height, along with methods, that are frequently used in the namespace. Added for convinience.</summary>
    public class ScreenData
    {
        private int _width;
        private int _height;
        /// <summary> Constructor. </summary>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public ScreenData(int w, int h)
        {
            _width = w;
            _height = h;
        }
        /// <summary> Width of the screen. </summary>
        public int width { get { return _width; } set { _width = value; } }
        /// <summary> Height of the screen. </summary>
        public int height { get { return _height; } set { _height = value; } }
        /// <summary> Center of the screen as a VBLEDrawing.Point. </summary>
        public Point center { get { return new Point(_width / 2, _height / 2); } }
        /// <summary> Upper left corner of the screen as a VBLEDrawing.Point. </summary>
        public Point upLeftCorner { get { return new Point(0, 0); } }
        /// <summary> Upper right of the screen as a VBLEDrawing.Point. </summary>
        public Point upRightCorner { get { return new Point(_width, 0); } }
        /// <summary> Lower left corner of the screen as a VBLEDrawing.Point. </summary>
        public Point downLeftCorner { get { return new Point(0, _height); } }
        /// <summary> Lower right corner of the screen as a VBLEDrawing.Point. </summary>
        public Point downRightCorner { get { return new Point(_width, _height); } }
        /// <summary> Upper side as Line. </summary>
        public Line upperSide { get { return new Line(0, 0, _width, 0); } }
        /// <summary> Bottom side as Line. </summary>
        public Line bottomSide { get { return new Line(0, _height, _width, _height); } }
        /// <summary> Left side as Line. </summary>
        public Line leftSide { get { return new Line(0, 0, 0, _height); } }
        /// <summary> Right side as Line. </summary>
        public Line rightSide { get { return new Line(_width, 0, _width, _height); } }
        /// <summary> Is a point inside the screen. </summary>
        /// <param name="p">The point</param>
        /// <returns>If the point is in the screen</returns>
        public bool isPointInside(Point p)
        {
            return p.x >= 0 && p.x <= _width &&
                   p.y >= 0 && p.y <= _height;
        }
        /// <summary> Is a point inside the screen. </summary>
        /// <param name="x">X coordinate of the point</param>
        /// <param name="y">Y coordinate of the point</param>
        /// <returns>If the point is in the screen</returns>
        public bool isPointInside(int x, int y) { return isPointInside(new Point(x, y)); }
    }
}