using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace BaseObject
{
    /// <summary>
    /// Delegate for methods to add to entites. The entity in the arguments can be any entity, but the main reason to use it is to have access to the plane and/or entity database, in which the entity belongs.
    /// </summary>
    /// <param name="e">Desired entity</param>
    public delegate void Action(Entity e);//in case you want to do some fancy stuff; yes the action can also have an effect on other entites
    /// <summary>
    /// Basic entity. All classes for making entities must be derived from this class, if you want ti to work with EntityDatabase objects. But if you are not going to use this class, why are you even using this library?
    /// </summary>
    public abstract class Entity//generic entity, based on that you should be able to build points, lines, polygons or other stuff
    {
        public Action method;//the method, that is going to do fancy stuff
        private int ind;
        public int indexInDatabase { get { return ind; } set { ind = value; } }//index in the entity database
        protected AutoResetEvent _block;//to let the method make one cycle per frame
        public AutoResetEvent block { get { return _block; } }
        protected Thread thread;
        protected int cycles;
        protected bool _stopAction = false;
        public void stopAction() { _stopAction = true; }
        //public void setBlock(AutoResetEvent block) { this.block = block; }
        public bool actionSet { get { return method != null; } }
        public void setAction(Action method, int cycles, AutoResetEvent block)
        {
            this.method = method;
            this.cycles = cycles;
            this._block = block;
            thread = new Thread(() => action());
        }
        public void launchAction()
        {
            if (method != null)
                thread.Start();
        }
        private void action()
        {
            if (_block == null) throw new SystemException("Entity block is not set!");
            if (cycles >= 1)
                for (int i = 0; i < cycles && _stopAction == false; i++)
                {
                    method(this);
                    _block.WaitOne();
                }
            else
                while (_stopAction == false)
                {
                    method(this);
                    _block.WaitOne();
                }
        }
        public abstract string getInfo();//gets a string of all the properties of the entity
        public abstract string dataString();
        public abstract void copy(Entity e);//copies the properties of the argument entity
    }
    /// <summary>
    /// Use objects of this class to store entities, release their AutoResetEvent blocks and stop their threads. Added for convenience.
    /// </summary>
    public class EntityDatabase
    {
        private List<Entity> ent;
        private List<AutoResetEvent> block;
        private SortedSet<int> freeInd;
        private int cnt;
        public int count { get { return cnt; } }
        public Entity entity(int index) { return ent[index]; }
        public EntityDatabase()
        {
            ent = new List<Entity>();
            block = new List<AutoResetEvent>();
            freeInd = new SortedSet<int>();
            cnt = 0;
        }
        public void removeEntity(int index)
        {
            if (index == ent.Count - 1)//if the last entity is getting removed
            {
                ent.RemoveAt(ent.Count - 1);
                block.RemoveAt(ent.Count - 1);
            }
            else//if not, leave a hole in the array, so it can be filled later
            {
                ent[index] = null;
                block[index] = null;
                freeInd.Add(index);
            }
            cnt--;
        }
        public void addEntity(Entity entity)
        {
            int res;
            AutoResetEvent block;
            if (entity.block == null)
                block = new AutoResetEvent(false);
            else
                block = entity.block;
            if (freeInd.Count > 0)
            {
                res = freeInd.Min;
                freeInd.Remove(res);
                ent[res] = entity;
                this.block[res] = block;
            }
            else
            {
                res = ent.Count;
                ent.Add(entity);
                this.block.Add(block);
            }
            entity.indexInDatabase = res;
            cnt++;
        }
        //Removing entites happens by leaving empty holes in the array. I am using the fact, that the array is made out of references, not objects.
        //I store the free positions' indexes in a separate data structure, so when adding new entites the empty spaces get filled.
        //I am thinking on re-arranging the arrays if too many empty spaces are left. I don't know if it a good idea, but I have added it to go
        //around the re-arranging problems of removing and adding elements from and in a Stack structure.
        public void setEntityWithAction(int index, Action method, int cycles)
        {
            ent[index].setAction(method, cycles, block[index]);
        }
        public void launchAnctions()
        {
            for (int i = 0; i < ent.Count; i++)
                if (ent[i] != null && ent[i].method != null)
                    ent[i].launchAction();
        }
        public void releaseBlocks()
        {
            for (int i = 0; i < ent.Count; i++)
                if (ent[i] != null && ent[i].method != null)
                    block[i].Set();
        }
        public void stopThreads()
        {
            for (int i = 0; i < ent.Count; i++)
                if (ent[i] != null && ent[i].method != null)
                {
                    ent[i].stopAction();
                    block[i].Set();
                }
        }
    }
}

