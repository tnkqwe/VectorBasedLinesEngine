﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace VBLEDrawing
{
    /// <summary> Custom point class. It contains algorithms, which calculate the coordinates of a point relative to the screen and plane, on which the point is located. </summary>
    public class Point
    {//point coordinates
        private double _x;
        private double _y;
        /// <summary> Constructor. Sets initial values to (0,0). </summary>
        public Point()                   { _x = 0;   _y = 0; }
        /// <summary> Constructor with two double parameters. </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public Point(double x, double y) { _x = x;   _y = y; }
        /// <summary> Constructor with IntPair </summary>
        /// <param name="p">Coordinates in IntPair</param>
        public Point(IntPair p)          { _x = p.a; _y = p.b; }
        /// <summary> Constructor with DoublePair </summary>
        /// <param name="p">Coordinates in DoublePair</param>
        public Point(DoublePair p)       { _x = p.a; _y = p.b; }
        /// <summary> Copy constructor. </summary>
        /// <param name="p">Original Point object</param>
        public Point(Point p)            { _x = p.x; _y = p.y; }
        /// <summary> Implicit conversion. Added for convinience. </summary>
        /// <param name="p">Point for converting.</param>
        /// <returns>System.Drawing.Point with rounded values</returns>
        public static implicit operator System.Drawing.Point(Point p) { return new System.Drawing.Point((int)Math.Round(p.x), (int)Math.Round(p.y)); }
        /// <summary> Explicit conversion. Added for convinience. </summary>
        /// <param name="p">Point for converting.</param>
        /// <returns>System.Drawing.Point with rounded values</returns>
        public static explicit operator Point(System.Drawing.Point p) { return new Point(p.X, p.Y); }
        public static explicit operator DoublePair(Point p) { return new DoublePair(p.x, p.y); }
        /// <summary> X coordinate. </summary>
        public double x
        {
            get { return _x; }
            set { _x = value; }
        }
        /// <summary> Y coordinate. </summary>
        public double y
        {
            get { return _y; }
            set { _y = value; }
        }
        /// <summary> Set coordinates. </summary>
        /// <param name="x">New X coordinate</param>
        /// <param name="y">New Y coordinate</param>
        public void setCoords(double x, double y) { _x = x; _y = y; }
        /// <summary> Coordinates. </summary>
        public DoublePair doubleCoords { get { return new DoublePair(_x, _y); } set { _x = value.a; _y = value.b; } }
        /// <summary> Rounded integer coordinates. </summary>
        public IntPair intCoords { get { return new IntPair((int)System.Math.Round(_x), (int)System.Math.Round(_y)); } }
        /// <summary> Double coordinates of a point on the plane relative to the screen. </summary>
        /// <param name="planeBasis">Plane's basis</param>
        /// <param name="zoom">Should zoom be calculated</param>
        /// <param name="div">Should diversions be calculated</param>
        /// <returns>Double coordinates on the screen</returns>
        public DoublePair doubleScrCoords(Basis planeBasis, bool zoom, bool div, bool rotation)
        {//coordinates of the point relative to the screen's basis
            //each dot is treated like a vector - this is how a vector's coordinates change when switching the basis
            double OXlen;
            double OYlen;
            if (zoom && div) { OXlen = planeBasis.OXlen; OYlen = planeBasis.OYlen; }
            else if (!zoom && div) { OXlen = planeBasis.OXlenNoZoom; OYlen = planeBasis.OXlenNoZoom; }
            else if (zoom && !div) { OXlen = planeBasis.OXlenNoDiv; OYlen = planeBasis.OYlenNoDiv; }
            else /*!zoom && !div */ { OXlen = planeBasis.OXlenBaseVals; OYlen = planeBasis.OYlenBaseVals; }
            Point vx = planeBasis.xVectorCoordinates(div, zoom, rotation);
            Point vy = planeBasis.yVectorCoordinates(div, zoom, rotation);
            Point bc = planeBasis.centerCoordinates(div, zoom, rotation);
            //Refer to LARGE COMMENT #1 for explaining the divisions by OXlen and OYlen
            double resx = (vx.x - bc.x) * _x / OXlen + (vy.x - bc.x) * _y / OYlen + bc.x;
            double resy = (vx.y - bc.y) * _x / OXlen + (vy.y - bc.y) * _y / OYlen + bc.y;
            return new DoublePair((int)resx, (int)resy);
        }
        /// <summary> Double coordinates of a point on the plane relative to the screen. Equivalent to calling return doubleScrCoords(planeBasis, true, true). </summary>
        /// <param name="planeBasis">Plane's basis</param>
        /// <returns>Double coordinates on the screen</returns>
        public DoublePair doubleScrCoords(Basis planeBasis) { return doubleScrCoords(planeBasis, true, true, true); }
        /// <summary> Integer coordinates of a point on the plane relative to the screen. </summary>
        /// <param name="planeBasis">Plane's basis</param>
        /// <param name="zoom">Should zoom be calculated</param>
        /// <param name="div">Should diversions be calculated</param>
        /// <returns>Rounded integer coordinates on the screen</returns>
        public IntPair intScrCoords(Basis planeBasis, bool zoom, bool div, bool rotation)//int coordinates without zoom
        {
            DoublePair res = doubleScrCoords(planeBasis, zoom, div, rotation);
            return new IntPair((int)Math.Round(res.a), (int)Math.Round(res.b));
        }
        /// <summary> Integer coordinates of a point on the plane relative to the screen. Equivalent to calling return intScrCoords(planeBasis, true, true). </summary>
        /// <param name="planeBasis">Plane's basis</param>
        /// <returns>Rounded integer coordinates on the screen</returns>
        public IntPair intScrCoords(Basis planeBasis)
        {
            DoublePair res = doubleScrCoords(planeBasis, true, true, true);
            return new IntPair((int)Math.Round(res.a), (int)Math.Round(res.b));
        }
        /// <summary> Double coordinates of a point on the screen relative to the plane. </summary>
        /// <param name="planeBasis">Basis of the plane</param>
        /// <param name="zoom">Should zoom be calculated</param>
        /// <param name="div">Should diversions be calculated</param>
        /// <returns>Double coordinates relative to the plane</returns>
        public DoublePair doublePlaneCoords(Basis planeBasis, bool zoom, bool div, bool rotation)
        {
            double OXlen;
            double OYlen;
            if (zoom && div) { OXlen = planeBasis.OXlen; OYlen = planeBasis.OYlen; }
            else if (zoom && !div) { OXlen = planeBasis.OXlenNoDiv; OYlen = planeBasis.OYlenNoDiv; }
            else if (!zoom && div) { OXlen = planeBasis.OXlenNoZoom; OYlen = planeBasis.OYlenNoZoom; }
            else { OXlen = planeBasis.OXlenBaseVals; OYlen = planeBasis.OYlenBaseVals; }
            Point vx = planeBasis.xVectorCoordinates(div, zoom, rotation);
            Point vy = planeBasis.yVectorCoordinates(div, zoom, rotation);
            Point bc = planeBasis.centerCoordinates(div, zoom, rotation);
            double ox1 = (vx.x - bc.x) / OXlen;
            double oy1 = (vx.y - bc.y) / OXlen;
            double ox2 = (vy.x - bc.x) / OYlen;
            double oy2 = (vy.y - bc.y) / OYlen;
            double resx;
            double resy;
            if (ox2 == 0.0)
            {
                if (ox1 == 0.0 || oy2 == 0) throw new DivideByZeroException("A value has become zero.");
                resx = (_x - planeBasis.center.x) / ox1;
                resy = (ox1 * (_y - planeBasis.center.y) + oy1 * (planeBasis.center.x - _x)) / (ox1 * oy2);
            }
            else
            {
                if (ox2 * oy1 - ox1 * oy2 == 0.0 || ox2 == 0) throw new DivideByZeroException("A value has become zero.");
                resx = (ox2 * (_y - planeBasis.center.y) + oy2 * (planeBasis.center.x - _x)) / (ox2 * oy1 - ox1 * oy2);
                resy = (_x - planeBasis.center.x - ox1 * resx) / ox2;
            }
            return new DoublePair(resx, resy);
        }
        /// <summary> Double coordinates of a point on the screen relative to the plane. Equivalent to calling doublePlaneCoords(planeBasis, true, true). </summary>
        /// <param name="planeBasis">Basis of the plane</param>
        /// <returns>Double coordinates relative to the plane</returns>
        public DoublePair doublePlaneCoords(Basis planeBasis) { return doublePlaneCoords(planeBasis, true, true, true); }
        /// <summary> Integer coordinates of a point on the screen relative to the plane. </summary>
        /// <param name="planeBasis">Basis of the plane</param>
        /// <param name="zoom">Should zoom be calculated</param>
        /// <param name="div">Should diversions be calculated</param>
        /// <returns>Rounded integer coordinates relative to the plane</returns>
        public IntPair intPlaneCoords(Basis planeBasis, bool zoom, bool div, bool rotation)
        {//the coordinates of the point on the screen relative to the plane's basis
            DoublePair res = doublePlaneCoords(planeBasis, zoom, div, rotation);
            return new IntPair((int)Math.Round(res.a), (int)Math.Round(res.b));
        }
        /// <summary> Integer coordinates of a point on the screen relative to the plane. Equivalent to calling intPlaneCoords(planeBasis, true, true). </summary>
        /// <param name="planeBasis">Basis of the plane</param>
        /// <returns>Rounded integer coordinates relative to the plane</returns>
        public IntPair intPlaneCoords(Basis planeBasis) { return intPlaneCoords(planeBasis, true, true, true); }
        /// <summary> Coordinates of the point after zooming. </summary>
        /// <param name="center">Center of zooming</param>
        /// <param name="zoom">Zoom value</param>
        /// <returns>The zoomed point</returns>
        public DoublePair zoomedCoords(Point center, double zoom)
        {//usefull here and there
            return new DoublePair(
                (_x - center.x) * zoom + center.x,
                (_y - center.y) * zoom + center.y);
        }

        public DoublePair rotated(double deg, DoublePair rotCent)
        {
            double rads = 0.0174533 * deg;
            DoublePair res = new DoublePair(
                ((_x - rotCent.a) * System.Math.Cos(rads) - (_y - rotCent.b) * System.Math.Sin(rads)) + rotCent.a,
                ((_x - rotCent.a) * System.Math.Sin(rads) + (_y - rotCent.b) * System.Math.Cos(rads)) + rotCent.b);
            return res;
        }
        //public bool isInLine(double[] ln) { return ln[0] * _x + ln[1] * _y + ln[2] == 0; }
        //public bool isInLine(Line ln) { return isInLine(ln.getEquation()); }
    }
    public class Line
    {
        private Point st;
        private Point en;
        private void setStuff(double stx, double sty, double enx, double eny)
        {
            st = new Point(stx, sty);
            en = new Point(enx, eny);
        }
        /// <summary> Constructor. Sets all initial values to 0. </summary>
        public Line() { setStuff(0, 0, 0, 0); }
        /// <summary> Constructor with specified start and end. </summary>
        /// <param name="stx">X coordinate of the start</param>
        /// <param name="sty">Y coordinate of the start</param>
        /// <param name="enx">X coordinate of the end</param>
        /// <param name="eny">Y coordinate of the end</param>
        public Line(double stx, double sty, double enx, double eny) { setStuff(stx, sty, enx, eny); }
        /// <summary> Constructor with specified start and end. </summary>
        /// <param name="st">Start as DoublePair</param>
        /// <param name="en">End as DoublePair</param>
        public Line(DoublePair st, DoublePair en) { setStuff(st.a, st.b, en.a, en.b); }
        /// <summary> Constructor with specified start and end. </summary>
        /// <param name="st">Start as VBLEDrawing.Point</param>
        /// <param name="en">End as VBLEDrawing.Point</param>
        public Line(Point st, Point en) { setStuff(Math.Round(st.x), Math.Round(st.y), Math.Round(en.x), Math.Round(en.y)); }
        /// <summary> Copy constructor. </summary>
        /// <param name="ln">Original line</param>
        public Line(Line ln) { setStuff(ln.start.x, ln.start.y, ln.end.x, ln.end.y); }
        /// <summary> Sets the end of the line. </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public void setStart(double x, double y) { st.setCoords(x, y); }
        /// <summary> Sets the end of the line. </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public void setEnd(double x, double y) { en.setCoords(x, y); }
        /// <summary> The start of the line. </summary>
        public Point start
        {
            get
            {
                Point res = new Point(st.x, st.y);
                return res;
            }
            set
            {
                st.x = value.x;
                st.y = value.y;
            }
        }
        /// <summary> The end of the line. </summary>
        public Point end
        {
            get
            {
                Point res = new Point(en.x, en.y);
                return res;
            }
            set
            {
                en.x = value.x;
                en.y = value.y;
            }
        }
        /// <summary> The line's equasion as an array. The equasion of a line is "eq[0] * X + eq[1] * Y + eq[2] = 0". The equasion is used to find how lines are positioned relative to eachother.</summary>
        /// <returns>Array of the equasion's </returns>
        public double[] getEquation()
        {
            double[] res = new double[3];
            res[0] = this.end.y - this.start.y;
            res[1] = this.start.x - this.end.x;
            res[2] = this.end.x * this.start.y - this.end.y * this.start.x;
            return res;
        }
        /// <summary> The line's equasion as string. Used for debugging. Refer to the summary of double[] getEquation() for info on the equasion. </summary>
        /// <returns>The equasion as a string</returns>
        public String getEquasionString()
        {
            double[] equ = getEquation();
            return equ[0] + " " + equ[1] + " " + equ[2];
        }
        private bool isPointInner(double[] ln, Point point)
        {//if the point is in the half-plane, where the normal vector of the line points
            if (point.x * ln[0] + point.y * ln[1] + ln[2] >= 0)
                return true;//I am also taking the case if the point belongs to the line, otherwise it causes problems when determining in which section a line's end is located
            return false;
        }
        /// <summary> Is line vertical. </summary>
        public bool isVertical { get { return st.x == en.x; } }
        /// <summary> Is line horizontal. </summary>
        public bool isHorizontal { get { return st.y == en.y; } }
        private bool crosses(double[] lnA, Line ln, double[] lnB)
        {
            //if the ends of the points are the same
            if ((ln.start.x == st.x && ln.start.y == st.y) ||
                (ln.start.x == en.x && ln.start.y == en.y) ||
                (ln.end.x == st.x && ln.end.y == st.y) ||
                (ln.end.x == en.x && ln.end.y == en.y))
                return true;
            //if one of the ends of one of the lines elongs to the other line
            if (lnA[0] * ln.start.x + lnA[1] * ln.start.y + lnA[2] == 0 ||
                lnA[0] * ln.end.x + lnA[1] * ln.end.y + lnA[2] == 0 ||
                lnB[0] * st.x + lnB[1] * st.y + lnB[2] == 0 ||
                lnB[0] * en.x + lnB[1] * en.y + lnB[2] == 0)
                return true;
            //mind the exclaimation marks!
            if (isPointInner(lnA, ln.start) == !isPointInner(lnA, ln.end) &&//if the ends of the one line are on both sides of the second
                isPointInner(lnB, st) == !isPointInner(lnB, en))//if the ends of the other line are on both sides of the first one
                return true;//means that they are crossed
            return false;
        }
        /// <summary> Does the line cross with another line. Added because it is faster to see if two lines cross, without calculating where they cross. Use if you don't care about the crossing point.</summary>
        /// <param name="ln"></param>
        /// <returns></returns>
        public bool crosses(Line ln)
        {
            double[] lnA = getEquation();//lnA[0] * X + lnA[1] * Y + lnA[2] = 0
            double[] lnB = ln.getEquation();//lnB[0] * X + lnB[1] * Y + lnB[2] = 0
            return crosses(lnA, ln, lnB);
        }
        /// <summary> Is the line and another line parts of the same line. </summary>
        /// <param name="ln">The second line</param>
        /// <returns>Are the lines part of the same line</returns>
        public bool paralel(Line ln)
        {
            double[] lnA = getEquation();
            double[] lnB = ln.getEquation();
            return lnA[0] / lnB[0] == lnA[1] / lnB[1] ||
                lnA[0] == lnB[0] ||
                lnA[1] == lnB[1];
        }
        private Point crossingPoint(double[] lnA, double[] lnB)
        {
            double[] _lnA = lnA;
            double[] _lnB = lnB;
            if (lnA[0] == 0)
            {
                _lnB = lnA;
                _lnA = lnB;
            }
            if (_lnA[0] == 0)
                throw new DivideByZeroException();
            double resY = (_lnA[2] * _lnB[0] - _lnB[2] * _lnA[0]) / (_lnB[1] * _lnA[0] - _lnA[1] * _lnB[0]);
            double resX = (-1) * (_lnA[1] * resY + _lnA[2]) / _lnA[0];
            return new Point(resX, resY);
        }
        /// <summary> Crossing point of the line with another line. Returns NULL if the lines don't cross. Use without calling crosses(double[], double[]) if you want to. Still being tested, something doesn't work here.</summary>
        /// <param name="ln">The second line</param>
        /// <returns>Crossing point or NULL if the lines do not cross.</returns>
        public Point crossingPoint(Line ln)
        {
            if (paralel(ln))
            {
                //double mx1 = st.x;
                //double my1 = st.y;
                //double mx2 = ln.end.x;
                //double my2 = ln.end.y;
                //if (ln.start.x > mx1) mx1 = ln.start.x;
                //if (ln.end.x > mx1) mx1 = ln.end.x;
                //if (en.x > mx1) mx1 = en.x;

                //if (ln.start.y > my1) my1 = ln.start.y;
                //if (ln.end.y > my1) my1 = ln.end.y;
                //if (en.y > my1) my1 = en.y;

                //if (st.x < mx2) mx2 = st.x;
                //if (en.x < mx2) mx2 = en.x;
                //if (ln.start.x < mx2) mx2 = ln.start.x;

                //if (st.y < my2) my2 = st.y;
                //if (en.y < my2) my2 = en.y;
                //if (ln.start.y < my2) my2 = ln.start.y;

                //return new Point((mx1 + mx2) / 2, (my1 + my2) / 2);
                return null;
            }
            double[] lnA = getEquation();//lnA[0] * X + lnA[1] * Y + lnA[2] = 0
            double[] lnB = ln.getEquation();//lnB[0] * X + lnB[1] * Y + lnB[2] = 0
            if (lnA[0] == 0.0 && lnA[1] == 0.0 && lnA[2] == 0.0) return st;
            if (lnB[0] == 0.0 && lnB[1] == 0.0 && lnB[2] == 0.0) return ln.start;
            if (crosses(lnA, ln, lnB))
            {//by this point it is certain, that the two lines cross
                if (lnA[0] == 0 && lnB[1] == 0)
                    return new Point(ln.start.x, st.y);//passed line is vertical - X does not change; the local line is horizontal - Y does not change
                if (lnA[1] == 0 && lnB[0] == 1)
                    return new Point(st.x, ln.end.y);//vice versa
                if (lnA[0] == 0)
                    return crossingPoint(lnB, lnA);
                if (lnB[0] == 0)
                    return crossingPoint(lnA, lnB);
                return crossingPoint(lnA, lnB);
            }
            else return null;
        }
        /// <summary> Is a point on the "inner" side of the line. Use it to see if more than one points are on the same side of the line. </summary>
        /// <param name="point">Point to chek</param>
        /// <returns>Is the point in the inner side</returns>
        public bool isPointInner(Point point) { return isPointInner(getEquation(), point); }
        /// <summary> Which part of the line is visible in the screen. Still being tested, something doesn't work here. </summary>
        /// <param name="basis">The basis which the line belongs to</param>
        /// <param name="screen">Screen data</param>
        /// <returns>The visible part of the line</returns>
        public Line lineToDraw(Basis basis, ScreenData screen)
        {
            if (screen.isPointInside(st) && screen.isPointInside(en))
                return this;
            if (screen.isPointInside(st) == false && screen.isPointInside(en) == false)
            {
                Point stp = st;
                Point enp = en;
                Point upCrs = crossingPoint(screen.upperSide);
                Point dnCrs = crossingPoint(screen.bottomSide);
                Point rtCrs = crossingPoint(screen.rightSide);
                Point ltCrs = crossingPoint(screen.leftSide);
                if (upCrs != null)
                {
                    stp = upCrs;
                    if (dnCrs != null) enp = dnCrs;
                    if (rtCrs != null) enp = rtCrs;
                    if (ltCrs != null) enp = ltCrs;
                    return new Line(stp, enp);
                }
                if (dnCrs != null)
                {
                    stp = dnCrs;
                    if (upCrs != null) enp = upCrs;
                    if (rtCrs != null) enp = rtCrs;
                    if (ltCrs != null) enp = ltCrs;
                    return new Line(stp, enp);
                }
                if (rtCrs != null)
                {
                    stp = rtCrs;
                    if (dnCrs != null) enp = dnCrs;
                    if (upCrs != null) enp = upCrs;
                    if (ltCrs != null) enp = ltCrs;
                    return new Line(stp, enp);
                }
                if (ltCrs != null)
                {
                    stp = ltCrs;
                    if (dnCrs != null) enp = dnCrs;
                    if (rtCrs != null) enp = rtCrs;
                    if (upCrs != null) enp = upCrs;
                    return new Line(stp, enp);
                }
            }
            Point inp;
            if (screen.isPointInside(st))
                inp = st;
            else
                inp = en;
            Point crs = crossingPoint(screen.upperSide);
            if (crs != null) return new Line(crs, inp);
            crs = crossingPoint(screen.bottomSide);
            if (crs != null) return new Line(crs, inp);
            crs = crossingPoint(screen.rightSide);
            if (crs != null) return new Line(crs, inp);
            crs = crossingPoint(screen.leftSide);
            if (crs != null) return new Line(crs, inp);
            else return null;
        }
    }
    /// <summary> Interface for all entites which are going to be drawn on the screen. BasicDrawableData class has the needed data, which you will need for the interface. </summary>
    public interface Drawable
    {
        /// <summary> The type of the Drawable as an integer. May prove useless, but it can be used to see what type of drawable it is, without the need to make any type casting.</summary>
        int drawableTypeAsInt { get; }
        /// <summary> The type of the Drawable as a string. May prove useless, but it can be used to see what type of drawable it is, without the need to make any type casting. </summary>
        string drawableType { get; }
        /// <summary> The plane the entity belongs to. </summary>
        Plane plane { get; }
        /// <summary> In which sections is the entity visible. </summary>
        IntPair[] section { get; }
        /// <summary> Has the entity moved. Both accessors are added to allow the entity to be manipulated from anywhere, not just from inside. </summary>
        bool moved { get; set; }
        LightShape lightShape { get; set; }
        /// <summary> Draw the info about the entity. </summary>
        /// <param name="gfx">Grapics</param>
        /// <param name="screen">Screen data</param>
        void drawInfo(System.Drawing.Graphics gfx, ScreenData screen);
        /// <summary> Index of the entity in the plane it belongs to. </summary>
        int indexInPlane { get; set; }
        /// <summary> Draw the entity. </summary>
        /// <param name="gfx">Graphics</param>
        /// <param name="screen">Screen data</param>
        void draw(System.Drawing.Graphics gfx, ScreenData screen);
        /// <summary> Calculate which sections are occupied by the entity. </summary>
        /// <param name="sectorSize">Size of sectors in the plane</param>
        /// <returns>Array of IntPair objects representing the indexes of the occupied sections</returns>
        IntPair[] calcOccupiedSects();
        /// <summary> Re-calculate the sections. WARNING! Always save the previously occupied sections to "tell" the plane from which sections to remove the entity's index! </summary>
        /// <param name="sectorSize">Size of sectors in the plane</param>
        void reCalcSects();
        /// <summary> Add an image to the entity's image database. </summary>
        /// <param name="imgDir"></param>
        void addImage(string imgDir);
        /// <summary> Change the image at a specified index in the image database. </summary>
        /// <param name="index">Index of image</param>
        /// <param name="imgDir">Directory of the new image in the program's files</param>
        void changeImage(int index, string imgDir);
        /// <summary> Removes the image from the specified index. </summary>
        /// <param name="index">Image's index</param>
        void removeImage(int index);
    }
    /// <summary> The basic data, which is needed for the Drawable interface. You can also use your own. I advise you to make objects from this class private. </summary>
    public class BasicDrawableData
    {
        /// <summary> Most properties are public, so this constructor is here to set a few default values. </summary>
        public BasicDrawableData()
        {
            plane = null;
            moved = false;
            section = null;
            indexInPlane = -1;
            drawn = false;
            image = new List<int>();
        }
        /// <summary> The plane, to which the entity belongs. </summary>
        public Plane plane;
        /// <summary> If the entity is moved. Each entity must track itself if it has moved or not, this is not handled here, because different entities will have different sets of coordinates.</summary>
        public bool moved;
        /// <summary> Which sections are occupied by the entity. </summary>
        public IntPair[] section;
        /// <summary> Experimental! Keeps track if the entity has been already drawn or not on the current cycle. </summary>
        public bool drawn;
        /// <summary> Which is the index of the entity in the plane it belongs to. </summary>
        public int indexInPlane;
        private List<int> image;
        /// <summary> Image at a specified index. </summary>
        /// <param name="index">Index of image</param>
        /// <returns>The image as System.Drawing.Bitmap</returns>
        public System.Drawing.Bitmap imgFromIndex(int index) { return plane.getImageFromIndex(image[index]); }
        /// <summary> Removes the image at the specified index. </summary>
        /// <param name="index">Index of the image</param>
        public void removeImage(int index) { image.RemoveAt(index); }
        /// <summary> Adds an image from a specified directory in the game's files. ONLY GIVE DIRECTORIES FROM THE GAME'S FILES. The program adds the game's directory to the string, so it can cause problems if you give a full directory.</summary>
        /// <param name="imgDir">Directory of the image in the game's files</param>
        public void addImage(string imgDir) { image.Add(plane.addImageResource(imgDir)); }
        /// <summary> Change the image in the specified index with a new image from a directory. ONLY GIVE DIRECTORIES FROM THE GAME'S FILES. The program adds the game's directory to the string, so it can cause problems if you give a full directory.</summary>
        /// <param name="index">The index of the image for changing</param>
        /// <param name="imgDir">Directory of the image in the game's files</param>
        public void changeImage(int index, string imgDir) { image[index] = plane.addImageResource(imgDir); }
    }
    /// <summary> Basic algorithms for detecting which sections are occupied by the basic Drawable types Dot, Line and Polygion.
    ///You can also use your own algorithms, however it was more trouble for me than what I thought at first.
    ///So many cases to write... Why me? </summary>
    public static class SectionAlgorithms
    {
        /// <summary> Which sections are occupied by the point based entity. Calling this method is equivalet to calling "point(new Point(x, y), image, sectorSize, statRotForScr, maxZoom, true)".</summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="image">Image (can be set to null)</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <param name="statRotForScr">Is the image stationary relative to the screen</param>
        /// <param name="maxZoom">How much is the image going to be zoomed-in</param>
        /// <returns>Which sections are occupied by the point based entity.</returns>
        public static IntPair[] point(double x, double y, System.Drawing.Bitmap image, int sectorSize, bool statRotForScr, double maxZoom) {
            return point(new Point(x, y), image, sectorSize, statRotForScr, maxZoom); }
        /// <summary> Which sections are occupied by the point based entity. Calling this method is equivalet to calling "point(new Point(x, y), image, sectorSize, true, maxZoom, true)". </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="image">Image (can be set to null)</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <param name="maxZoom">How much is the image going to be zoomed-in</param>
        /// <returns>Which sections are occupied by the point based entity.</returns>
        public static IntPair[] point(double x, double y, System.Drawing.Bitmap image, int sectorSize, double maxZoom) {
            return point(new Point(x, y), image, sectorSize, true, maxZoom); }
        /// <summary> Which sections are occupied by the point based entity. Calling this method is equivalet to calling "point(p, image, sectorSize, true, maxZoom, true)". </summary>
        /// <param name="p">Coordinates</param>
        /// <param name="image">Image (can be set to null)</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <param name="maxZoom">How much is the image going to be zoomed-in</param>
        /// <returns>Which sections are occupied by the point based entity.</returns>
        public static IntPair[] point(Point p, System.Drawing.Bitmap image, int sectorSize, double maxZoom) {
            return point(p, image, sectorSize, true, maxZoom); }
        /// <summary> Which sections are occupied by the point based entity. Calling this method is equivalet to calling "point(new Point(x, y), image, sectorSize, statRotForScr, 1.0, true)". </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="image">Image (can be set to null)</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <param name="statRotForScr">Is the image stationary relative to the screen</param>
        /// <returns>Which sections are occupied by the point based entity.</returns>
        public static IntPair[] point(double x, double y, System.Drawing.Bitmap image, int sectorSize, bool statRotForScr) {
            return point(new Point(x, y), image, sectorSize, statRotForScr, 1.0); }
        /// <summary> Which sections are occupied by the point based entity. Calling this method is equivalet to calling "point(p, image, sectorSize, statRotForScr, 1.0, true)". </summary>
        /// <param name="p">Coordinates</param>
        /// <param name="image">Image (can be set to null)</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <param name="statRotForScr">Is the image stationary relative to the screen</param>
        /// <returns>Which sections are occupied by the point based entity.</returns>
        public static IntPair[] point(Point p, System.Drawing.Bitmap image, int sectorSize, bool statRotForScr) {
            return point(p, image, sectorSize, statRotForScr, 1.0); }
        /// <summary> Which sections are occupied by the point based entity. Calling this method is equivalet to calling "point(new Point(x, y), image, sectorSize, true, 1.0, true)". </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="image">Image (can be set to null)</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <returns>Which sections are occupied by the point based entity.</returns>
        public static IntPair[] point(double x, double y, System.Drawing.Bitmap image, int sectorSize) {
            return point(new Point(x, y), image, sectorSize, true, 1.0); }
        /// <summary> Which sections are occupied by the point based entity. Calling this method is equivalet to calling "point(p, image, sectorSize, true, 1.0, true)". </summary>
        /// <param name="p">Coordinates</param>
        /// <param name="image">Image (can be set to null)</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <returns>Which sections are occupied by the point based entity.</returns>
        public static IntPair[] point(Point p, System.Drawing.Bitmap image, int sectorSize) {
            return point(p, image, sectorSize, true, 1.0); }
        /// <summary> Which sections are occupied by the point based entity. Calling this method is equivalet to calling "point(new Point(x, y), null, sectorSize, true, 1.0, true)". </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <returns>Which sections are occupied by the point based entity</returns>
        public static IntPair[] point(double x, double y, int sectorSize) {
            return point(new Point(x, y), null, sectorSize, true, 1.0); }
        /// <summary> Which sections are occupied by the point based entity. Calling this method is equivalet to calling "point(p, null, sectorSize, true, 1.0, true)". </summary>
        /// <param name="p">Coordinates</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <returns>Which sections are occupied by the point based entity.</returns>
        public static IntPair[] point(Point p, int sectorSize) {
            return point(p, null, sectorSize, true, 1.0); }
        /// <summary> Which sections are occupied by the point based entity. Full method calling. </summary>
        /// <param name="coords">Coordinates</param>
        /// <param name="image">Image (can be null)</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <param name="statRotForScr">Is the image stationary relative to the screen</param>
        /// <param name="maxZoom">How much is the image going to be zoomed-in</param>
        /// <returns>Which sections are occupied by the point based entity.</returns>
        public static IntPair[] point(Point coords, System.Drawing.Bitmap image, int sectorSize, bool statRotForScr, double maxZoom)
        {
            IntPair[] res;
            IntPair intCoords = new IntPair((int)coords.x, (int)coords.y);//coordinates in int, a few pixels difference is not going to make much of a difference
            if (image != null)
            {
                List<IntPair> res0 = new List<IntPair>();
                int entRad;
                if (statRotForScr)//if the image is stationary relative to the plane
                {
                    Point[] p = new Point[4];
                    p[0] = new Point(coords.x - Math.Round((double)image.Width * maxZoom / 2.0), coords.y - Math.Round((double)image.Height * maxZoom / 2.0));
                    p[1] = new Point(coords.x + Math.Round((double)image.Width * maxZoom / 2.0), coords.y - Math.Round((double)image.Height * maxZoom / 2.0));
                    p[2] = new Point(coords.x + Math.Round((double)image.Width * maxZoom / 2.0), coords.y + Math.Round((double)image.Height * maxZoom / 2.0));
                    p[3] = new Point(coords.x - Math.Round((double)image.Width * maxZoom / 2.0), coords.y + Math.Round((double)image.Height * maxZoom / 2.0));
                    SortedSet<IntPair> s = new SortedSet<IntPair>();
                    for (int k = 0; k < 4; k++ )
                        s.Add(point(p[k].x, p[k].y, sectorSize)[0]);
                    foreach (IntPair ip in s)
                        res0.Add(ip);
                }
                else
                {
                    lock (image)
                        entRad = (int)Math.Round(Math.Sqrt((double)(image.Width * image.Width + image.Height * image.Height) * maxZoom) / 2);//radius of the entity, if it is going to rotate along with the screen
                    if ((intCoords.a + entRad) / sectorSize == intCoords.a / sectorSize &&//if the whole image occupies only one sector
                        (intCoords.a - entRad) / sectorSize == intCoords.a / sectorSize &&
                        (intCoords.b + entRad) / sectorSize == intCoords.b / sectorSize &&
                        (intCoords.b - entRad) / sectorSize == intCoords.b / sectorSize)
                    {
                        res0.Add(new IntPair(intCoords.a / sectorSize, intCoords.b / sectorSize));
                    }
                    else
                    {
                        IntPair pnt = new IntPair(intCoords.a / sectorSize, intCoords.b / sectorSize);//in which sector is the center of the point located
                        int verDir = (int)Math.Round((double)intCoords.b / (double)sectorSize) - pnt.b;//which way is the nearest sector in the colum
                        if (verDir == 0) verDir = -1;
                        int horDir = (int)Math.Round((double)intCoords.a / (double)sectorSize) - pnt.a;//which way is the nearest sector in the row
                        if (horDir == 0) horDir = -1;
                        IntPair borAngl = new IntPair(
                            (int)Math.Round((double)intCoords.a / (double)sectorSize) * sectorSize,
                            (int)Math.Round((double)intCoords.b / (double)sectorSize) * sectorSize);//nearest point where 4 sectors border
                        if ((int)Math.Sqrt(Math.Pow(intCoords.a - borAngl.a, 2) + Math.Pow(intCoords.b - borAngl.b, 2)) <= entRad)
                        {//if the nearest border point is within the radius of the point
                            res0.Add(new IntPair(pnt.a, pnt.b));
                            res0.Add(new IntPair(pnt.a, pnt.b + verDir));
                            res0.Add(new IntPair(pnt.a + horDir, pnt.b));
                            res0.Add(new IntPair(pnt.a + horDir, pnt.b + verDir));//NOTE - this case is if the image stays still, relative to the camera
                        }
                        else if ((intCoords.a + horDir * entRad) / sectorSize != intCoords.a / sectorSize &&
                            (intCoords.b + verDir * entRad) / sectorSize == intCoords.b / sectorSize)//only two horisontal neighbour sectors are occupied
                        {
                            res0.Add(new IntPair(pnt.a, pnt.b));
                            res0.Add(new IntPair(pnt.a + horDir, pnt.b));
                        }
                        else if ((intCoords.a + horDir * entRad) / sectorSize == intCoords.a / sectorSize &&
                            (intCoords.b + verDir * entRad) / sectorSize != intCoords.b / sectorSize)//only two vertical neighbour sectors are occupied
                        {
                            res0.Add(new IntPair(pnt.a, pnt.b));
                            res0.Add(new IntPair(pnt.a, pnt.b + verDir));
                        }
                        else if ((intCoords.a + horDir * entRad) / sectorSize == intCoords.a / sectorSize &&//both a vertical and a horisontal neighbour sectors are occupied
                            (intCoords.b + verDir * entRad) / sectorSize == intCoords.b / sectorSize)//but a border point is not within the point's radius
                        {
                            res0.Add(new IntPair(pnt.a, pnt.b));
                            res0.Add(new IntPair(pnt.a, pnt.b + verDir));
                            res0.Add(new IntPair(pnt.a + horDir, pnt.b));
                        }
                    }
                }
                for (int i = 0; i < res0.Count; i++)
                    if (res0[i].a < 0 || res0[i].b < 0)
                        res0.Remove(res0[i]);//removing the sectors with negative indexes, because they are outside of the plane
                res = new IntPair[res0.Count];
                for (int i = 0; i < res0.Count; i++)
                    res[i] = res0[i];
            }
            else
            {
                res = new IntPair[1];
                res[0] = new IntPair(intCoords.a / sectorSize, intCoords.b / sectorSize);
            }
            return res;
        }
        /// <summary> Which sections are occupied by a line based entity. Equivalent to calling "line(new Line(x1, y1, x2, y2), sectorSize)". </summary>
        /// <param name="x1">Start's X coordinate of the line</param>
        /// <param name="y1">Start's Y coordinate of the line</param>
        /// <param name="x2">End's X coordinate of the line</param>
        /// <param name="y2">End's Y coordinate of the line</param>
        /// <param name="sectorSize">Which sections are occupied by a line based entity.</param>
        /// <returns></returns>
        public static IntPair[] line(double x1, double y1, double x2, double y2, int sectorSize) {
            return line(new Line(x1, y1, x2, y2), sectorSize); }
        /// <summary> Which sections are occupied by a line based entity. Equivalent to calling "line(new Line(new Point(stX, stY), end), sectorSize)". Yes, I did include this option. </summary>
        /// <param name="stX">Start's X coordinate</param>
        /// <param name="stY">Start's Y point</param>
        /// <param name="end">End point</param>
        /// <param name="sectSize">Size of each section in the plane</param>
        /// <returns>Which sections are occupied by a line based entity.</returns>
        public static IntPair[] line(double stX, double stY, Point end, int sectorSize) {
            return line(new Line(new Point(stX, stY), end), sectorSize); }
        /// <summary> Which sections are occupied by a line based entity. Equivalent to calling "line(new Line(new Point(stX, stY), end), sectorSize)". Yes, I did include this option. </summary>
        /// <param name="end">Start point</param>
        /// <param name="stX">End's X coordinate</param>
        /// <param name="stY">End's Y coordinate</param>
        /// <param name="sectSize">Size of each section in the plane</param>
        /// <returns>Which sections are occupied by a line based entity.</returns>
        public static IntPair[] line(Point start, double enX, double enY, int sectorSize) {
            return line(new Line(start, new Point(enX, enY)), sectorSize); }
        /// <summary> Which sections are occupied by a line based entity. Equivalent to calling "line(new Line(start, end), sectorSize)". Yes, I did include this option. </summary>
        /// <param name="start">Start point</param>
        /// <param name="end">End point</param>
        /// <param name="sectSize">Size of each section in the plane</param>
        /// <returns>Which sections are occupied by a line based entity.</returns>
        public static IntPair[] line(Point start, Point end, int sectorSize) {
            return line(new Line(start, end), sectorSize); }
        private static Line sectionSideLine(IntPair sect, char verhor, int dir, int sectSize)
        {
            char side;//determining which side is going to be checked
            if (verhor == 'h')//vertical or horizontal
                if (dir < 0) side = 'l';
                else if (dir > 0) side = 'r';
                else side = 'n';
            else if (verhor == 'v')
                if (dir < 0) side = 'u';
                else if (dir > 0) side = 'd';
                else side = 'n';
            else
            { throw new System.Exception("Invalid direction passed as argument!"); }//try not to put arguments, that can allow the program to set the dir variable to 'n'
            //and it is returned as a Line entity to use the Line.crosses() method
            if (side == 'u')
                return new Line(
                    sect.a * sectSize,
                    sect.b * sectSize,
                    (sect.a + 1) * sectSize,
                    sect.b * sectSize);
            else if (side == 'd')
                return new Line(
                    sect.a * sectSize,
                    (sect.b + 1) * sectSize,
                    (sect.a + 1) * sectSize,
                    (sect.b + 1) * sectSize);
            else if (side == 'l')
                return new Line(
                    sect.a * sectSize,
                    sect.b * sectSize,
                    sect.a * sectSize,
                    (sect.b + 1) * sectSize);
            else if (side == 'r')
                return new Line(
                    (sect.a + 1) * sectSize,
                    sect.b * sectSize,
                    (sect.a + 1) * sectSize,
                    (sect.b + 1) * sectSize);
            else return null;//will cause problems on purpose; I will have to learn how to make custom exceptions
        }
        /// <summary> Which sections are occupied by a line based entity. </summary>
        /// <param name="coords">Line coordinates</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <returns>Which sections are occupied by a line based entity.</returns>
        public static IntPair[] line(Line coords, int sectorSize)
        {
            IntPair sts = new IntPair((int)coords.start.x / sectorSize, (int)coords.start.y / sectorSize);//start section
            IntPair ens = new IntPair((int)coords.end.x / sectorSize, (int)coords.end.y / sectorSize);//end section
            int verDir;
            int horDir;
            //Refer to large coment #2 for information
            if (coords.start.x % sectorSize == 0 && coords.start.x > coords.end.x) sts.a -= 1;
            if (coords.start.y % sectorSize == 0 && coords.start.y > coords.end.y) sts.b -= 1;
            if (coords.end.x % sectorSize == 0 && coords.start.x < coords.end.x) ens.a -= 1;
            if (coords.end.y % sectorSize == 0 && coords.start.y < coords.end.y) ens.b -= 1;
            if (sts.a > ens.a) horDir = -1;//move left
            else if (sts.a == ens.a) horDir = 0;//stay on the colum
            else   /*sts.a < ens.a*/ horDir = 1;//move right
            if (sts.b > ens.b) verDir = -1;//move up
            else if (sts.b == ens.b) verDir = 0;//stay on the row
            else   /*sts.b < ens.b*/ verDir = 1;//move down
            //number of sections the line passes
            int passedSections;
            if (sts.a == ens.a && sts.b == ens.b)
                passedSections = 1;
            else
                passedSections = Math.Max(Math.Abs(sts.a - ens.a), Math.Abs(sts.b - ens.b)) +//if we have colums or rows of passed sections, add their total lengths
                                 Math.Min(Math.Abs(sts.a - ens.a), Math.Abs(sts.b - ens.b)) + 1;//and add the number of colums/rows
            if (passedSections == 1)//it will be 1 if the starting and ending sections are the same
            {
                IntPair[] res = new IntPair[passedSections];
                res[0] = new IntPair(sts.a, sts.b);
                return res;
            }
            List<IntPair> res0 = new List<IntPair>();
            IntPair crrSect = new IntPair(sts.a, sts.b);
            for (int i = 0; i < passedSections; i++)
            {
                if (crrSect.a >= 0 && crrSect.b >= 0)//Refer to Large comment #1
                    res0.Add(new IntPair(crrSect));//from this section it is cheked which is the next passed section
                if (verDir != 0 && coords.crosses(sectionSideLine(crrSect, 'v', verDir, sectorSize)))
                    crrSect.b += verDir;
                else if (horDir != 0 && coords.crosses(sectionSideLine(crrSect, 'h', horDir, sectorSize)))
                    crrSect.a += horDir;
            }
            //debug
            for (int i = 0; i < res0.Count; i++)
                if (res0[i].a < 0 || res0[i].b < 0)
                { throw new System.Exception("Section with negative coordinates!"); }
            return res0.ToArray();
        }
        //Polygon
        private class Cluster//clusters of sections
        {
            public bool inner;
            public List<int> cellr;
            public List<int> cellc;
            public Cluster()
            {
                inner = true;
                cellr = new List<int>();
                cellc = new List<int>();
            }
        }
        private static void setOuterInnerCells(int[][] cell, int cr, int cc, Cluster cluster)//cell row; cell colum
        {
            if (cell[cr][cc] == 0)
            {
                cell[cr][cc] = 1;
                cluster.cellr.Add(cr);
                cluster.cellc.Add(cc);
                if (cr + 1 < cell.Count()) setOuterInnerCells(cell, cr + 1, cc, cluster);
                else cluster.inner = false;
                if (cc + 1 < cell[cr].Count()) setOuterInnerCells(cell, cr, cc + 1, cluster);
                else cluster.inner = false;
                if (cr - 1 > -1) setOuterInnerCells(cell, cr - 1, cc, cluster);
                else cluster.inner = false;
                if (cc - 1 > -1) setOuterInnerCells(cell, cr, cc - 1, cluster);
                else cluster.inner = false;
            }
            if (cluster.inner == false)
            {
                for (int i = 0; i < cluster.cellc.Count; i++)
                    cell[cluster.cellr[i]][cluster.cellc[i]] = -1;
            }
        }
        /// <summary> Which sections are occupied by a polygon based entity. </summary>
        /// <param name="side">Array of the sides</param>
        /// <param name="filled">Is the polygon filled</param>
        /// <param name="sectorSize">Size of each section in the plane</param>
        /// <returns>Which sections are occupied by a polygon based entity.</returns>
        public static IntPair[] polygon(Line[] side, bool filled, int sectorSize)
        {
            if (filled)
            {//if the polygon is filled
                int lowerCol = -1;//the lowest and highest (by value) colums and rows, which are occupied by the polygon
                int lowerRow = -1;//set as -1, because there are no sections with such coordinates
                int higherCol = -1;
                int higherRow = -1;
                SortedSet<IntPair> sect = new SortedSet<IntPair>();
                for (int i = 0; i < side.Count(); i++)
                {
                    IntPair[] sip = line(side[i], sectorSize);
                    if (lowerCol == -1 || lowerRow == -1 || higherCol == -1 || higherRow == -1)
                    {//setting initial values
                        lowerCol = sip[0].a;
                        lowerRow = sip[0].b;
                        higherCol = sip[0].a;
                        higherRow = sip[0].b;
                    }
                    for (int l = 0; l < sip.Count(); l++)
                    {//finding the colums and rows
                        if (sip[l].a < lowerCol) lowerCol = sip[l].a;
                        if (sip[l].b < lowerRow) lowerRow = sip[l].b;
                        if (sip[l].a > higherCol) higherCol = sip[l].a;
                        if (sip[l].b > higherRow) higherRow = sip[l].b;
                        sect.Add(sip[l]);
                    }
                }
                int rows = higherRow - lowerRow + 1;
                int cols = higherCol - lowerCol + 1;
                int[][] sectOccVal = new int[rows][];//section occupation value: 2 - occupied and added; 1 - newly found occupied; 0 - potentially occupied; -1 - not occupied
                for (int i = 0; i < sectOccVal.Count(); i++) sectOccVal[i] = new int[cols];
                foreach (IntPair ip in sect)//creating a table
                    sectOccVal[ip.a][ip.b] = 2;
                for (int r = 0; r < sectOccVal.Count(); r++)
                    for (int c = 0; c < sectOccVal[r].Count(); c++)
                        if (sectOccVal[r][c] == 0)
                            setOuterInnerCells(sectOccVal, r, c, new Cluster());
                for (int r = 0; r < sectOccVal.Count(); r++)
                    for (int c = 0; c < sectOccVal[r].Count(); c++)
                        if (sectOccVal[r][c] == 1)
                            sect.Add(new IntPair(c + lowerCol, r + lowerRow));
                return sect.ToArray();
            }
            //if the polygon is not filled
            SortedSet<IntPair> sections = new SortedSet<IntPair>();
            for (int i = 0; i < side.Count(); i++)
            {
                IntPair[] sip = line(side[i], sectorSize);
                for (int l = 0; l < sip.Count(); l++)
                    sections.Add(sip[l]);
            }
            return sections.ToArray();
        }
    }
    /// <summary> Algorithms for drawing. You can use your own if mine prove to be slow (which they probably will).</summary>
    public static class DrawingAlgorithms
    {
        static System.Drawing.Drawing2D.GraphicsState gfxgs;
        private static void rotateGFX(System.Drawing.Graphics gfx, int posX, int posY, double deg, bool hq)
        {
            gfxgs = gfx.Save();
            if (hq) gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            gfx.TranslateTransform(posX, posY);
            gfx.RotateTransform((float)deg);
            gfx.TranslateTransform((-1) * posX, (-1) * posY);
        }
        /// <summary> Draws a specified image on a specified location. POSITION IS THE CENTER OF THE IMAGE! </summary>
        /// <param name="gfx">Graphics to draw with</param>
        /// <param name="img">Image</param>
        /// <param name="px">X coordinate of the center of the image</param>
        /// <param name="py">Y coordinate of the center of the image</param>
        public static void drawImage(System.Drawing.Graphics gfx, System.Drawing.Bitmap img, int px, int py, double deg, double zoom, bool hq)
        {
            if (deg == 0 && zoom == 0.0)
                gfx.DrawImage(img, px - img.Width / 2, py - img.Height / 2, img.Width, img.Height);
            else if (deg == 0 && zoom != 0.0)
            {
                int imgW = (int)(img.Width * zoom);
                int imgH = (int)(img.Height * zoom);
                gfx.DrawImage(img, px - imgW / 2, py - imgH / 2, imgW, imgH);
            }
            else
            {
                int imgW = (int)(img.Width * zoom);
                int imgH = (int)(img.Height * zoom);
                rotateGFX(gfx, px, py, deg, hq);
                gfx.DrawImage(img, px - imgW / 2, py - imgH / 2, imgW, imgH);
                gfx.Restore(gfxgs);
            }
        }
        /// <summary> Draw a displaced image. Displacement is seamless, or the image "comes out" of the opposite side.</summary>
        /// <param name="gfx">Graphics to draw with</param>
        /// <param name="img">Image</param>
        /// <param name="_posX">The X coordinate of the center of the drawn image</param>
        /// <param name="_posY">The Y coordinate of the center of the drawn image</param>
        /// <param name="dispX">Displacement on X</param>
        /// <param name="dispY">Displacement on Y</param>
        public static void drawDisplacedImage(System.Drawing.Graphics gfx, System.Drawing.Bitmap img, int posX, int posY, int dispX, int dispY, double zoom)
        {
            dispX = dispX % img.Width;
            dispY = dispY % img.Height;
            if (dispX < 0) dispX = img.Width + dispX;
            if (dispY < 0) dispY = img.Height + dispY;
            int ziw = (int)Math.Round(img.Width * zoom);
            int zih = (int)Math.Round(img.Height * zoom);
            int _dispX = (int)Math.Round(dispX * zoom);
            int _dispY = (int)Math.Round(dispY * zoom);
            int _posX = posX - ziw / 2;
            int _posY = posY - zih / 2;
            int x = _dispX % ziw;
            int y = _dispY % zih;
            if (dispX == 0 && dispY == 0)
            {
                gfx.DrawImage(img, _posX, _posY, ziw, zih);
                return;//Why would you even want to have no displacement when calling the method? Why are you here?
            }
            if (x < 0) x = ziw + x;
            if (y < 0) y = zih + y;
            int mpw = ziw - x;
            int mph = zih - y;
            //drawPortionOfImage(gfx, img, x, y, mpw, mph, _posX, _posY);
            //drawPortionOfImage(gfx, img, _dispX % img.Width, _dispY % img.Height, img.Width - dispX, img.Height - dispY, x, y, mpw, mph);
            gfx.DrawImage(img,
                new System.Drawing.Rectangle(_posX, _posY, mpw, mph),
                new System.Drawing.Rectangle(dispX, dispY, img.Width - dispX, img.Height - dispY),
                System.Drawing.GraphicsUnit.Pixel);
            if (x != 0 && y != 0)
            {
                //drawPortionOfImage(gfx, img, 0, 0,/*   */x,/*   */y, _posX + mpw, _posY + mph);
                //drawPortionOfImage(gfx, img, x, 0, ziw - x,/*   */y, _posX,/*   */_posY + mph);
                //drawPortionOfImage(gfx, img, 0, y,/*   */x, zih - y, _posX + mpw, _posY);
                gfx.DrawImage(img,
                    new System.Drawing.Rectangle(_posX + ziw - _dispX, _posY + zih - _dispY, _dispX, _dispY),
                    new System.Drawing.Rectangle(0, 0, dispX, dispY),
                    System.Drawing.GraphicsUnit.Pixel);
                gfx.DrawImage(img,
                    new System.Drawing.Rectangle(_posX, _posY + zih - _dispY, ziw - _dispX, _dispY),
                    new System.Drawing.Rectangle(dispX, 0, img.Width - dispX, dispY),
                    System.Drawing.GraphicsUnit.Pixel);
                gfx.DrawImage(img,
                    new System.Drawing.Rectangle(_posX + ziw - _dispX, _posY, _dispX, zih - _dispY),
                    new System.Drawing.Rectangle(0, dispY, dispX, img.Height - dispY),
                    System.Drawing.GraphicsUnit.Pixel);
            }
            //if the point with coordinates (dispX % img.Width, dispY % img.Height) lies on one of the sides of the image, the image can be clipped in only two parts, instead of 4
            else if (x == 0 && y != 0)//only vertical displacement
            {
                gfx.DrawImage(img,
                    new System.Drawing.Rectangle(_posX, _posY, ziw, zih - _dispY),
                    new System.Drawing.Rectangle(0, dispY, img.Width, img.Height - dispY),
                    System.Drawing.GraphicsUnit.Pixel);
                gfx.DrawImage(img,
                    new System.Drawing.Rectangle(_posX, _posY + zih - _dispY, ziw, _dispY),
                    new System.Drawing.Rectangle(0, 0, img.Width, dispY),
                    System.Drawing.GraphicsUnit.Pixel);
            }
            else if (x != 0 && y == 0)
            {
                gfx.DrawImage(img,
                    new System.Drawing.Rectangle(_posX, _posY, ziw - _dispX, zih),
                    new System.Drawing.Rectangle(dispX, 0, img.Width - dispX, img.Height),
                    System.Drawing.GraphicsUnit.Pixel);
                gfx.DrawImage(img,
                    new System.Drawing.Rectangle(_posX + ziw - _dispX, _posY, _dispX, zih),
                    new System.Drawing.Rectangle(0, 0, dispX, img.Height),
                    System.Drawing.GraphicsUnit.Pixel);
            }
        }
        /// <summary> Creates a displaced version of the specified image with the specified displacement. Displacement is seamless, or the image "comes out" of the opposite side.</summary>
        /// <param name="img">Image</param>
        /// <param name="disp">Displacement in pixels</param>
        /// <returns>Displaced image</returns>
        public static System.Drawing.Bitmap displacedImage(System.Drawing.Bitmap img, IntPair disp, double zoom)
        {
            System.Drawing.Bitmap res = new System.Drawing.Bitmap((int)(img.Width * zoom), (int)(img.Height * zoom));
            System.Drawing.Graphics gfx = System.Drawing.Graphics.FromImage(res);
            drawDisplacedImage(gfx, img, res.Width / 2, res.Height / 2, disp.a, disp.b, zoom);
            return res;
        }
        /// <summary> Creates a displaced version of the specified image with the specified displacement. Displacement is seamless, or the image "comes out" of the opposite side.</summary>
        /// <param name="img">Image</param>
        /// <param name="dispX">Displacement on X</param>
        /// <param name="dispY">Displacement on Y</param>
        /// <returns>Displaced image</returns>
        public static System.Drawing.Bitmap displacedImage(System.Drawing.Bitmap img, int dispX, int dispY, double zoom)
        {
            return displacedImage(img, new IntPair(dispX, dispY), zoom);
        }
        public static void drawRotDispImage(
            System.Drawing.Graphics gfx,
            System.Drawing.Bitmap img,
            int posX, int posY,
            int dispX, int dispY,
            double deg, double zoom, bool hq)
        {
            rotateGFX(gfx, posX, posY, deg, hq);
            drawDisplacedImage(gfx, img, posX, posY, dispX, dispY, zoom);
            gfx.Restore(gfxgs);
        }
        public static System.Drawing.Bitmap rotDispImage(
            System.Drawing.Bitmap img,
            int dispX, int dispY,
            double deg, double zoom, bool hq)
        {
            System.Drawing.Bitmap res = new System.Drawing.Bitmap(img.Width, img.Height);
            System.Drawing.Graphics gfx = System.Drawing.Graphics.FromImage(res);
            drawRotDispImage(gfx, img, img.Width / 2, img.Height / 2, dispX, dispY, deg, zoom, hq);
            return res;
        }
        /// <summary> Creates a tiled image from the specified image. Note - System.Drawing sucks at handling large images so use the generated images with caution. </summary>
        /// <param name="img">Image</param>
        /// <param name="width">Width of the new image</param>
        /// <param name="height">Height of the new image</param>
        /// <returns>The tiled image</returns>
        public static System.Drawing.Bitmap tiledImage(System.Drawing.Bitmap img, int width, int height)
        {
            if (width == img.Width && height == img.Height) return img;
            System.Drawing.Bitmap res = new System.Drawing.Bitmap(width, height);
            System.Drawing.Graphics gfx = System.Drawing.Graphics.FromImage(res);
            System.Drawing.TextureBrush tb = new System.Drawing.TextureBrush(img);
            gfx.FillRectangle(tb, 0, 0, width, height);
            return res;
        }
        /// <summary> Draw a tiled image in a specified rectangle.</summary>
        /// <param name="gfx">Graphics to draw with</param>
        /// <param name="img">Image to fill rectangle with</param>
        /// <param name="x">X coordinate of the upper left corner</param>
        /// <param name="y">Y coordinate of the upper left corner</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        public static void drawTiledImage(System.Drawing.Graphics gfx, System.Drawing.Bitmap img, int x, int y, int width, int height)
        {
            System.Drawing.Rectangle r = new System.Drawing.Rectangle(x, y, width, height);
            System.Drawing.TextureBrush tb = new System.Drawing.TextureBrush(img);
            gfx.FillRectangle(tb, r);
        }
        private static void dtr(System.Drawing.Graphics gfx, System.Drawing.Bitmap img, System.Drawing.Point[] pnt, double deg, Point p, double zoom)
        {
            System.Drawing.Bitmap di = displacedImage(img,//Refer to large comment #2
                (-1) * (int)(((int)(p.x) % (int)(img.Width * zoom)) / zoom),
                (-1) * (int)(((int)(p.y) % (int)(img.Height * zoom)) / zoom), zoom);
            rotateGFX(gfx, (int)p.x, (int)p.y, deg, false);
            System.Drawing.TextureBrush tb = new System.Drawing.TextureBrush(di);
            gfx.FillPolygon(tb, pnt);
            gfx.Restore(gfxgs);
        }
        /// <summary>Draws a textured polygon. The texture is drawn according to the rotation and position of the camera. Use larger images for textures with caution, System.Drawing is not too good at handling larger images.</summary>
        /// <param name="gfx">Graphics to draw with</param>
        /// <param name="img">Image to fill polygon with</param>
        /// <param name="pnt">The points forming the polygon</param>
        /// <param name="deg">Rotation of the screen</param>
        /// <param name="or">Upper left corner of the polygon, in which the polygon can fit, if the screen is not rotated</param>
        public static void drawTexturedRectangle(System.Drawing.Graphics gfx, System.Drawing.Bitmap img, System.Drawing.Point[] pnt, double deg, Point or, double zoom)
        {
            System.Drawing.Point[] pnt0 = new System.Drawing.Point[pnt.Count()];
            for (int i = 0; i < pnt.Count(); i++)
            {
                Point p = ((Point)pnt[i]).rotated(deg * (-1), (DoublePair)or);//the coordinates get further rotated when the graphics' matrics gets rotated, that is why the first has to be rotated back
                pnt0[i] = new System.Drawing.Point((int)p.x, (int)p.y);
            }
            dtr(gfx, img, pnt0, deg, or, zoom);
        }
        /// <summary>Draws a textured polygon. The texture is drawn according to the rotation and position of the camera. Use larger images for textures with caution, System.Drawing is not too good at handling larger images.</summary>
        /// <param name="gfx">Graphics to draw with</param>
        /// <param name="img">Image to fill polygon with</param>
        /// <param name="pnt">The points forming the polygon</param>
        /// <param name="deg">Rotation of the screen</param>
        /// <param name="or">Upper left corner of the polygon, in which the polygon can fit, if the screen is not rotated</param>
        public static void drawTexturedRectangle(System.Drawing.Graphics gfx, System.Drawing.Bitmap img, Point[] pnt, double deg, Point or, double zoom)
        {
            System.Drawing.Point[] pnt0 = new System.Drawing.Point[pnt.Count()];
            for (int i = 0; i < pnt.Count(); i++)
            {
                Point p = ((Point)pnt[i]).rotated(deg * (-1), (DoublePair)or);
                pnt0[i] = new System.Drawing.Point((int)p.x, (int)p.y);
            }
            dtr(gfx, img, pnt0, deg, or, zoom);
        }
        public static void drawTexturedRectangle(
            System.Drawing.Graphics gfx,
            System.Drawing.Bitmap img,
            System.Drawing.Color fillColor,
            Line[] side,
            Plane plane)
        {
            //building polygon
            int x = (int)side[0].start.x;//upper left corner of the rectangle fitting the polygon
            int y = (int)side[0].start.y;
            System.Drawing.Point[] point = new System.Drawing.Point[side.Count()];
            for (int i = 0; i < side.Count(); i++)
            {
                if ((int)side[i].start.x < x) x = (int)side[i].start.x;
                if ((int)side[i].start.y < y) y = (int)side[i].start.y;
                IntPair pnt = new IntPair(side[i].start.intScrCoords(plane.basis));
                point[i] = new System.Drawing.Point(pnt.a, pnt.b);
            }
            Point rp = new Point(x, y);
            //drawing the polygon
            if (!fillColor.Equals(System.Drawing.Color.Transparent))
                gfx.FillPolygon(new System.Drawing.SolidBrush(fillColor), point);
            if (img != null)
            {
                //rp = rp.dobulePlaneCoords(plane.basis);
                //rp = rp.zoomedCoords(screen.center, plane.zoom);
                DrawingAlgorithms.drawTexturedRectangle(gfx, img, point, plane.rotation, rp.doubleScrCoords(plane.basis), plane.zoom);
            }
        }
    }
    /// <summary> Class for all entites, which are going to block light. In development! </summary>
    public class LightShape
    {
        public LightShape(DoublePair[] point, System.Drawing.Color color, double height, double transparancy)
        {
            pt = new List<Point>();
            for (int i = 0; i < point.Count(); i++)
                pt.Add(point[i]);
            tr = transparancy;
            ht = height;
            clr = color;
        }
        private List<Point> pt;
        public List<Point> point { get { return pt; } set { pt = value; } }
        public Line shadowCastingLine(Point lightSource)
        {
            return ShadowAlgorithms.shadowCastingLine(lightSource, this);
        }
        private System.Drawing.Color clr;
        public System.Drawing.Color color { get { return clr; } set { clr = value; } }
        private double tr;
        public double transparancy { get { return tr; } set { tr = value; } }
        private double ht;
        public double height { get { return ht; } set { ht = value; } }
    }
    public class Light
    {
        private Point p;
        public Point point { get { return p; } set { p = value; } }
        private int rad;
        public int radius { get { return rad; } set { rad = value; } }
        private System.Drawing.Color clr;
        public System.Drawing.Color color { get { return clr; } set { clr = value; } }
    }
    public static class ShadowAlgorithms
    {
        /// <summary> Returns the line which casts the same shadow which the shape would. Requires speed improving.</summary>
        /// <param name="lightSource">The point from where the light comes from</param>
        /// <param name="ls">The object implementing the LightShape interface</param>
        /// <returns>The line which casts the same shadow which the shape would.</returns>
        public static Line shadowCastingLine(Point lightSource, LightShape ls)
        {
            Point st = ls.point[0];
            Point en = ls.point[0];
            for (int i = 1; i < ls.point.Count; i++)
            {//seraching for the two lines, for which all points are on the one side of each ray (or the two rays, between which all points are located)
                Line ln1 = new Line(lightSource, st);
                Line ln2 = new Line(lightSource, en);
                if (ln1.isPointInner(ls.point[i])) st = ls.point[i];
                if (ln2.isPointInner(ls.point[i]) == false) en = ls.point[i];
            }
            return new Line(st, en);
        }
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
    co-ordinate systems.
    Currently, the basis vectors are set to 1px, but I advise not to remove the divisions, because the basis vector lengths
    have a really heavy effect on the projecting of the entities on the screen. However, it should not cause bugs. I have
    implemented an option to add diversions for the basis vectors, that would have the same effect on the program as removing
    the divisions. Why not just remove the divisions? I think this way I and you can have easier control over the program.*/

/*LARGE COMMENT #2
    Because of how drawing of an image starts from a specific point on the screen, a displaced image must be created. This
    image is then used to fill the polygon. The coordinates of the point, from where the drawing starts, determine the
    displacement of the image. However, the pane might be zoomed in/out, which changes the coordinates of the point and thus
    how much the image needs to be displaced.
    Lets assume, that the zoomed image requires to be displaced. For that the size of the zoomed image is needed. With it the
    dislplacement of the zoomed image is calculated:
    ziw - zoomed width;
    zih - zoomed height;
    p - coordinates of the origin;
    ziw % p.x - displacement on X;
    zih & p.y - displacement on Y;
    But because the image is zoomed, the displacement of the original image will be
    (ziw % p.x) / zoom;
    (zih & p.y) / zoom;
 */

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

/*
        /// <summary> Draw a specified potion of the image on a specified location. Maintains the original size of the porion.</summary>
        /// <param name="gfx">Graphics</param>
        /// <param name="img">Image</param>
        /// <param name="px">X coordinate of the upper left corner of the portion relative to the image</param>
        /// <param name="py">Y coordinate of the upper left corner of the portion relative to the image</param>
        /// <param name="width">Width of the portion</param>
        /// <param name="height">Height of the portion</param>
        /// <param name="cx">X coordinate where to draw the portion</param>
        /// <param name="cy">Y coordinate where to draw the portion</param>
        public static void drawPortionOfImage(
            System.Drawing.Graphics gfx,
            System.Drawing.Bitmap img,
            int px, int py, int width, int height,
            int cx, int cy, int cw, int ch)
        {
            drawPortionOfImage(gfx, img, new System.Drawing.Rectangle(px, py, width, height), new System.Drawing.Rectangle(cx, cy, cw, ch));
        }
        /// <summary> Draw a specified potion of the image on a specified location. Maintains the original size of the porion.</summary>
        /// <param name="gfx">Graphics</param>
        /// <param name="img">Image</param>
        /// <param name="portion">Rectangle of the portion of the image to draw</param>
        /// <param name="cx">X coordinate where to draw the portion</param>
        /// <param name="cy">Y coordinate where to draw the portion</param>
        public static void drawPortionOfImage(
            System.Drawing.Graphics gfx,
            System.Drawing.Bitmap img,
            System.Drawing.Rectangle portion,
            System.Drawing.Rectangle porRect)
        {
            System.Drawing.Rectangle r = new System.Drawing.Rectangle(porRect.X, porRect.Y, portion.Width, portion.Height);
            gfx.DrawImage(img, r, porRect.X, porRect.Y, porRect.Width, porRect.Height, System.Drawing.GraphicsUnit.Pixel);
        }
        */
//cycle for finding the shadow casting line - has an error and scans the points of the polygon twice
//Line ln = new Line(ls.point[i], lightSource);//first potential line
//bool inner;//are points going to be innter or outer
//int ci;//if the i counter is zero, then the first checked point will be the 1st, so scanning of the other poitns will start from the 2nd point
//if (i != 0) { inner = ln.isPointInner(ls.point[0]); ci = 1; }//on which side is the zero point located
//else { inner = ln.isPointInner(ls.point[1]); ci = 2; }//if the zero point is the end of the current potential line, then check the 1st point on which side is located
//bool all = true;//are all points on the same side
//for (int c = ci; c < ls.point.Count; c++) 
//    if (c != i) 
//        if (ln.isPointInner(ls.point[c]) != inner)//if there is a point on the other side of the line, this line is not needed
//        {
//            all = false;
//            break;
//        }
//if (all)
//{
//    if (st == null)//set the one end of the shadow line
//        st = ls.point[i];
//    else//set the second end
//        en = ls.point[i];
//}