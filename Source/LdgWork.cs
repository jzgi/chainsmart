using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class LdgWork : WebWork
    {
        protected static void ClearTable(HtmlBuilder h, IEnumerable<Clear> arr)
        {
            h.TABLE_();
            var last = 0;
            foreach (var o in arr)
            {
                if (o.prtid != last)
                {
                    var spr = GrabObject<int, Org>(o.prtid);
                    h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 3).T(spr.name)._TD()._TR();
                }
                h.TR_();
                h.TD(o.till);
                h.TD(o.name);
                h._TR();

                last = o.prtid;
            }
            h._TABLE();
        }
    }


    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("供应链业务账表", "财务")]
    public class AdmlyBookLdgWork : LdgWork
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("消费业务账表", "财务")]
    public class AdmlyBuyLdgWork : LdgWork
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [OrglyAuthorize(Org.TYP_SHP, 1)]
    [Ui("销售总账", "商户")]
    public class ShplyBuyLdgWork : LdgWork
    {
        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM buyldgs WHERE orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Ldgr>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD(Buy.Typs[(short) o.acct]);
                    h.TD_("uk-text-right").T(o.trans)._TD();
                    h.TD_("uk-text-right").CNY(o.amt)._TD();
                }, thead: () =>
                {
                    h.TH("日期", css: "uk-width-medium");
                    h.TH("类型");
                    h.TH("笔数");
                    h.TH("金额");
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 60);
        }
    }


    [OrglyAuthorize(Org.TYP_SRC, 1)]
    [Ui("销售明细账", "商户")]
    public class SrclyBookLdgWork : LdgWork
    {
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM bookldgs WHERE orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = await dc.QueryAsync<Ldgr>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TABLE(arr, o =>
                {
                    h.TD_().T(o.dt, 3, 0)._TD();
                    var item = GrabObject<int, Item>(o.acct);
                    h.TD(item.name);
                    h.TD_("uk-text-right").T(o.trans)._TD();
                    h.TD_("uk-text-right").CNY(o.amt)._TD();
                }, thead: () =>
                {
                    h.TH("日期", css: "uk-width-medium");
                    h.TH("类型");
                    h.TH("笔数");
                    h.TH("金额");
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 60);
        }
    }
}