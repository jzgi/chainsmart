using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class LdgWork<V> : WebWork where V : LdgVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }

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
    public class AdmlyBookLdgWork : LdgWork<AdmlyLdgVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [AdmlyAuthorize(User.ROL_FIN)]
    [Ui("消费业务账表", "财务")]
    public class AdmlyBuyLdgWork : LdgWork<AdmlyLdgVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            wc.GivePage(200, h => { h.TOOLBAR(); });
        }
    }

    [OrglyAuthorize(Org.TYP_ZON, 1)]
    [Ui("综合报表", "盟主")]
    public class ZonlyLdgWork : LdgWork<OrglyLdgVarWork>
    {
        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT * FROM rpts WHERE orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
            var arr = dc.Query<Rptie>(p => p.Set(org.id).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD(Rptie.Typs[o.typ]);
                    h.TD_().T(o.dt, 3, 0)._TD();
                    h.TD_().T(o.count).SP().T('笔')._TD();
                    h.TD(o.amt, money: true);
                });
                h.PAGINATION(arr?.Length == 30);
            }, false, 3);
        }

        [OrglyAuthorize(Org.TYP_ZON, User.ROL_MGT)]
        [Ui("生成"), Tool(Modal.ButtonOpen)]
        public async Task gen(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            short orgid = wc[-1];
            DateTime date;
            short typ = 0;
            decimal amt = 0;
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("指定统计区间");
                    h.LI_().DATE("从日期", nameof(date), DateTime.Today, required: true)._LI();
                    h.LI_().DATE("到日期", nameof(date), DateTime.Today, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                date = f[nameof(date)];
                date = f[nameof(date)];
                wc.GivePane(200); // close dialog
            }
        }
    }


    [OrglyAuthorize(Org.TYP_SRC, User.ROL_)]
    [Ui("业务报表", "商户")]
    public class SrclyLdgWork : LdgWork<OrglyLdgVarWork>
    {
        public async Task @default(WebContext wc, int page)
        {
        }
    }

    [OrglyAuthorize(Org.TYP_DST, User.ROL_)]
    [Ui("业务报表", "中库")]
    public class CtrlyLdgWork : LdgWork<OrglyLdgVarWork>
    {
        [Ui("待收", group: 1), Tool(Modal.Anchor)]
        public async Task @default(WebContext wc, int page)
        {
        }
    }
}