using SkyChain.Web;

namespace Revital.Mart
{
    [UserAuthorize(Org.TYP_BIZ | Org.TYP_BIZCO, 1)]
    [Ui("商户端")]
    public class BizlyVarWork : OrglyVarWork
    {
        protected override void OnMake()
        {
            // MakeWork<BizlyBookWork>("buy");

            MakeWork<OrglyClearWork>("clear");

            MakeWork<BizColyOrgWork>("org");

            MakeWork<OrglyAccessWork>("access", User.Orgly);
        }

        public void @default(WebContext wc)
        {
            short orgid = wc[0];
            var org = Obtain<short, Org>(orgid);
            var co = Obtain<int, Org>(org.sprid);
            var ctr = Obtain<int, Org>(org.ctrid);

            var prin = (User) wc.Principal;
            using var dc = NewDbContext();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name + "（" + User.Orgly[prin.orgly] + "）");

                h.UL_("uk-card uk-card-primary uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD("协作类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD("地址", org.addr)._LI();
                if (!org.IsBizCo)
                {
                    h.LI_().FIELD("商户社", co.name)._LI();
                }
                h.LI_().FIELD("分拣中心", ctr.name)._LI();
                h.LI_().FIELD2("负责人", org.mgrname, org.mgrtel)._LI();
                h.LI_().FIELD("授权代办", org.trust)._LI();
                h._UL();

                h.TASKUL();
            }, false, 3);
        }
    }
}