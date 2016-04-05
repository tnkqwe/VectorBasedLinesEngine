using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorBasedLinesEngine
{
    class Point
    {//point coordinates
        double _x;
        double _y;
        //private - I am ensuring that there is no way to change coordinates, without recalculating lines' lengths
        public Point()
        {
            _x = 0;
            _y = 0;
        }
        public Point(double x, double y)
        {
            _x = x;
            _y = y;
        }
        public Point(IntPair p)
        {
            _x = p.a;
            _y = p.b;
        }
        public double x() { return _x; }
        public double y() { return _y; }
        public void setx(double x) { _x = x; }
        public void sety(double y) { _y = y; }
        public void setCoords(double x, double y) { _x = x; _y = y; }
        public DoublePair accurateScreenCoords(Basis planeBasis)
        {//coordinates of the point relative to the screen's basis
            //each dot is treated like a vector - this is how a vector's coordinates change when switching the basis
            double OXlen = planeBasis.OXlen();
            double OYlen = planeBasis.OYlen();
            //Refer to LARGE COMMENT #1 for explaining the divisions by OXlen and OYlen
            double resx = (planeBasis.x().x() - planeBasis.center().x()) * _x / OXlen + (planeBasis.y().x() - planeBasis.center().x()) * _y / OYlen + planeBasis.center().x();
            double resy = (planeBasis.x().y() - planeBasis.center().y()) * _x / OXlen + (planeBasis.y().y() - planeBasis.center().y()) * _y / OYlen + planeBasis.center().y();
            return new DoublePair((int)resx, (int)resy);
        }
        public IntPair screenCoords(Basis planeBasis)
        {
            DoublePair res = accurateScreenCoords(planeBasis);
            return new IntPair((int)res.a, (int)res.b);
        }
        public IntPair planeCoords(Basis planeBasis)
        {//the coordinates of the point relative to the plane's basis
            DoublePair res = realPlaneCoords(planeBasis);
            return new IntPair((int)Math.Round(res.a), (int)Math.Round(res.b));
        }
        public DoublePair realPlaneCoords(Basis planeBasis)
        {
            DoublePair x = new DoublePair(planeBasis.x().x(), planeBasis.x().y());//X basis vector
            DoublePair y = new DoublePair(planeBasis.y().x(), planeBasis.y().y());//Y basis vector
            DoublePair c = new DoublePair(planeBasis.center().x(), planeBasis.center().y());//center
            double ox1 = (x.a - c.a) / planeBasis.OXlen();
            double oy1 = (x.b - c.b) / planeBasis.OXlen();
            double ox2 = (y.a - c.a) / planeBasis.OYlen();
            double oy2 = (y.b - c.b) / planeBasis.OYlen();
            double resx;
            double resy;
            if (ox2 == 0.0)
            {
                resx = (_x - c.a) / ox1;
                resy = (ox1 * (_y - c.b) + oy1 * (c.a - _x)) / (ox1 * oy2);
            }
            else
            {
                resx = (ox2 * (_y - c.b) + oy2 * (c.a - _x)) / (ox2 * oy1 - ox1 * oy2);
                resy = (_x - c.a - ox1 * resx) / ox2;
            }
            return new DoublePair(resx, resy);
        }
        public bool isInLine(double[] ln)
        {
            return ln[0] * _x + ln[1] * _y + ln[2] == 0;
        }
        public bool isInLine(Line ln)
        {
            return isInLine(ln.getEquation());
        }
    }
    class Line
    {
        private Point st;
        private Point en;
        private void setStuff(double stx, double sty, double enx, double eny)
        {
            st = new Point(stx, sty);
            en = new Point(enx, eny);
        }
        public Line() { setStuff(0, 0, 0, 0); }
        public Line(double stx, double sty, double enx, double eny) { setStuff(stx, sty, enx, eny); }
        public Line(DoublePair st, DoublePair en) { setStuff(st.a, st.b, en.a, en.b); }
        public Line(Point st, Point en) { setStuff(Math.Round(st.x()), Math.Round(st.y()), Math.Round(en.x()), Math.Round(en.y())); }
        public void setStart(double x, double y) { st.setCoords(x, y); }
        public void setEnd(double x, double y) { en.setCoords(x, y); }
        public Point start()
        {
            Point res = new Point(st.x(), st.y());
            return res;
        }
        public Point end()
        {
            Point res = new Point(en.x(), en.y());
            return res;
        }
        public double[] getEquation()
        {
            double[] res = new double[3];
            res[0] = this.end().y() - this.start().y();
            res[1] = this.start().x() - this.end().x();
            res[2] = this.end().x() * this.start().y() - this.end().y() * this.start().x();
            return res;
        }
        public String getEquasionString()
        {
            double[] equ = getEquation();
            return equ[0] + " " + equ[1] + " " + equ[2];
        }
        private bool isPointInner(double[] ln, Point point)
        {//if the point is in the half-plane, where the normal vector of the line points
            if (point.x() * ln[0] + point.y() * ln[1] + ln[2] >= 0)
                return true;//I am also taking the case if the point belongs to the line, otherwise it causes problems when determining in which section a line's end is located
            return false;
        }
        private bool crosses(double[] lnA, Line ln, double[] lnB)
        {
            //if the ends of the points are the same
            if ((ln.start().x() == st.x() && ln.start().y() == st.y()) ||
                (ln.start().x() == en.x() && ln.start().y() == en.y()) ||
                (ln.end().x() == st.x() && ln.end().y() == st.y()) ||
                (ln.end().x() == en.x() && ln.end().y() == en.y()))
                return true;
            //if one of the ends of one of the lines elongs to the other line
            if (lnA[0] * ln.start().x() + lnA[1] * ln.start().y() + lnA[2] == 0 ||
                lnA[0] * ln.end().x() + lnA[1] * ln.end().y() + lnA[2] == 0 ||
                lnB[0] * st.x() + lnB[1] * st.y() + lnB[2] == 0 ||
                lnB[0] * en.x() + lnB[1] * en.y() + lnB[2] == 0)
                return true;
            //mind the exclaimation marks!
            if (isPointInner(lnA, ln.start()) == !isPointInner(lnA, ln.end()) &&//if the ends of the one line are on both sides of the second
                isPointInner(lnB, st) == !isPointInner(lnB, en))//if the ends of the other line are on both sides of the first one
                return true;//means that they are crossed
            return false;
        }
        public bool crosses(Line ln)
        {
            double[] lnA = getEquation();//lnA[0] * X + lnA[1] * Y + lnA[2] = 0
            double[] lnB = ln.getEquation();//lnB[0] * X + lnB[1] * Y + lnB[2] = 0
            return crosses(lnA, ln, lnB);
        }
        private Point crossingPoint(double[] lnA, double[] lnB)
        {
            double resY = (lnA[2] * lnB[0] - lnB[2] * lnA[0]) / (lnB[1] * lnA[0] - lnA[1] * lnB[0]);
            double resX = (-1) * (lnA[1] * resY + lnA[2]) / lnA[0];
            return new Point(resX, resY);
        }
        public Point crossingPoint(Line ln)
        {
            double[] lnA = getEquation();//lnA[0] * X + lnA[1] * Y + lnA[2] = 0
            double[] lnB = ln.getEquation();  //lnB[0] * X + lnB[1] * Y + lnB[2] = 0
            if (crosses(lnA, ln, lnB))
            {//by this point it is certain, that the two lines cross
                if (lnA[0] == 0)
                    return crossingPoint(lnB, lnA);
                if (lnB[0] == 0)
                    return crossingPoint(lnA, lnB);
                return crossingPoint(lnA, lnB);
            }
            else return null;
        }
        public bool isPointInner(Point point)
        {
            return isPointInner(getEquation(), point);
        }
        public Line lineToDraw(Basis basis, ScreenData screen)
        {
            if (screen.isPointInside(st) && screen.isPointInside(en))
                return new Line(st.x(), st.y(), en.x(), en.y());
            Point stPnt = null;
            Point enPnt = null;
            Point upCrs = crossingPoint(new Line(0.0,   0.0,   700.0, 0.0));
            Point dnCrs = crossingPoint(new Line(0.0,   700.0, 700.0, 700.0));
            Point rtCrs = crossingPoint(new Line(700.0, 0.0,   700.0, 700.0));
            Point ltCrs = crossingPoint(new Line(0.0,   0.0,   0.0,   700.0));
            if (upCrs != null) stPnt = upCrs;
            if (dnCrs != null) stPnt = dnCrs;
            if (ltCrs != null) stPnt = ltCrs;
            if (rtCrs != null) stPnt = rtCrs;
            if (upCrs != null && stPnt != upCrs) enPnt = upCrs;
            if (dnCrs != null && stPnt != dnCrs) enPnt = dnCrs;
            if (ltCrs != null && stPnt != ltCrs) enPnt = ltCrs;
            if (rtCrs != null && stPnt != rtCrs) enPnt = rtCrs;
            if (enPnt == null)//one end is inside
            {
                if (screen.isPointInside(st)) return new Line(stPnt.x(), stPnt.y(), st.x(), st.y());
                if (screen.isPointInside(en)) return new Line(stPnt.x(), stPnt.y(), en.x(), en.y());
            }
            return new Line(stPnt.x(), stPnt.y(), enPnt.x(), enPnt.y());
        }
    }
    class IntPair
    {
        public int a;
        public int b;
        public IntPair() { a = 0; b = 0; }
        public IntPair(int a, int b) { this.a = a; this.b = b; }
        public IntPair(IntPair ip) { a = ip.a; b = ip.b; }
    }
    class DoublePair
    {
        public double a;
        public double b;
        public DoublePair() { set(0, 0); }
        public DoublePair(double a, double b) { set(a, b); }
        public DoublePair(DoublePair ip) { set(ip.a, ip.b); }
        public void set(double a, double b)
        {
            this.a = a;
            this.b = b;
        }
    }
    class ScreenData
    {
        private int _width;
        private int _height;
        public ScreenData (int w, int h)
        {
            _width = w;
            _height = h;
        }
        public int width() { return _width; }
        public int height() { return _height; }
        public Point center() { return new Point(_width / 2, _height / 2); }
        public Point upLeftCorner() { return new Point(0, 0); }
        public Point upRightCorner() { return new Point(_width, 0); }
        public Point downLeftCorner() { return new Point(0, _height); }
        public Point downRightCorner() { return new Point(_width, _height); }
        public bool isPointInside(Point p)
        {
            return p.x() >= 0 && p.x() <= _width &&
                   p.y() >= 0 && p.y() <= _height;
        }
        public bool isPointInside(int x, int y)
        {
            return isPointInside(new Point(x, y));
        }
    }
    class PointMovementData
    {
        public int dir;
        public int limX;
        public int limY;
    }
    class LineMovementData
    {
        public PointMovementData start;
        public PointMovementData end;
    }
}

/*LARGE COMMENT #1
    I divide by the lengths of the basis vectors, to get coordinates as if the lengths of the vectors are 1 px;
    A point's coordinates are the same as the coordinates of the vector starting from the center of the basis and ending at the point;
    A basis can be represented as a co-ordinates system, where axises are devided by the lengths of the basis vectors,
    thus, a point can be represented as a vector with coordinates (a, b). But, a vector can be represented as the sum
    of other vectors - in this case, the basis vectors.
    A vector with coordinates (a, b) is the sum of 'a' times the X axis vector and 'b' times the Y axis vector, or:
    vect = a * X + b * Y .
    So, the vector's length can sky-rocket if the basis vectors' lengths are 5-10 px or more. This is the main reason I
    devide the basis vectors by their lengths, so I can work with the basis vectors X / length(X) and Y / length(Y).
    I may have explained this badly, but this is part of the basics of Analytical Geometry, where the whole thing is
    explained much better. In this case I have used sums of vectors, miltiplication of vectors with numbers and switching
    co-ordinate systems.*/

//double resx = -1;
//double resy = -1;
//double ox1 = planeBasis.x().x() - planeBasis.center().x(); ox1 = ox1 / planeBasis.OXlen();
//double oy1 = planeBasis.x().y() - planeBasis.center().y(); oy1 = oy1 / planeBasis.OXlen();
//double ox2 = planeBasis.y().x() - planeBasis.center().x(); ox2 = ox2 / planeBasis.OYlen();
//double oy2 = planeBasis.y().y() - planeBasis.center().y(); oy2 = oy2 / planeBasis.OYlen();
////switching from the screen's basis to the plane's basis
//int x = _x;// -planeBasis.center().x();
//int y = _y;// -planeBasis.center().y();
//if (ox1 == 0.0)
//{
//    resy = x / ox2;
//    resx = (y - resy * oy2) / oy1;
//}
//else if (ox2 == 0.0)
//{
//    resx = x / ox1;
//    resy = (y - resx * oy1) / oy2;
//}
//else if (oy1 == 0.0)
//{
//    resy = y / oy2;
//    resx = (x - resy * ox2) / ox1;
//}
//else if (oy2 == 0.0)
//{
//    resx = y / oy1;
//    resy = (x - resx * ox1) / ox2;
//}
//else if (ox1 != 0.0 && oy1 != 0.0 && ox2 != 0.0 && oy2 != 0.0)
//{
//    resx = (y * ox2 - x * oy2) / (ox2 * oy1 - ox1 * oy2);
//    resy = (x - resx * ox1) / ox2;
//}
//return new IntPair((int)resx + planeBasis.center().x(),
//                   (int)resy + planeBasis.center().y());
//I honestly don't feel like explaining...


//if      (ln.crosses(new Line(0.0,   0.0,   700.0, 0.0)))   { stt = crossCoorX(0);   side = 'u'; }//passes through the top side of the screen
//else if (ln.crosses(new Line(700.0, 0.0,   700.0, 700.0))) { stt = crossCoorY(700); side = 'r'; }//passes through the right side of the screen
//else if (ln.crosses(new Line(0.0,   700.0, 700.0, 700.0))) { stt = crossCoorX(700); side = 'd'; }//passes through the bottom side of the screen
//else if (ln.crosses(new Line(0.0,   0.0,   0.0,   700.0))) { stt = crossCoorY(0);   side = 'l'; }//passes through the left side of the screen
////==============================
//if      (side != 'u' && ln.crosses(new Line(0.0,   0.0,   700.0, 0.0)))   end = crossCoorX(0);//passes through the top side of the screen
//else if (side != 'r' && ln.crosses(new Line(700.0, 0.0,   700.0, 700.0))) end = crossCoorY(700);//passes through the right side of the screen
//else if (side != 'd' && ln.crosses(new Line(0.0,   700.0, 700.0, 700.0))) end = crossCoorX(700);//passes through the bottom side of the screen
//else if (side != 'l' && ln.crosses(new Line(0.0,   0.0,   0.0,   700.0))) end = crossCoorY(0);//passes through the left side of the screen

//private IntPair crossCoorX (double y)//if you know the Y coordinate of the point, and you need the X coordinate
//{
//    double[] equ = getEquation(this);
//    if (equ[1] == 0.0) { int k = 1; }
//    return new IntPair((int)Math.Round((-1) * (equ[1] * y + equ[2]) / equ[0]), (int)Math.Round(y));
//}
//private IntPair crossCoorY(double x)//if you know the X coordinate of the point, and you need the Y coordinate
//{
//    double[] equ = getEquation(this);
//    return new IntPair((int)Math.Round(x), (int)Math.Round((-1) * (equ[0] * x + equ[2]) / equ[1]));
//}

//public Line lineToDraw(Basis basis, ScreenData screen)
//{
//    IntPair stt = new IntPair((int)(st.x()), (int)(st.y()));
//    IntPair end = new IntPair((int)(en.x()), (int)(en.y()));
//    char side = '0';//one side, that has been crossed
//    if (screen.isPointInside(st) && screen.isPointInside(en))//if both ends are inside the screen
//        return new Line(stt.a, stt.b, end.a, end.b);
//    Line ln = new Line((double)stt.a, (double)stt.b, (double)end.a, (double)end.b);
//    Point stPnt;
//    stPnt = ln.crossingPoint(new Line(0.0, 0.0, 700.0, 0.0));//passes through the top side of the screen
//    if (stPnt != null) side = 'u';
//    if (stPnt == null) { stPnt = ln.crossingPoint(new Line(700.0, 0.0, 700.0, 700.0)); side = 'r'; }//passes through the right side of the screen
//    if (stPnt == null) { stPnt = ln.crossingPoint(new Line(0.0, 700.0, 700.0, 700.0)); side = 'd'; }//passes through the bottom side of the screen
//    if (stPnt == null) { stPnt = ln.crossingPoint(new Line(0.0, 0.0, 0.0, 700.0)); side = 'l'; }//passes through the left side of the screen
//    Point enPnt = null;
//    if (side != 'u')
//    {//passes from the upper side of the screen
//        enPnt = ln.crossingPoint(new Line(0.0, 0.0, 700.0, 0.0));
//        //if (enPnt != null)
//        //{
//        //    if (stPnt != null) return new Line(stPnt.x(), stPnt.y(), enPnt.x(), enPnt.y());
//        //    if (stPnt == null) return new Line(stt.a, stt.b, enPnt.x(), enPnt.y());
//        //}
//    }
//    if (side != 'r')
//    {//passes through the right side of the screen
//        enPnt = ln.crossingPoint(new Line(700.0, 0.0, 700.0, 700.0));
//        //if (enPnt != null)
//        //{
//        //    if (stPnt != null) return new Line(stPnt.x(), stPnt.y(), enPnt.x(), enPnt.y());
//        //    if (stPnt == null) return new Line(stt.a, stt.b, enPnt.x(), enPnt.y());
//        //}
//    }
//    if (side != 'd')
//    {//passes through the bottom side of the screen
//        enPnt = ln.crossingPoint(new Line(0.0, 700.0, 700.0, 700.0));
//        //if (enPnt != null)
//        //{
//        //    if (stPnt != null) return new Line(stPnt.x(), stPnt.y(), enPnt.x(), enPnt.y());
//        //    if (stPnt == null) return new Line(stt.a, stt.b, enPnt.x(), enPnt.y());
//        //}
//    }
//    if (side != 'l')
//    {//passes through the left side of the screen
//        enPnt = ln.crossingPoint(new Line(0.0, 0.0, 0.0, 700.0));
//        //if (enPnt != null)
//        //{
//        //    if (stPnt != null) return new Line(stPnt.x(), stPnt.y(), enPnt.x(), enPnt.y());
//        //    if (stPnt == null) return new Line(stt.a, stt.b, enPnt.x(), enPnt.y());
//        //}
//    }
//    //if (enPnt == null && stPnt != null)
//    //    return new Line(stPnt.x(), stPnt.y(), end.a, end.b);
//    //if (stPnt == null && enPnt != null)
//    //    return new Line(stt.a, stt.b, enPnt.x(), enPnt.y());
//    if (stPnt == null && enPnt == null)
//        return new Line(stt.a, stt.b, end.a, end.b);
//    if (enPnt == null)
//        return new Line(stPnt.x(), stPnt.y(), end.a, end.b);
//    return new Line(stPnt.x(), stPnt.y(), enPnt.x(), enPnt.y());
//}

//if (lnA[0] == 0.0 && lnA[1] != 0.0 && lnB[0] != 0.0 && lnB[1] == 0.0) return vertHorsCross(lnB, lnA);//if the first line is vertical and the second is horisontal
//if (lnB[0] == 0.0 && lnB[1] != 0.0 && lnA[0] != 0.0 && lnA[1] == 0.0) return vertHorsCross(lnA, lnB);//if the first line is horisontal and the second is vertical
//if (lnA[0] == 0.0 && lnA[1] != 0.0 && lnB[0] != 0.0 && lnB[1] != 0.0) return horsNormCross(lnA, lnB);//if only the first line is straight horisontal
//if (lnB[0] == 0.0 && lnB[1] != 0.0 && lnA[0] != 0.0 && lnA[1] != 0.0) return horsNormCross(lnB, lnA);//if only the second line is straight horisontal
//if (lnA[0] != 0.0 && lnA[1] == 0.0 && lnB[0] != 0.0 && lnB[1] != 0.0) return vertNormCross(lnA, lnB);//if only the first line is straight vertical
//if (lnB[0] != 0.0 && lnB[1] == 0.0 && lnA[0] != 0.0 && lnA[1] != 0.0) return vertNormCross(lnB, lnA);//if only the second line is straight vertical
//private Point vertNormCross(double[] lnA, double[] lnB)
//{//assume that lnA is the vertical line
//    double resX = (-1) * lnA[2] / lnA[0];
//    double resY = (-1) * (resX * lnB[0] + lnB[2]) / lnB[1];
//    if (lnA[0] == 0.0 || lnB[1] == 0.0) { int k = 0; }//for debugging
//    return new Point(resX, resY);
//}
//private Point horsNormCross(double[] lnA, double[] lnB)
//{
//    double resY = (-1) * lnA[2] / lnA[1];
//    double resX = (-1) * (lnB[1] * resY + lnB[2]) / lnB[0];
//    if (lnA[1] == 0.0 || lnB[0] == 0.0) { int k = 0; }//for debugging
//    return new Point(resX, resY);
//}
//private Point vertHorsCross(double[] lnA, double[] lnB)
//{//lnA is horisnotal; lnB is vertical
//    double resX = (-1) * lnA[2] / lnA[0];
//    double resY = (-1) * lnB[2] / lnB[1];
//    if (lnA[0] == 0.0 || lnB[1] == 0.0) { int k = 0; }//for debugging
//    return new Point(resX, resY);
//}