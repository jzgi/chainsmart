using System;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

[UserAuthenticate(OmitDefault = true)]
public class PublyVarWork : WebWork
{
    protected override void OnCreate()
    {
        CreateVarWork<PublyItemWork>(); // home for one shop
    }

    public void @default(WebContext wc)
    {
        int orgid = wc[0];
        var org = GrabTwin<int, Org>(orgid);

        wc.GivePage(200, h =>
        {
            h.ARTICLE_("uk-card uk-card-primary");
            h.H2(org.Ext, css: "uk-card-header");
            h.SECTION_("uk-card-body");
            if (org.scene)
            {
                h.PIC_("/org/", org.id, "/scene");
            }
            else
            {
                h.PIC_("/void.webp");
            }
            h._PIC();

            h._SECTION();

            h.FOOTER_("uk-card-footer").T("地处").T(org.addr);
            if (org.tip != null)
            {
                h.T('，').T(org.tip);
            }
            h._FOOTER();

            h._ARTICLE();

            h.ARTICLE_("uk-card uk-card-primary");
            h.H3("免费派送区域", css: "uk-card-header");
            // h.SECTION_("uk-card-body");
            var specs = org.specs;
            for (int i = 0; i < specs?.Count; i++)
            {
                var spec = specs.EntryAt(i);
                var v = spec.Value;
                if (v.IsObject)
                {
                    h.DL_(css: "uk-card-body");
                    h.DT(spec.Key);

                    h.DD_();
                    var sub = (JObj)v;
                    for (int k = 0; k < sub.Count; k++)
                    {
                        if (k > 0) h.T('，');
                        var e = sub.EntryAt(k);
                        h.T(e.Key);
                    }
                    h._DD();
                    h._DL();
                }
            }
            // h._SECTION();
            h._ARTICLE();

            h.BOTTOMBAR_().A_(nameof(lst), parent: true, css: "uk-button uk-button-default").T("　进入市场").ICON("chevron-right")._A()._BOTTOMBAR();
        }, true, 720, org.Ext);
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
            h.NAVBAR(nameof(lst), sector, regs, (_, v) => v.IsSection, "star");

            if (sector != 0 && arr == null)
            {
                h.ALERT("尚无商户");
                return;
            }

            var now = DateTime.Now.TimeOfDay;
            h.MAINGRID(arr, o =>
            {
                var open = o.IsOpen(now);
                if (o.EqBrand)
                {
                    h.A_(o.addr, css: "uk-card-body uk-flex");
                }
                else
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.Title, inactive: !open, "uk-card-body uk-flex");
                }

                if (o.icon)
                {
                    h.PIC("/org/", o.id, "/icon", css: "uk-width-1-5");
                }
                else
                    h.PIC("/void.webp", css: "uk-width-1-5");

                h.ASIDE_();
                h.HEADER_().H4(o.Name).SPAN(open ? "营业" : "打烊", css: "uk-badge uk-badge-success")._HEADER();
                h.Q(o.tip, "uk-width-expand");
                h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                h._ASIDE();

                h._A();
            });
        }, true, 720, org.Ext);
    }
}