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

    /// <summary>
    /// The market homepage. 
    /// </summary>
    public void @default(WebContext wc)
    {
        int orgid = wc[0];
        var org = GrabTwin<int, Org>(orgid);

        // whether in the intermediate dialog or full page
        bool inner = wc.Query[nameof(inner)];

        wc.GivePage(200, h =>
        {
            lock (org)
            {
                h.ARTICLE_("uk-card uk-card-primary");
                h.H2(org.Cover, css: "uk-card-header");
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
                h.H3("统一派送区域", css: "uk-card-header");
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
                            h.T(sub.KeyAt(k));
                        }
                        h._DD();
                        h._DL();
                    }
                }
                h._ARTICLE();

                h.BOTTOMBAR_().A_(nameof(lst), parent: inner, css: "uk-button uk-button-default").T("　进入市场").ICON("chevron-right")._A()._BOTTOMBAR();
            }
        }, true, 720, org.Cover);
    }

    /// <summary>
    /// The list of shops by sector
    /// </summary>
    public void lst(WebContext wc, int sector)
    {
        if (sector == 0)
        {
            sector = wc.Subscript = Reg.SVC_REG_ID;
        }

        int mktid = wc[0];
        var regs = Grab<short, Reg>();

        var mkt = GrabTwin<int, Org>(mktid);

        Org[] arr;
        if (sector == Reg.SVC_REG_ID) // when default sector
        {
            arr = GrabTwinSet<int, Org>(mktid, x => x.regid == Reg.SVC_REG_ID && x.status == 4);
            arr = arr.AddOf(mkt, first: true);
        }
        else
        {
            arr = GrabTwinSet<int, Org>(mktid, x => x.regid == sector && x.status == 4);
        }

        wc.GivePage(200, h =>
        {
            h.NAVBAR(nameof(lst), sector, regs, (_, v) => v.IsSector);

            if (arr == null)
            {
                h.ALERT("尚无上线商户");
                return;
            }

            var now = DateTime.Now.TimeOfDay;
            h.MAINGRID(arr, m =>
            {
                lock (m)
                {
                    var open = m.IsOpen(now);
                    h.ADIALOG_(m.Key, "/", MOD_OPEN, false, tip: m.Title, inactive: !open, "uk-card-body uk-flex");

                    if (m.icon)
                    {
                        h.PIC("/org/", m.id, "/icon", css: "uk-width-1-5");
                    }
                    else
                        h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H4(m.Name).SPAN(open ? "营业" : "打烊", css: "uk-badge uk-badge-success")._HEADER();
                    h.Q(m.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                }
            });
        }, true, 720, mkt.Cover);
    }
}