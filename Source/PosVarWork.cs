using System;
using System.Threading.Tasks;
using ChainFx.Nodal;
using ChainFx.Web;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public class RtllyPosVarWork : BuyVarWork
{
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("撤销", "确认撤销该笔记录？", icon: "trash"), Tool(ButtonConfirm, status: 4)]
    public async Task @void(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = Nodality.NewDbContext();
        dc.Sql("UPDATE buys SET refund = pay, status = 0, adapted = @1, adapter = @2 WHERE id = @3 AND rtlid = @4 AND status = 4");
        await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        wc.Give(204);
    }
}