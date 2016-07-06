using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using VBLEDrawing;
using BaseObject;

namespace VectorBasedLinesEngine
{
    public partial class Form1 : Form
    {
        Plane plane;
        System.Drawing.Graphics gfx;
        System.Drawing.Bitmap buffer;
        bool isAntiAlias = false;
        int scrHeight;
        int scrWidth;
        int heightParts = 9;//use to set the aspect ratio accurately
        int widthParts = 16;//it is also used when resizing the window
        int pixelsPerPart = 40;
        System.Diagnostics.Stopwatch timer;
        EntityDatabase ed;
        List<System.Threading.AutoResetEvent> entBlock;
        System.Threading.AutoResetEvent planeMoveBlock;
        int fps = 60;
        public Form1()
        {
            InitializeComponent();
            scrHeight = heightParts * pixelsPerPart;
            scrWidth = widthParts * pixelsPerPart;
            pictureBox1.Height = scrHeight;
            pictureBox1.Width = scrWidth;
            //pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            int cx = pictureBox1.Width / 2;//coordinates X
            int cy = pictureBox1.Height / 2;// coordinates Y
            this.Height = scrHeight + 27;
            this.Width = scrWidth + 8;
            this.MinimumSize = new System.Drawing.Size(this.Width, this.Height);
            timer = new System.Diagnostics.Stopwatch();
            planeMoveBlock = new System.Threading.AutoResetEvent(false);
            plane = new Plane(5, 5, scrWidth, scrHeight, fancyCameraMovementMethod, planeMoveBlock);
            plane.addMethodToHart(fancyTestMethod0);
            plane.addMethodToHart(fancyTestMethod1);
            using (StreamWriter file = new StreamWriter(Directory.GetCurrentDirectory() + @"\data\entities.ed"))
            {//encoding the entity data
                file.WriteLine("This is an example for an entities data file.");
                file.WriteLine("Change the data at your responsibility!");
                file.WriteLine("Should I cipher the text to prevent modifications?");
                file.WriteLine("Anyway, the constructors of the base entities will ignore all lines, that do NOT start with '|',");
                file.WriteLine("and all the text after the last '|' symbol of each line.");
                int sectSize = ((int)System.Math.Sqrt(scrHeight * scrHeight + scrWidth * scrWidth) / 100) * 100 + 100;
                for (int i = 0; i < 10; i++)
                {
                    IntPair p1c = new IntPair(sectSize / 5 + i * 100, sectSize);//coordinates
                    IntPair p2c = new IntPair(sectSize, sectSize + i * sectSize / 5);
                    IntPair p3c = new IntPair(sectSize - i * 70, i * 70);
                    PointEntity pe;
                    if (i % 2 == 0) pe = new PointEntity(@"\data\images\blueSquare1.png", p1c.a, p1c.b, plane);
                    else/*        */pe = new PointEntity(@"\data\images\redSquare0.png", p1c.a, p1c.b, plane);
                    file.WriteLine(pe.dataString());
                    if (i % 2 == 0) pe = new PointEntity(@"\data\images\redStar.png", p2c.a, p2c.b, plane);
                    else/*        */pe = new PointEntity(@"\data\images\blueStar.png", p2c.a, p2c.b, plane);
                    file.WriteLine(pe.dataString());
                    if (i % 2 == 0) pe = new PointEntity(@"\data\images\verGrayLine0.png", p3c.a, p3c.b, plane);
                    else/*        */pe = new PointEntity(@"\data\images\verGrayLine0.png", p3c.a, p3c.b, plane);
                    file.WriteLine(pe.dataString());
                    LineEntity le = new LineEntity(System.Drawing.Color.FromArgb(200, 255, 0, 0), p1c, p2c, plane);
                    file.WriteLine(le.dataString());
                    le = new LineEntity(System.Drawing.Color.FromArgb(200, 0, 255, 0), 300, 300, sectSize + i * 100, sectSize, plane);
                    file.WriteLine(le.dataString());
                    le = new LineEntity(System.Drawing.Color.FromArgb(200, 0, 0, 255), p3c, p1c, plane);
                    file.WriteLine(le.dataString());
                }
                DoublePair[] dp = new DoublePair[6];
                dp[0] = new DoublePair(plane.sectorSize / 2,                        plane.sectorSize / 2);
                dp[1] = new DoublePair(plane.sectorSize / 2 + 3 * plane.sectorSize, plane.sectorSize / 2);
                dp[2] = new DoublePair(plane.sectorSize / 2 + 3 * plane.sectorSize, plane.sectorSize / 2 + 2 * plane.sectorSize);
                dp[3] = new DoublePair(plane.sectorSize / 2 + 2 * plane.sectorSize, plane.sectorSize / 2 + 2 * plane.sectorSize);
                dp[4] = new DoublePair(plane.sectorSize / 2 + 2 * plane.sectorSize, plane.sectorSize / 2 + 3 * plane.sectorSize);
                dp[5] = new DoublePair(plane.sectorSize / 2, plane.sectorSize / 2 + 3 * plane.sectorSize);
                System.Drawing.Color[] clr = new System.Drawing.Color[4];
                clr[0] = System.Drawing.Color.IndianRed;
                clr[1] = System.Drawing.Color.MediumAquamarine;
                clr[2] = System.Drawing.Color.LimeGreen;
                clr[3] = System.Drawing.Color.Yellow;
                PolygonEntity ple = new PolygonEntity(dp, clr, @"\data\images\randTexture.png", plane);
                file.WriteLine(ple.dataString());
            }
            using (StreamReader file = new StreamReader(Directory.GetCurrentDirectory() + @"\data\entities.ed"))
            {
                string crrLine = file.ReadLine();
                int pi = 0;
                while (crrLine != null)
                {
                    if (crrLine[0] == '|')
                    {
                        if (crrLine[1] == '1')
                        {
                            PointEntity pe = new PointEntity(crrLine, plane);
                            if (pi % 2 == 1) pe.rotating = true;
                            if (pi % 3 == 0) pe.disp = new IntPair(10, 15);
                            plane.addEntity(pe);
                            pi++;
                        }
                        else if (crrLine[1] == '2') plane.addEntity(new LineEntity(crrLine, plane));
                        else if (crrLine[1] == '3') plane.addEntity(new PolygonEntity(crrLine, plane));
                    }
                    crrLine = file.ReadLine();
                }
            }
            ed = new EntityDatabase();
            entBlock = new List<System.Threading.AutoResetEvent>();
            //entBlock.Add(new System.Threading.AutoResetEvent(false));
            Entity tmp;
            tmp = plane.entity(0) as Entity;
            //tmp.setAction(fancyMethod00, 0, entBlock[entBlock.Count - 1]);
            tmp.setAction(fancyMethod00, 0, new System.Threading.AutoResetEvent(false));
            //tmp.launchAction();
            ed.addEntity(tmp);
            //ed.setEntityWithAction(tmp.indexInDatabase, fancyMethod00, 0);
            //entBlock.Add(new System.Threading.AutoResetEvent(false));
            tmp = plane.entity(3) as Entity;
            //tmp.setAction(fancyMethod01, 0, entBlock[entBlock.Count - 1]);
            tmp.setAction(fancyMethod01, 0, new System.Threading.AutoResetEvent(false));
            //tmp.launchAction();
            ed.addEntity(tmp);
            //ed.setEntityWithAction(tmp.indexInDatabase, fancyMethod01, 0);
            ed.launchAnctions();
            timer1.Enabled = true;
            refresh();
        }
        public void refresh()
        {
            if (plane != null)
            {
                buffer = new Bitmap(scrWidth, scrHeight);
                gfx = Graphics.FromImage(buffer);
                gfx.SetClip(new Rectangle(0, 0, scrWidth, scrHeight));
                if (isAntiAlias)
                    gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                plane.draw(gfx, new ScreenData(scrWidth, scrHeight));
                pictureBox1.Image = buffer;
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //int movementSpeed = 3;
            if (e.KeyCode == Keys.A) cw = true;
            if (e.KeyCode == Keys.D) ccw = true;
            if (e.KeyCode == Keys.W) up = true;
            if (e.KeyCode == Keys.S) down = true;
            if (e.KeyCode == Keys.Q) left = true;
            if (e.KeyCode == Keys.E) right = true;
            //refresh();
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A) cw = false;
            if (e.KeyCode == Keys.D) ccw = false;
            if (e.KeyCode == Keys.W) up = false;
            if (e.KeyCode == Keys.S) down = false;
            if (e.KeyCode == Keys.Q) left = false;
            if (e.KeyCode == Keys.E) right = false;
        }
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'r')
            {
                if (plane.debug) plane.debug = false;
                else             plane.debug = true;
            }
            if (e.KeyChar == 'f')
            {
                if (isAntiAlias) isAntiAlias = false;
                else             isAntiAlias = true;
            }
            if (e.KeyChar == 'z') plane.zoom += 0.02;
            if (e.KeyChar == 'x') plane.zoom -= 0.02;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer.Start();
            plane.doActions();
            //for (int i = 0; i < entBlock.Count; i++)
            //    entBlock[i].Set();
            ed.releaseBlocks();
            planeMoveBlock.Set();
            int workTime = 1000 / fps;
            refresh();
            timer.Stop();
            int sleepTime = workTime - (int)timer.ElapsedMilliseconds;
            if (sleepTime > 0)
                timer1.Interval = sleepTime;
            else
                timer1.Interval = 1;
            label1.Text = (1000.0 / (double)((int)timer.ElapsedMilliseconds + sleepTime)) + " " + sleepTime;
            timer.Reset();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //for (int i = 0; i < plane.entityCount; i++)
            //    if (plane.entity(i) != null)
            //        (plane.entity(i) as Entity).stopAction();
            ed.stopThreads();
            //for (int i = 0; i < entBlock.Count; i++)
            //    entBlock[i].Set();
            plane.stopMovementThread(); planeMoveBlock.Set();
            timer1.Enabled = false;
        }
        public bool up = false;
        public bool down = false;
        public bool left = false;
        public bool right = false;
        public bool cw = false;
        public bool ccw = false;
        private void fancyCameraMovementMethod(Plane plane)
        {
            if (up) plane.move(0, 2.0 / plane.zoom);//screen movement
            if (down) plane.move(0, -2.0 / plane.zoom);
            if (left) plane.move(2.0 / plane.zoom, 0);
            if (right) plane.move(-2.0 / plane.zoom, 0);
            if (cw) plane.rotate(0.8, new ScreenData(widthParts * pixelsPerPart, heightParts * pixelsPerPart));
            if (ccw) plane.rotate(-0.8, new ScreenData(widthParts * pixelsPerPart, heightParts * pixelsPerPart));
        }
        DoublePair maxCDiv = new DoublePair(0.02, 0.02);
        DoublePair minCDiv = new DoublePair(-0.02, -0.01);
        DoublePair divSpC = new DoublePair(0.0001, 0.0003);
        DoublePair maxXDiv = new DoublePair(0.02, 0.03);
        DoublePair minXDiv = new DoublePair(-0.04, -0.05);
        DoublePair divSpX = new DoublePair(0.0003, 0.0001);
        DoublePair maxYDiv = new DoublePair(0.02, 0.04);
        DoublePair minYDiv = new DoublePair(-0.03, -0.05);
        DoublePair divSpY = new DoublePair (0.0002, 0.0002);
        private void fancyTestMethod0(Plane plane)
        {
            //if (plane.centerDiversion.a >= maxCDiv.a || plane.centerDiversion.a <= minCDiv.a)
            //    divSpC.a = (-1) * divSpC.a;
            //if (plane.centerDiversion.b >= maxCDiv.b || plane.centerDiversion.b <= minCDiv.b)
            //    divSpC.b = (-1) * divSpC.b;
            //plane.centerDiversionX += divSpC.a;
            //plane.centerDiversionY += divSpC.b;
            ////plane.centerDiversion = new DoublePair(plane.centerDiversion.a + divSpC.a, plane.centerDiversion.b + divSpC.b);

            //if (plane.xVectorDiversion.a >= maxXDiv.a || plane.xVectorDiversion.a <= minXDiv.a)
            //    divSpX.a = (-1) * divSpX.a;
            //if (plane.xVectorDiversion.b >= maxXDiv.b || plane.xVectorDiversion.b <= minXDiv.b)
            //    divSpX.b = (-1) * divSpX.b;
            //plane.xVectorDiversionX = divSpX.a;
            //plane.xVectorDiversionY = divSpX.b;
            ////plane.xVectorDiversion = new DoublePair(plane.xVectorDiversion.a + divSpX.a, plane.xVectorDiversion.b + divSpX.b);

            //if (plane.yVectorDiversion.a >= maxYDiv.a || plane.yVectorDiversion.a <= minYDiv.a)
            //    divSpY.a = (-1) * divSpY.a;
            //if (plane.yVectorDiversion.b >= maxYDiv.b || plane.yVectorDiversion.b <= minYDiv.b)
            //    divSpY.b = (-1) * divSpY.b;
            //plane.yVectorDiversionX += divSpY.a;
            //plane.yVectorDiversionY += divSpY.b;
            ////plane.yVectorDiversion = new DoublePair(plane.yVectorDiversion.a + divSpY.a, plane.yVectorDiversion.b + divSpY.b);
        }
        int cycles = 0;
        Entity[] ent = new Entity[5];
        private void fancyTestMethod1(Plane plane)
        {
            if (cycles < 5)
                ent[cycles] = plane.removeEntity(cycles + 5) as Entity;
            if (cycles > 12)
                plane.addEntity(ent[cycles % 5] as Drawable);
            cycles++;
            if (cycles >= 17) cycles = 0;
        }
        int hor1 = -1;
        int ver1 = -1;
        private void fancyMethod00(Entity e)
        {
            PointEntity pe = e as PointEntity;
            if (pe != null)
            {
                //PointEntity pe = (PointEntity)e;
                if (pe.coordinates.x <= 100 &&  hor1 == -1) hor1 = 1;
                if (pe.coordinates.y <= 100 &&  ver1 == -1) ver1 = 1;
                if (pe.coordinates.x >= 500 &&  hor1 == 1)  hor1 = -1;
                if (pe.coordinates.y >= 1000 && ver1 == 1)  ver1 = -1;
                pe.setCoords(pe.coordinates.x + hor1 * 1.5, pe.coordinates.y + ver1 * 2);
            }
        }
        int hor2 = -1;
        int ver2 = -1;
        int hor3 = -1;
        int ver3 = -1;
        private void fancyMethod01(Entity e)
        {
            LineEntity le = e as LineEntity;
            if (le != null)
            {
                if (le.lineCoords.start.x <= 700 && hor2 == -1) hor2 = 1;
                if (le.lineCoords.start.y <= 0 && ver2 == -1) ver2 = 1;
                if (le.lineCoords.start.x >= 1400 && hor2 == 1) hor2 = -1;
                if (le.lineCoords.start.y >= 700 && ver2 == 1) ver2 = -1;
                if (le.lineCoords.end.x <= 700 && hor3 == -1) hor3 = 1;
                if (le.lineCoords.end.y <= 0 && ver3 == -1) ver3 = 1;
                if (le.lineCoords.end.x >= 1400 && hor3 == 1) hor3 = -1;
                if (le.lineCoords.end.y >= 700 && ver3 == 1) ver3 = -1;
                le.setStartCoords(le.lineCoords.start.x + (double)hor2 * 2, le.lineCoords.start.y + (double)ver2 * 2);
                le.setEndCoords(le.lineCoords.end.x + (double)hor3 * 1, le.lineCoords.end.y + (double)ver3 * 2);
            }
        }
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private void pictureBox1_Repainted(object sender, EventArgs e)
        {
            sw.Stop();
            label2.Text = sw.ElapsedMilliseconds.ToString();
            sw.Restart();
        }
    }
}

/*Old code for encoding the entity data
            List<string> coord = new List<string>();
            List<string> style = new List<string>();
            List<string> point = new List<string>();
            List<string> stdLine = new List<string>();
            int sectSize = ((int)System.Math.Sqrt(scrHeight * scrHeight + scrWidth * scrWidth) / 100) * 100 + 100;
            for (int i = 0; i < 10; i++)
            {
                string temp;
                IntPair p1c = new IntPair(sectSize / 5 + i * 100, sectSize);//coordinates
                IntPair p2c = new IntPair(sectSize, sectSize + i * sectSize / 5);
                IntPair p3c = new IntPair(sectSize - i * 70, i * 70);
                temp = '|' + Directory.GetCurrentDirectory() + @"\data\images\blueSquare1.png" + '|' + "null" + '|';//style
                createDotEntityString(coord, p1c, style, temp, point);//set the string to write
                temp = '|' + Directory.GetCurrentDirectory() + @"\data\images\blueStar.png" + '|' + "null" + '|';
                createDotEntityString(coord, p2c, style, temp, point);
                temp = '|' + Directory.GetCurrentDirectory() + @"\data\images\redStar.png" + '|' + "null" + '|';
                createDotEntityString(coord, p3c, style, temp, point);
                //==========
                temp = '|' + "null" + '|' + "200,255,0,0" + '|';
                createLineEntityString(coord, p1c, p2c, style, temp, stdLine);
                temp = '|' + "null" + '|' + "200,0,255,0" + '|';
                createLineEntityString(coord, p2c, p3c, style, temp, stdLine);
                temp = '|' + "null" + '|' + "200,0,0,255" + '|';
                createLineEntityString(coord, p3c, p1c, style, temp, stdLine);
                //==========
                //createPolygonEntityString();
            }
            using (StreamWriter file = new StreamWriter(Directory.GetCurrentDirectory() + @"\data\entities.ed"))
            {
                file.WriteLine("This is an example for an entities data file.");
                file.WriteLine("Change the data at your responsibility!");
                file.WriteLine("Should I cipher the text to prevent modifications?");
                file.WriteLine("Anyway, the program will ignore all lines, that do NOT start with '|',");
                file.WriteLine("and all the text after the last '|' symbol of each line.");
                //=============================================================
                file.WriteLine("|coordinates|" + coord.Count.ToString() + '|');
                for (int c = 0; c < coord.Count; c++)
                    file.WriteLine(coord[c] + c);
                file.WriteLine("|styles|" + style.Count.ToString() + '|');
                for (int c = 0; c < style.Count; c++)
                    file.WriteLine(style[c] + c);
                int entities = point.Count + stdLine.Count;
                file.WriteLine("|entites|" + entities + '|' + "The order in which entities are stored here should not be important if all entities are derived from the Entity class");
                for (int c = 0; c < point.Count; c++)//points
                    file.WriteLine(point[c] + c);
                for (int c = 0; c < stdLine.Count; c++)//lines
                    file.WriteLine(stdLine[c] + c);
            }
        private void createDotEntityString(List<string> coord, IntPair pc, List<string> style, string stl, List<string> point)
        {
            string pnt = '|' + pc.a.ToString() + '|' + pc.b.ToString() + '|';//|coorX|coorY|
            string pntstl = stl;//|img dir|color| ; more drawing style data can be added here
            string pntdat = '|' + "stdPoint" + '|' + coord.Count.ToString() + '|' + style.Count + '|';//|entity type|ID of the coords|ID of the style|
            point.Add(pntdat);
            coord.Add(pnt);
            style.Add(pntstl);
            //the vertical line is added in the end to help ignore any extra text after the needed data
        }
        private void createLineEntityString(List<string> coord, IntPair st, IntPair en, List<string> style, string stl, List<string> line)
        {
            string start = '|' + st.a.ToString() + '|' + st.b.ToString() + '|';//|coorX|coorY|
            string end = '|' + en.a.ToString() + '|' + en.b.ToString() + '|';//|coorX|coorY|
            string pntstl = stl;//|img dir|color| ; more drawing style data can be added here
            coord.Add(start);
            coord.Add(end);
            string lndat = '|' + "stdLine" + '|' + (coord.Count - 1).ToString() + '|' + (coord.Count - 2).ToString() + '|' + style.Count + '|';//|entity type|start coords ID|end coords ID|ID of the style|
            line.Add(lndat);
            style.Add(pntstl);
            //the vertical line is added in the end to help ignore any extra text after the needed data
        }
 */