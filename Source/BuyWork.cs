using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Zhnt.Supply.User;

namespace Zhnt.Supply
{
    [UserAuthorize(admly: 1)]
    [Ui("销售订单")]
    public class AdmlyBuyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyBuyVarWork>();
        }

        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [UserAuthorize(Org.TYP_BIZ, 1)]
    [Ui("订货", "进货管理")]
    public class BizlyBuyWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyBuyVarWork>();
        }

        [Ui("购物车"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE partyid = @1 AND status = 0 ORDER BY id");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(orgid));

            var items = Obtain<short, Item>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: "购物车");
                h.TABLE(arr, o =>
                {
                    h.TD(items[o.itemid].name);
                    h.TD(o.qty);
                    h.TDFORM(() => { });
                });
            });
        }

        [Ui("订单"), Tool(Anchor)]
        public async Task lst(WebContext wc, int page)
        {
        }

        [Ui("✚"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            if (wc.IsGet)
            {
                var o = new Item();
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("标品属性");
                    h.LI_().SELECT("类别", nameof(o.typ), o.typ, Item.Typs)._LI();
                    h.LI_().TEXT("标品名称", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().TEXT("亮点", nameof(o.tip), o.tip, max: 10)._LI();
                    // h.LI_().SELECT("方案关联", nameof(o.unit), o.unit, Item.Progg)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, Item.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Item>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO items ").colset(Item.Empty, 0)._VALUES_(Item.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));
                wc.GivePane(200); // close dialog
            }
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("销售单")]
    public class CtrlyDownLnWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyBuyVarWork>();
        }

        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            int orgid = wc[-1];
        }

        [Ui("停止", group: 2), Tool(Anchor)]
        public async Task closed(WebContext wc, int page)
        {
            short orgid = wc[-1];
        }

        [Ui("发布", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
        }

        [Ui("复制", group: 2), Tool(ButtonPickOpen)]
        public async Task copy(WebContext wc)
        {
            short orgid = wc[-1];
            var prin = (User) wc.Principal;
            var ended = DateTime.Today.AddDays(3);
            int[] key;
            if (wc.IsGet)
            {
                key = wc.Query[nameof(key)];
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("目标截止日期");
                    h.LI_().DATE("截止", nameof(ended), ended)._LI();
                    h._FIELDSUL();
                    h.HIDDENS(nameof(key), key);
                    h.BOTTOM_BUTTON("确认", nameof(copy));
                    h._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ended = f[nameof(ended)];
                key = f[nameof(key)];
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO lots (typ, status, orgid, issued, ended, span, name, tag, tip, unit, unitip, price, min, max, least, step, extern, addr, start, author, icon, img) SELECT typ, 0, orgid, issued, @1, span, name, tag, tip, unit, unitip, price, min, max, least, step, extern, addr, start, @2, icon, img FROM lots WHERE orgid = @3 AND id")._IN_(key);
                await dc.ExecuteAsync(p => p.Set(ended).Set(prin.name).Set(orgid).SetForIn(key));

                wc.GivePane(201);
            }
        }
    }
}