using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// An event bag pertaining to a particular org.
    /// </summary>
    public class NoticeSlot : IKeyable<int>
    {
        const int CAP = 12;


        readonly int id;

        string name;

        string tel;


        // for pushing
        readonly Notice[] pushs = new Notice[CAP];

        private int toPush;

        // for pulling
        readonly Notice[] pulls = new Notice[CAP];

        private int toPull;


        public void Add(string name, string tel, int typ, int num, decimal amt)
        {
            lock (this)
            {
                // add push

                pushs[typ].Add(num, amt);
                toPush++;

                // add pull
                pulls[typ].Add(num, amt);
                toPull++;
            }
        }

        public bool HasToPush
        {
            get
            {
                lock (this)
                {
                    return toPush > 0;
                }
            }
        }

        public bool HasToPull
        {
            get
            {
                lock (this)
                {
                    return toPull > 0;
                }
            }
        }

        public int Key => id;
    }


    public struct Notice
    {
        short id;

        int count;

        decimal sum;

        internal void Add(int num, decimal amt)
        {
            count += num;
            sum += amt;
        }
    }
}