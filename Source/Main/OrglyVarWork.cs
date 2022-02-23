using SkyChain.Web;

namespace Revital.Main
{
    [UserAuthorize(Org.TYP_SRC, 1)]
    public class PrvlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            CreateWork<PrvlyOrgWork>("org");

            CreateWork<PrvlyDailyWork>("daily");

            CreateWork<SrclyProductWork>("prod");

            CreateWork<SrclyCtrBookWork, SrclyElseBookWork>("book");

            CreateWork<OrglyClearWork>("clear");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var co = GrabObject<int, Org>(org.sprid);
            var prin = (User) wc.Principal;

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(tip: prin.name + "（" + wc.Role + "）");

                h.FORM_("uk-card uk-card-primary");
                h.UL_("uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("主体名称", org.name)._LI();
                h.LI_().FIELD("类型", Org.Typs[org.typ])._LI();
                h.LI_().FIELD("地址", org.addr)._LI();
                if (!org.IsPrt)
                {
                    h.LI_().FIELD("供给板块", co.name)._LI();
                }
                h.LI_().FIELD2("管理员", org.mgrname, org.mgrtel)._LI();
                h._UL();
                h._FORM();

                h.TASKLIST();
            }, false, 3);
        }
    }
}