using SkyChain;

namespace Zhnt.Mart
{
    /// <summary>
    /// A daily diet schedule.
    /// </summary>
    public class Plan : IData, IKeyable<short>
    {
        public static readonly Plan Empty = new Plan();

        public const byte ID = 1, LATER = 4;

        // tracks
        public const short
            TRACK_DETOX = 1,
            TRACK_REGULAR = 2,
            TRACK_HOME = 3;

        // day of week
        internal short dw;
        internal short[] detox;
        internal short[] regular;
        internal short[] home;
        internal short status;

        public void Read(ISource s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(dw), ref dw);
            }
            s.Get(nameof(detox), ref detox);
            s.Get(nameof(regular), ref regular);
            s.Get(nameof(home), ref home);
            s.Get(nameof(status), ref status);
        }

        public void Write(ISink s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(dw), dw);
            }
            s.Put(nameof(detox), detox);
            s.Put(nameof(regular), regular);
            s.Put(nameof(home), home);
            s.Put(nameof(status), status);
        }

        public static string ContentCol(short track)
        {
            return track switch
            {
                TRACK_DETOX => nameof(detox),
                TRACK_REGULAR => nameof(regular),
                _ => nameof(home)
            };
        }

        static decimal Sum(Map<short, Item> map, short[] itemids, short prog)
        {
            var sum = 0.00M;
            if (itemids != null)
            {
                foreach (var itemid in itemids)
                {
                    var item = map[itemid];
                    if (item != null && item.IsFor(prog))
                    {
                        sum += item.price;
                    }
                }
            }
            return sum;
        }

        public decimal GetTrackPrice(short track, short prog, Map<short, Item> map)
        {
            var sum = 0.00M;
            sum += track switch
            {
                TRACK_DETOX => Sum(map, detox, prog),
                TRACK_REGULAR => Sum(map, regular, prog),
                _ => Sum(map, home, prog)
            };
            return sum;
        }

        public short[] this[short track]
        {
            get
            {
                return track switch
                {
                    TRACK_DETOX => detox,
                    TRACK_REGULAR => regular,
                    _ => home
                };
            }
            set
            {
                if (track == TRACK_DETOX)
                {
                    detox = value;
                }
                else if (track == TRACK_REGULAR)
                {
                    regular = value;
                }
                else
                {
                    home = value;
                }
            }
        }

        public short Key => dw;
    }
}