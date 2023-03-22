using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart
{
    /// 
    /// The home for a market
    ///
    [UserAuthenticate]
    public class PublyVarWork : WebWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyItemWork>(); // home for one shop
        }

        public async Task @default(WebContext wc, int sec)
        {
            int orgid = wc[0];
            var org = GrabObject<int, Org>(orgid);
            var regs = Grab<short, Reg>();

            Org[] arr;
            if (sec == 0) // when default section
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND regid IS NULL AND status = 4 ORDER BY addr");
                arr = await dc.QueryAsync<Org>(p => p.Set(orgid));
                arr = arr.AddOf(org, first: true);
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND regid = @2 AND status = 4 ORDER BY addr");
                arr = await dc.QueryAsync<Org>(p => p.Set(orgid).Set(sec));
            }

            wc.GivePage(200, h =>
            {
                h.NAVBAR(string.Empty, sec, regs, (k, v) => v.IsSection, "star");

                if (sec != 0 && arr == null)
                {
                    h.ALERT("尚无商户");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    if (o.IsVirtual)
                    {
                        h.A_(o.addr, css: "uk-card-body uk-flex");
                    }
                    else
                    {
                        h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.Name, css: "uk-card-body uk-flex");
                    }

                    if (o.icon)
                    {
                        h.PIC("/org/", o.id, "/icon", css: "uk-width-1-5");
                    }
                    else
                        h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H4(o.Name).SPAN("")._HEADER();
                    h.Q(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            }, shared: sec > 0, 900, org.Ext); // shared cache when no personal data
        }
    }
}