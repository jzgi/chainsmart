using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Revital.User;

namespace Revital
{
    public abstract class BidWork : WebWork
    {
    }

    public class PublyBidWork : BidWork
    {
        public async Task @default(WebContext wc, int code)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bid.Empty).T(" FROM bids WHERE codend >= @1 ORDER BY codend LIMIT 1");
            var o = await dc.QueryTopAsync<Bid>(p => p.Set(code));
            wc.GivePage(200, h =>
            {
                if (o == null || o.codend - o.codes >= code)
                {
                    h.ALERT("编码没有找到");
                }
                else
                {
                    var plan = Obtain<short, Plan>(o.planid);
                    var frm = Obtain<int, Org>(o.toid);
                    var ctr = Obtain<int, Org>(o.fromid);

                    h.FORM_();
                    h.FIELDSUL_("溯源信息");
                    h.LI_().FIELD("生产户／地块", frm.name);
                    h.LI_().FIELD("分拣中心", ctr.name);
                    h._FIELDSUL();
                    h._FORM();
                }
            }, title: "中惠农通溯源系统");
        }
    }


    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("采购收货管理", "sign-in")]
    public class CtrlyBidWork : BidWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyBidVarWork>();
        }

        [Ui("已确认", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bid.Empty).T(" FROM bids WHERE ctrid = @1 AND status < 2 ORDER BY id DESC LIMIT 10 OFFSET @2 * 10");
            var arr = await dc.QueryAsync<Bid>(p => p.Set(orgid).Set(page), 0xff);

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr != null)
                {
                }
                h.PAGINATION(arr?.Length == 10);
            }, false, 3);
        }

        [Ui("已收货", group: 2), Tool(Anchor)]
        public async Task closed(WebContext wc, int page)
        {
            short orgid = wc[-1];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bid.Empty).T(" FROM bids WHERE ctrid = @1 AND status >= 2 ORDER BY status, id DESC LIMIT 10 OFFSET @2 * 10");
            var arr = await dc.QueryAsync<Bid>(p => p.Set(orgid).Set(page), 0xff);

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr != null)
                {
                }
                h.PAGINATION(arr?.Length == 10);
            }, false, 3);
        }

        [Ui("发布", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
            if (wc.IsGet)
            {
                if (typ == 0) // display type selection
                {
                    wc.GivePane(200, h => { h.FORM_().FIELDSUL_("请选择推广类型"); });
                }
                else // typ specified
                {
                }
            }
            else // POST
            {
                var today = DateTime.Today;
                var o = await wc.ReadObjectAsync(inst: new Bid
                {
                    // @extern = org.refid,
                });
                // database op
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO lots ").colset(o, 0)._VALUES_(o, 0);
                await dc.QueryTopAsync(p => o.Write(p, 0));

                wc.GivePane(201);
            }
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
                dc.Sql("INSERT INTO bids (typ, status, orgid, issued, ended, span, name, tag, tip, unit, unitip, price, min, max, least, step, extern, addr, start, author, icon, img) SELECT typ, 0, orgid, issued, @1, span, name, tag, tip, unit, unitip, price, min, max, least, step, extern, addr, start, @2, icon, img FROM lots WHERE orgid = @3 AND id")._IN_(key);
                await dc.ExecuteAsync(p => p.Set(ended).Set(prin.name).Set(orgid).SetForIn(key));

                wc.GivePane(201);
            }
        }
    }


    [UserAuthorize(Org.TYP_PRD, ORGLY_OP)]
    [Ui("订货管理")]
    public class FrmlyBidWork : BidWork
    {
        protected override void OnMake()
        {
            MakeVarWork<FrmlySubscribeVarWork>();
        }

        [Ui("来单"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bid.Empty).T(" FROM purchs WHERE partyid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Bid>(p => p.Set(orgid));

            wc.GivePage(200, h => { h.TOOLBAR(); });
        }

        [Ui("历史"), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bid.Empty).T(" FROM purchs WHERE partyid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Bid>(p => p.Set(orgid));

            wc.GivePage(200, h => { h.TOOLBAR(caption: "来自平台的订单"); });
        }
    }
}