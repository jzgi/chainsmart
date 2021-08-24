using SkyChain;

namespace Zhnt
{
    public class Need : _Art
    {
        public static readonly Need Empty = new Need();

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