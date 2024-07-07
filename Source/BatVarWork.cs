using System;
using System.Threading.Tasks;
using ChainFX.Web;
using static ChainFX.Entity;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;
using static ChainSmart.MainUtility;

namespace ChainSmart;

public abstract class BatVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE id = @1");
        var o = await dc.QueryTopAsync<Bat>(p => p.Set(id));

        wc.GivePane(200, h =>
        {
            h.FORM_().FIELDSUL_("基本");

            h.LI_().FIELD("操作类型", o.typ, Bat.Typs)._LI();
            h.LI_().FIELD("商品", o.name)._LI();
            h.LI_().FIELD("附加说明", o.tip)._LI();
            h.LI_().FIELD("当前存量", o.stock)._LI();
            h.LI_().FIELD("数量", o.qty)._LI();
            if (o.srcid > 0)
            {
                var src = GrabTwin<int, Org>(o.srcid);
                h.LI_().FIELD("产源", src.name)._LI();
            }
            if (o.hubid > 0)
            {
                var hub = GrabTwin<int, Org>(o.hubid);
                h.LI_().FIELD("品控云仓", hub.name)._LI();
            }

            h._FIELDSUL().FIELDSUL_("状态");

            h.LI_().FIELD("状态", o.status, Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "生效", o.oker, o.oked, sep: "<br>")._LI();

            h._FIELDSUL()._FORM();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class PublyBatVarWork : CodeVarWork
{
    public override async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();

        dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE id = @1");
        var bat = await dc.QueryTopAsync<Bat>(p => p.Set(id));

        dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE id = @1");
        var item = await dc.QueryTopAsync<Item>(p => p.Set(bat.itemid));

        wc.GivePage(200, h =>
        {
            if (bat == null)
            {
                h.ALERT("无效的货次号");
                return;
            }

            var src = GrabTwin<int, Org>(bat.srcid);

            var tag = Grab<short, Tag>()?[bat.tag];

            h.TOPBARXL_();
            h.IMG(OrgUrl, src.id, "/icon", circle: true, css: "uk-width-small");
            h.HEADER_("uk-width-expand uk-padding-left").H2_().T(src.name).T(bat.name)._H2()._HEADER();
            h._TOPBARXL();

            // batch and item info
            h.ARTICLE_("uk-card uk-card-primary");

            h.HEADER_("uk-card-header").H3("商品信息")._HEADER();
            h.IMG(OrgUrl, bat.itemid, "/pic", css: "uk-width-1-1");

            h.UL_("uk-list uk-list-divider");
            h.LI_().FIELD("商品", item.name)._LI();
            h.LI_().FIELD(string.Empty, item.tip)._LI();
            h.LI_().FIELD("货次", bat.oked)._LI();
            h.LI_().FIELD_("溯源", css: "uk-col").T(tag.name).Q(tag.tip)._FIELD()._LI();

            var sym = Grab<short, Sym>()?[item.sym];

            h.LI_().FIELD_("标志", css: "uk-col");
            if (sym == null)
            {
                h.T("无");
            }
            else
            {
                h.T(sym.name).BR().Q(sym.tip, css: "uk-margin-auto-left");
            }
            h._FIELD()._LI();

            h._UL();

            if (item.m1)
            {
                h.PIC(OrgUrl, item.id, "/m-1", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (item.m2)
            {
                h.PIC(OrgUrl, item.id, "/m-2", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (item.m3)
            {
                h.PIC(OrgUrl, item.id, "/m-3", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (item.m4)
            {
                h.PIC(OrgUrl, item.id, "/m-4", css: "uk-width-1-1 uk-padding-bottom");
            }

            h._ARTICLE();

            h.ARTICLE_("uk-card uk-card-primary");

            h.HEADER_("uk-card-header").H3("产源信息")._HEADER();
            h.IMG(OrgUrl, src.id, "/pic", css: "uk-width-1-1");

            h.UL_("uk-card-body uk-list uk-list-divider");
            h.LI_().FIELD("产源", src.name)._LI();
            h.LI_().FIELD(string.Empty, src.tip)._LI();
            h.LI_().FIELD("工商登记", src.legal)._LI();
            h.LI_().FIELD("地址", src.addr)._LI();
            h._UL();

            if (src.m1)
            {
                h.PIC(OrgUrl, src.id, "/m-1", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.m2)
            {
                h.PIC(OrgUrl, src.id, "/m-2", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.m3)
            {
                h.PIC(OrgUrl, src.id, "/m-3", css: "uk-width-1-1 uk-padding-bottom");
            }
            if (src.m4)
            {
                h.PIC(OrgUrl, src.id, "/m-4", css: "uk-width-1-1 uk-padding-bottom");
            }

            h._ARTICLE();
        }, true, 3600, title: "产品溯源信息");
    }
}

public class ShplyBatVarWork : BatVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改或调整消息", icon: "pencil", status: 1 | 2 | 4), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
    }
}

// supplier or source
//
public class SrclyBatVarWork : BatVarWork
{
    [MgtAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改或调整消息", icon: "pencil", status: 1 | 2 | 4), Tool(ButtonShow)]
    public async Task upd(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bat.Empty).T(" FROM bats WHERE id = @1");
            var o = await dc.QueryTopAsync<Bat>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("消息类型", nameof(o.typ), o.typ, Bat.Typs)._LI();
                h.LI_().TEXT("标题", nameof(o.name), o.name, max: 12)._LI();
                // h.LI_().TEXTAREA("内容", nameof(o.content), o.content, max: 300)._LI();
                h.LI_().TEXTAREA("注解", nameof(o.tip), o.tip, max: 40)._LI();
                // h.LI_().SELECT("级别", nameof(o.rank), o.rank, Lotop.Ranks)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(upd))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Bat
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE bats ")._SET_(Bat.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }


    [MgtAuthorize(Org.TYP_SRC, User.ROL_MGT)]
    [Ui("发货", "安排发布", status: 1), Tool(ButtonConfirm)]
    public async Task adapt(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;


        int nstart = 0, nend = 0;

        if (wc.IsGet)
        {
            var tags = Grab<short, Tag>();

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();

                // h.LI_().SELECT("溯源标签", nameof(tag), tag, tags)._LI();
                h.LI_().NUMBER("起始号", nameof(nstart), nstart, min: 1)._LI();
                h.LI_().NUMBER("截至号", nameof(nend), nend, min: 1)._LI();

                h._FIELDSUL().BUTTON(nameof(adapt), "确认")._FORM();
            });
        }
        else
        {
            using var dc = NewDbContext();
            dc.Sql("UPDATE bats SET status = 2, adapted = @1, adapter = @2 WHERE id = @3 AND orgid = @4 RETURNING ").collst(Bat.Empty);
            var o = await dc.QueryTopAsync<Bat>(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

            // org.EventPack.AddMsg(o);

            wc.GivePane(200);
        }
    }
}

public class HublyBatVarWork : BatVarWork
{
    [MgtAuthorize(Org.TYP_HUB, User.ROL_OPN)]
    [Ui("收货", "收货入仓", status: 2), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var hub = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE bats SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND hubidid = @4 RETURNING ").collst(Bat.Empty);
        var o = await dc.QueryTopAsync<Bat>(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(hub.id));

        // var org = GrabTwin<int, Org>(o.orgid);
        // hub.EventPack.a.AddMsg(o);

        wc.GivePane(200);
    }
}