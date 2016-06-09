using System;

namespace VectorBasedLinesEngine
{
    public class IntPair : IComparable
    {
        public int a;
        public int b;
        public IntPair() { a = 0; b = 0; }
        public IntPair(int a, int b) { this.a = a; this.b = b; }
        public IntPair(double a, double b) { this.a = (int)a; this.b = (int)b; }
        public IntPair(IntPair ip) { a = ip.a; b = ip.b; }
        public static implicit operator DoublePair(IntPair ip) { return new DoublePair(ip.a, ip.b); }
        //public static implicit operator Point(IntPair ip) { return new Point(ip.a, ip.b); }
        public static implicit operator System.Drawing.Point(IntPair ip) { return new System.Drawing.Point(ip.a, ip.b); }
        public static implicit operator IntPair(Tuple<int, int> t) { return new IntPair(t.Item1, t.Item2); }
        public static IntPair operator /(IntPair dp, double d)
        {
            return new IntPair((double)(dp.a) / d, (double)(dp.b) / d);
        }
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
    public class DoublePair : IComparable
    {
        public double a;
        public double b;
        public DoublePair() { set(0, 0); }
        public DoublePair(double a, double b) { set(a, b); }
        public DoublePair(DoublePair ip) { set(ip.a, ip.b); }
        //public static implicit operator Point(DoublePair dp) { return new Point(dp.a, dp.b); }
        public static implicit operator System.Drawing.Point(DoublePair ip) { return new System.Drawing.Point((int)Math.Round(ip.a), (int)Math.Round(ip.b)); }
        public static implicit operator DoublePair(Tuple<double, double> t) { return new DoublePair(t.Item1, t.Item2); }
        public static DoublePair operator /(DoublePair dp, double d)
        {
            return new DoublePair(dp.a / d, dp.b / d);
        }
        public void set(double a, double b)
        {
            this.a = a;
            this.b = b;
        }
        public int CompareTo(object obj)
        {
            if (obj as DoublePair == null || obj == null)
                throw new ArgumentException("Invalid argument: must be non null DoublePair.");
            DoublePair ip = obj as DoublePair;
            if (this.a != ip.a && this.b != ip.b)
                return 1;
            if (this.a == ip.a && this.b == ip.b)
                return 0;
            else
                return -1;
        }
    }
    public class ScreenData
    {
        private int _width;
        private int _height;
        public ScreenData(int w, int h)
        {
            _width = w;
            _height = h;
        }
        public int width { get { return _width; } set { _width = value; } }
        public int height { get { return _height; } set { _height = value; } }
        public Point center { get { return new Point(_width / 2, _height / 2); } }
        public Point upLeftCorner { get { return new Point(0, 0); } }
        public Point upRightCorner { get { return new Point(_width, 0); } }
        public Point downLeftCorner { get { return new Point(0, _height); } }
        public Point downRightCorner { get { return new Point(_width, _height); } }
        public Line upperSide { get { return new Line(0, 0, _width, 0); } }
        public Line bottomSide { get { return new Line(0, _height, _width, _height); } }
        public Line leftSide { get { return new Line(0, 0, 0, _height); } }
        public Line rightSide { get { return new Line(_width, 0, _width, _height); } }
        public bool isPointInside(Point p)
        {
            return p.x >= 0 && p.x <= _width &&
                   p.y >= 0 && p.y <= _height;
        }
        public bool isPointInside(int x, int y)
        {
            return isPointInside(new Point(x, y));
        }
    }
}