using SkyChain;

namespace Zhnt.Supply
{
    /// 
    /// A partucular supply.plan
    /// 
    public class Prod : Art_
    {
        public static readonly Prod Empty = new Prod();


        internal short id;
        internal short itemid;
        internal short seasong;


        public override void Read(ISource s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            base.Read(s, proj);

            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(seasong), ref seasong);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            base.Write(s, proj);

            s.Put(nameof(itemid), itemid);
            s.Put(nameof(seasong), seasong);
        }
    }
}