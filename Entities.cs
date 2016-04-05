using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace VectorBasedLinesEngine
{
    delegate void Action(Plane plane, Entity e);//in case you want to do some fancy stuff; yes the action can also have an effect on other entites
    abstract class Entity //generic entity, based on that you should be able to build points, lines, polygons or other stuff
    {
        protected char _type;
        protected Plane plane;//the plane the entity belongs to
        protected int index;//index in the plane
        protected IntPair[] section;//located in these sections; will be used when objects move
        public bool moved;//if the entity has moved
        public Action method;//the method, that is going to do fancy stuff
       // private Object actionData;//data, that is going to be used by the action method
        private AutoResetEvent block;//to let the method make one cycle per frame
        private Thread thread;
        private int cycles;
        private bool _stopAction = false;
        public void stopAction() { _stopAction = true; }
        public void setBlock(AutoResetEvent block) { this.block = block; }
        public void setAction(Action method, int cycles, AutoResetEvent block)
        {
            this.method = method;
            this.cycles = cycles;
            thread = new Thread(() => action());
        }
        public void launchAction()
        {
            if (method != null)
                thread.Start();
        }
        private void action()
        {
            if (cycles >= 1)
                for (int i = 0; i < cycles && _stopAction == false; i++)
                {
                    method(plane, this);
                    block.WaitOne();
                }
            else
                while (_stopAction == false)
                {
                    method(plane, this);
                    block.WaitOne();
                }
        }
        public abstract void drawInfo(System.Drawing.Graphics gfx, Basis basis, ScreenData screen, int id);
        public abstract string[] getProperties();//gets a string of all the properties of the entity
        public abstract void copy(Entity e);//copies the properties of the argument entity
        public char type() { return _type; }
        public void setIndexInPlane(int ind) { index = ind; }
        public int indexInPlane() { return index; }
        public abstract void draw(System.Drawing.Graphics gfx, Basis basis, ScreenData screen);
        public abstract IntPair[] _locatedInSections(int sectorSize);//calculates which sections are the occupied section
        public IntPair[] locatedInSections(int sectorSize)//sets and returns which sections are the occupied section
        {
            section = _locatedInSections(sectorSize);
            return section;
        }
        public IntPair[] locatedInSections() { return section; }
        public void reCalcSects()
        {
            IntPair[] oldSect = section;
            section = _locatedInSections(plane.sectorSize());
            plane.changeEntitySections(index, section, oldSect);
        }
    }
    class PointEntity : Entity
    {
        private Point _coords;
        private System.Drawing.Color _color;
        private string imgDir;
        private System.Drawing.Bitmap _image;
        public void setCoords(double x, double y)
        {
            _coords.setx(x);
            _coords.sety(y);
            moved = true;
        }
        public Point coordinates() { return new Point(_coords.x(), _coords.y()); }
        public void setColor(System.Drawing.Color color) { _color = color; }
        public void setImage(string image)
        {
            if (image.Equals("null") == false)
                _image = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(image);
            imgDir = image;
        }
        public override void drawInfo(System.Drawing.Graphics gfx, Basis basis, ScreenData screen, int id)
        {
            IntPair res = _coords.screenCoords(basis);
            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black);
            if (isInScreen(res.a, res.b, screen))
            {
                gfx.DrawLine(pen, res.a, res.b, res.a + 3, res.b + 3);
                gfx.DrawLine(pen, res.a - 1, res.b + 1, res.a - 3, res.b + 3);
                gfx.DrawLine(pen, res.a + 1, res.b - 1, res.a + 3, res.b - 3);
                gfx.DrawLine(pen, res.a - 1, res.b - 1, res.a - 3, res.b - 3);
            }
            gfx.DrawString(id + " " + _coords.x().ToString() + " " + _coords.y().ToString(),
                            new System.Drawing.Font("Consolas", 12),
                            new System.Drawing.SolidBrush(System.Drawing.Color.Black),
                            res.a, res.b);
        }
        public override string[] getProperties()
        {
            string[] res = new string[3];
            res[0] = _coords.x() + " " + _coords.y();//the type is not returned, because there is already a method for that
            res[1] = _color.A + " " + _color.R + " " + _color.G + " " + _color.B;
            res[2] = imgDir;
            return res;
        }
        public override void copy(Entity e)
        {
            if (e.type() == _type)
            {
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
            }
        }
        //==============================================================================================
        private void setStuff(System.Drawing.Color color, string image, int x, int y, Plane plane)
        {
            _type = 'p';
            this.plane = plane;
            _coords = new Point();
            _coords.setCoords(x, y);
            setImage(image);
            _color = color;
        }
        public PointEntity(Plane plane)                                           { setStuff(System.Drawing.Color.FromArgb(0, 0, 0), null,  0, 0, plane); }
        public PointEntity(System.Drawing.Color color, Plane plane)               { setStuff(color, null, 0, 0, plane); }
        public PointEntity(string image, Plane plane)                             { setStuff(System.Drawing.Color.FromArgb(0, 0, 0), image, 0, 0, plane); }
        public PointEntity(int x, int y, Plane plane)                             { setStuff(System.Drawing.Color.FromArgb(0, 0, 0), null, x, y, plane); }
        public PointEntity(System.Drawing.Color color, int x, int y, Plane plane) { setStuff(color, null, x, y, plane); }
        public PointEntity(string image, int x, int y, Plane plane)               { setStuff(System.Drawing.Color.FromArgb(0, 0, 0), image, x, y, plane); }
        public PointEntity(System.Drawing.Color color, string image, IntPair c, Plane plane) { setStuff(color, image, c.a, c.b, plane); }
        //==============================================================================================
        private bool isInScreen(int x, int y, ScreenData screen)
        {
            lock (_image)
                if (_image != null)
                {
                    if (x >= 0 - _image.Width / 2 && x < screen.width() + _image.Width / 2 &&
                        y >= 0 - _image.Height / 2 && y < screen.height() + _image.Height / 2)
                        return true;
                }
                else
                    return screen.isPointInside(_coords);
            return false;
        }
        public override void draw(System.Drawing.Graphics gfx, Basis basis, ScreenData screen)
        {
            IntPair res = _coords.screenCoords(basis);
            int resx = res.a;
            int resy = res.b;
            lock (_image)
                if (isInScreen(resx, resy, screen))
                    if (_image != null)
                        gfx.DrawImage(_image,
                            resx - _image.Width / 2,
                            resy - _image.Height / 2,
                            _image.Width, _image.Height);//plus half of the height and width of the image to place the point in the center of the image
                    else
                        gfx.DrawLine(new System.Drawing.Pen(_color), resx, resy, resx + 1, resy + 1);
        }
        public override IntPair[] _locatedInSections(int sectorSize)
        {
            IntPair intCoords = new IntPair((int)_coords.x(), (int)_coords.y());//coordinates in int, a few pixels difference is not going to make much of a difference
            List<IntPair> res0 = new List<IntPair>();
            int entRad;
            lock (_image)
                entRad = (int)(Math.Sqrt(_image.Width * _image.Width + _image.Height * _image.Height) / 2);//radius of the entity, if it is going to rotate along with the screen
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
    }
    class LineEntity : Entity
    {
        private Line ln;
        private System.Drawing.Color _color;
        private System.Drawing.Bitmap _image;//it will be used somewhere in the future
        private string imgDir;
        public override void drawInfo(System.Drawing.Graphics gfx, Basis basis, ScreenData screen, int id)
        {
            System.Drawing.Pen pen = new System.Drawing.Pen(_color);
            IntPair st = ln.start().screenCoords(basis);
            IntPair en = ln.end().screenCoords(basis);
            Line scrLn = new Line((double)st.a, (double)st.b, (double)en.a, (double)en.b);
            //scrLn = scrLn.lineToDraw(basis, screen);
            //gfx.DrawString(_color.A + " " + _color.R + " " + _color.G + " " + _color.B,
            //        new System.Drawing.Font("Consolas", 12),
            //        new System.Drawing.SolidBrush(System.Drawing.Color.Gray),
            //        st.a, st.b);
            //gfx.DrawString(scrLn.start().x().ToString() + " " + scrLn.start().y().ToString(),
            //        new System.Drawing.Font("Consolas", 12),
            //        new System.Drawing.SolidBrush(System.Drawing.Color.Gray),
            //        st.a, st.b + 12);
            //gfx.DrawString(scrLn.end().x().ToString() + " " + scrLn.end().y().ToString(),
            //        new System.Drawing.Font("Consolas", 12),
            //        new System.Drawing.SolidBrush(System.Drawing.Color.Gray),
            //        st.a, st.b + 24);
            gfx.DrawString(id.ToString(),
                    new System.Drawing.Font("Consolas", 12),
                    new System.Drawing.SolidBrush(System.Drawing.Color.Gray),
                    st.a, st.b);
            String sections = "";
            for (int i = 0; i < section.Count(); i++)
                sections += section[i].a + "-" + section[i].b + "; ";
            gfx.DrawString(sections,
                    new System.Drawing.Font("Consolas", 12),
                    new System.Drawing.SolidBrush(_color),
                    (st.a + en.a) / 2, (st.b + en.b) / 2);
            gfx.DrawLine(new System.Drawing.Pen(_color), st.a + 3, st.b + 3, st.a - 3, st.b - 3);
            gfx.DrawLine(new System.Drawing.Pen(_color), en.a + 3, en.b + 3, en.a - 3, en.b - 3);
        }
        public override string[] getProperties()
        {
            string[] res = new string[3];
            res[0] = ln.start().x() + " " + ln.start().y() + " " + ln.end().x() + " " + ln.end().y();//the type is not returned, because there is already a method for that
            res[1] = _color.A + " " + _color.R + " " + _color.G + " " + _color.B;
            res[2] = imgDir;
            return res;
        }
        public override void copy(Entity e)
        {
            if (e.type() == _type)
            {
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
            }
        }
        //==============================================================================================
        public void setImage(string img)
        {
            imgDir = img;
            if (img.Equals("null") == false)
                _image = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(img);
        }
        public void setStartCoords(double x, double y) { ln.setStart(x, y); moved = true; }
        public void setEndCoords(double x, double y) { ln.setEnd(x, y); moved = true; }
        public Line getLineCoords () { return new Line(ln.start(), ln.end()); }
        private void setStuff(IntPair st, IntPair en, System.Drawing.Color clr, string img, Plane plane)
        {
            _type = 'l';
            this.plane = plane;
            ln = new Line(st.a, st.b, en.a, en.b);
            _color = clr;
            setImage(img);
        }
        //Should I simply use the constructors that don't need the IntPair arguments?
        public LineEntity(Plane plane) { setStuff(new IntPair(0, 0), new IntPair(0, 0), System.Drawing.Color.Black, null, plane); }
        public LineEntity(IntPair st, IntPair en, Plane plane) { setStuff(st, en, System.Drawing.Color.Black, null, plane); }
        public LineEntity(int stx, int sty, int enx, int eny, Plane plane) { setStuff(new IntPair(stx, sty), new IntPair(enx, eny), System.Drawing.Color.Black, null, plane); }
        public LineEntity(string img, IntPair st, IntPair en, Plane plane) { setStuff(st, en, System.Drawing.Color.Black, img, plane); }
        public LineEntity(string img, int stx, int sty, int enx, int eny, Plane plane) { setStuff(new IntPair(stx, sty), new IntPair(enx, eny), System.Drawing.Color.Black, img, plane); }
        public LineEntity(System.Drawing.Color clr, IntPair st, IntPair en, Plane plane) { setStuff(st, en, clr, null, plane); }
        public LineEntity(System.Drawing.Color clr, int stx, int sty, int enx, int eny, Plane plane) { setStuff(new IntPair(stx, sty), new IntPair(enx, eny), clr, null, plane); }
        public LineEntity(string img, System.Drawing.Color clr, IntPair st, IntPair en, Plane plane) { setStuff(st, en, clr, img, plane); }
        //==============================================================================================
        private bool isInScrren(Basis basis, ScreenData screen)
        {
            IntPair scrSt = ln.start().screenCoords(basis);
            IntPair scrEn = ln.end().screenCoords(basis);
            if ((scrSt.a >= 0 && scrSt.a <= screen.width() && scrSt.b >= 0 && scrSt.b <= screen.height()) ||
                (scrEn.a >= 0 && scrEn.a <= screen.width() && scrEn.b >= 0 && scrEn.b <= screen.height()))
                return true;//if one of the ends is visible by the screen
            Line scrLn = new Line(scrSt.a, scrSt.b, scrEn.a, scrEn.b);
            Line left =  new Line(screen.upLeftCorner(),    screen.downLeftCorner());
            Line right = new Line(screen.downRightCorner(), screen.upRightCorner());
            Line up =    new Line(screen.upRightCorner(),   screen.upLeftCorner());
            Line down =  new Line(screen.downRightCorner(), screen.downLeftCorner());
            if (scrLn.crosses(left)) return true;
            if (scrLn.crosses(right)) return true;
            if (scrLn.crosses(up)) return true;
            if (scrLn.crosses(down)) return true;
            return false;
        }
        public override void draw(System.Drawing.Graphics gfx, Basis basis, ScreenData screen)
        {
            if (isInScrren(basis, screen))
            {
                System.Drawing.Pen pen = new System.Drawing.Pen(_color);
                IntPair st = ln.start().screenCoords(basis);
                IntPair en = ln.end().screenCoords(basis);
                Line scrLn = new Line((double)st.a, (double)st.b, (double)en.a, (double)en.b);
                gfx.DrawLine(pen,
                    (int)Math.Round(scrLn.start().x()),
                    (int)Math.Round(scrLn.start().y()),
                    (int)Math.Round(scrLn.end().x()),
                    (int)Math.Round(scrLn.end().y()));
            }
        }
        private Line sectionSideLine(IntPair sect, char verhor, int dir, int sectSize)
        {
            char side;//determining which side is going to be checked
            if (verhor == 'h')//vertical or horizontal
                if      (dir < 0) side = 'l';
                else if (dir > 0) side = 'r';
                else    side = 'n';
            else if (verhor == 'v')
                if      (dir < 0) side = 'u';
                else if (dir > 0) side = 'd';
                else    side = 'n';
            else
                side = 'n';//try not to put arguments, that can allow the program to set the dir variable to 'n'
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
        public override IntPair[] _locatedInSections(int sectorSize)
        {
            IntPair sts = new IntPair((int)ln.start().x() / sectorSize, (int)ln.start().y() / sectorSize);//start section
            IntPair ens = new IntPair((int)ln.end().x() / sectorSize,   (int)ln.end().y() / sectorSize);//end section
            //number of sections the line passes
            int passedSections;
            if (sts.a == ens.a && sts.b == ens.b)
                passedSections = 1;
            else
                passedSections = Math.Max(Math.Abs(sts.a - ens.a), Math.Abs(sts.b - ens.b)) +//if we have colums or rows of passed sections, add their total lengths
                                 Math.Min(Math.Abs(sts.a - ens.a), Math.Abs(sts.b - ens.b)) + 1;//and add the number of colums/rows
            if (passedSections == 1)//it will be 1 if the starting and ending sections are the same
            {
                IntPair[]  res = new IntPair[passedSections];
                res[0] = new IntPair(sts.a, sts.b);
                return res;
            }
            List<IntPair> res0 = new List<IntPair>();
            int verDir;
            int horDir;
            if      (sts.a > ens.a)  horDir = -1;//move left
            else if (sts.a == ens.a) horDir = 0;//stay on the colum
            else   /*sts.a < ens.a*/ horDir = 1;//move right
            if      (sts.b > ens.b)  verDir = -1;//move up
            else if (sts.b == ens.b) verDir = 0;//stay on the row
            else   /*sts.b < ens.b*/ verDir = 1;//move down
            IntPair crrSect = new IntPair(sts.a, sts.b);
            for (int i = 0; i < passedSections; i++)
            {
                if (crrSect.a >= 0 && crrSect.b >= 0)//Refer to Large comment #1
                res0.Add(new IntPair(crrSect));//from this section it is cheked which is the next passed section
                if (verDir != 0 && ln.crosses(sectionSideLine(crrSect, 'v', verDir, sectorSize)))
                    crrSect.b += verDir;
                else if (horDir != 0 && ln.crosses(sectionSideLine(crrSect, 'h', horDir, sectorSize)))
                    crrSect.a += horDir;
            }
            //debug
            for (int i = 0; i < res0.Count; i++)
                if (res0[i].a < 0 || res0[i].b < 0)
                { int k; }
            return res0.ToArray();
        }
    }
}

//if (screen.isPointInside(stp))
//{
//    gfx.DrawString(scrLn.start().x().ToString() + " " + scrLn.start().y().ToString(),
//            new System.Drawing.Font("Consolas", 12),
//            new System.Drawing.SolidBrush(System.Drawing.Color.Gray),
//            st.a, st.b + 12);
//    gfx.DrawString(scrLn.end().x().ToString() + " " + scrLn.end().y().ToString(),
//            new System.Drawing.Font("Consolas", 12),
//            new System.Drawing.SolidBrush(System.Drawing.Color.Gray),
//            st.a, st.b + 24);
//}
//if (screen.isPointInside(enp))
//{
//    gfx.DrawString(scrLn.start().x().ToString() + " " + scrLn.start().y().ToString(),
//            new System.Drawing.Font("Consolas", 12),
//            new System.Drawing.SolidBrush(System.Drawing.Color.Green),
//            en.a + 36, en.b);
//    gfx.DrawString(scrLn.end().x().ToString() + " " + scrLn.end().y().ToString(),
//            new System.Drawing.Font("Consolas", 12),
//            new System.Drawing.SolidBrush(System.Drawing.Color.Green),
//            en.a + 48, en.b);
//}

/*Large comment #1
    Because of the way the program checks if two lines cross, you can get sections with negative coordinates, when the end of the line's
    coordinates are (0 , n * sectionSize) or (n * sectionSize , 0). Two lines cross if the ends of lines are the same, one of the ends of
    one line lies on the other line or when the ends of each line are on both sides of the other line.
    When the program checks through how many sections the line passes, the program can calculate extra sections, if one of the ends lies
    on a border of two sections. After that, the program scans which sections are passed. This is where the problem with negative indexes
    occures.
 */