using ChainFx;

namespace ChainMart
{
    public class ItemImg : Entity
    {
        internal int itemid;

        internal short idx;


        public override void Read(ISource s, short msk = 255)
        {
            base.Read(s, msk);

            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(idx), ref idx);
        }

        public override void Write(ISink s, short msk = 255)
        {
            base.Write(s, msk);

            s.Put(nameof(itemid), itemid);
            s.Put(nameof(idx), idx);
        }
    }
}