using SkyChain;

namespace Zhnt.Supply
{
    /// 
    /// An upstream supply of a particular item..
    /// 
    public class Up : _Art
    {
        public static readonly Up Empty = new Up();

        short itemid;

        decimal price;

        decimal discount;

        public override void Read(ISource s, byte proj = 15)
        {
            base.Read(s, proj);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            base.Write(s, proj);
        }
    }
}