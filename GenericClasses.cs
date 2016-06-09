using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace VectorBasedLinesEngine
{
    public delegate void Action(Entity e);//in case you want to do some fancy stuff; yes the action can also have an effect on other entites
    public abstract class Entity//generic entity, based on that you should be able to build points, lines, polygons or other stuff
    {
        public Action method;//the method, that is going to do fancy stuff
        //private Object actionData;//data, that is going to be used by the action method
        protected AutoResetEvent block;//to let the method make one cycle per frame
        protected Thread thread;
        protected int cycles;
        protected bool _stopAction = false;
        public void stopAction() { _stopAction = true; }
        //public void setBlock(AutoResetEvent block) { this.block = block; }
        public void setAction(Action method, int cycles, AutoResetEvent block)
        {
            this.method = method;
            this.cycles = cycles;
            this.block = block;
            thread = new Thread(() => action());
        }
        public void launchAction()
        {
            if (method != null)
                thread.Start();
        }
        private void action()
        {
            if (block == null) throw new SystemException("Entity block is not set!");
            if (cycles >= 1)
                for (int i = 0; i < cycles && _stopAction == false; i++)
                {
                    method(this);
                    block.WaitOne();
                }
            else
                while (_stopAction == false)
                {
                    method(this);
                    block.WaitOne();
                }
        }
        //public abstract string[] getProperties();//gets a string of all the properties of the entity
        public abstract string dataString();
        public abstract void copy(Entity e);//copies the properties of the argument entity
    }
}

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