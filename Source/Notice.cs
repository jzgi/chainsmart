using System.Text;
using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A notice pertaining to a particular org.
    /// </summary>
    public class Notice : IKeyable<int>
    {
        public const short
            BOOK_CREATED = 1,
            BOOK_ADAPTED = 2,
            BOOK_OKED = 3,
            BOOK_ABORTED = 4,
            BUY_CREATED = 5,
            BUY_ADAPTED = 6,
            BUY_OKED = 7,
            BUY_ABORTED = 8;

        public static readonly Map<short, string> Slots = new Map<short, string>
        {
            {BOOK_CREATED, "供应链新单"},
            {BOOK_ADAPTED, "供应链发货"},
            {BOOK_OKED, "供应链收货"},
            {BOOK_ABORTED, "供应链撤单"},
            {BUY_CREATED, "零售新单"},
            {BUY_ADAPTED, "零售发货"},
            {BUY_OKED, "零售收货"},
            {BUY_ABORTED, "零售撤单"},
        };


        const int CAPACITY = 12;

        private static readonly char[] NUMS =
        {
            '①', '②', '③', '④', '⑤', '⑥', '⑦', '⑧', '⑨', '⑩', '⑪', '⑫'
        };


        readonly int id;

        readonly string name;

        readonly string tel;


        // entries for push
        readonly Entry[] pushy = new Entry[CAPACITY];

        private int toPush;

        // entries for pull
        readonly Entry[] pully = new Entry[CAPACITY];

        private int toPull;


        internal Notice(int id, string name, string tel)
        {
            this.id = id;
            this.name = name;
            this.tel = tel;
        }

        public int Key => id;

        public string Name => name;

        public string Tel => tel;


        public void Put(short slot, int num, decimal amt)
        {
            if (slot > CAPACITY)
            {
                return;
            }

            var idx = slot - 1;

            lock (this)
            {
                // add push
                pushy[idx].Feed(slot, num, amt);
                toPush += num;

                // add pull
                pully[idx].Feed(slot, num, amt);
                toPull += num;
            }
        }


        public void PushToBuffer(StringBuilder sb)
        {
            lock (this)
            {
                var ord = 0;
                for (var i = 0; i < pushy.Length; i++)
                {
                    if (pushy[i].IsStuffed)
                    {
                        sb.Append(NUMS[ord++]).Append(' ');

                        pushy[i].PutToBuffer(sb);

                        pushy[i].Reset();
                    }
                }

                toPush = 0;
            }
        }

        public int CheckPully(short slot)
        {
            if (slot > CAPACITY)
            {
                return 0;
            }

            var idx = slot - 1;

            lock (this)
            {
                return pully[idx].count;
            }
        }

        public int CheckAndClearPully(short slot)
        {
            if (slot > CAPACITY)
            {
                return 0;
            }

            var idx = slot - 1;

            lock (this)
            {
                var ret = pully[idx].count;

                pully[idx].Reset();

                return ret;
            }
        }

        public void ClearPully(short slot)
        {
            if (slot > CAPACITY)
            {
                return;
            }

            var idx = slot - 1;

            lock (this)
            {
                pully[idx].Reset();
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


        public struct Entry
        {
            internal short typ;

            internal int count;

            internal decimal sum;


            internal bool IsEmpty => typ == 0 || count == 0;

            internal bool IsStuffed => typ != 0 && count != 0;

            internal void Feed(short slot, int num, decimal amt)
            {
                typ = slot;
                count += num;
                sum += amt;
            }

            internal void Reset()
            {
                count = 0;
                sum = 0;
            }

            internal void PutToBuffer(StringBuilder sb)
            {
                sb.Append(Slots[typ]).Append(' ').Append(count).Append(' ').Append('￥').Append(sum);
            }
        }
    }
}