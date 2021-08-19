using SkyChain;

namespace Zhnt
{
    public class UOrd : IData, IKeyable<int>
    {
        public static readonly UOrd Empty = new UOrd();

        public const byte ID = 1, LATER = 2;

        public const short
            TYP_PRODUCT = 1,
            TYP_SERVICE = 2,
            TYP_EVENT = 3;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PRODUCT, "产品拼团"},
            {TYP_SERVICE, "服务拼团"},
            {TYP_EVENT, "社工活动"},
        };


        internal int id;
        internal decimal price;

        public void Read(ISource s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(price), ref price);
            if ((proj & LATER) == LATER)
            {
            }
        }

        public void Write(ISink s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            s.Put(nameof(price), price);


            if ((proj & LATER) == LATER)
            {
            }
        }

        public int Key => id;
    }
}