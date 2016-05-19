using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorBasedLinesEngine
{
    class Basis
    {
        private DoublePair c;
        public Point center
        {
            get { return new Point(c.a, c.b); }
            set
            {
                c.a = value.x;
                c.b = value.y;
                calcOXlen();
                calcOYlen();
            }
        }
        private DoublePair x;
        public Point xVector
        {
            get { return new Point(x.a, x.b); }
            set
            {
                x.a = value.x;
                x.b = value.y;
                calcOXlen();
            }
        }
        private DoublePair y;
        public Point yVector
        {
            get { return new Point(y.a, y.b); }
            set
            {
                y.a = value.x;
                y.b = value.y;
                calcOYlen();
            }
        }
        private double rotation;
        private double vectLen;
        public double vectorLength { get { return vectLen; } }
        public int vectorLengthInt { get { return (int)vectLen; } }
        //private double _zoom = 1; public double zoom { get { return _zoom; } set { _zoom = value; } }
        private double _OXlen; public double OXlen { get { calcOXlen(); return _OXlen; } }
        private double _OYlen; public double OYlen { get { calcOYlen(); return _OYlen; } }
        private void setStuff(double cx, double cy, double xx, double xy, double yx, double yy)
        {
            c = new DoublePair(cx, cy);
            x = new DoublePair(xx, xy);
            y = new DoublePair(yx, yy);
            //calcOXlen();
            //calcOYlen();
            rotation = 0;
            vectLen = System.Math.Sqrt((cx - xx) * (cx - xx) + (cy - xy) * (cy - xy));
        }
        private void calcOXlen()
        {
            _OXlen = System.Math.Sqrt(
                System.Math.Pow(x.a - c.a, 2) +
                System.Math.Pow(x.b - c.b, 2));
        }
        private void calcOYlen()
        {
            _OYlen = System.Math.Sqrt(
                System.Math.Pow(y.a - c.a, 2) +
                System.Math.Pow(y.b - c.b, 2));
        }
        public Basis(Basis b)
        {
            setStuff(b.center.x, b.center.y, b.xVector.x, b.xVector.y, b.yVector.x, b.yVector.y);
        }
        //public Basis(int cx, int cy, int xx, int xy, int yx, int yy) { setStuff(cx, cy, xx, xy, yx, yy); }//refer to large comment #1
        public Basis(int cx, int cy, int vl) { setStuff(cx, cy, cx + vl, cy, cx, cy + vl); }
        public Basis()
        {
            setStuff(0, 0, 0, 0, 0, 0);
        }
        //private void setPoint(DoublePair p, double x, double y)
        //{
        //    p.a = x;
        //    p.b = y;
        //}
        //public void setCenter(double x, double y) { setPoint(_c, x, y); }
        //public void setCenter(DoublePair p)       { setCenter(p.a, p.b); }
        //public void setOX(double x, double y)     { setPoint(_x, x, y); }
        //public void setOX(DoublePair p)           { setOX(p.a, p.b); }
        //public void setOY(double x, double y)     { setPoint(_y, x, y); }
        //public void setOY(DoublePair p)           { setOY(p.a, p.b); }
        private DoublePair rotatePoint(double x, double y, double deg, DoublePair scrCent)
        {
            double rads = 0.0174533 * deg;
            DoublePair res = new DoublePair(
                ((x - scrCent.a) * System.Math.Cos(rads) - (y - scrCent.b) * System.Math.Sin(rads)) + scrCent.a,//in this case scrCent.b is the X coordinate of the center
                ((x - scrCent.a) * System.Math.Sin(rads) + (y - scrCent.b) * System.Math.Cos(rads)) + scrCent.b);//scrCent.a is the Y coordinate
            return res;
        }
        private void _rotate(double deg, DoublePair scrCent)
        {
            c = rotatePoint(c.a, c.b, deg, scrCent);
            x = rotatePoint(x.a, x.b, deg, scrCent);
            y = rotatePoint(y.a, y.b, deg, scrCent);
            //change of the rotation variable is done in rotate(int, IntPair)
        }
        private void setBasisVect (DoublePair vect)
        {
            if (System.Math.Abs(System.Math.Abs(vect.a) - System.Math.Abs(c.a)) > System.Math.Abs(System.Math.Abs(vect.b) - System.Math.Abs(c.b)))//vector is horisontal
            {
                if (vect.a > c.a)//points in the positive direction
                    vect.a = c.a + vectLen;
                else//points in the negative direction
                    vect.a = c.a - vectLen;
                vect.b = c.b;
            }
            else//vector is now vertical
            {
                if (vect.b > c.b)//points in the positive direction
                    vect.b = c.b + vectLen;
                else
                    vect.b = c.b - vectLen;
                vect.a = c.a;
            }
        }
        public void rotate(double deg, DoublePair scrCent)
        {
            int intDeg = (int)deg;
            intDeg = (intDeg / 360) * 360;
            deg = deg - (double)intDeg; //if deg >= n * 360, it will have to rotate by only deg % 360; deg is double, so I am using a slightly different method
            if ((int)rotation / 90 != (int)(rotation + deg) / 90)//it has passed through a state, where the basis is rotated at 90, 180, 270 or 360 deg
            {//rounding the values of the coordinates to avoid mistakes to add up from using double variables
                _rotate(((rotation + deg) / 90) * 90 - rotation, scrCent);//rotate by the degrees left to reach a position of 90, 180, 270 or 360 deg
                c.set(c.a, c.b);
                setBasisVect(x);
                setBasisVect(y);
                _rotate((rotation + deg) - ((rotation + deg) / 90) * 90, scrCent);//rotate by the remaining degrees
            }
            else
                _rotate(deg, scrCent);
            rotation += deg;
            if (System.Math.Abs(rotation) > 0)//a full circle has been made
            {
                if (rotation > 0)
                    rotation -= 360;
                else//rotation < 0
                    rotation += 360;
            }
            if (x.a / y.a == x.b / y.b) throw new SystemException("Basis vectors have become paralel!");
        }
        public void move(double x, double y)
        {
            this.x.a += x; this.y.a += x; c.a += x;
            this.x.b += y; this.y.b += y; c.b += y;
        }
        public Basis zoomedIn(double zoom)
        {
            DoublePair zx = new DoublePair (
                (x.a - c.a) * zoom + c.a,//x coordinate of the X vector
                (x.b - c.b) * zoom + c.b);//y coordinate of the X vector
            DoublePair zy = new DoublePair (
                (y.a - c.a) * zoom + c.a,//x coordinate of the Y vector
                (y.b - c.b) * zoom + c.b);//y coordinate of the Y vector
            Basis res = new Basis(this);
            res.xVector = zx;
            res.yVector = zy;
            return res;
        }
    }
}



//Large Comment #1
//This constructor can also be used, but it can potentially cause bugs, that require complicated calculations, that may never need to be used anyway.
//Such cases are when the basis is pre-rotated and/or the basis vectors are NOT perpendicular. Such cases will require calculations to find the
//rotation angle of the basis and the angle between the basis vectors. You are welcome to try to implement these yourself. Yes, you can also simply
//change the coordinates of the center and vectors, but the rotation algorithms will automatically make the basis vectors perpendicular, which you can
//disable. However, disabling the prevent-mistakes-from-using-double-to-add-up algorithm can also potentially cause problems.