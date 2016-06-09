using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorBasedLinesEngine
{
    public class Point
    {//point coordinates
        private double _x;
        private double _y;
        //private - I am ensuring that there is no way to change coordinates, without recalculating lines' lengths
        public Point()                   { _x = 0;   _y = 0; }
        public Point(double x, double y) { _x = x;   _y = y; }
        public Point(IntPair p)          { _x = p.a; _y = p.b; }
        public Point(DoublePair p)       { _x = p.a; _y = p.b; }
        public Point(Point p)            { _x = p.x; _y = p.y; }
        public static implicit operator System.Drawing.Point(Point p) { return new System.Drawing.Point((int)Math.Round(p.x), (int)Math.Round(p.y)); }
        public double x
        {
            get { return _x; }
            set { _x = value; }
        }
        public double y
        {
            get { return _y; }
            set { _y = value; }
        }
        public void setCoords(double x, double y) { _x = x; _y = y; }
        public DoublePair doubleCoords { get { return new DoublePair(_x, _y); } set { _x = value.a; _y = value.b; } }
        public IntPair intCoords { get { return new IntPair((int)System.Math.Round(_x), (int)System.Math.Round(_y)); } }
        public DoublePair doubleScrCoords(Basis planeBasis, bool zoom, bool div)
        {//coordinates of the point relative to the screen's basis
            //each dot is treated like a vector - this is how a vector's coordinates change when switching the basis
            double OXlen;
            double OYlen;
            if (zoom && div) { OXlen = planeBasis.OXlen; OYlen = planeBasis.OYlen; }
            else if (!zoom && div) { OXlen = planeBasis.OXlenNoZoom; OYlen = planeBasis.OXlenNoZoom; }
            else if (zoom && !div) { OXlen = planeBasis.OXlenNoDiv; OYlen = planeBasis.OYlenNoDiv; }
            else /*!zoom && !div*/ { OXlen = planeBasis.OXlenBaseVals; OYlen = planeBasis.OYlenBaseVals; }
            //Refer to LARGE COMMENT #1 for explaining the divisions by OXlen and OYlen
            double resx = (planeBasis.xVector.x - planeBasis.center.x) * _x / OXlen + (planeBasis.yVector.x - planeBasis.center.x) * _y / OYlen + planeBasis.center.x;
            double resy = (planeBasis.xVector.y - planeBasis.center.y) * _x / OXlen + (planeBasis.yVector.y - planeBasis.center.y) * _y / OYlen + planeBasis.center.y;
            return new DoublePair((int)resx, (int)resy);
        }
        public IntPair intScrCoords(Basis planeBasis, bool zoom, bool div)//int coordinates without zoom
        {
            DoublePair res = doubleScrCoords(planeBasis, zoom, div);
            return new IntPair((int)Math.Round(res.a), (int)Math.Round(res.b));
        }
        public IntPair intScrCoords(Basis planeBasis, ScreenData screen)//int coordinates with zoom
        {
            DoublePair res = doubleScrCoords(planeBasis, true, true);
            return new IntPair((int)Math.Round(res.a), (int)Math.Round(res.b));
        }
        public DoublePair dobulePlaneCoords(Basis planeBasis, bool zoom, bool div)
        {
            double OXlen;
            double OYlen;
            if (zoom && div) { OXlen = planeBasis.OXlen; OYlen = planeBasis.OYlen; }
            else if (zoom && !div) { OXlen = planeBasis.OXlenNoDiv; OYlen = planeBasis.OYlenNoDiv; }
            else if (!zoom && div) { OXlen = planeBasis.OXlenNoZoom; OYlen = planeBasis.OYlenNoZoom; }
            else { OXlen = planeBasis.OXlenBaseVals; OYlen = planeBasis.OYlenBaseVals; }
            double ox1 = (planeBasis.xVector.x - planeBasis.center.x) / OXlen;
            double oy1 = (planeBasis.xVector.y - planeBasis.center.y) / OXlen;
            double ox2 = (planeBasis.yVector.x - planeBasis.center.x) / OYlen;
            double oy2 = (planeBasis.yVector.y - planeBasis.center.y) / OYlen;
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
        public DoublePair dobulePlaneCoords(Basis planeBasis) { return dobulePlaneCoords(planeBasis, true, true); }
        public IntPair intPlaneCoords(Basis planeBasis, bool zoom, bool div)
        {//the coordinates of the point on the screen relative to the plane's basis
            DoublePair res = dobulePlaneCoords(planeBasis, zoom, div);
            return new IntPair((int)Math.Round(res.a), (int)Math.Round(res.b));
        }
        public IntPair intPlaneCoords(Basis planeBasis) { return intPlaneCoords(planeBasis, true, true); }
        public DoublePair zoomedCoords(Point center, double zoom)
        {//usefull here and there
            return new DoublePair(
                (_x - center.x) * zoom + center.x,
                (_y - center.y) * zoom + center.y);
        }
        public bool isInLine(double[] ln) { return ln[0] * _x + ln[1] * _y + ln[2] == 0; }
        public bool isInLine(Line ln) { return isInLine(ln.getEquation()); }
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
        public Line() { setStuff(0, 0, 0, 0); }
        public Line(double stx, double sty, double enx, double eny) { setStuff(stx, sty, enx, eny); }
        public Line(DoublePair st, DoublePair en) { setStuff(st.a, st.b, en.a, en.b); }
        public Line(Point st, Point en) { setStuff(Math.Round(st.x), Math.Round(st.y), Math.Round(en.x), Math.Round(en.y)); }
        public Line(Line ln) { setStuff(ln.start.x, ln.start.y, ln.end.x, ln.end.y); }
        public void setStart(double x, double y) { st.setCoords(x, y); }
        public void setEnd(double x, double y) { en.setCoords(x, y); }
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
        public double[] getEquation()
        {
            double[] res = new double[3];
            res[0] = this.end.y - this.start.y;
            res[1] = this.start.x - this.end.x;
            res[2] = this.end.x * this.start.y - this.end.y * this.start.x;
            return res;
        }
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
        public bool isPointInner(Point point) { return isPointInner(getEquation(), point); }
        public Line lineToDraw(Basis basis, ScreenData screen)
        {
            if (screen.isPointInside(st) && screen.isPointInside(en))
                return new Line(st.x, st.y, en.x, en.y);
            Point stPnt = null;
            Point enPnt = null;
            Point upCrs = crossingPoint(new Line(0.0, 0.0, 700.0, 0.0));
            Point dnCrs = crossingPoint(new Line(0.0, 700.0, 700.0, 700.0));
            Point rtCrs = crossingPoint(new Line(700.0, 0.0, 700.0, 700.0));
            Point ltCrs = crossingPoint(new Line(0.0, 0.0, 0.0, 700.0));
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
                if (screen.isPointInside(st)) return new Line(stPnt.x, stPnt.y, st.x, st.y);
                if (screen.isPointInside(en)) return new Line(stPnt.x, stPnt.y, en.x, en.y);
            }
            return new Line(stPnt.x, stPnt.y, enPnt.x, enPnt.y);
        }
    }
    public interface Drawable
    {
        int drawableTypeAsInt { get; }
        string drawableType { get; }
        Plane plane { get; }//the plane the entity belongs to
        IntPair[] section { get; }//located in these sections; will be used when objects move
        bool moved { get; set; }//if the entity has moved
        void drawInfo(System.Drawing.Graphics gfx, Basis basis, ScreenData screen, int id);
        int indexInPlane { get; set; }
        void draw(System.Drawing.Graphics gfx, Basis basis, ScreenData screen);
        IntPair[] calcOccupiedSects(int sectorSize);//calculates which sections are the occupied
        void reCalcSects(int sectorSize);
        void addImage(string imgDir);
        void changeImage(int index, string imgDir);
        void removeImage(int index);
    }
    public class BasicDrawableData//basic data for use by the Drawable interface; you can also use your own
    {//all are public so I don't have to write accessors for the objects, that are already written in the Drawable interface, I advise you to make objects of this class private
        public BasicDrawableData()
        {
            plane = null;
            moved = false;
            section = null;
            indexInPlane = -1;
            image = new List<int>();
        }
        public Plane plane;
        public bool moved;
        public IntPair[] section;
        public int indexInPlane;
        private List<int> image;
        public System.Drawing.Bitmap imgFromIndex(int index) { return plane.getImageFromIndex(image[index]); }
        public void removeImage(int index) { image.RemoveAt(index); }
        public void addImage(string imgDir) { image.Add(plane.addImageResource(imgDir)); }
        public void changeImage(int index, string imgDir) { image[index] = plane.addImageResource(imgDir); }
    }
    public static class SectionAlgorithms
    {
        public static IntPair[] point(Object e, double x, double y, System.Drawing.Bitmap image, bool rotates, int sectorSize)
        {
            return point(e, new Point(x, y), image, rotates, sectorSize);
        }
        public static IntPair[] point(Object e, Point coords, System.Drawing.Bitmap image, bool rotates, int sectorSize)
        {
            Drawable pe = e as Drawable;
            if (pe != null)
            {
                IntPair intCoords = new IntPair((int)coords.x, (int)coords.y);//coordinates in int, a few pixels difference is not going to make much of a difference
                List<IntPair> res0 = new List<IntPair>();
                int entRad;
                lock (image)
                    entRad = (int)(Math.Sqrt(image.Width * image.Width + image.Height * image.Height) / 2);//radius of the entity, if it is going to rotate along with the screen
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
                for (int i = 0; i < res0.Count; i++)
                    if (res0[i].a < 0 || res0[i].b < 0)
                        res0.Remove(res0[i]);//removing the sectors with negative indexes, because they are outside of the plane
                IntPair[] res = new IntPair[res0.Count];
                for (int i = 0; i < res0.Count; i++)
                    res[i] = res0[i];
                return res;
            }
            else
                return null;
        }
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
        public static IntPair[] line(Object e, double x1, double y1, double x2, double y2, int sectorSize) {
            return line(e, new Line(x1, y1, x2, y2), sectorSize); }
        public static IntPair[] line(Object e, double stX, double stY, Point end, int sectorSize) {
            return line(e, new Line(new Point(stX, stY), end), sectorSize);
        }
        public static IntPair[] line(Object e, double stX, double stY, DoublePair end, int sectorSize) {
            return line(e, new Line(new Point(stX, stY), new Point(end)), sectorSize); }
        public static IntPair[] line(Object e, Point start, double enX, double enY, int sectorSize) {
            return line(e, new Line(start, new Point(enX, enY)), sectorSize); }
        public static IntPair[] line(Object e, DoublePair start, double enX, double enY, int sectorSize) {
            return line(e, new Line(new Point(start), new Point(enX, enY)), sectorSize); }
        public static IntPair[] line(Object e, Point start, Point end, int sectorSize) {
            return line(e, new Line(start, end), sectorSize);
        }
        public static IntPair[] line(Object e, DoublePair start, Point end, int sectorSize) {
            return line(e, new Line(new Point(start), end), sectorSize);
        }
        public static IntPair[] line(Object e, Point start, DoublePair end, int sectorSize)
        {
            return line(e, new Line(start, new Point(end)), sectorSize);
        }
        public static IntPair[] line(Object e, Line coords, int sectorSize)
        {
            Drawable le = e as Drawable;
            if (le != null) return line(coords, sectorSize);
            else return null;
        }
        public static IntPair[] line(Object e, DoublePair start, DoublePair end, int sectorSize) {
            return line(e, new Line(start, end), sectorSize); }
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
        public static IntPair[] polygon(Object e, Line[] side, bool filled, int sectorSize)
        {
            Drawable pe = e as Drawable;
            if (pe != null)
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
            else return null;
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