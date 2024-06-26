﻿using System;
using ChainFX;

namespace ChainSmart;

public class User : Entity, IKeyable<int>
{
    public static readonly User Empty = new();

    public const short
        ROL_VST = 0b000001, // visit
        ROL_OPN = 0b000011, // operation
        ROL_DLV = 0b000101, // delivery
        ROL_OPN_LOG = ROL_OPN | ROL_DLV,
        ROL_FIN = 0b001001, // finance
        ROL_OPN_FIN = ROL_OPN | ROL_FIN,
        ROL_MGT = 0b111111; // management

    public static readonly Map<short, string> Roles = new()
    {
        { ROL_VST, "访客" },
        { ROL_OPN, "业务" },
        { ROL_DLV, "输送" },
        { ROL_OPN_LOG, "业务＋输送" },
        { ROL_FIN, "财务" },
        { ROL_OPN_FIN, "业务＋财务" },
        { ROL_MGT, "管理" },
    };


    internal int id;
    internal string tel;
    internal string addr;
    internal string im;

    // later
    internal string credential;
    internal short admly;
    internal int supid;
    internal short suply;
    internal int mktid;
    internal short mktly;
    internal int[] vip;
    internal DateTime agreed;
    internal int orgid;
    internal bool icon;

    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Get(nameof(id), ref id);
        }

        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Get(nameof(tel), ref tel);
            s.Get(nameof(im), ref im);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(addr), ref addr);
            s.Get(nameof(credential), ref credential);
            s.Get(nameof(admly), ref admly);
            s.Get(nameof(supid), ref supid);
            s.Get(nameof(suply), ref suply);
            s.Get(nameof(mktid), ref mktid);
            s.Get(nameof(mktly), ref mktly);
            s.Get(nameof(vip), ref vip);
            s.Get(nameof(agreed), ref agreed);
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(icon), ref icon);
        }
    }

    public override void Write(ISink s, short msk = 0xff)
    {
        base.Write(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Put(nameof(id), id);
        }

        s.Put(nameof(tel), tel);
        s.Put(nameof(im), im);
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(addr), addr);
            s.Put(nameof(credential), credential);
            s.Put(nameof(admly), admly);
            s.Put(nameof(supid), supid);
            s.Put(nameof(suply), suply);
            s.Put(nameof(mktid), mktid);
            s.Put(nameof(mktly), mktly);
            s.Put(nameof(vip), vip);
            s.Put(nameof(agreed), agreed);
            s.Put(nameof(orgid), orgid);
            s.Put(nameof(icon), icon);
        }
    }

    public int Key => id;

    public bool IsCertified => typ >= 1;

    public bool HasAdmly => admly > 0;

    public bool HasAdmlyMgt => (admly & ROL_MGT) == ROL_MGT;

    public bool IsVipOf(int orgid) => vip != null && vip.Contains(orgid);

    public bool IsVipFor(int orgid) => vip == null || vip.Contains(orgid);

    public override string ToString() => name;


    //
    // state

    public const short
        STA_ORDI = 1,
        STA_OP = 2;

    public override short ToState()
    {
        short v = 0;
        if (supid > 0 || mktid > 0)
        {
            if (credential != null)
            {
                v |= STA_OP;
            }
        }
        else
        {
            v |= STA_ORDI;
        }
        return v;
    }

    public bool IsStationOp => admly > 0 || supid > 0 || mktid > 0;
}