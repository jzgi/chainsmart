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

    public void @default(WebContext wc)
    {
        int orgid = wc[0];
        var regs = Grab<short, Reg>();

        var org = GrabTwin<int, Org>(orgid);

        wc.GivePage(200, h =>
            {
                if (org.pic)
                {
                    h.PIC_("/org/", org.id, "/pic");
                }
                else
                    h.PIC_("/void-shop.webp");

                h.ATEL(org.tel, css: "uk-overlay uk-position-top-right");
                h.SPAN(org.Ext, css: "uk-label uk-dark uk-overlay uk-position-bottom-right");
                h._PIC();

                h.BOTTOMBAR_().A_(nameof(lst), parent: true, css: "uk-button uk-button-default").T("进入市场")._A()._BOTTOMBAR();
            }
        );
    }

    /// <summary>
    /// The public home for a market.
    /// </summary>
    public void lst(WebContext wc, int sector)
    {
        int orgid = wc[0];
        var regs = Grab<short, Reg>();

        var org = GrabTwin<int, Org>(orgid);

        Org[] arr;
        if (sector == 0) // when default sector
        {
            arr = GrabTwinSet<int, Org>(orgid, x => x.regid == 0 && x.status == 4);
            arr = arr.AddOf(org, first: true);
        }
        else
        {
            arr = GrabTwinSet<int, Org>(orgid, x => x.regid == sector && x.status == 4);
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