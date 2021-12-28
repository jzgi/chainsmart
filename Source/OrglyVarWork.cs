using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class OrglyVarWork : WebWork
    {
        [UserAuthorize(orgly: 15)]
        [Ui("操作权限"), Tool(ButtonOpen)]
        public async Task access(WebContext wc, int cmd)
        {
            var org = wc[0].As<Org>();
            short orgly = 0;
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE orgid = @1 AND orgly > 0");
            var arr = dc.Query<User>(p => p.Set(org.id));

            if (wc.IsGet)
            {
                string tel = wc.Query[nameof(tel)];
                wc.GivePage(200, h =>
                {
                    h.LIST(arr, o =>
                        {
                            h.SPAN_("uk-width-1-3").T(o.name).SP().SUB(o.tel)._SPAN();
                            h.SPAN(User.Orgly[o.orgly], "uk-width-1-3");
                        }
                    );
                    h.FORM_().FIELDSUL_("添加操作权限");
                    h.LI_("uk-flex").TEXT("用户手机", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(access), 1, post: false)._LI();
                    h._FIELDSUL();
                    if (cmd == 1) // search user
                    {
                        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                        var o = dc.QueryTop<User>(p => p.Set(tel));
                        if (o != null)
                        {
                            h.FIELDSUL_();
                            h.HIDDEN(nameof(o.id), o.id);
                            h.LI_().FIELD("用户姓名", o.name)._LI();
                            h.LI_().SELECT("权限", nameof(orgly), orgly, User.Orgly, filter: (k, v) => k > 0)._LI();
                            h._FIELDSUL();
                            h.BOTTOMBAR_().BUTTON("确认", nameof(access), 2)._BOTTOMBAR();
                        }
                    }
                    h._FORM();
                }, false, 3);
            }
            else
            {
                short orgid = wc[-1];
                var f = await wc.ReadAsync<Form>();
                int id = f[nameof(id)];
                orgly = f[nameof(orgly)];
                dc.Execute("UPDATE users SET orgid = @1, orgly = @2 WHERE id = @3", p => p.Set(orgid).Set(orgly).Set(id));
                wc.GivePane(200); // ok
            }
        }

        [Ui("运行设置"), Tool(ButtonShow)]
        public async Task setg(WebContext wc)
        {
            var org = wc[0].As<Org>();
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改基本设置");
                    h.LI_().TEXT("标语", nameof(org.tip), org.tip, max: 16)._LI();
                    h.LI_().TEXT("地址", nameof(org.addr), org.addr, max: 16)._LI();
                    h.LI_().SELECT("状态", nameof(org.status), org.status, _Info.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                var o = await wc.ReadObjectAsync(inst: org); // use existing object
                using var dc = NewDbContext();
                // update the db record
                await dc.ExecuteAsync("UPDATE orgs SET tip = @1, cttid = CASE WHEN @2 = 0 THEN NULL ELSE @2 END, status = @3 WHERE id = @4",
                    p => p.Set(o.tip).Set(o.status).Set(org.id));

                wc.GivePane(200);
            }
        }
    }

    [UserAuthorize(Org.TYP_BIZ, 1)]
#if ZHNT
    [Ui("市场端操作")]
#else
    [Ui("驿站端操作")]
#endif
    public class MrtlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<MrtlyOrgWork>("org");

            MakeWork<MrtlyUserWork>("user");

            MakeWork<MrtlyReportWork>("rpt");

            MakeWork<BizlyPostWork>("post");

            MakeWork<BizlyBuyWork>("buy");

            MakeWork<BizlyBookWork>("book");

            MakeWork<BizlyStationWork>("station");

            MakeWork<OrglyClearWork>("clear");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            wc.GivePage(200, h =>
            {
                var role = prin.orgid != org.id ? "代办" : User.Orgly[prin.orgly];
                h.TOOLBAR(caption: prin.name + "（" + role + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD("类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD(org.IsMrt ? "地址" : "编址", org.addr)._LI();
                if (org.sprid > 0)
                {
                    var spr = Obtain<int, Org>(org.sprid);
#if ZHNT
                    h.LI_().FIELD("所在市场", spr.name)._LI();
#else
                    h.LI_().FIELD("所在驿站", spr.name)._LI();
#endif
                }
                h.LI_().FIELD2("负责人", org.mgrname, org.mgrtel)._LI();
                h.LI_().FIELD("委托代办", org.trust)._LI();
                h._UL();
                h._FORM();

                h.TASKUL();
            }, false, 3);
        }
    }

    [UserAuthorize(Org.TYP_SRC, 1)]
    [Ui("产供端操作")]
    public class PrvlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<PrvlyOrgWork>("org");

            MakeWork<PrvlyReportWork>("rpt");

            MakeWork<SrclyPlanWork>("plan");

            MakeWork<SrclyBookWork>("bid");

            MakeWork<OrglyClearWork>("clear");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var co = Obtain<int, Org>(org.sprid);

            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                var role = prin.orgid != org.id ? "代办" : User.Orgly[prin.orgly];
                h.TOOLBAR(caption: prin.name + "（" + role + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD("类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD("地址", org.addr)._LI();
                if (!org.IsMrt)
                {
                    // h.LI_().FIELD("产源社", co.name)._LI();
                }
                h.LI_().FIELD2("负责人", org.mgrname, org.mgrtel)._LI();
                h._UL();
                h._FORM();

                h.TASKUL();
            }, false, 3);
        }
    }

    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("分拣中心操作")]
    public class CtrlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            MakeWork<CtrlyReachWork>("reach");

            MakeWork<CtrlyReceiveWork>("receive");

            MakeWork<CtrlyAgriBookWork, CtrlyFactBookWork, CtrlySrvcBookWork>("book");

            MakeWork<OrglyClearWork>("clear");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var regs = ObtainMap<short, Reg>();
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            wc.GivePage(200, h =>
            {
                var role = prin.orgid != org.id ? "代办" : User.Orgly[prin.orgly];
                h.TOOLBAR(caption: prin.name + "（" + role + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body ul-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD2("地址", regs[org.regid]?.name, org.addr)._LI();
                h.LI_().FIELD2("负责人", org.mgrname, org.mgrtel)._LI();
                h.LI_().FIELD("状态", _Info.Statuses[org.status])._LI();
                h._UL();
                h._FORM();

                h.TASKUL();
            }, false, 3);
        }
    }
}