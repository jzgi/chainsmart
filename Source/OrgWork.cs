using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFX;
using ChainFX.Nodal;
using ChainFX.Web;
using static ChainFX.Web.Modal;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.ToolAttribute;

namespace ChainSmart;

public abstract class OrgWork<V> : WebWork where V : OrgVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IEnumerable<Org> lst, User prin, bool mktly)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            if (o.icon)
            {
                h.PIC(MainApp.WwwUrl, "/org/", o.id, "/icon", circle: true, css: "uk-width-1-6");
            }
            else
            {
                h.PIC("/void.webp", circle: true, css: "uk-width-1-6");
            }

            h.ASIDE_();
            h.HEADER_().H4(o.WholeName).SPAN(Entity.Statuses[o.status], "uk-badge")._HEADER();
            h.Q(o.tip, css: "uk-width-expand");
            h.FOOTER_().BUTTONDIALOG_(mktly ? "/mktly/" : "/suply/", o.Key, "/", mode: MOD_ASTACK, false, css: "uk-button-link uk-margin-auto-left").ICON("forward")._BUTTON()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }
}

public class PublyOrgWork : OrgWork<PublyOrgVarWork>
{
}

[Ui("成员机构")]
public class AdmlyEstWork : OrgWork<AdmlyEstVarWork>
{
    [Ui("市场机构", status: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        var prin = (User)wc.Principal;
        var arr = GrabTwinArray<int, Org>(0, filter: x => x.IsMkt, sorter: (x, y) => x.regid - y.regid).GetSegment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);
            if (arr == null)
            {
                h.ALERT("尚无市场机构");
                return;
            }

            MainGrid(h, arr, prin, true);
        }, false, 6);
    }

    [Ui("供应机构", status: 2), Tool(Anchor)]
    public void hub(WebContext wc)
    {
        var prin = (User)wc.Principal;
        var arr = GrabTwinArray<int, Org>(0, filter: x => x.IsHub, sorter: (k, v) => v.id - k.id);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);
            if (arr == null)
            {
                h.ALERT("尚无供应机构");
                return;
            }

            MainGrid(h, arr, prin, true);
        }, false, 6);
    }


    [Ui("新建", icon: "plus"), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int cmd)
    {
        var prin = (User)wc.Principal;
        var regs = Grab<short, Reg>();

        var o = new Org
        {
            typ = cmd switch
            {
                1 => Org.TYP_MKT, _ => Org.TYP_HUB
            },
            created = DateTime.Now,
            creator = prin.name,
        };

        if (wc.IsGet)
        {
            o.Read(wc.Query, 0);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(o.IsMkt ? "新建市场机构" : "新建供应机构");

                h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().TEXT("工商登记名", nameof(o.legal), o.legal, max: 20, required: true)._LI();
                h.LI_().TEXT("总体名", nameof(o.whole), o.whole, max: 12, required: true)._LI();
                h.LI_().SELECT("地市", nameof(o.regid), o.regid, regs, filter: (_, v) => v.IsCity, required: true)._LI();
                h.LI_().TEXT("地址", nameof(o.addr), o.addr, max: 30)._LI();
                h.LI_().NUMBER("经度", nameof(o.x), o.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(o.y), o.y, min: -90.000, max: 90.000)._LI();
                if (o.IsMkt)
                {
                    var hubs = GrabTwinArray<int, Org>(0, x => x.IsHub);
                    h.LI_().SELECT("业务模式", nameof(o.style), o.style, Org.Styles, (k, _) => k >= Org.STY_STN, required: true).SELECT("关联云仓", nameof(o.hubid), o.hubid, hubs, required: true)._LI();
                }
                h.LI_().TEXT("联系电话", nameof(o.tel), o.tel, pattern: "[0-9]+", max: 11, min: 11, required: true).CHECKBOX("托管", nameof(o.trust), true, o.trust)._LI();
                h.LI_().TEXT("收款账名", nameof(o.bankacctname), o.bankacctname, tip: "工商银行账户名称", max: 20, required: true)._LI();
                h.LI_().TEXT("收款账号", nameof(o.bankacct), o.bankacct, pattern: "[0-9]+", min: 19, max: 19, required: true)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short Msk = Entity.MSK_BORN | Entity.MSK_EDIT;
            await wc.ReadObjectAsync(Msk, o);

            await GetTwinCache<OrgCache, int, Org>().CreateAsync(async dc =>
            {
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, Msk)._VALUES_(Org.Empty, Msk).T(" RETURNING ").collst(Org.Empty);
                return await dc.QueryTopAsync<Org>(p => o.Write(p, Msk));
            });

            wc.GivePane(201); // created
        }
    }
}

[Ui("成员供应源")]
public class AdmlySupWork : OrgWork<AdmlySupVarWork>
{
    [Ui("供应商", status: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        var prin = (User)wc.Principal;
        var arr = GrabTwinArray<int, Org>(0, filter: x => !x.IsVoid && x.IsSup, sorter: (a, b) => a.status - b.status).GetSegment(30 * page, 30);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 1);
            if (arr == null)
            {
                h.ALERT("尚无供应商");
                return;
            }
            MainGrid(h, arr, prin, true);
            h.PAGINATION(arr.Count == 30);
        }, false, 6);
    }

    [Ui("产源", status: 2), Tool(Anchor)]
    public void src(WebContext wc, int page)
    {
        var prin = (User)wc.Principal;
        var arr = GrabTwinArray<int, Org>(0, filter: x => !x.IsVoid && x.IsSrc, sorter: (a, b) => a.status - b.status).GetSegment(30 * page, 30);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 2);
            if (arr == null)
            {
                h.ALERT("尚无产源");
                return;
            }
            MainGrid(h, arr, prin, true);
            h.PAGINATION(arr.Count == 30);
        }, false, 6);
    }

    [Ui(tip: "查询", icon: "search", status: 4), Tool(AnchorPrompt)]
    public void search(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var regs = Grab<short, Reg>();
        var prin = (User)wc.Principal;

        bool inner = wc.Query[nameof(inner)];
        short regid = 0;
        if (inner)
        {
            wc.GivePane(200, h => h.FORM_().RADIOSET(nameof(regid), regid, regs, filter: (k, v) => v.IsSector)._FORM());
        }
        else // OUTER
        {
            regid = wc.Query[nameof(regid)];
            var arr = GrabTwinArray<int, Org>(org.id, filter: x => x.regid == regid && x.AsMkt);

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: regid);

                if (arr == null)
                {
                    h.ALERT("该版块尚无主体");
                    return;
                }

                // MainGrid(h, arr, prin);
            }, false, 6);
        }
    }

    [Ui(icon: "trash", status: 8), Tool(Anchor)]
    public void @void(WebContext wc, int page)
    {
        var prin = (User)wc.Principal;
        var arr = GrabTwinArray<int, Org>(0, filter: x => x.IsVoid, sorter: (a, b) => b.oked.CompareTo(a.oked)).GetSegment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(subscript: 8);
            if (arr == null)
            {
                h.ALERT("尚无已作废供应源");
                return;
            }
            MainGrid(h, arr, prin, true);
            h.PAGINATION(arr.Count == 20);
        }, false, 6);
    }

    [Ui("新建", icon: "plus", status: 1 | 2), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int cmd)
    {
        var prin = (User)wc.Principal;
        var regs = Grab<short, Reg>();

        var o = new Org
        {
            typ = cmd switch
            {
                1 => Org.TYP_SUP, _ => Org.TYP_SRC
            },
            created = DateTime.Now,
            creator = prin.name,
            trust = true
        };

        if (wc.IsGet)
        {
            o.Read(wc.Query, 0);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(cmd switch { 1 => "新建供应商", _ => "新建产源" });

                h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().TEXT("工商登记名", nameof(o.legal), o.legal, max: 20, required: true)._LI();
                h.LI_().SELECT("地市", nameof(o.regid), o.regid, regs, filter: (_, v) => v.IsCity, required: true)._LI();
                h.LI_().TEXT("地址", nameof(o.addr), o.addr, max: 30)._LI();
                h.LI_().NUMBER("经度", nameof(o.x), o.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(o.y), o.y, min: -90.000, max: 90.000)._LI();
                h.LI_().TEXT("联系电话", nameof(o.tel), o.tel, pattern: "[0-9]+", max: 11, min: 11, required: true).CHECKBOX("托管", nameof(o.trust), true, o.trust)._LI();
                if (o.IsSup)
                {
                    h.LI_().TEXT("收款账名", nameof(o.bankacctname), o.bankacctname, tip: "工商银行账户名称", max: 20, required: true)._LI();
                    h.LI_().TEXT("收款账号", nameof(o.bankacct), o.bankacct, pattern: "[0-9]+", min: 19, max: 19, required: true)._LI();
                }
                if (o.IsSrc)
                {
                    var cats = Grab<short, Cat>();
                    var envs = Grab<short, Env>();
                    var syms = Grab<short, Sym>();
                    var tags = Grab<short, Tag>();

                    h.LI_().SELECT("品类", nameof(o.cat), o.cat, cats).SELECT("环境", nameof(o.env), o.env, envs)._LI();
                    h.LI_().SELECT("标志", nameof(o.sym), o.sym, syms).SELECT("溯源", nameof(o.tag), o.tag, tags)._LI();
                }

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short Msk = Entity.MSK_BORN | Entity.MSK_EDIT;
            await wc.ReadObjectAsync(Msk, o);

            await GetTwinCache<OrgCache, int, Org>().CreateAsync(async dc =>
            {
                dc.Sql("INSERT INTO orgs_vw ").colset(Org.Empty, Msk)._VALUES_(Org.Empty, Msk).T(" RETURNING ").collst(Org.Empty);
                return await dc.QueryTopAsync<Org>(p => o.Write(p, Msk));
            });

            wc.GivePane(201); // created
        }
    }
}

[MgtAuthorize(Org.TYP_MKT)]
public class MktlyOrgWork : OrgWork<MktlyOrgVarWork>
{
    private short OrgTyp => (short)State;

    static void MainGrid(HtmlBuilder h, IEnumerable<Org> lst, User prin)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/-", o.typ, MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            if (o.icon)
            {
                h.PIC(MainApp.WwwUrl, "/org/", o.id, "/icon", css: "uk-width-1-5");
            }
            else
            {
                h.PIC("/void.webp", css: "uk-width-1-5");
            }

            h.ASIDE_();
            h.HEADER_().H4(o.name).SPAN(Entity.Statuses[o.status], "uk-badge")._HEADER();
            h.Q(o.tip, "uk-width-expand");
            h.FOOTER_().BUTTONDIALOG_("/mktly/", o.Key, "/", mode: MOD_ASTACK, false, css: "uk-button-link uk-margin-auto-left").ICON("forward")._BUTTON()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }

    [Ui(status: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var arr = GrabTwinArray<int, Org>(org.id, filter: x => x.typ == OrgTyp && x.status == 4, sorter: (x, y) => x.addr.CompareWith(y.addr))
            .GetSegment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr.Count == 0)
            {
                h.ALERT("尚无上线的商户");
                return;
            }

            MainGrid(h, arr, prin);

            h.PAGINATION(arr.Count == 20);
        }, false, 6);
    }

    [Ui(tip: "已下线", icon: "cloud-download", status: 2), Tool(Anchor)]
    public void down(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var arr = GrabTwinArray<int, Org>(org.id, filter: x => x.typ == OrgTyp && x.status is 1 or 2, sorter: (x, y) => x.addr.CompareWith(y.addr))
            .GetSegment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr.Count == 0)
            {
                h.ALERT("尚无下线的商户");
                return;
            }

            MainGrid(h, arr, prin);

            h.PAGINATION(arr.Count == 20);
        }, false, 6);
    }

    [Ui(tip: "已禁用", icon: "trash", status: 4), Tool(Anchor)]
    public void @void(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var array = GrabTwinArray<int, Org>(org.id, filter: x => x.typ == OrgTyp && x.status == 0, sorter: (x, y) => x.addr.CompareWith(y.addr));
        var arr = array.GetSegment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr.Count == 0)
            {
                h.ALERT("尚无禁用的商户");
                return;
            }

            MainGrid(h, arr, prin);

            h.PAGINATION(arr.Count == 20);
        }, false, 6);
    }


    [Ui(tip: "按版块", icon: "search", status: 8), Tool(AnchorPrompt)]
    public void search(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var regs = Grab<short, Reg>();
        var prin = (User)wc.Principal;

        bool inner = wc.Query[nameof(inner)];
        short regid = 0;
        if (inner)
        {
            wc.GivePane(200, h => h.FORM_().RADIOSET(nameof(regid), regid, regs, filter: (k, v) => v.IsSector)._FORM());
        }
        else // OUTER
        {
            regid = wc.Query[nameof(regid)];
            var arr = GrabTwinArray<int, Org>(org.id, filter: x => x.regid == regid && x.AsMkt);

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: regid);

                if (arr == null)
                {
                    h.ALERT("该版块尚无商户");
                    return;
                }

                MainGrid(h, arr, prin);
            }, false, 6);
        }
    }

    [MgtAuthorize(Org.TYP_MKT, User.ROL_OPN)]
    [Ui("新建", icon: "plus", status: 2), Tool(ButtonOpen)]
    public async Task @new(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var regs = Grab<short, Reg>();
        var o = new Org
        {
            typ = OrgTyp,
            created = DateTime.Now,
            creator = prin.name,
            parentid = org.id,
            hubid = org.hubid,
            trust = true,
            status = Entity.STU_CREATED
        };

        if (wc.IsGet)
        {
            // o.Read(wc.Query, 0);
            wc.GivePane(200, h =>
            {
                h.FORM_("uk-card uk-card-primary").FIELDSUL_(o.IsMch ? "新建成员商户" : "新建成员门店");

                h.LI_().SELECT("版块", nameof(o.regid), o.regid, regs, filter: (_, v) => v.IsSectorWith(org.style), required: true).TEXT("编址", nameof(o.addr), o.addr, max: 12)._LI();
                h.LI_().TEXT("名称", nameof(o.name), o.name, max: 12, required: true)._LI();
                h.LI_().SELECT("输送模式", nameof(o.style), o.style, Org.Styles, filter: (k, _) => k <= Org.STY_SVC, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().TEXT("工商登记名", nameof(o.legal), o.legal, max: 20, required: true)._LI();
                h.LI_().TEXT("联系电话", nameof(o.tel), o.tel, pattern: "[0-9]+", max: 11, min: 11, required: true).CHECKBOX("托管", nameof(o.trust), true, o.trust)._LI();
                h.LI_().TEXT("收款账号", nameof(o.bankacct), o.bankacct, pattern: "[0-9]+", min: 19, max: 19)._LI();
                h.LI_().TEXT("收款户名", nameof(o.bankacctname), o.bankacctname, max: 20)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
            await wc.ReadObjectAsync(msk, instance: o);

            await GetTwinCache<OrgCache, int, Org>().CreateAsync(async dc =>
            {
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, msk)._VALUES_(Org.Empty, msk).T(" RETURNING ").collst(Org.Empty);
                return await dc.QueryTopAsync<Org>(p => o.Write(p, msk));
            });

            wc.GivePane(201); // created
        }
    }
}