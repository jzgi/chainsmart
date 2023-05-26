using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

[UserAuthenticate]
public class PublyVarWork : WebWork
{
    protected override void OnCreate()
    {
        CreateVarWork<PublyItemWork>(); // home for one shop
    }

    /// <summary>
    /// The public home for a market.
    /// </summary>
    public void @default(WebContext wc, int sector)
    {
        int orgid = wc[0];
        var regs = Grab<short, Reg>();

        var org = GrabTwin<int, Org>(orgid);

        Org[] arr;
        if (sector == 0) // when default sector
        {
            arr = GrabTwinArray<int,  Org>(orgid, x => x.regid == 0 && x.status == 4);
            arr = arr.AddOf(org, first: true);
        }
        else
        {
            arr = GrabTwinArray<int, Org>(orgid, x => x.regid == sector && x.status == 4);
        }

        wc.GivePage(200, h =>
        {
            h.NAVBAR(string.Empty, sector, regs, (_, v) => v.IsSection, "star");

            if (sector != 0 && arr == null)
            {
                h.ALERT("尚无商户");
                return;
            }

            h.MAINGRID(arr, o =>
            {
                if (o.EqBrand)
                {
                    h.A_(o.addr, css: "uk-card-body uk-flex");
                }
                else
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.Title, css: "uk-card-body uk-flex");
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
        }, true, 360, org.Ext);
    }
}