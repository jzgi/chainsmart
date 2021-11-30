using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for produced piece.
    /// </summary>
    public class Piece : _Article, IKeyable<int>
    {
        public static readonly Piece Empty = new Piece();


        internal short id;

        public override void Read(ISource s, byte proj = 15)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
        }

        public override void Write(ISink s, byte proj = 15)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
        }

        public int Key => id;
    }
}