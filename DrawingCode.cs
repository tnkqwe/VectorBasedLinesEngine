using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorBasedLinesEngine
{
    interface Drawable
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
        IntPair[] locatedInSections { get; }
    }
    class BasicDrawableData//basic data for use by the Drawable interface; you can also use your own
    {//all are public so I don't have to write accessors for the objects, that are already written in the Drawable interface, I advise you to make objects of this class private
        public BasicDrawableData()
        {
            plane = null;
            moved = false;
            section = null;
            indexInPlane = -1;
        }
        public Plane plane;
        public bool moved;
        public IntPair[] section;
        public int indexInPlane;
    }
}
