using ChainFx;

namespace ChainSmart;

public class Credit : Entity, IKeyable<int>
{
    public static readonly Cat Empty = new();

    internal short id;

    internal short idx;

    internal short size; // number of items

    // must have an icon

    public override void Read(ISource s, short proj = 0xff)
    {
        base.Read(s, proj);

        s.Get(nameof(id), ref id);
        s.Get(nameof(idx), ref idx);
        s.Get(nameof(size), ref size);
    }

    public override void Write(ISink s, short proj = 0xff)
    {
        base.Write(s, proj);

        s.Put(nameof(id), id);
        s.Put(nameof(idx), idx);
        s.Put(nameof(size), size);
    }

    public int Key => id;
}