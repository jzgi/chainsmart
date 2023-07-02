using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class RegWork : WebWork
{
    protected static void MainGrid(HtmlBuilder h, Reg[] arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");

            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name).SPAN(Entity.Statuses[o.status], "uk-badge")._HEADER();
            h.Q(o.tip, "uk-width-expand");
            h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

[AdmlyAuthorize(User.ROL_MGT)]
[Ui("区域设置")]
public class AdmlyRegWork : RegWork
{
    protected override void OnCreate()
    {
        CreateVarWork<AdmlyRegVarWork>();
    }

    [Ui("省份", status: 1), Tool(Anchor)]
    public void @default(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = ").T(Reg.TYP_PROVINCE).T(" ORDER BY id, status DESC");
        var arr = dc.Query<Reg>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: Reg.TYP_PROVINCE);

            MainGrid(h, arr);
        }, false, 12);
    }

    [Ui("地市", status: 2), Tool(Anchor)]
    public void city(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = ").T(Reg.TYP_CITY).T(" ORDER BY id, status DESC");
        var arr = dc.Query<Reg>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: Reg.TYP_CITY);

            MainGrid(h, arr);
        }, false, 15);
    }

    [Ui("场区", status: 4), Tool(Anchor)]
    public void sect(WebContext wc)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = ").T(Reg.TYP_SECTION).T(" ORDER BY id, status DESC");
        var arr = dc.Query<Reg>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: Reg.TYP_SECTION);

            MainGrid(h, arr);
        }, false, 15);
    }

    [Ui("新建", "新建区域", icon: "plus", status: 7), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int typ)
    {
        var prin = (User)wc.Principal;
        var o = new Reg
        {
            typ = (short)typ,
            created = DateTime.Now,
            creator = prin.name,
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("区域信息");
                h.LI_().NUMBER("区域编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, min: 2, max: 40)._LI();
                h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                h.LI_().NUMBER("子项数", nameof(o.num), o.num, min: 0, max: 9999)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new), subscript: typ)._FORM();
            });
        }
        else // POST
        {
            const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;

            o = await wc.ReadObjectAsync(msk, instance: o);
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO regs ").colset(Reg.Empty, msk)._VALUES_(Item.Empty, msk);
            await dc.ExecuteAsync(p => o.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}