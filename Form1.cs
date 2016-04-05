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

namespace VectorBasedLinesEngine
{
    public partial class Form1 : Form
    {
        Plane plane;
        System.Drawing.Graphics gfx;
        System.Drawing.Bitmap buffer;
        System.Drawing.Graphics screenDraw;
        System.Drawing.Bitmap scrDrawBuffer;
        int scrHeight;
        int scrWidth;
        System.Diagnostics.Stopwatch timer;
        int fps = 60;
        public Form1()
        {
            InitializeComponent();
            int cx = pictureBox1.Width / 2;//coordinates X
            int cy = pictureBox1.Height / 2;// coordinates Y
            scrHeight = 480;
            scrWidth = 480;
            timer = new System.Diagnostics.Stopwatch();
            List<string> coord = new List<string>();
            List<string> style = new List<string>();
            List<string> point = new List<string>();
            List<string> stdLine = new List<string>();
            int sectSize = ((int)System.Math.Sqrt(pictureBox1.Height * pictureBox1.Height + pictureBox1.Width * pictureBox1.Width) / 100) * 100 + 100;
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
            plane = new Plane(5, 5, pictureBox1.Width, pictureBox1.Height, Directory.GetCurrentDirectory() + @"\data\entities.ed");
            plane.setEntityWithAction(0, fancyMethod00, 0);
            plane.setEntityWithAction(30, fancyMethod01, 0);
            //plane.command(scrWidth, scrHeight);
            plane.enableEntityActions();
            timer1.Enabled = true;
            refresh();
        }
        private void createDotEntityString(List<string> coord, IntPair pc, List<string> style, string stl, List<string> point)
        {
            string pnt = '|' + pc.a.ToString() + '|' + pc.b.ToString() + '|';//|coorX|coorY|
            string pntstl = stl;//|img dir|color| ; more drawing style data can be added here
            string pntdat = '|' + "point" + '|' + coord.Count.ToString() + '|' + style.Count + '|';//|entity type|ID of the coords|ID of the style|
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
        public void refresh()
        {
            buffer = new Bitmap(scrWidth, scrHeight);
            gfx = Graphics.FromImage(buffer);
            gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            plane.draw(gfx, new ScreenData(480, 480));
            int imgRes = Math.Min(pictureBox1.Width, pictureBox1.Height);
            scrDrawBuffer = new Bitmap(imgRes, imgRes);
            screenDraw = Graphics.FromImage(scrDrawBuffer);
            screenDraw.DrawImage(buffer, (pictureBox1.Width - imgRes) / 2, (pictureBox1.Height - imgRes) / 2, imgRes, imgRes);
            pictureBox1.Image = scrDrawBuffer;
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            refresh();
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //int movementSpeed = 3;
            if (e.KeyCode == Keys.A)
                plane.cw = true;
            if (e.KeyCode == Keys.D)
                plane.ccw = true;
            if (e.KeyCode == Keys.W)
                plane.up = true;
            if (e.KeyCode == Keys.S)
                plane.down = true;
            if (e.KeyCode == Keys.Q)
                plane.left = true;
            if (e.KeyCode == Keys.E)
                plane.right = true;
            //refresh();
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
                plane.cw = false;
            if (e.KeyCode == Keys.D)
                plane.ccw = false;
            if (e.KeyCode == Keys.W)
                plane.up = false;
            if (e.KeyCode == Keys.S)
                plane.down = false;
            if (e.KeyCode == Keys.Q)
                plane.left = false;
            if (e.KeyCode == Keys.E)
                plane.right = false;
        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            int picRes = Math.Min(this.Width - 8, this.Height - 27);
            pictureBox1.Width = picRes;
            pictureBox1.Height = picRes;
            System.Drawing.Point picLoc = new System.Drawing.Point((this.Width - 8 - picRes) / 2, (this.Height - 27 - picRes) / 2);
            pictureBox1.Location = picLoc;
            refresh();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //if (plane.refreshed == false)
            //{
            //    plane.refreshed = true;
            //    refresh();
            //}

            plane.hartMethod(new ScreenData(scrWidth, scrHeight), timer);
            int workTime = 1000 / fps;
            int sleepTime = workTime - (int)(timer.ElapsedMilliseconds);
            refresh();
            if (sleepTime > 0)
                timer1.Interval = sleepTime;
            else
                timer1.Interval = 1;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            plane.command("stop");
            timer1.Enabled = false;
        }
        int hor1 = -1;
        int ver1 = -1;
        private void fancyMethod00(Plane plane, Entity e)
        {
            if (e.type() == 'p')
            {
                PointEntity pe = (PointEntity)e;
                if (pe.coordinates().x() <= 100 && hor1 == -1) hor1 = 1;
                if (pe.coordinates().y() <= 100 && ver1 == -1) ver1 = 1;
                if (pe.coordinates().x() >= 500 && hor1 == 1) hor1 = -1;
                if (pe.coordinates().y() >= 1000 && ver1 == 1) ver1 = -1;
                pe.setCoords(pe.coordinates().x() + hor1 * 0.5, pe.coordinates().y() + ver1 * 1);
            }
        }
        int hor2 = -1;
        int ver2 = -1;
        int hor3 = -1;
        int ver3 = -1;
        private void fancyMethod01(Plane plane, Entity e)
        {
            if (e.type() == 'l')
            {
                LineEntity le = (LineEntity)e;
                if (le.getLineCoords().start().x() <= 700 && hor2 == -1) hor2 = 1;
                if (le.getLineCoords().start().y() <= 0 && ver2 == -1) ver2 = 1;
                if (le.getLineCoords().start().x() >= 1400 && hor2 == 1) hor2 = -1;
                if (le.getLineCoords().start().y() >= 700 && ver2 == 1) ver2 = -1;
                if (le.getLineCoords().end().x() <= 700 && hor3 == -1) hor3 = 1;
                if (le.getLineCoords().end().y() <= 0 && ver3 == -1) ver3 = 1;
                if (le.getLineCoords().end().x() >= 1400 && hor3 == 1) hor3 = -1;
                if (le.getLineCoords().end().y() >= 700 && ver3 == 1) ver3 = -1;
                le.setStartCoords(le.getLineCoords().start().x() + (double)hor2 * 2, le.getLineCoords().start().y() + (double)ver2 * 2);
                le.setEndCoords(le.getLineCoords().end().x() + (double)hor3 * 1, le.getLineCoords().end().y() + (double)ver3 * 2);
            }
        }
    }
}

