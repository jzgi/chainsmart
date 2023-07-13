using ChainFx;

namespace ChainSmart;

/// <summary>
/// The data modal for an standard category.
/// </summary>
public class Cat : Entity, IKeyable<short>, IFolderable
{
    public static readonly Cat Empty = new();

    internal short idx;

    internal short size; // number of items

    // must have an icon

    public override void Read(ISource s, short proj = 0xff)
    {
        base.Read(s, proj);

        s.Get(nameof(idx), ref idx);
        s.Get(nameof(size), ref size);
    }

    public override void Write(ISink s, short proj = 0xff)
    {
        base.Write(s, proj);

        s.Put(nameof(idx), idx);
        s.Put(nameof(size), size);
    }

    public short Key => typ;

    public override string ToString() => name;

    public short Index => idx;

    public short Size => size;
}