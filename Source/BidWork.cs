using System;
using System.Threading.Tasks;
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


    [UserAuthorize(Org.TYP_CTR, ORGLY_OP)]
    public abstract class CtrlyBidWork : BidWork
    {
        [Ui("当前", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Bid.Empty).T(" FROM bids WHERE fromid = @1 AND status < 2 ORDER BY id DESC LIMIT 30 OFFSET @2 * 30");
            var arr = await dc.QueryAsync<Bid>(p => p.Set(org.id).Set(page), 0xff);
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr != null)
                {
                    h.TABLE(arr, x => { h.TD(x.name); });
                }
                h.PAGINATION(arr?.Length == 30);
            }, false, 3);
        }

        [Ui("以往", group: 2), Tool(Anchor)]
        public async Task past(WebContext wc, int page)
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

        [Ui("✚", "新建", group: 1), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int cmd)
        {
            var prin = (User) wc.Principal;
            var org = wc[-1].As<Org>();
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_();
                    if (cmd == 0)
                    {
                        h.FIELDSUL_("选择供应项目");

                        h._FIELDSUL();
                        h.BOTTOM_BUTTON("下一步", nameof(@new), 1, post: false);
                    }
                    else if (cmd == 1)
                    {
                        h.FIELDSUL_("选择产源产品");

                        h._FIELDSUL();
                        h.BOTTOM_BUTTON("下一步", nameof(@new), 2, post: false);
                    }
                    else
                    {
                        h.FIELDSUL_("采购信息");

                        h._FIELDSUL();
                        h.BOTTOM_BUTTON("确定", nameof(@new), 3);
                    }
                    h._FORM();
                });
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
                dc.Sql("INSERT INTO bids ").colset(o, 0)._VALUES_(o, 0);
                await dc.QueryTopAsync(p => o.Write(p, 0));

                wc.GivePane(201);
            }
        }
    }

    [Ui("采购及收货管理")]
    public class CtrlyAgriBidWork : CtrlyBidWork
    {
        protected override void OnMake()
        {
            MakeVarWork<CtrlyAgriBidVarWork>();
        }
    }

    [UserAuthorize(Org.TYP_PRD, ORGLY_OP)]
    [Ui("订货管理")]
    public abstract class PrdlyBidWork : BidWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PrdlyBidVarWork>();
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

    [Ui("订货管理")]
    public class PrdlyAgriBidWork : PrdlyBidWork
    {
    }

    [Ui("订货管理")]
    public class PrdlyDietBidWork : PrdlyBidWork
    {
    }


    [UserAuthorize(Org.TYP_SRC, ORGLY_OP)]
    [Ui("产源销售报表")]
    public class SrclyBidWork : BidWork
    {
    }
}