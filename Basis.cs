using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorBasedLinesEngine
{
    
    class Basis
    {
        private DoublePair _c;
        private DoublePair _x;
        private DoublePair _y;
        private double rotation;
        private int vectLen;
        private double _OXlen;
        private double _OYlen;
        private void setStuff(double cx, double cy, double xx, double xy, double yx, double yy)
        {
            _c = new DoublePair(cx, cy);
            _x = new DoublePair(xx, xy);
            _y = new DoublePair(yx, yy);
            //calcOXlen();
            //calcOYlen();
            rotation = 0;
            vectLen = (int)System.Math.Sqrt((cx - xx) * (cx - xx) + (cy - xy) * (cy - xy));
        }
        private void calcOXlen()
        {
            _OXlen = System.Math.Sqrt(
                System.Math.Pow(_x.a - _c.a, 2) +
                System.Math.Pow(_x.b - _c.b, 2));
        }
        private void calcOYlen()
        {
            _OYlen = System.Math.Sqrt(
                System.Math.Pow(_y.a - _c.a, 2) +
                System.Math.Pow(_y.b - _c.b, 2));
        }
        public Basis(Basis b)
        {
            setStuff(b.center().x(), b.center().y(), b.x().x(), b.x().y(), b.y().x(), b.y().y());
        }
        //public Basis(int cx, int cy, int xx, int xy, int yx, int yy) { setStuff(cx, cy, xx, xy, yx, yy); }//refer to large comment #1
        public Basis(int cx, int cy, int vl) { setStuff(cx, cy, cx + vl, cy, cx, cy + vl); }
        public Basis()
        {
            setStuff(0, 0, 0, 0, 0, 0);
        }
        private void setPoint(DoublePair p, double x, double y)
        {
            p.a = x;
            p.b = y;
        }
        public void setCenter(double x, double y) { setPoint(_c, x, y); }
        public void setCenter(DoublePair p)       { setCenter(p.a, p.b); }
        public void setOX    (double x, double y) { setPoint(_x, x, y); }
        public void setOX    (DoublePair p)       { setOX(p.a, p.b); }
        public void setOY    (double x, double y) { setPoint(_y, x, y); }
        public void setOY    (DoublePair p)       { setOY(p.a, p.b); }
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
            _c = rotatePoint(_c.a, _c.b, deg, scrCent);
            _x = rotatePoint(_x.a, _x.b, deg, scrCent);
            _y = rotatePoint(_y.a, _y.b, deg, scrCent);
            //change of the rotation variable is done in rotate(int, IntPair)
        }
        private void setBasisVect (DoublePair vect)
        {
            if (System.Math.Abs(System.Math.Abs(vect.a) - System.Math.Abs(_c.a)) > System.Math.Abs(System.Math.Abs(vect.b) - System.Math.Abs(_c.b)))//vector is horisontal
            {
                if (vect.a > _c.a)//points in the positive direction
                    vect.a = _c.a + vectLen;
                else//points in the negative direction
                    vect.a = _c.a - vectLen;
                vect.b = _c.b;
            }
            else//vector is now vertical
            {
                if (vect.b > _c.b)//points in the positive direction
                    vect.b = _c.b + vectLen;
                else
                    vect.b = _c.b - vectLen;
                vect.a = _c.a;
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
                _c.set(System.Math.Round(_c.a), System.Math.Round(_c.b));
                setBasisVect(_x);
                setBasisVect(_y);
                _rotate((rotation + deg) - ((rotation + deg) / 90) * 90, scrCent);//rotate by the remaining degrees
            }
            else
                _rotate(deg, scrCent);
            rotation += deg;
            if (System.Math.Abs(rotation / 360) > 0)//a full circle has been made
            {
                rotation = rotation - ((int)rotation / 360) * 360;
            }
        }
        public void move(double x, double y)
        {
            _x.a += x; _y.a += x; _c.a += x;
            _x.b += y; _y.b += y; _c.b += y;
        }
        //I can also return references to the members _center, _x and _y,
        //but I want to make sure that the lengths of the basis vectors
        //get re-calculated each time the vectors' coordinates change
        public Point center()
        {
            Point res = new Point((int)_c.a, (int)_c.b);
            return res;
        }
        public Point x()
        {
            Point res = new Point((int)_x.a, (int)_x.b);
            return res;
        }
        public Point y()
        {
            Point res = new Point((int)_y.a, (int)_y.b);
            return res;
        }
        public int vectorLength() { return vectLen; }
        public double OXlen() { calcOXlen(); return _OXlen; }//length of the OX vector
        public double OYlen() { calcOYlen(); return _OYlen; }//length of the OY vector
    }
}



//Large Comment #1
//This constructor can also be used, but it can potentially cause bugs, that require complicated calculations, that may never need to be used anyway.
//Such cases are when the basis is pre-rotated and/or the basis vectors are NOT perpendicular. Such cases will require calculations to find the
//rotation angle of the basis and the angle between the basis vectors. You are welcome to try to implement these yourself. Yes, you can also simply
//change the coordinates of the center and vectors, but the rotation algorithms will automatically make the basis vectors perpendicular, which you can
//disable. However, disabling the prevent-mistakes-from-using-double-to-add-up algorithm can also potentially cause problems.