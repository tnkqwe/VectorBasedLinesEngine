using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VectorBasedLinesEngine
{
    delegate void GenericMethod(Plane plane);
    class Plane
    {
        private Basis basis;//basis
        private int sectRows;//section rows
        private int sectCols;//section colums
        private int sectSize;//width and height of each sector
        private double minZoom = 0.5;
        //private double maxZoom = 2.0;
        public double zoom
        {
            get { return basis.zoom; }
            set { basis.zoom = value; }
        }
        //diversion in the basis vectors, because why not: setting it should cause all coordinates to diverse from their original positions, use for screen effects
        //center diversion accessors
        public DoublePair centerDiversion
        {
            get { return basis.centerDiversion; }
            set { basis.centerDiversion = new DoublePair(value.a, value.b); }
        }
        public double centerDiversionX
        {
            get { return centerDiversion.a; }
            set { basis.centerDiversion = new DoublePair(value, basis.centerDiversion.b); }
        }
        public double centerDiversionY
        {
            get { return centerDiversion.b; }
            set { basis.centerDiversion = new DoublePair(basis.centerDiversion.a, value); }
        }
        //X vector diversion accessors
        public DoublePair xVectorDiversion
        {
            get { return basis.xVectorDiversion; }
            set { basis.xVectorDiversion = new DoublePair(value.a, value.b); }
        }
        public double xVectorDiversionX
        {
            get { return xVectorDiversion.a; }
            set { basis.xVectorDiversion = new DoublePair(value, basis.xVectorDiversion.b); }
        }
        public double xVectorDiversionY
        {
            get { return xVectorDiversion.b; }
            set { basis.xVectorDiversion = new DoublePair(basis.xVectorDiversion.a, value); }
        }
        //Y vector diversion accessors
        public DoublePair yVectorDiversion
        {
            get { return basis.yVectorDiversion; }
            set { basis.yVectorDiversion = new DoublePair(value.a, value.b); }
        }
        public double yVectorDiversionX
        {
            get { return yVectorDiversion.a; }
            set { basis.yVectorDiversion = new DoublePair(value, basis.yVectorDiversion.b); }//we are changing the X value, the Y value remain the same
        }
        public double yVectorDiversionY
        {
            get { return yVectorDiversion.b; }
            set { basis.yVectorDiversion = new DoublePair(basis.yVectorDiversion.a, value); }//we are changing the Y value, the X value remain the same
        }
        //Refer to Large Comment #1 for more info on the sections
        private List<List<SortedSet<int>>> entInd;//[sector row][sector column]
        private List<Drawable> _entity; public int entityCount { get { return _entity.Count; } }
        public Entity entity(int id) { return _entity[id] as Entity; }
        //private System.Threading.Thread hart;//because the WinForm forms can be manipulated only from the main thread, the hart must be called by a timer in the game's code
        //private List<System.Threading.AutoResetEvent> block;
        private Object entLock;
        //private bool hartStopped = false;
        public bool debug = false;
        private GenericMethod movement;//a method for the "camera" movement - the movement of the basis relative to the screen
        private bool stopMoveThread = false;
        public void stopMovementThread() { stopMoveThread = true; }
        private List<GenericMethod> method;
        private System.Threading.AutoResetEvent movementBlock;//works in a ceperate thread to give more flexibility
        private System.Threading.Thread moveThread;
        private void moveCycle()
        {
            while (stopMoveThread == false)
            {
                movementBlock.WaitOne();
                if (movement != null)
                    movement(this);
            }
        }
        //the basis' center coordinates are relative to the basis of the screen's upper left corner
        private void setStuff(int sc, int sr, int scrWd, int scrHt, GenericMethod mm, System.Threading.AutoResetEvent mb)
        {
            sectCols = sc;
            sectRows = sr;
            sectSize = ((int)(System.Math.Sqrt(Math.Pow((double)scrHt / minZoom, 2) + Math.Pow((double)scrWd / minZoom, 2))) / 100) * 100 + 100;
            basis = new Basis(0, 0, new ScreenData(scrWd, scrHt));
            _entity = new List<Drawable>();
            entInd = new List<List<SortedSet<int>>>();
            for (int row = 0; row < sr; row++)
            {
                List<SortedSet<int>> temp = new List<SortedSet<int>>();
                for (int col = 0; col < sc; col++)
                    temp.Add(new SortedSet<int>());
                entInd.Add(temp);
            }
            //stopHart = false;
            //block = new List<System.Threading.AutoResetEvent>();
            entLock = new Object();
            //fps = 60;
            List<IntPair> coord = new List<IntPair>();
            List<string> imgDir = new List<string>();
            List<System.Drawing.Color> color = new List<System.Drawing.Color>();
            List<Point> point = new List<Point>();
            List<LineEntity> stdLine = new List<LineEntity>();
            this.movement = mm;//setting the movement method
            method = new List<GenericMethod>();
            this.movementBlock = mb;
            moveThread = new System.Threading.Thread(() => moveCycle());
            moveThread.Start();
        }
        public Plane(int sectionCols, int sectionRows, int screenWidth, int screenHeight, GenericMethod moveMethod, System.Threading.AutoResetEvent moveBlock) {
            setStuff(sectionCols, sectionRows, screenWidth, screenHeight, moveMethod, moveBlock); }
        public Plane(int basisCenterX, int basisCenterY, int sectionCols, int sectionRows, int screenWidth, int screenHeight, GenericMethod moveMethod, System.Threading.AutoResetEvent moveBlock){
            setStuff(sectionCols, sectionRows, screenWidth, screenHeight, moveMethod, moveBlock); }
        public void addEntity(Entity e)
        {
            Drawable d = e as Drawable;
            if (d != null)
            {
                IntPair[] sect = d.calcOccupiedSects(sectSize);//the pair is returned in the format (colum, row), or sect[n].a is the colum and sect[n].b is the row
                for (int i = 0; i < sect.Length; i++)
                    if (sect[i].a >= 0 && sect[i].b >= 0 && sect[i].a < sectCols && sect[i].b < sectRows)
                        entInd[sect[i].b][sect[i].a].Add(_entity.Count);
                d.indexInPlane = _entity.Count;
                _entity.Add(d);
            }
        }
        public void changeEntitySections(int entInd, IntPair[] newSect, IntPair[] oldSect)//called from the entites
        {//what are the entity's new sections is calculated in each entity's coordinates changing methods
            lock(entLock)
            {
                for (int i = 0; i < oldSect.Count(); i++)//remove the entity's index from the old set of sections
                    this.entInd[oldSect[i].b][oldSect[i].a].Remove(entInd);
                for (int i = 0; i < newSect.Count(); i++)//add the entity's index to the new set of sections
                    this.entInd[newSect[i].b][newSect[i].a].Add(entInd);
            }
        }
        public void rotate(double deg, ScreenData screen)
        {
            basis.rotate(deg, new DoublePair(screen.center.x, screen.center.y));//rotating
        }
        public void move(double pxx, double pxy)
        {
            basis.move(pxx, pxy);
            //if (basis.x().x == basis.y().x || basis.x().y == basis.y().y) throw new SystemException("Basis vectors have become the same!");
        }
        private bool sectAdded(List<IntPair> sector, IntPair sect)
        {
            for (int i = 0; i < sector.Count; i++)
                if (sector[i].a == sect.a && sector[i].b == sect.b)
                    return true;
            return false;
        }
        private void addSects(List<IntPair> sector, IntPair ul, IntPair ur, IntPair dl, IntPair dr, System.Drawing.Graphics gfx, ScreenData scr)
        {
            //int zSectSize = (int)Math.Round((double)sectSize / _zoom);
            IntPair uls = new IntPair(ul.a / sectSize, ul.b / sectSize);
            IntPair urs = new IntPair(ur.a / sectSize, ur.b / sectSize);
            IntPair dls = new IntPair(dl.a / sectSize, dl.b / sectSize);
            IntPair drs = new IntPair(dr.a / sectSize, dr.b / sectSize);
            if (ul.a >= 0 && ul.b >= 0 && uls.a < sectCols && uls.b < sectRows)
                sector.Add(uls);
            if (ur.a >= 0 && ur.b >= 0 && urs.a < sectCols && urs.b < sectRows && sectAdded(sector, urs) == false)
                sector.Add(urs);
            if (dl.a >= 0 && dl.b >= 0 && dls.a < sectCols && dls.b < sectRows && sectAdded(sector, dls) == false)
                sector.Add(dls);
            if (dr.a >= 0 && dr.b >= 0 && drs.a < sectCols && drs.b < sectRows && sectAdded(sector, drs) == false)
                sector.Add(drs);
            if (debug)
            {
                Line ln0 = new Line(ul, ur); ln0 = new Line(ln0.start.intScrCoords(basis, scr), ln0.end.intScrCoords(basis, scr));
                Line ln1 = new Line(ur, dr); ln1 = new Line(ln1.start.intScrCoords(basis, scr), ln1.end.intScrCoords(basis, scr));
                Line ln2 = new Line(dl, dr); ln2 = new Line(ln2.start.intScrCoords(basis, scr), ln2.end.intScrCoords(basis, scr));
                Line ln3 = new Line(dl, ul); ln3 = new Line(ln3.start.intScrCoords(basis, scr), ln3.end.intScrCoords(basis, scr));
                System.Drawing.Pen gray = new System.Drawing.Pen(System.Drawing.Color.Gray);
                gfx.DrawLine(gray, (int)(ln0.start.x), (int)(ln0.start.y), (int)(ln0.end.x), (int)(ln0.end.y));
                gfx.DrawLine(gray, (int)(ln1.start.x), (int)(ln1.start.y), (int)(ln1.end.x), (int)(ln1.end.y));
                gfx.DrawLine(gray, (int)(ln2.start.x), (int)(ln2.start.y), (int)(ln2.end.x), (int)(ln2.end.y));
                gfx.DrawLine(gray, (int)(ln3.start.x), (int)(ln3.start.y), (int)(ln3.end.x), (int)(ln3.end.y));
            }
        }
        public void addMethodToHart(GenericMethod method) { this.method.Add(method); }
        public void draw(System.Drawing.Graphics gfx, ScreenData screen)
        {
            //Refer to large comment #2 for info on how the algorithm detects which section the screen occupies
            List<IntPair> sector = new List<IntPair>();//sectors the screen occupies
            IntPair scrCent = screen.center.intPlaneCoords(basis, true, true);//screen center's coordinates according to the plane's basis
            IntPair scrSect = new IntPair(scrCent.a / sectSize, scrCent.b / sectSize);//column, row; in which sector is the center of the screen located
            Point meetPoint = new Point(//nearest point, where four sectors border
                Math.Round(Math.Round((double)scrCent.a / (double)sectSize)) * sectSize,//the coordinates of the top left corner of each
                Math.Round(Math.Round((double)scrCent.b / (double)sectSize)) * sectSize);//sector are sectCol * sectSize and sectRow * sectSize
            IntPair meetPointSect = new IntPair((int)(meetPoint.x) / sectSize, (int)(meetPoint.y) / sectSize);
            //meetPoint = meetPoint.zoomedCoords(scrCent, _zoom);
            if (screen.isPointInside(meetPoint.intScrCoords(basis, screen)))
            {
                sector.Add(new IntPair(meetPointSect.a, meetPointSect.b));
                if (meetPointSect.a - 1 > -1)
                    sector.Add(new IntPair(meetPointSect.a - 1, meetPointSect.b));
                if (meetPointSect.b - 1 > -1)
                    sector.Add(new IntPair(meetPointSect.a, meetPointSect.b - 1));
                if (meetPointSect.a - 1 > -1 && meetPointSect.b - 1 > -1)
                    sector.Add(new IntPair(meetPointSect.a - 1, meetPointSect.b - 1));
            }
            else
            {
                Point upLt = new Point(0, 0); upLt = new Point(upLt.intPlaneCoords(basis, true, true));// upLt = upLt.zoomedCoords(scrCent, 1 / _zoom);
                Point upRt = new Point(screen.width, 0); upRt = new Point(upRt.intPlaneCoords(basis, true, true));// upRt = upRt.zoomedCoords(scrCent, 1 / _zoom);
                Point dnLt = new Point(0, screen.height); dnLt = new Point(dnLt.intPlaneCoords(basis, true, true));// dnLt = dnLt.zoomedCoords(scrCent, 1 / _zoom);
                Point dnRt = new Point(screen.width, screen.height); dnRt = new Point(dnRt.intPlaneCoords(basis, true, true));// dnRt = dnRt.zoomedCoords(scrCent, 1 / _zoom);
                IntPair ul = new IntPair(upLt.x, upLt.y);//up left angle
                IntPair ur = new IntPair(upRt.x, upRt.y);//up right
                IntPair dl = new IntPair(dnLt.x, dnLt.y);//down left
                IntPair dr = new IntPair(dnRt.x, dnRt.y);//down right
                addSects(sector, ul, ur, dl, dr, gfx, screen);
            }
            lock (entLock)
            {
                //tracking of what is already drawn and what not
                SortedSet<int> forDrawing = new SortedSet<int>();
                for (int sect = 0; sect < sector.Count; sect++)
                    for (int i = 0; i < entInd[sector[sect].b][sector[sect].a].Count; i++)
                        if (forDrawing.Add(entInd[sector[sect].b][sector[sect].a].ElementAt(i)))
                            _entity[entInd[sector[sect].b][sector[sect].a].ElementAt(i)].draw(gfx, basis, screen);
            }
            //==============
            if (debug)
            {
                for (int i = 0; i < _entity.Count; i++)
                    if (true/*if the entity is in the visible sections*/)
                        _entity[i].drawInfo(gfx, basis, screen, i);
                for (int c = 0; c < entInd.Count(); c++)
                    for (int r = 0; r < entInd[c].Count; r++)
                    {
                        int[] set = new int[entInd[c][r].Count];
                        entInd[c][r].CopyTo(set);
                        String str = "";
                        for (int i = 0; i < set.Count(); i++ )
                        {
                            str += set[i] + " ";
                            if (i % 8 == 0)
                                str += "\n";
                        }
                        Point pnt0 = new Point(c * sectSize + sectSize / 2, r * sectSize + sectSize / 2);
                        pnt0.setCoords(pnt0.intScrCoords(basis, screen).a, pnt0.intScrCoords(basis, screen).b);
                        gfx.DrawString(str,
                        new System.Drawing.Font("Consolas", 12),
                        new System.Drawing.SolidBrush(System.Drawing.Color.Blue),
                        (int)pnt0.x, (int)pnt0.y);
                    }
                //Point pnt = new Point(scrCent.a, scrCent.b);
                Point pnt = scrCent;
                IntPair pntScr = new IntPair(pnt.intScrCoords(basis, screen));
                System.Drawing.Pen gray = new System.Drawing.Pen(System.Drawing.Color.Gray);
                gfx.DrawLine(gray, pntScr.a - 5, pntScr.b, pntScr.a + 5, pntScr.b);
                gfx.DrawLine(gray, pntScr.a, pntScr.b - 5, pntScr.a, pntScr.b + 5);
                gfx.DrawString(Math.Round((double)scrCent.a / (double)sectSize) + " " + Math.Round((double)scrCent.b / (double)sectSize),
                    new System.Drawing.Font("Consolas", 12),
                    new System.Drawing.SolidBrush(System.Drawing.Color.Blue),
                    pntScr.a, pntScr.b);
                System.Drawing.Pen red = new System.Drawing.Pen(System.Drawing.Color.Red);
                System.Drawing.Pen blue = new System.Drawing.Pen(System.Drawing.Color.Blue);
                for (int s = 0; s < sector.Count; s++)
                {
                    Point sectAngl = new Point(sector[s].a * sectSize, sector[s].b * sectSize);
                    Point sectX = new Point((sector[s].a + 1) * sectSize, sector[s].b * sectSize);
                    Point sectY = new Point(sector[s].a * sectSize, (sector[s].b + 1) * sectSize);
                    Line tmpLnA = new Line(sectAngl, sectX);
                    Line tmpLnB = new Line(sectAngl, sectY);
                    sectAngl = sectAngl.intScrCoords(basis, screen);// sectAngl = sectAngl.zoomedCoords(screen.center, _zoom);
                    sectX = sectX.intScrCoords(basis, screen);// sectX = sectX.zoomedCoords(screen.center, _zoom);
                    sectY = sectY.intScrCoords(basis, screen);// sectY = sectY.zoomedCoords(screen.center, _zoom);
                    gfx.DrawLine(red, (int)sectAngl.x, (int)sectAngl.y, (int)sectX.x, (int)sectX.y);//section borders
                    gfx.DrawLine(blue, (int)sectAngl.x, (int)sectAngl.y, (int)sectY.x, (int)sectY.y);
                    gfx.DrawString(tmpLnA.getEquasionString(),//equasions of section borders
                    new System.Drawing.Font("Consolas", 12),
                    new System.Drawing.SolidBrush(System.Drawing.Color.Red),
                    (int)(sectAngl.x + sectX.x) / 2 - 5, (int)(sectAngl.y + sectX.y) / 2 - 5);
                    gfx.DrawString(tmpLnB.getEquasionString(),//equasions of section borders
                    new System.Drawing.Font("Consolas", 12),
                    new System.Drawing.SolidBrush(System.Drawing.Color.Blue),
                    (int)(sectAngl.x + sectY.x) / 2 - 5, (int)(sectAngl.y + sectY.y) / 2 - 5);
                    String str = "";
                    for (int i = 0; i < sector.Count; i++)
                        str = str + sector[i].a + sector[i].b + "; ";
                    gfx.DrawString(str + "\n" + scrSect.a + scrSect.b,
                        new System.Drawing.Font("Consolas", 12),
                        new System.Drawing.SolidBrush(System.Drawing.Color.Red),
                        0, 0);
                }
                Point coorTxt = new Point(meetPoint.x + 10, meetPoint.y + 10);
                coorTxt = coorTxt.intScrCoords(basis, screen);
                gfx.DrawString(meetPoint.x + " " + meetPoint.y + " " + screen.isPointInside(meetPoint),
                    new System.Drawing.Font("Consolas", 12),
                    new System.Drawing.SolidBrush(System.Drawing.Color.Black),
                    (int)coorTxt.x - 5, (int)coorTxt.y - 5);
            }
        }
        public int sectorSize() { return sectSize; }
        public void doActions()
        {
            for (int i = 0; i < method.Count; i++ )
                if (method[i] != null)
                    method[i](this);//going through methods for doing other stuff
            //for (int i = 0; i < block.Count; i++)
            //    block[i].Set();//releasing the threads of all the entities
            //movementBlock.Set();//release the movement thread
            for (int i = 0; i < _entity.Count; i++)
                if (_entity[i].moved)
                    _entity[i].reCalcSects(sectSize);
        }
        //private void stopHart()
        //{
        //    for (int i = 0; i < _entity.Count; i++)
        //        _entity[i].stopAction();
        //    for (int i = 0; i < block.Count; i++)
        //        block[i].Set();
        //    hartStopped = true;
        //    movementBlock.Set();
        //}
        //public void setEntityWithAction(int entInd, Action method, int cycles)
        //{
        //    if (entInd < _entity.Count && entInd >= 0)
        //    {
        //        System.Threading.AutoResetEvent tmp = new System.Threading.AutoResetEvent(false);
        //        block.Add(tmp);
        //        _entity[entInd].setAction(method, cycles, block[block.Count - 1]);
        //        //_entity[entInd].setBlock(tmp);
        //    }
        //}
        //public void command(string cmd)
        //{
        //    if (cmd.Equals("stop")) { stopHart(); }
        //    else if (cmd.Equals("debug"))
        //    {
        //        if (debug) debug = false;
        //        else debug = true;
        //    }
        //}
        //public void enableEntityActions()
        //{
        //    for (int i = 0; i < _entity.Count; i++)
        //        _entity[i].launchAction();
        //}
    }
}

/*Large Comment #1
    At first, I planned on dividing the plane in sections, that are larger than the screen, but with roughly the same width / height ratios.
    If the screen is a square, in the worst case the screen would occupy 4 sections. But if the screen is a rectangle with a much longer width than height
    (or vice-versa), the screen would be able to occupy a lot more sections in the worse case. By using squares for sections, calculating which sections
    are occupied by the screen becomes much easier - in the worst case the screen can occupie only 4 sections. That is why I am restricting the sections to
    be squares, with sizes wider than the screen's diagonal.
 */
/*Large Comment #2
    The concept of finding which sections are visible by the screen is pretty simple, untill you have to start calculating the coordinates.
    If a point, where four sections border, is visible by the screen, that means, that all four sections are visible by the screen.
    Otherwise, the sections, that are visible by the screen, are the sections, where the angles of the screen are located, relative
    to the plane's basis.
 */

            //changeScreenSize(screen);
            //Refer to Large Comment #3 fo info on the stabilization
            //Point crrCent = new Point(screen.center.intPlaneCoords(basis, screen));//point with the screen center's current coordinates according to the plane's basis
            //DoublePair crrCentScr = crrCent.doubleScrCoords(basis, screen);//screen coordinates of the point according to the screen
            //basis.rotate(deg, new DoublePair(screen.center.x, screen.center.y));//rotating
            //DoublePair newCentScr = crrCent.doubleScrCoords(basis, screen);//the new screen coordinates of the point according to the screen
            //move(crrCentScr.a - newCentScr.a, crrCentScr.b - newCentScr.b);//moving with the pixels of the diversion
            //if (basis.x().x == basis.y().x || basis.x().y == basis.y().y) throw new SystemException("Basis vectors have become the same!");
/*Large Comment #3 (read last line in the comment)
    When rotating the basis, everty time the "rotation" variable goes above 360 or lower than 0, the value gets set to 0 or 360 respectively,
    and then it gets changed further, according to how much the basis must rotate. Check "Basis.cs" for more info. When doing that, the
    coordinates of the basis' center change and diverse with some pixels. That is why a stabilization algorithm is needed.
    Here I do the following things:
        Find the point, where the screen's center is located according to the plane's basis;
        Calculate where that point is according to the screen;
        Rotate the plane;
        Once more calculate where the point is according to the screen (rotating causing a diversion);
        Move the plane with the number of pixels of the diversion between the old screen center coordinates and new ones;
    However, that still causes some diversion and leaves 1px rocking. I may need a better, more accurate algorithm.
    Scrap all that, it is better not have any stabilization at all, but use double variables everywhere.
 */

/*IntPair pbscp = new IntPair();//point between sectors coordinates pair
if (scrSect.a * basis.vectorLength() - scrCent.a > 0)//the point is one of the bottom angles of the sector
    pbscp.a = (scrSect.a + 1) * basis.vectorLength();
else//the point is one of the top angles of the sector
    pbscp.a = scrSect.a * basis.vectorLength();
if (scrSect.b * basis.vectorLength() - scrCent.b > 0)//the point is one of the right angles of the sector
    pbscp.b = (scrSect.b + 1) * basis.vectorLength();
else//the point is one of the left angles of the sector
    pbscp.b = scrSect.b * basis.vectorLength();
Point pbs = new Point(pbscp.a, pbscp.b);//this is a point that is between four sectors
IntPair pbssc = new IntPair(pbs.screenCoords(basis));//the point's coordinates according to the screen's basis
if (pbssc.a >= 0 && pbssc.a <= screen.width() &&
    pbssc.b >= 0 && pbssc.b <= screen.height())//if the point is inside the screen, the screen occupies all four sections
{
    IntPair ul = new IntPair(pbs.x(),                        pbs.y());//up left angle
    IntPair ur = new IntPair(pbs.x() - basis.vectorLength(), pbs.y());//up right
    IntPair dl = new IntPair(pbs.x(),                        pbs.y() - basis.vectorLength());//down left
    IntPair dr = new IntPair(pbs.x() - basis.vectorLength(), pbs.y() - basis.vectorLength());//down right
    addSects(sector, ul, ur, dl, dr);
}
else
{
    Point upLt = new Point(0, 0);
    Point upRt = new Point(screen.width(), 0);
    Point dnLt = new Point(0, screen.height());
    Point dnRt = new Point(screen.width(), screen.height());
    IntPair ul = upLt.planeCoords(basis);//up left angle
    IntPair ur = upRt.planeCoords(basis);//up right
    IntPair dl = dnLt.planeCoords(basis);//down left
    IntPair dr = dnRt.planeCoords(basis);//down right
    //ugh...
    addSects(sector, ul, ur, dl, dr);
}*/
/*Large comment for the above deleted code: CODE HAS A LOGIC ERROR IN FINDING WHICH BORDER POINT TO CHECK
    Determining which sections the screen occupies is actually pretty simple (untill you reach the basis switching calculations) - if the screen can "see"
    a point where four sections meet, that means, that the screen occupies all four sections; if not, then the screen occupies the sections where the
    screen's angles are located. To determine which is the visible point where sections meet, I have used the screen's center to find which are the
    closest sections to the center. The point where thous sections and the section, where the screen's center is located, meet is the point that will be
    checked if it is visible by the screen. There may be a faster way, though.
 */

/*class IntNode
{
    public int val;
    public IntNode left;
    public IntNode right;
    public IntNode(int val) { this.val = val; }
    public void add(int val)
    {
        if (this.val > val)
        {
            if (left == null) left = new IntNode(val);
            else left.add(val);
        }
        else if (this.val < val)
        {
            if (right == null) right = new IntNode(val);
            else right.add(val);
        }
        //I am avoiding adding the same value multiple times
    }
}
class Sector
{
    private IntNode start;
    public void Add(int val)
    {
        add(val, start);
    }
    private void add(int val, IntNode node)
    {
        if (node == null) node = new IntNode(val);
        else node.add(val);
    }
    private bool contains(int val, IntNode node)
    {
        if (node == null) return false;
        if (node.val == val) return true;
        else
        {
            if (contains(val, node.left))
                return true;
            return contains(val, node.right);
        }
    }
    public bool contains(int val) { return contains(val, start); }
    private void draw(List<Entity> ent, System.Drawing.Graphics gfx, Basis basis, ScreenData screen, IntNode node)
    {
        ent[node.val].draw(gfx, basis, screen);
        if (node.left != null)
            draw(ent, gfx, basis, screen, node.left);
        if (node.right != null)
            draw(ent, gfx, basis, screen, node.right);
    }
    public void draw(List<Entity> ent, System.Drawing.Graphics gfx, Basis basis, ScreenData screen)
    {
        if (start != null)
            draw(ent, gfx, basis, screen, start);
    }
}*/
//used to be used for extracting entity data from a file, but this process is better to be done outside of the engine;
/*using (StreamReader file = new StreamReader(entData))//this whole part should be done outside of the plane class
            {//extracting the coordinates
                string crrLine = file.ReadLine();
                while (crrLine != null)
                {
                    char[] ignoreChar = { '|' };
                    if (crrLine[0] == '|')//ignore any lines that don't start with '|'
                    {
                        string[] dataSect = crrLine.Split(ignoreChar, StringSplitOptions.RemoveEmptyEntries);
                        if (dataSect[0].Equals("coordinates"))//we have reached the coordinates section
                        {
                            int count = Convert.ToInt32(dataSect[1]);//get how many coordinates are recorded
                            crrLine = file.ReadLine();
                            int i = 0;
                            while (i < count)
                            {
                                if (crrLine[0] == '|')//ignore any lines that don't start with '|'
                                {
                                    string[] coords = crrLine.Split(ignoreChar, StringSplitOptions.RemoveEmptyEntries);
                                    coord.Add(new IntPair(Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1])));
                                    i++;
                                }
                                crrLine = file.ReadLine();
                            }
                        }
                        dataSect = crrLine.Split(ignoreChar, StringSplitOptions.RemoveEmptyEntries);//the current line has been read in the last cycle of the coordinates extraction loop
                        if (dataSect[0].Equals("styles"))//we have reached the styles section
                        {
                            int count = Convert.ToInt32(dataSect[1]);//get how many styles are recorded
                            crrLine = file.ReadLine();
                            int i = 0;
                            while (i < count)
                            {
                                if (crrLine[0] == '|')
                                {
                                    string[] stylePt = crrLine.Split(ignoreChar, StringSplitOptions.RemoveEmptyEntries);
                                    imgDir.Add(stylePt[0]);//if the address is "null", later in the program the case will be handled
                                    if (stylePt[1].Equals("null") == false)
                                    {
                                        string[] clr = stylePt[1].Split(',');
                                        color.Add(System.Drawing.Color.FromArgb(
                                            Convert.ToInt32(clr[0]),
                                            Convert.ToInt32(clr[1]),
                                            Convert.ToInt32(clr[2]),
                                            Convert.ToInt32(clr[3])));
                                    }
                                    else
                                        color.Add(System.Drawing.Color.FromArgb(0, 0, 0, 0));
                                    i++;
                                }
                                crrLine = file.ReadLine();
                            }
                        }
                        dataSect = crrLine.Split(ignoreChar, StringSplitOptions.RemoveEmptyEntries);
                        if (dataSect[0].Equals("entites"))//we have reached the entites section
                        {
                            int count = Convert.ToInt32(dataSect[1]);//get how many entites are recorded
                            crrLine = file.ReadLine();
                            int i = 0;
                            while (i < count && crrLine != null)//the last may be scanned here
                            {
                                if (crrLine[0] == '|')
                                {
                                    bool entProc = false;//has an entity been processed
                                    string[] entityPt = crrLine.Split(ignoreChar, StringSplitOptions.RemoveEmptyEntries);
                                    if (entityPt[0].Equals("stdPoint"))
                                    {
                                        addEntity(new PointEntity(
                                            color[Convert.ToInt32(entityPt[2])],
                                            imgDir[Convert.ToInt32(entityPt[2])],
                                            coord[Convert.ToInt32(entityPt[1])], this));
                                        entProc = true;
                                    }
                                    if (entityPt[0].Equals("stdLine"))
                                    {
                                        addEntity(new LineEntity(
                                            color[Convert.ToInt32(entityPt[3])],
                                            coord[Convert.ToInt32(entityPt[1])],
                                            coord[Convert.ToInt32(entityPt[2])], this));
                                        entProc = true;
                                    }
                                    //if (entityPt[0].Equals("stdPolygon"))
                                    //{

                                    //}
                                    if (entProc == false)
                                    { throw new ArgumentException("Unknown entity!"); }//for debugging, if a valid record with an unknown entity word has been found, use when adding new entities
                                    crrLine = file.ReadLine();
                                }
                                else
                                    crrLine = file.ReadLine();
                            }
                        }
                    }
                    else
                        crrLine = file.ReadLine();
                }
            }*/