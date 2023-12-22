using ChainFX;

namespace ChainSmart;

/// <summary>
/// An ad item to publish.
/// </summary>
public class Ad : Entity, IKeyable<int>
{
    public static readonly Ad Empty = new();

    public static readonly Map<short, string> Typs = new()
    {
        { 1, "文字" },
        { 2, "图片" },
        { 4, "视频" },
    };

    public static readonly Map<short, string> Ranks = new()
    {
        { 1, "普通" },
        { 2, "重要" },
        { 3, "特别" },
    };


    public new static readonly Map<short, string> Statuses = new()
    {
        { STU_VOID, "作废" },
        { STU_CREATED, "新建" },
        { STU_ADAPTED, "调整" },
        { STU_OKED, "发布" },
    };


    internal int id;

    internal int orgid;

    internal short rank;

    internal string content;

    // must have an icon

    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Get(nameof(id), ref id);
        }
        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Get(nameof(orgid), ref orgid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(content), ref content);
            s.Get(nameof(rank), ref rank);
        }
    }

    public override void Write(ISink s, short msk = 0xff)
    {
        base.Write(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Put(nameof(id), id);
        }
        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Put(nameof(orgid), orgid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(content), content);
            s.Put(nameof(rank), rank);
        }
    }

    public int Key => id;

    public override string ToString() => name;

    public short Index => rank;
}