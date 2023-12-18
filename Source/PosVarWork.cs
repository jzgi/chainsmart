﻿using System;
using System.Threading.Tasks;
using ChainFX.Nodal;
using ChainFX.Web;
using static ChainFX.Web.Modal;

namespace ChainSmart;

public class RtllyPosVarWork : BuyVarWork
{
    [UserAuthorize(Org.TYP_RTL, User.ROL_OPN)]
    [Ui(tip: "确认撤销该笔记录？", icon: "trash", status: 4), Tool(ButtonConfirm)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = Nodality.NewDbContext();
        dc.Sql("UPDATE buys SET refund = pay, status = 0, oked = @1, oker = @2 WHERE id = @3 AND rtlid = @4 AND status = 4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.Give(204);
    }
}