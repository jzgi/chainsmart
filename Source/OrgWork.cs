using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Nodal;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class OrgWork<V> : WebWork where V : OrgVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }
}

public class PublyOrgWork : OrgWork<PublyOrgVarWork>
{
}

[AdmlyAuthorize(User.ROL_OPN)]
[Ui("机构管理")]
public class AdmlyOrgWork : OrgWork<AdmlyOrgVarWork>
{
    protected static void MainGrid(HtmlBuilder h, IList<Org> lst, User prin, bool rtlly)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            if (o.icon)
            {
                h.PIC(MainApp.WwwUrl, "/org/", o.id, "/icon", css: "uk-width-1-5");
            }
            else
                h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name).SPAN(Org.Statuses[o.status], "uk-badge")._HEADER();
            h.Q2(o.Cover, o.tip, css: "uk-width-expand");
            h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR((rtlly ? "/rtlly/" : "/suply/"), o.Key, "/", icon: "link", disabled: !prin.CanBeUpperOf(o))._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }

    [Ui("市场机构", status: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        var prin = (User)wc.Principal;
        var array = GrabTwinSet<int, Org>(0, filter: x => x.IsMarket, sorter: (x, y) => x.regid - y.regid);
        var arr = array.Segment(20 * page, 20);

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
    public void ctr(WebContext wc)
    {
        var prin = (User)wc.Principal;
        var arr = GrabTwinSet<int, Org>(0, filter: x => x.IsCenter, sorter: (x, y) => y.id - x.id);

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

    [Ui("新建", icon: "plus", status: 3), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int cmd)
    {
        var prin = (User)wc.Principal;
        var regs = Grab<short, Reg>();

        var o = new Org
        {
            typ = cmd == 1 ? Org.TYP_MKT : Org.TYP_CTR,
            created = DateTime.Now,
            creator = prin.name,
        };

        if (wc.IsGet)
        {
            o.Read(wc.Query, 0);

            var ctrs = GrabTwinSet<int, Org>(0, x => x.IsCenter);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(cmd == 1 ? "新建市场机构" : "新建供应机构");

                h.LI_().TEXT("商户名", nameof(o.name), o.name, min: 2, max: 12, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().TEXT("工商登记名", nameof(o.legal), o.legal, max: 20, required: true)._LI();
                h.LI_().TEXT("涵盖市场名", nameof(o.cover), o.cover, max: 12, required: true)._LI();
                h.LI_().SELECT("地市", nameof(o.regid), o.regid, regs, filter: (_, v) => v.IsCity, required: true)._LI();
                h.LI_().TEXT("地址", nameof(o.addr), o.addr, max: 30)._LI();
                h.LI_().NUMBER("经度", nameof(o.x), o.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(o.y), o.y, min: -90.000, max: 90.000)._LI();
                if (cmd == 1)
                {
                    h.LI_().SELECT("关联品控仓", nameof(o.hubid), o.hubid, ctrs, required: true)._LI();
                }
                h.LI_().TEXT("联系电话", nameof(o.tel), o.tel, pattern: "[0-9]+", max: 11, min: 11, required: true).CHECKBOX("托管", nameof(o.trust), true, o.trust)._LI();
                h.LI_().TEXT("收款账号", nameof(o.bankacct), o.bankacct, pattern: "[0-9]+", min: 19, max: 19, required: true)._LI();
                h.LI_().TEXT("收款账号名", nameof(o.bankacctname), o.bankacctname, max: 20, required: true)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short Msk = Entity.MSK_BORN | Entity.MSK_EDIT;
            await wc.ReadObjectAsync(Msk, o);

            await GetGraph<OrgGraph, int, Org>().CreateAsync(async dc =>
            {
                dc.Sql("INSERT INTO orgs_vw ").colset(Org.Empty, Msk)._VALUES_(Org.Empty, Msk).T(" RETURNING ").collst(Org.Empty);
                return await dc.QueryTopAsync<Org>(p => o.Write(p, Msk));
            });

            wc.GivePane(201); // created
        }
    }
}

[OrglyAuthorize(Org.TYP_MKT)]
[Ui("成员商户")]
public class MktlyOrgWork : OrgWork<MktlyOrgVarWork>
{
    static void MainGrid(HtmlBuilder h, IList<Org> lst, User prin)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/-", o.typ, MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            if (o.icon)
            {
                h.PIC(MainApp.WwwUrl, "/org/", o.id, "/icon", css: "uk-width-1-5");
            }
            else
                h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name).SPAN(Org.Statuses[o.status], "uk-badge")._HEADER();
            h.Q(o.tip, "uk-width-expand");
            h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR("/rtlly/", o.Key, "/", icon: "link", disabled: !prin.CanBeUpperOf(o))._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }

    [Ui("成员商户", status: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var array = GrabTwinSet<int, Org>(org.id, filter: x => x.IsRetail && x.status == 4, sorter: (x, y) => x.addr.CompareWith(y.addr));
        var arr = array.Segment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr.Count == 0)
            {
                h.ALERT("尚无上线的成员商户");
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
        var array = GrabTwinSet<int, Org>(org.id, filter: x => x.IsRetail && x.status is 1 or 2, sorter: (x, y) => x.addr.CompareWith(y.addr));
        var arr = array.Segment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr.Count == 0)
            {
                h.ALERT("尚无下线的成员商户");
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
        var array = GrabTwinSet<int, Org>(org.id, filter: x => x.IsRetail && x.status == 0, sorter: (x, y) => x.addr.CompareWith(y.addr));
        var arr = array.Segment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr.Count == 0)
            {
                h.ALERT("尚无禁用的成员商户");
                return;
            }

            MainGrid(h, arr, prin);

            h.PAGINATION(arr.Count == 20);
        }, false, 6);
    }


    [Ui(tip: "按版块查找", icon: "search", status: 8), Tool(AnchorPrompt)]
    public void section(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var regs = Grab<short, Reg>();
        var prin = (User)wc.Principal;

        bool inner = wc.Query[nameof(inner)];
        short regid = 0;
        if (inner)
        {
            wc.GivePane(200, h => h.FORM_().RADIOSET(nameof(regid), regid, regs, filter: v => v.IsSector)._FORM());
        }
        else // OUTER
        {
            regid = wc.Query[nameof(regid)];
            var arr = GrabTwinSet<int, Org>(org.id, filter: x => x.regid == regid && x.IsRetail);

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

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("新建", "新建成员商户", icon: "plus", status: 2), Tool(ButtonOpen)]
    public async Task @new(WebContext wc, int regid)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var regs = Grab<short, Reg>();
        var o = new Org
        {
            typ = Org.TYP_RTL,
            created = DateTime.Now,
            creator = prin.name,
            upperid = org.id,
            hubid = org.hubid,
            regid = (short)regid,
            status = Entity.STU_CREATED
        };

        if (wc.IsGet)
        {
            o.Read(wc.Query, 0);
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("商户信息");

                h.LI_().SELECT("版块", nameof(o.regid), o.regid, regs, filter: (_, v) => v.IsSector, @readonly: regid > 0, required: true).TEXT("编号或场址", nameof(o.addr), o.addr, max: 12)._LI();
                h.LI_().TEXT("商户名", nameof(o.name), o.name, max: 12, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().TEXT("工商登记名", nameof(o.legal), o.legal, max: 20, required: true)._LI();
                h.LI_().TEXT("联系电话", nameof(o.tel), o.tel, pattern: "[0-9]+", max: 11, min: 11, required: true).CHECKBOX("托管", nameof(o.trust), true, o.trust)._LI();
                h.LI_().TEXTAREA("说明", nameof(o.descr), o.descr, max: 100)._LI();
                h.LI_().TEXT("收款账号", nameof(o.bankacct), o.bankacct, pattern: "[0-9]+", min: 19, max: 19, required: true)._LI();
                h.LI_().TEXT("收款账号名", nameof(o.bankacctname), o.bankacctname, max: 20, required: true)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short Msk = Entity.MSK_BORN | Entity.MSK_EDIT;
            await wc.ReadObjectAsync(Msk, instance: o);

            await GetGraph<OrgGraph, int, Org>().CreateAsync(async dc =>
            {
                dc.Sql("INSERT INTO orgs_vw ").colset(Org.Empty, Msk)._VALUES_(Org.Empty, Msk).T(" RETURNING ").collst(Org.Empty);
                return await dc.QueryTopAsync<Org>(p => o.Write(p, Msk));
            });

            wc.GivePane(201); // created
        }
    }
}

[OrglyAuthorize(Org.TYP_CTR)]
[Ui("成员商户")]
public class CtrlyOrgWork : OrgWork<CtrlyOrgVarWork>
{
    static void MainGrid(HtmlBuilder h, IList<Org> lst, User prin)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

            if (o.icon)
            {
                h.PIC(MainApp.WwwUrl, "/org/", o.id, "/icon", css: "uk-width-1-5");
            }
            else
                h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name).SPAN(Org.Statuses[o.status], "uk-badge")._HEADER();
            h.Q(o.tip, "uk-width-expand");
            h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR("/suply/", o.Key, "/", icon: "link", disabled: !prin.CanBeUpperOf(o))._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }

    [Ui("成员商户"), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        var array = GrabTwinSet<int, Org>(org.id, filter: x => x.status == 4, sorter: (x, y) => x.oked.CompareTo(y.oked));
        var arr = array.Segment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无上线商户");
                return;
            }

            MainGrid(h, arr, prin);
        }, false, 12);
    }

    [Ui(tip: "已下线", icon: "cloud-download"), Tool(Anchor)]
    public void down(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        var array = GrabTwinSet<int, Org>(org.id, filter: x => x.status is 1 or 2, sorter: (x, y) => x.oked.CompareTo(y.oked));
        var arr = array.Segment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无下线的商户");
                return;
            }

            MainGrid(h, arr, prin);
        }, false, 15);
    }

    [Ui(tip: "已删除", icon: "trash"), Tool(Anchor)]
    public void @void(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        var array = GrabTwinSet<int, Org>(org.id, filter: x => x.status == 0, sorter: (x, y) => x.adapted.CompareTo(y.adapted));
        var arr = array.Segment(20 * page, 20);

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            if (arr == null)
            {
                h.ALERT("尚无删除商户");
                return;
            }

            MainGrid(h, arr, prin);
        }, false, 15);
    }

    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui("新建", "新建成员商户", icon: "plus"), Tool(ButtonOpen)]
    public async Task @new(WebContext wc)
    {
        var zon = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        var regs = Grab<short, Reg>();

        var o = new Org
        {
            typ = Org.TYP_SUP,
            upperid = zon.id,
            created = DateTime.Now,
            creator = prin.name,
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_();

                h.LI_().TEXT("商户名", nameof(o.name), o.name, max: 12, required: true)._LI();
                h.LI_().TEXTAREA("简介语", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().TEXT("工商登记名", nameof(o.legal), o.legal, max: 20, required: true)._LI();
                h.LI_().SELECT("省份", nameof(o.regid), o.regid, regs, filter: (_, v) => v.IsProvince, required: true)._LI();
                h.LI_().TEXT("联系地址", nameof(o.addr), o.addr, max: 30)._LI();
                h.LI_().NUMBER("经度", nameof(o.x), o.x, min: 0.0000, max: 180.0000).NUMBER("纬度", nameof(o.y), o.y, min: -90.000, max: 90.000)._LI();
                h.LI_().TEXT("联系电话", nameof(o.tel), o.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                h.LI_().CHECKBOX("委托代办", nameof(o.trust), true, o.trust)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short Msk = Entity.MSK_BORN | Entity.MSK_EDIT;
            await wc.ReadObjectAsync(Msk, instance: o);

            await GetGraph<OrgGraph, int, Org>().CreateAsync(async dc =>
            {
                dc.Sql("INSERT INTO orgs_vw ").colset(Org.Empty, Msk)._VALUES_(Org.Empty, Msk).T(" RETURNING ").collst(Org.Empty);
                return await dc.QueryTopAsync<Org>(p => o.Write(p, Msk));
            });

            wc.GivePane(201); // created
        }
    }
}