using SkyChain;

namespace Zhnt.Mart
{
    public class MatAgg : IData, IKeyable<short>
    {
        short id;

        int amt;

        public MatAgg(short id, int amt)
        {
            this.id = id;
            this.amt = amt;
        }

        public void Read(ISource s, byte proj = 15)
        {
            s.Get(nameof(id), ref id);
            s.Get(nameof(amt), ref amt);
        }

        public void Write(ISink s, byte proj = 15)
        {
        }

        public void Add(int mat)
        {
            this.amt += mat;
        }

        public short Key => id;

        public int Amt => amt;
    }
}