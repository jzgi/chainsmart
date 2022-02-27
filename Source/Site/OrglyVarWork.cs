using SkyChain.Web;

namespace Revital.Site
{
    [UserAuthorize(Org.TYP_BIZ, 1)]
#if ZHNT
    [Ui("市场端操作")]
#else
    [Ui("驿站端操作")]
#endif
    public class MrtlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            CreateWork<MrtlyOrgWork>("org");

            CreateWork<MrtlyUserWork>("user");

            CreateWork<MrtlyDailyWork>("daily");

            CreateWork<BizlyPieceWork>("piece");

            CreateWork<BizlyBuyWork>("buy");

            CreateWork<BizlyShopWork>("shop");

            CreateWork<BizlyBookWork>("book");

            CreateWork<OrglyClearWork>("clear");

            // CreateWork<OrglyMsgWork>("msg");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            wc.GivePage(200, h =>
            {
                var role = prin.orgid != org.id ? "代办" : User.Orgly[prin.orgly];
                h.TOOLBAR(tip: prin.name + "（" + role + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD("类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD(org.IsMrt ? "地址" : "编址", org.addr)._LI();
                if (org.sprid > 0)
                {
                    var spr = GrabObject<int, Org>(org.sprid);
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

                h.TASKLIST();
            }, false, 3);
        }
    }


    [UserAuthorize(Org.TYP_CTR, 1)]
    [Ui("中枢操作")]
    public class CtrlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            CreateWork<CtrlyBookWork>("book");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var regs = Grab<short, Reg>();
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            wc.GivePage(200, h =>
            {
                var role = prin.orgid != org.id ? "代办" : User.Orgly[prin.orgly];
                h.TOOLBAR(tip: prin.name + "（" + role + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body ul-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD2("地址", regs[org.regid]?.name, org.addr)._LI();
                h.LI_().FIELD2("管理员", org.mgrname, org.mgrtel)._LI();
                h.LI_().FIELD("状态", Info.Statuses[org.status])._LI();
                h._UL();
                h._FORM();

                h.TASKLIST();
            }, false, 3);
        }
    }
}