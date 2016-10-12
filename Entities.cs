﻿using System;
using System.Collections.Generic;
using System.Linq;
using VBLEDrawing;
using BaseObject;

namespace VectorBasedLinesEngine
{
    public class PointEntity : Entity, Drawable
    {
        private Point _coords; public Point coordinates { get { return new Point(_coords.x, _coords.y); } }
        private System.Drawing.Color _color; public System.Drawing.Color color { get { return System.Drawing.Color.FromArgb(_color.A, _color.R, _color.G, _color.B); } }
        private string imgDir; public string imageDir
        {
            get { return string.Copy(imgDir); }
            set
            {
                if (value.Equals("null") == false && value != null)
                {
                    //_image = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(System.IO.Directory.GetCurrentDirectory() + value);
                    imgDir = string.Copy(value);
                    bdd.addImage(value);
                    _image = bdd.imgFromIndex(0) as System.Drawing.Bitmap;
                }
            }
        }
        private System.Drawing.Bitmap _image;
        private System.Drawing.Bitmap prevImg;
        public IntPair disp;
        public override void copy(Entity e)
        {
            if (e is PointEntity)
            {
                PointEntity pe = (PointEntity)e;
                this.setStuff(_color, pe.imageDir, pe.coordinates.x, pe.coordinates.y, pe.plane);
            }
            else
                throw new ArgumentException("The entity requested for copying is not a Point based entity.");
        }
        public override string getInfo()
        {
            throw new NotImplementedException();
        }
        //Drawable interface
        private BasicDrawableData bdd;
        public LightShape lightShape { get { return null; } set { } }
        public string drawableType { get { return "PointEntity"; } }
        public int drawableTypeAsInt { get { return 1; } }
        private bool isInScreen(int x, int y, ScreenData screen)
        {
            lock (_image)
                if (_image != null)
                {
                    int ziw = (int)(_image.Width * bdd.plane.zoom);
                    int zih = (int)(_image.Height * bdd.plane.zoom);
                    if (!rotating)
                    {
                        if (x >= 0 - ziw / 2 && x < screen.width + ziw / 2 &&
                            y >= 0 - zih / 2 && y < screen.height + zih / 2)
                            return true;
                    }
                    else
                    {
                        int hfdi = (int)Math.Sqrt(Math.Pow(ziw, 2) + Math.Pow(zih, 2));
                        if (x >= 0 - hfdi / 2 && x < screen.width + hfdi / 2 &&
                            y >= 0 - hfdi / 2 && y < screen.height + hfdi / 2)
                            return true;
                    }
                }
                else
                    return screen.isPointInside(_coords);
            return false;
        }
        public void draw(System.Drawing.Graphics gfx, ScreenData screen)
        {
            IntPair res = _coords.intScrCoords(bdd.plane.basis);
            //IntPair res0 = _coords.intScrCoords(bdd.plane.basis, true, true, false);
            System.Drawing.Bitmap img = _image;
            if (isInScreen(res.a, res.b, screen))
                if (_image != null)
                {
                    if (disp.a % img.Width != 0 && disp.b % img.Height != 0)
                    {
                        if (rotating)
                            DrawingAlgorithms.drawRotDispImage(gfx, img,
                                res.a, res.b, disp.a, disp.b,
                                bdd.plane.rotation, 1, false);
                        else
                        {
                            DrawingAlgorithms.drawDisplacedImage(gfx, img,
                                res.a, res.b, disp.a, disp.b, plane.zoom);
                            //gfx.DrawImage(DrawingAlgorithms.displacedImage(img, disp, plane.zoom), res.a - (int)(img.Width * plane.zoom / 2), res.b - (int)(img.Height * plane.zoom / 2));
                        }
                    }
                    else if (rotating)
                        DrawingAlgorithms.drawImage(gfx, img,
                            res.a, res.b,
                            bdd.plane.rotation, plane.zoom, false);
                    else
                        DrawingAlgorithms.drawImage(gfx, img, res.a, res.b, 0, plane.zoom, false);
                }
                else
                    gfx.DrawLine(new System.Drawing.Pen(_color), res, new System.Drawing.Point(res.a + 1, res.b + 1));
            prevImg = img;
        }
        public void drawInfo(System.Drawing.Graphics gfx, ScreenData screen)
        {
            //IntPair res = zoomedInScrPoint(_coords, basis, screen);
            IntPair res = _coords.intScrCoords(bdd.plane.basis);
            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black);
            if (isInScreen(res.a, res.b, screen))
            {
                gfx.DrawLine(pen, res.a, res.b, res.a + 3, res.b + 3);
                gfx.DrawLine(pen, res.a - 1, res.b + 1, res.a - 3, res.b + 3);
                gfx.DrawLine(pen, res.a + 1, res.b - 1, res.a + 3, res.b - 3);
                gfx.DrawLine(pen, res.a - 1, res.b - 1, res.a - 3, res.b - 3);
            }
            gfx.DrawString(bdd.indexInPlane + " " + _coords.x.ToString() + " " + _coords.y.ToString(),
                            new System.Drawing.Font("Consolas", 12),
                            new System.Drawing.SolidBrush(System.Drawing.Color.Black),
                            res.a, res.b + 12);
            if (prevImg != null)
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                    res.a - prevImg.Width / 2,
                    res.b - prevImg.Height / 2,
                    prevImg.Width, prevImg.Height);
                gfx.DrawRectangle(pen, rect);
            }
        }
        public Plane plane { get { return bdd.plane; } }//the plane the entity belongs to
        public IntPair[] section { get { return bdd.section; } }//located in these sections; will be used when objects move
        public bool moved { get { return bdd.moved; } set { bdd.moved = value; } }//if the entity has moved
        public int indexInPlane { get { return bdd.indexInPlane; } set { bdd.indexInPlane = value; } }
        //public bool drawn { get { return bdd.drawn; } set { bdd.drawn = value; } }
        public IntPair[] calcOccupiedSects()
        {
            bdd.section = SectionAlgorithms.point(_coords, _image, bdd.plane.sectorSize, true, 1.0);
            return bdd.section;
        }//calculates which sections are the occupied
        public void reCalcSects()
        {
            IntPair[] oldSect = bdd.section;
            bdd.section = SectionAlgorithms.point(_coords, _image, bdd.plane.sectorSize, true, 1.0);
            bdd.plane.changeEntitySections(bdd.indexInPlane, bdd.section, oldSect);
        }
        public IntPair[] locatedInSections { get { return bdd.section; } }
        public void addImage(string imgDir) { bdd.addImage(imgDir); }
        public void changeImage(int index, string imgDir) { bdd.changeImage(index, imgDir); }
        public void removeImage(int index) { bdd.removeImage(index); }
        //==============================================================================================
        private void setStuff(System.Drawing.Color color, string image, double x, double y, Plane plane)
        {
            bdd = new BasicDrawableData();
            bdd.plane = plane;
            _coords = new Point();
            _coords.setCoords(x, y);
            imageDir = image;
            _color = color;
            disp = new IntPair();
        }
        public PointEntity(Plane plane){
            setStuff(System.Drawing.Color.FromArgb(0, 0, 0), null,  0, 0, plane); }
        public PointEntity(System.Drawing.Color color, Plane plane){
            setStuff(color, null, 0, 0, plane); }
        public PointEntity(string image, Plane plane)
        {
            if (image[0] != '|')
                setStuff(System.Drawing.Color.FromArgb(0, 0, 0), image, 0, 0, plane);
            else
                stringConstructor(image, plane);
        }
        public PointEntity(double x, double y, Plane plane){
            setStuff(System.Drawing.Color.FromArgb(0, 0, 0), null, x, y, plane); }
        public PointEntity(System.Drawing.Color color, double x, double y, Plane plane){
            setStuff(color, null, x, y, plane); }
        public PointEntity(string image, double x, double y, Plane plane){
            setStuff(System.Drawing.Color.FromArgb(0, 0, 0), image, x, y, plane); }
        public PointEntity(System.Drawing.Color color, string image, IntPair c, Plane plane){
            setStuff(color, image, c.a, c.b, plane); }
        private void stringConstructor(string data, Plane plane)
        {
            char[] ignoreChar = { '|' };
            string[] dataSect = data.Split(ignoreChar, StringSplitOptions.RemoveEmptyEntries);
            if (dataSect[0].Equals(drawableTypeAsInt.ToString()) == false) throw new ArgumentException("Invalid data input for PointEntity.");
            //|type|coord X,coord Y|alpha,red,green,blue|image dir|
            char[] ignoreChar0 = { ' ' };
            //coordinates
            string[] coordData = dataSect[1].Split(ignoreChar0, StringSplitOptions.RemoveEmptyEntries);
            DoublePair coords = new DoublePair(Convert.ToDouble(coordData[0]), Convert.ToDouble(coordData[1]));
            //color
            string[] colorData = dataSect[2].Split(ignoreChar0, StringSplitOptions.RemoveEmptyEntries);
            System.Drawing.Color clr = System.Drawing.Color.FromArgb(
                Convert.ToInt32(colorData[0]), Convert.ToInt32(colorData[1]),
                Convert.ToInt32(colorData[2]), Convert.ToInt32(colorData[3]));
            //image is dataSect[3]
            setStuff(clr, dataSect[3], coords.a, coords.b, plane);
        }
        public override string dataString()
        {
            //|type|coordX coordY|alpha red green blue|imageDir|
            return "|" + drawableTypeAsInt.ToString() + '|' + _coords.x + ' ' + _coords.y + '|' + _color.A + ' ' + _color.R + ' ' + _color.G + ' ' + _color.B + '|' + imgDir + '|';
        }
        //==============================================================================================
        public void setCoords(double x, double y)
        {
            _coords.x = x;
            _coords.y = y;
            bdd.moved = true;
        }
        public void setColor(System.Drawing.Color color) { _color = color; }
        public bool rotating = false;
        //==============================================================================================
    }
    public class LineEntity : Entity, Drawable
    {
        private Line ln; public Line lineCoords { get { return new Line(ln.start, ln.end); } }
        public Point start
        {
            get
            {
                Line ln = lineCoords;
                return new Point(ln.start.x, ln.start.y);
            }
        }
        public Point end
        {
            get
            {
                Line ln = lineCoords;
                return new Point(ln.end.x, ln.end.y);
            }
        }
        private System.Drawing.Color _color; public System.Drawing.Color color { get { return System.Drawing.Color.FromArgb(_color.A, _color.R, _color.G, _color.B); } }
        public override void copy(Entity e)
        {
            LineEntity le = e as LineEntity;
            if (le != null)
            {
                setStuff(start.doubleCoords, end.doubleCoords, _color, bdd.plane);
            }
            else
                throw new ArgumentException("The entity requested for copying is not a Line based entity.");
        }
        public override string getInfo()
        {
            throw new NotImplementedException();
        }
        //Drawable interface
        private BasicDrawableData bdd;
        public LightShape lightShape { get { return null; } set { } }
        public string drawableType { get { return "LineEntity"; } }
        public int drawableTypeAsInt { get { return 2; } }
        public bool isInScreen(Basis basis, ScreenData screen)
        {
            Point scrSt0 = new Point(ln.start.doubleScrCoords(basis, true, true, true));// scrSt0 = scrSt0.zoomedCoords(screen.center, plane.zoom);
            Point scrEn0 = new Point(ln.end.doubleScrCoords(basis, true, true, true));//   scrEn0 = scrEn0.zoomedCoords(screen.center, plane.zoom);
            IntPair scrSt = new IntPair(Math.Round(scrSt0.x), Math.Round(scrSt0.y));
            IntPair scrEn = new IntPair(Math.Round(scrEn0.x), Math.Round(scrEn0.y));
            if ((scrSt.a >= 0 && scrSt.a <= screen.width && scrSt.b >= 0 && scrSt.b <= screen.height) ||
                (scrEn.a >= 0 && scrEn.a <= screen.width && scrEn.b >= 0 && scrEn.b <= screen.height))
                return true;//if one of the ends is visible by the screen
            Line scrLn = new Line(scrSt.a, scrSt.b, scrEn.a, scrEn.b);
            Line left = new Line(screen.upLeftCorner, screen.downLeftCorner);
            Line right = new Line(screen.downRightCorner, screen.upRightCorner);
            Line up = new Line(screen.upRightCorner, screen.upLeftCorner);
            Line down = new Line(screen.downRightCorner, screen.downLeftCorner);
            if (scrLn.crosses(left)) return true;
            if (scrLn.crosses(right)) return true;
            if (scrLn.crosses(up)) return true;
            if (scrLn.crosses(down)) return true;
            return false;
        }
        public void draw(System.Drawing.Graphics gfx, ScreenData screen)
        {
            if (isInScreen(bdd.plane.basis, screen) && _color != null)
            {
                System.Drawing.Pen pen = new System.Drawing.Pen(_color);
                DoublePair st = ln.start.doubleScrCoords(bdd.plane.basis, true, true, true);
                DoublePair en = ln.end.doubleScrCoords(bdd.plane.basis, true, true, true);
                Line temp = new Line(st, en);//draw only the part of the line which is visible
                temp = temp.lineToDraw(bdd.plane.basis, screen);
                if (temp != null)
                gfx.DrawLine(pen, temp.start, temp.end);
                //gfx.DrawLine(pen, st, en);
                if (bdd.plane.pseudo3D)
                {
                    int ht = (int)(40 * bdd.plane.zoom);
                    System.Drawing.Point[] pa = new System.Drawing.Point[4];
                    pa[0] = new System.Drawing.Point((int)st.a, (int)st.b);
                    pa[1] = new System.Drawing.Point((int)en.a, (int)en.b);
                    pa[2] = new System.Drawing.Point((int)en.a, (int)en.b - ht);
                    pa[3] = new System.Drawing.Point((int)st.a, (int)st.b - ht);
                    System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100, 255, 255, 255));
                    gfx.FillPolygon(sb, pa);
                    gfx.DrawPolygon(pen, pa);
                }
                //gfx.DrawLine(pen, (int)st.a, (int)st.b, (int)st.a, (int)st.b - ht);
                //gfx.DrawLine(pen, (int)en.a, (int)en.b, (int)en.a, (int)en.b - ht);
                //gfx.DrawLine(pen, (int)st.a, (int)st.b - ht, (int)en.a, (int)en.b - ht);
            }
        }
        public void drawInfo(System.Drawing.Graphics gfx, ScreenData screen)
        {
            System.Drawing.Pen pen = new System.Drawing.Pen(_color);
            IntPair st = ln.start.intScrCoords(bdd.plane.basis);
            IntPair en = ln.end.intScrCoords(bdd.plane.basis);
            gfx.DrawString(bdd.indexInPlane.ToString(),
                    new System.Drawing.Font("Consolas", 12),
                    new System.Drawing.SolidBrush(System.Drawing.Color.Gray),
                    st.a, st.b);
            String sections = "";
            //if (bdd.section != null)
            for (int i = 0; i < bdd.section.Count(); i++)
                sections += bdd.section[i].a + "-" + bdd.section[i].b + ";";
            gfx.DrawString(sections,
                    new System.Drawing.Font("Consolas", 12),
                    new System.Drawing.SolidBrush(_color),
                    (st.a + en.a) / 2, (st.b + en.b) / 2);
            gfx.DrawLine(new System.Drawing.Pen(_color), st.a + 3, st.b + 3, st.a - 3, st.b - 3);
            gfx.DrawLine(new System.Drawing.Pen(_color), en.a + 3, en.b + 3, en.a - 3, en.b - 3);
        }
        public Plane plane { get { return bdd.plane; } }//the plane the entity belongs to
        public IntPair[] section { get { return bdd.section; } }//located in these sections; will be used when objects move
        public bool moved { get { return bdd.moved; } set { bdd.moved = value; } }//if the entity has moved
        public int indexInPlane { get { return bdd.indexInPlane; } set { bdd.indexInPlane = value; } }
        public bool drawn { get { return bdd.drawn; } set { bdd.drawn = value; } }
        public IntPair[] calcOccupiedSects()
        {
            bdd.section = SectionAlgorithms.line(ln, bdd.plane.sectorSize);
            return bdd.section;
        }//calculates which sections are the occupied
        public void reCalcSects()
        {
            IntPair[] oldSect = bdd.section;
            bdd.section = SectionAlgorithms.line(ln, bdd.plane.sectorSize);
            bdd.plane.changeEntitySections(bdd.indexInPlane, bdd.section, oldSect);
        }
        //public IntPair[] locatedInSections { get { return bdd.section; } }
        //for a line with an image, use a polygon based entity, or an entity that uses the polygon algorithm for the "_locatedInSections(int)" method
        public void addImage(string imgDir) { bdd.addImage(imgDir); }
        public void changeImage(int index, string imgDir) { bdd.changeImage(index, imgDir); }
        public void removeImage(int index) { bdd.removeImage(index); }
        //==============================================================================================
        public void setColor(System.Drawing.Color color) {_color = color; }
        public void setStartCoords(double x, double y) { ln.setStart(x, y); bdd.moved = true; }
        public void setEndCoords(double x, double y) { ln.setEnd(x, y); bdd.moved = true; }
        private void setStuff(DoublePair st, DoublePair en, System.Drawing.Color clr, Plane plane)
        {
            bdd = new BasicDrawableData();
            bdd.plane = plane;
            ln = new Line(st.a, st.b, en.a, en.b);
            _color = clr;
            //setImage(img);
        }
        public LineEntity(Plane plane){
            setStuff(new IntPair(0, 0), new IntPair(0, 0), System.Drawing.Color.Black, plane); }
        public LineEntity(DoublePair st, DoublePair en, Plane plane){
            setStuff(st, en, System.Drawing.Color.Black, plane); }
        public LineEntity(double stx, double sty, double enx, double eny, Plane plane){
            setStuff(new DoublePair(stx, sty), new DoublePair(enx, eny), System.Drawing.Color.Black, plane); }
        public LineEntity(System.Drawing.Color clr, DoublePair st, DoublePair en, Plane plane){
            setStuff(st, en, clr, plane); }
        public LineEntity(System.Drawing.Color clr, double stx, double sty, double enx, double eny, Plane plane){
            setStuff(new DoublePair(stx, sty), new DoublePair(enx, eny), clr, plane); }
        public LineEntity(string data, Plane plane)
        {
            char[] ignoreChar = { '|' };
            string[] dataSect = data.Split(ignoreChar, StringSplitOptions.RemoveEmptyEntries);
            if (dataSect[0].Equals(drawableTypeAsInt.ToString()) == false) throw new ArgumentException("Invalid data input for LineEntity.");
            //|type|startX startY|endX endY|alpha red green blue|
            char[] ignoreChar0 = { ' ' };
            //coordinates
            string[] stCor = dataSect[1].Split(ignoreChar0, StringSplitOptions.RemoveEmptyEntries);
            DoublePair st = new DoublePair(Convert.ToDouble(stCor[0]), Convert.ToDouble(stCor[1]));
            string[] enCor = dataSect[2].Split(ignoreChar0, StringSplitOptions.RemoveEmptyEntries);
            DoublePair en = new DoublePair(Convert.ToDouble(enCor[0]), Convert.ToDouble(enCor[1]));
            //color
            string[] colorData = dataSect[3].Split(ignoreChar0, StringSplitOptions.RemoveEmptyEntries);
            System.Drawing.Color clr = System.Drawing.Color.FromArgb(
                Convert.ToInt32(colorData[0]), Convert.ToInt32(colorData[1]),
                Convert.ToInt32(colorData[2]), Convert.ToInt32(colorData[3]));
            setStuff(st, en, clr, plane);
        }
        public override string dataString()
        {
            //|type|startX startY|endX endY|alpha red green blue|
            return "|" + drawableTypeAsInt.ToString() + '|' + ln.start.x + ' ' + ln.start.y + '|' + ln.end.x + ' ' + ln.end.y + '|' + _color.A + ' ' + _color.R + ' ' + _color.G + ' ' + _color.B + '|';
        }
        //==============================================================================================
    }
    public class PolygonEntity : Entity, Drawable
    {
        private bool _filled; public bool filled { get { return _filled; } }
        private System.Drawing.Color _fillColor; public System.Drawing.Color fillColor { get { return System.Drawing.Color.FromArgb(_fillColor.A, _fillColor.R, _fillColor.G, _fillColor.B); } }
        private bool fillClrSet = false;
        private System.Drawing.Bitmap _image;
        private bool imageSet = false;
        private string imgDir; public string imageLocation { get { return string.Copy(imgDir); } }
        //Drawable interface
        private BasicDrawableData bdd;
        public LightShape lightShape { get { return null; } set { } }
        public string drawableType { get { return "PolygonEntity"; } }
        public int drawableTypeAsInt { get { return 3; } }
        private void drawOutlines(System.Drawing.Graphics gfx, Basis basis, ScreenData screen)
        {
            for (int i = 0; i < _side.Count(); i++)
                if (_side[i].isInScreen(basis, screen))
                    _side[i].draw(gfx, screen);
        }
        public void draw(System.Drawing.Graphics gfx, ScreenData screen)
        {
            if (_filled)
            {
                //building polygon
                int x = (int)_side[0].start.x;//upper left corner of the rectangle fitting the polygon
                int y = (int)_side[0].start.y;
                System.Drawing.Point[] point = new System.Drawing.Point[_side.Count()];
                for (int i = 0; i < _side.Count(); i++)
                {
                    if ((int)_side[i].start.x < x) x = (int)_side[i].start.x;
                    if ((int)_side[i].start.y < y) y = (int)_side[i].start.y;
                    IntPair pnt = new IntPair(_side[i].start.intScrCoords(bdd.plane.basis));
                    point[i] = new System.Drawing.Point(pnt.a, pnt.b);
                }
                Point rp = new Point(x, y);
                //drawing the polygon
                if (fillClrSet)
                    gfx.FillPolygon(new System.Drawing.SolidBrush(_fillColor), point);
                if (imageSet)
                {
                    //rp = rp.dobulePlaneCoords(plane.basis);
                    //rp = rp.zoomedCoords(screen.center, plane.zoom);
                    DrawingAlgorithms.drawTexturedRectangle(gfx, _image, point, plane.rotation, rp.doubleScrCoords(plane.basis), plane.zoom);
                }
            }
            drawOutlines(gfx, bdd.plane.basis, screen);
        }
        public void drawInfo(System.Drawing.Graphics gfx, ScreenData screen)
        {
            for (int i = 0; i < _side.Count(); i++)
                _side[i].drawInfo(gfx, screen);
        }
        public Plane plane { get { return bdd.plane; } }//the plane the entity belongs to
        public IntPair[] section { get { return bdd.section; } }//located in these sections; will be used when objects move
        public bool moved { get { return bdd.moved; } set { bdd.moved = value; } }//if the entity has moved
        public int indexInPlane { get { return bdd.indexInPlane; } set { bdd.indexInPlane = value; } }
        public bool drawn { get { return bdd.drawn; } set { bdd.drawn = value; } }
        public Line[] sidesAsLine()
        {
            Line[] res = new Line[_side.Count()];
            for (int i = 0; i < _side.Count(); i++)
                res[i] = new Line(_side[i].lineCoords);
            return res;
        }
        public IntPair[] calcOccupiedSects()
        {
            bdd.section = SectionAlgorithms.polygon(sidesAsLine(), filled, bdd.plane.sectorSize);
            for (int i = 0; i < _side.Count(); i++)
                _side[i].calcOccupiedSects();
            return bdd.section;
        }//calculates which sections are the occupied
        public void reCalcSects()
        {
            IntPair[] oldSect = bdd.section;
            bdd.section = SectionAlgorithms.polygon(sidesAsLine(), filled, bdd.plane.sectorSize);
            bdd.plane.changeEntitySections(bdd.indexInPlane, bdd.section, oldSect);
            for (int i = 0; i < _side.Count(); i++)
                _side[i].calcOccupiedSects();
        }
        public IntPair[] locatedInSections { get { return bdd.section; } }
        public void addImage(string imgDir) { bdd.addImage(imgDir); }
        public void changeImage(int index, string imgDir) { bdd.changeImage(index, imgDir); }
        public void removeImage(int index) { bdd.removeImage(index); }
        //==============================================================================================
        private void setStuff(DoublePair[] point, System.Drawing.Color[] lnClr,
                              bool filled, System.Drawing.Color fillColor, string imgDir, Plane plane)
        {
            _filled = filled;
            _fillColor = fillColor;
            if (filled == true && (imgDir == null || imgDir.Equals("null")))
                fillClrSet = true;
            if (imgDir != null && imgDir.Equals("null") == true && _filled == true)
                imageSet = true;
            bdd = new BasicDrawableData();
            bdd.plane = plane;
            setTextureImage(imgDir);
            _side = new LineEntity[point.Count()];
            for (int i = 0; i < point.Count() - 1; i++)//building the sides
                _side[i] = new LineEntity(point[i], point[i + 1], plane);
            _side[point.Count() - 1] = new LineEntity(point[point.Count() - 1], point[0], plane);
            if (lnClr != null)//setting their colors, if each must have its own color (not really needed for the base code, but whatever)
                for (int i = 0; i < _side.Count(); i++)
                {
                    if (i < lnClr.Count())
                        _side[i].setColor(lnClr[i]);
                    else
                        _side[i].setColor(System.Drawing.Color.Black);
                }
            else
                for (int i = 0; i < _side.Count(); i++)
                    _side[i].setColor(System.Drawing.Color.Black);
        }
        public PolygonEntity(DoublePair[] point, Plane plane){
            fillClrSet = false;
            setStuff(point, null, false, System.Drawing.Color.Black, null, plane); }
        public PolygonEntity(DoublePair[] point, System.Drawing.Color[] lnClr, Plane plane) {
            fillClrSet = false;
            setStuff(point, lnClr, false, System.Drawing.Color.Black, null, plane); }
        public PolygonEntity(DoublePair[] point, System.Drawing.Color[] lnClr, System.Drawing.Color fillColor, Plane plane) {
            fillClrSet = true;
            setStuff(point, lnClr, true, fillColor, null, plane); }
        public PolygonEntity(DoublePair[] point, System.Drawing.Color[] lnClr, string imgDir, Plane plane) {
            fillClrSet = false;
            setStuff(point, lnClr, true, System.Drawing.Color.Black, imgDir, plane); }
        public PolygonEntity(DoublePair[] point, System.Drawing.Color[] lnClr, System.Drawing.Color fillColor, string imgDir, Plane plane) {
            fillClrSet = true;
            setStuff(point, lnClr, true, fillColor, imgDir, plane); }
        public PolygonEntity(string data, Plane plane)
        {
            char[] ignoreChar = { '|' };
            string[] dataSect = data.Split(ignoreChar, StringSplitOptions.RemoveEmptyEntries);
            if (dataSect[0].Equals(drawableTypeAsInt.ToString()) == false) throw new ArgumentException("Invalid data input for PolygonEntity.");
            //|type|point0; ... ;pointN|side0Color; ... ;sideNColor|isFilled|imadeDir|fillClrA fillClrR fillClrG fillClrB|
            //pointN is written as "pointNX pointNY"
            //sideNColor is written as "sideNColorA sideNColorR sideNColorG sideNColorB"
            char[] ignoreChar0 = { ';' };
            char[] ignoreChar1 = { ' ' };
            //coordinates
            string[] coordStr = dataSect[1].Split(ignoreChar0, StringSplitOptions.RemoveEmptyEntries);
            DoublePair[] coord = new DoublePair[coordStr.Count()];
            for (int i = 0; i < coordStr.Count(); i++)
            {
                string[] point = coordStr[i].Split(ignoreChar1, StringSplitOptions.RemoveEmptyEntries);
                coord[i] = new DoublePair(Convert.ToDouble(point[0]), Convert.ToDouble(point[1]));
            }
            //line colors
            string[] clrStr = dataSect[2].Split(ignoreChar0, StringSplitOptions.RemoveEmptyEntries);
            System.Drawing.Color[] clr = new System.Drawing.Color[clrStr.Count()];
            for (int i = 0; i < coordStr.Count(); i++)
            {
                string[] clrDat = clrStr[i].Split(ignoreChar1, StringSplitOptions.RemoveEmptyEntries);
                clr[i] = System.Drawing.Color.FromArgb(
                    Convert.ToInt32(clrDat[0]), Convert.ToInt32(clrDat[1]),
                    Convert.ToInt32(clrDat[2]), Convert.ToInt32(clrDat[3]));
            }
            bool isFilled = Convert.ToBoolean(dataSect[3]);//is it filled
            //fill color
            string[] fillClrDat = dataSect[5].Split(ignoreChar1, StringSplitOptions.RemoveEmptyEntries);
            System.Drawing.Color fillClr = System.Drawing.Color.FromArgb(
                    Convert.ToInt32(fillClrDat[0]), Convert.ToInt32(fillClrDat[1]),
                    Convert.ToInt32(fillClrDat[2]), Convert.ToInt32(fillClrDat[3]));
            //image directory is set in the setStuff method
            setStuff(coord, clr, isFilled, fillClr, dataSect[4], plane);
        }
        public override string dataString()
        {
            //|type|point0; ... ;pointN|side0Color; ... ;sideNColor|isFilled|imadeDir|fillClrA fillClrR fillClrG fillClrB|
            //pointN is written as "pointNX pointNY"
            //sideNColor is written as "sideNColorA sideNColorR sideNColorG sideNColorB"
            string[] resArr = {"",""};
            //resArr[0] = _type.ToString();
            for (int i = 0; i < _side.Count(); i++ )
            {
                if (i != _side.Count() - 1) resArr[0] += _side[i].start.x + " " + _side[i].start.y + ';';
                else                        resArr[0] += _side[i].start.x + " " + _side[i].start.y;
            }
            for (int i = 0; i < _side.Count(); i++)
            {
                if (i != _side.Count() - 1) resArr[1] += _side[i].color.A + " " + _side[i].color.R + " " + _side[i].color.G + " " + _side[i].color.B + ";";
                else                        resArr[1] += _side[i].color.A + " " + _side[i].color.R + " " + _side[i].color.G + " " + _side[i].color.B;
            }
            return "|" + drawableTypeAsInt + '|' + resArr[0] + '|' + resArr[1] + '|' +
                _filled + '|' + imgDir + '|' + _fillColor.A + ' ' + _fillColor.R + ' ' + _fillColor.G + ' ' + _fillColor.B + '|';
        }
        public void setTextureImage(string img)
        {
            imgDir = img;
            imageSet = false;
            if (img != null && img.Equals("null") == false)
            {
                //_image = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(System.IO.Directory.GetCurrentDirectory() + img);
                imageSet = true;
                bdd.addImage(img);
                _image = bdd.imgFromIndex(0) as System.Drawing.Bitmap;
            }
        }
        //==============================================================================================
        public override void copy(Entity e)
        {
            PolygonEntity pe = e as PolygonEntity;
            if (pe != null)
            {
                setStuff(pe.point, pe.lineColor, pe.filled, pe.fillColor, pe.imageLocation, pe.plane);
            }
            else
                throw new ArgumentException("The entity requested to copying is not a Polygon based entity.");
        }
        public override string getInfo()
        {
            throw new NotImplementedException();
        }
        public DoublePair[] point
        {
            get
            {
                DoublePair[] res = new DoublePair[_side.Count()];
                for (int i = 0; i < _side.Count(); i++)
                    res[i] = new DoublePair(_side[i].start.x, _side[i].start.y);
                return res;
            }
        }
        public System.Drawing.Color[] lineColor
        {
            get
            {
                System.Drawing.Color[] res = new System.Drawing.Color[_side.Count()];
                for (int i = 0; i < _side.Count(); i++)
                    res[i] = _side[i].color;
                return res;
            }
        }
        private LineEntity[] _side;//for adding and removing sides I may need to use a data structure, for now I will leave it like that
    }
    public class CrossingLines : Entity, Drawable//for testing the crossing lines algorithm
    {
        private LineEntity le1;
        private LineEntity le2;
        private System.Drawing.Color clr;
        public CrossingLines(string image, Point e1, Point s1, Point e2, Point s2, Plane plane)
        {
            bdd = new BasicDrawableData();
            bdd.plane = plane;
            clr = System.Drawing.Color.DarkCyan;
            le1 = new LineEntity(clr, s1.x, s1.y, e1.x, e1.y, plane);
            le2 = new LineEntity(clr, s2.x, s2.y, e2.x, e2.y, plane);
            bdd.addImage(image);
        }
        //Entity overrides
        public override void copy(Entity e)
        {
            throw new NotImplementedException();
        }
        public override string dataString()
        {
            throw new NotImplementedException();
        }
        public override string getInfo()
        {
            throw new NotImplementedException();
        }
        //Drawable interface
        private BasicDrawableData bdd;
        public LightShape lightShape { get { return null; } set { } }
        public int drawableTypeAsInt { get { return 4; } }
        public string drawableType { get { return "CrossingLinesEntity"; } }
        public Plane plane { get { return bdd.plane; } }
        public IntPair[] section { get { return bdd.section; } }
        public bool moved { get { return bdd.moved; } set { bdd.moved = value; } }
        public void drawInfo(System.Drawing.Graphics gfx, ScreenData screen)
        {
            le1.drawInfo(gfx, screen);
            le2.drawInfo(gfx, screen);
        }
        public int indexInPlane { get { return bdd.indexInPlane; } set { bdd.indexInPlane = value; } }
        public void draw(System.Drawing.Graphics gfx, ScreenData screen)
        {
            le1.draw(gfx, screen);
            le2.draw(gfx, screen);
            Point crs = le1.lineCoords.crossingPoint(le2.lineCoords);
            if (crs != null)
            {
                crs = crs.intScrCoords(bdd.plane.basis);
                DrawingAlgorithms.drawImage(gfx, bdd.imgFromIndex(0), (int)crs.x, (int)crs.y, 0, 1, false);
            }
        }
        public IntPair[] calcOccupiedSects()
        {
            le1.calcOccupiedSects();
            le2.calcOccupiedSects();
            SortedSet<IntPair> s = new SortedSet<IntPair>();
            for (int i = 0; i < le1.section.Count(); i++)
                s.Add(le1.section[i]);
            for (int i = 0; i < le2.section.Count(); i++)
                s.Add(le2.section[i]);
            bdd.section = s.ToArray<IntPair>();
            return bdd.section;
        }
        public void reCalcSects()
        {
            bdd.section = calcOccupiedSects();
        }
        public void addImage(string imgDir) { bdd.addImage(imgDir); }
        public void changeImage(int index, string imgDir) { bdd.changeImage(index, imgDir); }
        public void removeImage(int index) { bdd.removeImage(index); }
    }
    public class ShadowEntity : Entity, Drawable
    {
        //Class specific
        private PolygonEntity pe;
        public ShadowEntity(DoublePair[] point, Plane plane)
        {
            ls = new LightShape(point, System.Drawing.Color.Yellow, 1, 1);
            pe = new PolygonEntity(point, plane);
            bdd = new BasicDrawableData();
            bdd.plane = plane;
        }
        //Entity overrides
        public override void copy(Entity e)
        {
            throw new NotImplementedException();
        }
        public override string dataString()
        {
            throw new NotImplementedException();
        }
        public override string getInfo()
        {
            throw new NotImplementedException();
        }
        //Drawable interface
        private BasicDrawableData bdd;
        private LightShape ls;
        public LightShape lightShape { get { return ls; } set { ls = value; } }
        public int drawableTypeAsInt { get { return 5; } }
        public string drawableType { get { return "ShadowEntity"; } }
        public Plane plane { get { return bdd.plane; } }
        public IntPair[] section { get { return bdd.section; } }
        public bool moved { get { return bdd.moved; } set { bdd.moved = value; } }
        public void drawInfo(System.Drawing.Graphics gfx, ScreenData screen) { }
        public int indexInPlane { get { return bdd.indexInPlane; } set { bdd.indexInPlane = value; } }
        public void draw(System.Drawing.Graphics gfx, ScreenData screen)
        {
            pe.draw(gfx, screen);
        }
        public IntPair[] calcOccupiedSects()
        {
            bdd.section = SectionAlgorithms.polygon(pe.sidesAsLine(), false, bdd.plane.sectorSize);
            return bdd.section;
        }
        public void reCalcSects()
        {
            bdd.section = calcOccupiedSects();
        }
        public void addImage(string imgDir) { bdd.addImage(imgDir); }
        public void changeImage(int index, string imgDir) { bdd.changeImage(index, imgDir); }
        public void removeImage(int index) { bdd.removeImage(index); }
    }
}

/*Large comment #1
    Because of the way the program checks if two lines cross, you can get sections with negative coordinates, when the end of the line's
    coordinates are (0 , n * sectionSize) or (n * sectionSize , 0). Two lines cross if the ends of lines are the same, one of the ends of
    one line lies on the other line or when the ends of each line are on both sides of the other line.
    When the program checks through how many sections the line passes, the program can calculate extra sections, if one of the ends lies
    on a border of two sections. After that, the program scans which sections are passed. This is where the problem with negative indexes
    occures.
 */
/*Large comment #2
    To determine in which sections a line starts and ends, the program calculates the residal from the devision of the ends' coordinates
    with the size of a section. For example, if the coordinates of a point are:
    X == 3 * sectorSize + 20;
    Y == 5 * sectorSize + 50.
    That means, that the section is (3;5). But in some cases, one end lies on the border of two or four sections or:
    X == n1 * sectorSize and/or
    Y == n2 * sectorSize.
    Technically, the point should be in section (n1;n2), but things are more complicated than that. Now there is an extra row and colum with
    sections, that are potentially occupied. More specifically, sections (n1-1;n2) and (n1;n2-1) will confuse the program, causing it to miss
    a section, that is occupied by more than one ofe the line's pixels. By concept, a line cannot go through 4 neighbour sections (that are
    arranged in a square) at once, but the do-lines-cross algorithm will say otherwise (if the ends are the same, then the lines cross).
 */
/*if (screen.isPointInside(stp))
{
    gfx.DrawString(scrLn.start().x().ToString() + " " + scrLn.start().y().ToString(),
            new System.Drawing.Font("Consolas", 12),
            new System.Drawing.SolidBrush(System.Drawing.Color.Gray),
            st.a, st.b + 12);
    gfx.DrawString(scrLn.end().x().ToString() + " " + scrLn.end().y().ToString(),
            new System.Drawing.Font("Consolas", 12),
            new System.Drawing.SolidBrush(System.Drawing.Color.Gray),
            st.a, st.b + 24);
}
if (screen.isPointInside(enp))
{
    gfx.DrawString(scrLn.start().x().ToString() + " " + scrLn.start().y().ToString(),
            new System.Drawing.Font("Consolas", 12),
            new System.Drawing.SolidBrush(System.Drawing.Color.Green),
            en.a + 36, en.b);
    gfx.DrawString(scrLn.end().x().ToString() + " " + scrLn.end().y().ToString(),
            new System.Drawing.Font("Consolas", 12),
            new System.Drawing.SolidBrush(System.Drawing.Color.Green),
            en.a + 48, en.b);
}*/
/*Dumb line entity copy sequence...
string[] ent = e.getProperties();
char[] igChar = { ' ' };
//extracting coordinates
string[] coords = ent[0].Split(igChar, StringSplitOptions.RemoveEmptyEntries);
ln.setStart(Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1]));
ln.setEnd(Convert.ToInt32(coords[2]), Convert.ToInt32(coords[3]));
//extracting color
string[] clr = ent[1].Split(igChar, StringSplitOptions.RemoveEmptyEntries);
_color = System.Drawing.Color.FromArgb(
    Convert.ToInt32(clr[0]),
    Convert.ToInt32(clr[1]),
    Convert.ToInt32(clr[2]),
    Convert.ToInt32(clr[3]));
//extracting image
setImage(ent[2]);
*/
/*Dumb point entity copy sequence...
string[] ent = e.getProperties();
char[] igChar = { ' ' };
//extracting coordinates
string[] coords = ent[0].Split(igChar, StringSplitOptions.RemoveEmptyEntries);
_coords.setx(Convert.ToInt32(coords[0]));
_coords.sety(Convert.ToInt32(coords[1]));
//extracting color
string[] clr = ent[1].Split(igChar, StringSplitOptions.RemoveEmptyEntries);
_color = System.Drawing.Color.FromArgb(
    Convert.ToInt32(clr[0]),
    Convert.ToInt32(clr[1]),
    Convert.ToInt32(clr[2]),
    Convert.ToInt32(clr[3]));
//extracting image
setImage(ent[2]);
*/
//drawing methods
//public abstract int drawableTypeAsInt { get; }
//public abstract string drawableType { get; }
//protected Plane bddplane; public Plane plane { get { return bddplane; } }//the plane the entity belongs to
//protected int bddindex; public int indexInPlane { get { return bddindex; } }//index in the plane
//protected IntPair[] bddsection;//located in these sections; will be used when objects move
//public bool moved;//if the entity has moved
//public void setIndexInPlane(int ind) { bddindex = ind; }
//public abstract void draw(System.Drawing.Graphics gfx, Basis basis, ScreenData screen);
//public abstract void drawInfo(System.Drawing.Graphics gfx, Basis basis, ScreenData screen, int id);
//protected abstract IntPair[] _locatedInSections(int sectorSize);//calculates which sections are the occupied
//public IntPair[] calcOccupiedSects(int sectorSize)//sets and returns which sections are the occupied sections
//{
//    bddsection = _locatedInSections(sectorSize);
//    return bddsection;
//}
//public IntPair[] locatedInSections() { return bddsection; }
//public void reCalcSects(int sectorSize)
//{
//    IntPair[] oldSect = bddsection;
//    bddsection = _locatedInSections(sectorSize);
//    bddplane.changeEntitySections(bddindex, bddsection, oldSect);
//}
