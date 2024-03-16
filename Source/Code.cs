using ChainFX;

namespace ChainSmart;

/// <summary>
/// A range of tracebility codes. 
/// </summary>
public class Code : Entity, IKeyable<int>
{
    public static readonly Code Empty = new();

    public static readonly Map<short, string> Typs = new()
    {
        { 1, "特金" },
        { 2, "普金" },
        { 3, "特塑" },
        { 4, "普塑" },
        { 5, "特贴" },
        { 6, "普贴" },
        { 7, "RFID" },
    };


    internal int id;
    internal int orgid;
    internal int num;
    internal int nstart;
    internal int nend;
    internal int cnt;

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
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(num), ref num);
        }
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(nstart), ref nstart);
            s.Get(nameof(nend), ref nend);
            s.Get(nameof(cnt), ref cnt);
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
            s.Put(nameof(typ), typ);
            s.Put(nameof(num), num);
        }
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(nstart), nstart);
            s.Put(nameof(nend), nend);
            s.Put(nameof(cnt), cnt);
        }
    }

    public int Key => id;

    public override string ToString() => name;
}