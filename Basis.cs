using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorBasedLinesEngine
{
    public class Basis
    {
        //Refer to large comment #2 for info on how the program works with all the variables
        private DoublePair c;
        public Point center         { get { Point res = new Point(c); return new Point(res.zoomedCoords(screen.center, _zoom)); } }
        public Point centerNoZoom   { get { return new Point(c.a + divC.a, c.b + divC.b); } }
        public Point centerNoDiv    { get { return new Point(c.a * _zoom, c.b * _zoom); } }
        public Point centerBaseVals { get { return new Point(c.a, c.b); } }
        private DoublePair x;
        public Point xVector         { get { return new Point(center.x + x.a - c.a, center.y + x.b - c.b); } }
        public Point xVectorNoZoom   { get { return new Point(x.a + divX.a, x.b + divX.b); } }
        public Point xVectorNoDiv    { get { return new Point(x.a * _zoom, x.b * _zoom); } }
        public Point xVectorBaseVals { get { return new Point(x.a, x.b); } }
        private DoublePair y;
        public Point yVector         { get { return new Point(center.x + y.a - c.a, center.y + y.b - c.b); } }
        public Point yVectorNoZoom   { get { return new Point(y.a + divY.a, y.b + divY.b); } }
        public Point yVectorNoDiv    { get { return new Point(y.a * _zoom, y.b * _zoom); } }
        public Point yVectorBaseVals { get { return new Point(y.a, y.b); } }
        private ScreenData screen;
        //diversion of the bassis vectors; added because why not; use for effects
        private DoublePair divC;
        public DoublePair centerDiversion
        { 
            get { return new DoublePair(divC); }
            set
            {
                divC.a = value.a;
                divC.b = value.b;
            }
        }
        private DoublePair divX;
        public DoublePair xVectorDiversion
        {
            get { return new DoublePair(divX); }
            set
            {
                divX.a = value.a;
                divX.b = value.b;
            }
        }
        private DoublePair divY;
        public DoublePair yVectorDiversion
        {
            get { return new DoublePair(divY); }
            set
            {
                divY.a = value.a;
                divY.b = value.b;
            }
        }
        //zoom; theoretically, any zoom values should be able to be handled
        private double maxZoom = 2.0;
        private double minZoom = 0.5;
        private double _zoom = 1;
        public double zoom
        {
            get { return _zoom; }
            set { if (value >= minZoom && value <= maxZoom) _zoom = value; }//where the basis should be after zooming is calculated in the center, xVector and yVector accessors
        }
        private double rotation;
        private double vectLen;
        public double vectorLength { get { return vectLen; } }
        public int vectorLengthInt { get { return (int)vectLen; } }
        //private double _zoom = 1; public double zoom { get { return _zoom; } set { _zoom = value; } }
        //private double _OXlen;
        public double OXlen         { get { return calcVectLen(true,  true,  c, x, divC, divX); } }
        public double OXlenNoZoom   { get { return calcVectLen(false, true,  c, x, divC, divX); } }
        public double OXlenNoDiv    { get { return calcVectLen(true,  false, c, x, divC, divX); } }
        public double OXlenBaseVals { get { return calcVectLen(false, false, c, x, divC, divX); } }
        //private double _OYlen;
        public double OYlen         { get { return calcVectLen(true,  true,  c, y, divC, divY);} }
        public double OYlenNoZoom   { get { return calcVectLen(false, true,  c, y, divC, divY); } }
        public double OYlenNoDiv    { get { return calcVectLen(true,  false, c, y, divC, divY); } }
        public double OYlenBaseVals { get { return calcVectLen(false, false, c, y, divC, divY); } }
        private void setStuff(double cx, double cy, ScreenData screen)
        {
            double xx = cx + 1;
            double xy = cy;
            double yx = cx;
            double yy = cy + 1;
            c = new DoublePair(cx, cy);
            x = new DoublePair(xx, xy);
            y = new DoublePair(yx, yy);
            divC = new DoublePair(0, 0);
            divX = new DoublePair(0, 0);
            divY = new DoublePair(0, 0);
            _zoom = 1;
            //calcOXlen();
            //calcOYlen();
            rotation = 0;
            //vectLen = System.Math.Sqrt((cx - xx) * (cx - xx) + (cy - xy) * (cy - xy));
            vectLen = 1;
            this.screen = screen;
        }
        private double calcVectLen(bool zoom, bool div, DoublePair st, DoublePair en, DoublePair divSt, DoublePair divEn)
        {
            DoublePair s = new DoublePair(st);
            DoublePair e = new DoublePair(en);
            if (div)
            {
                s.a += divSt.a; s.b += divSt.b;
                e.a += divEn.a; e.b += divEn.b;
            }
            if (zoom)
            {
                s.a = s.a / _zoom; s.b = s.b / _zoom;//after divisions to get 1px vector lengths when calculating the screen coordinates,
                e.a = e.a / _zoom; e.b = e.b / _zoom;//the results get multiplied by the zoom or basisVectCoor / (basisVectLength / zoom) = (basisVectCoor / basisVectLength) * zoom
            }
            double res = System.Math.Sqrt(
                System.Math.Pow(e.a - s.a, 2) +
                System.Math.Pow(e.b - s.b, 2));
            return res;
        }
        //public Basis(Basis b) { setStuff(b.center.x, b.center.y); }
        //public Basis(int cx, int cy, int xx, int xy, int yx, int yy) { setStuff(cx, cy, xx, xy, yx, yy); }//refer to large comment #1
        public Basis(int cx, int cy, ScreenData screen) { setStuff(cx, cy, screen); }
        public Basis(ScreenData screen) { setStuff(0, 0, screen); }
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
            _rotate(deg, scrCent);
            rotation += deg;
            if (System.Math.Abs(rotation) > 0)//a full circle has been made
            {
                if (rotation > 0)
                    rotation -= 360;
                else//rotation < 0
                    rotation += 360;
            }
            //if (x.a / y.a == x.b / y.b) throw new SystemException("Basis vectors have become paralel!");
        }
        public void move(double x, double y)
        {
            this.x.a += x; this.y.a += x; c.a += x;
            this.x.b += y; this.y.b += y; c.b += y;
        }
    }
}



//Large Comment #1 (Constructor is no longer needed, for I have implemented a solution)
//This constructor can also be used, but it can potentially cause bugs, that require complicated calculations, that may never need to be used anyway.
//Such cases are when the basis is pre-rotated and/or the basis vectors are NOT perpendicular. Such cases will require calculations to find the
//rotation angle of the basis and the angle between the basis vectors. You are welcome to try to implement these yourself. Yes, you can also simply
//change the coordinates of the center and vectors, but the rotation algorithms will automatically make the basis vectors perpendicular, which you can
//disable. However, disabling the prevent-mistakes-from-using-double-to-add-up algorithm can also potentially cause problems.
//The mistakes prevention algorithm:=======
//int intDeg = (int)deg;
//intDeg = (intDeg / 360) * 360;
//deg = deg - (double)intDeg; //if deg >= n * 360, it will have to rotate by only deg % 360; deg is double, so I am using a slightly different method
//if ((int)rotation / 90 != (int)(rotation + deg) / 90)//it has passed through a state, where the basis is rotated at 90, 180, 270 or 360 deg
//{//rounding the values of the coordinates to avoid mistakes to add up from using double variables
//    _rotate(((rotation + deg) / 90) * 90 - rotation, scrCent);//rotate by the degrees left to reach a position of 90, 180, 270 or 360 deg
//    c.set(c.a, c.b);
//    setBasisVect(x);
//    setBasisVect(y);
//    _rotate((rotation + deg) - ((rotation + deg) / 90) * 90, scrCent);//rotate by the remaining degrees
//}
//else
//    _rotate(deg, scrCent);
//rotation += deg;
//=========================================
//Which won't happen, because in the Point class I simply divide the vectors by their lengths to get the 1px long basis vectors, resulting into losing
//the mistakes, which prevents problems.

/*Large Comment #2
    The center, xVector and yVector are the base coordinates of the basis' vectors. Those vectors can be rotated and moved to change their coordinates.
    However, adding diversions and zooming the screen must not change the base values, for that can conflict with other algorithms (I don't know for
    zooming, really, but diversions will cause problems when rotating).
    Zooming determines the OXLen and OYLen values, that are used by the Point class to calculate where a plane point projects on the screen and a screen
    point projects on the plane. The zooming can be handled in the point class, but from an interface perspective, I think it is better with two boolean
    arguments to choose if you want the diversion and zoom to be used when calculating. When zooming, the basis vectors' need to move towards to or away
    from the screen's center, otherwize the point, around which the zooming happens, will be he basis center.
    Diversions are pretty simple - simply return the modified values of the vactors' coordinates, and the rest of the algorithms will handle the rest.
    Coordinates can be represented as "how many X and Y vectors is a vector" or V = n * X + m * Y. The center point, X and Y vectors have coordinates
    relative to the screen's basis. Before drawing on the screen, a point's coordinates get recalculated to get what their coordinates would be relative
    to the screen's basis. The calculations take into accout the coordinates of the point relative to the plane's basis and the basis vectors' coordinates
    relative to the screen. So, changing any of those coordinates will cause changes in the place where the point would be on the screen.
    Diversion and zooming is best done in the core part of thr program, because the coordinates switching formulas work with the basis vectors, so changing
    their coordinates will have a direct effect on all the calculations, without the need for extra modifications in the rest of the code. I did succeed to
    implement zooming in the plane's code, outside of the basis' code, and that required me to change the interfaces of the plane and entites classes,
    along with a lot of changes in correctly functioning code.
    Rotation and movment change the base coordinates of the basis vectors. Rotation's value is being reset to 0 when it goes over 360 or -360 deg.
*/