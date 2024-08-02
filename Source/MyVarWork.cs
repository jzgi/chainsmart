using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Storage;
using static ChainFX.Web.Modal;

namespace ChainSmart;

[MyAuthorize]
[Ui("我的个人账号")]
public class MyVarWork : BuyWork<MyBuyVarWork>
{
    public async Task @default(WebContext wc)
    {
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE uid = @1 AND status <> 1 ORDER BY id DESC LIMIT 20");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(prin.id));

        wc.GivePage(200, h =>
        {
            h.TOPBARXL_();

            h.HEADER_("uk-width-expand uk-col uk-padding-left");
            h.H1(prin.name);
            h.H4(prin.tel);
            h.P_().T("注册").SP().T(prin.created, time: 0);
            if (prin.typ > 0)
            {
                var cers = Grab<short, Cer>();
                h.SP().MARK(cers[prin.typ]?.ToString());
            }
            h._P();
            h._HEADER();

            if (prin.icon)
            {
                h.IMG("/user/", prin.id, "/icon/", circle: true, css: "uk-width-medium");
            }
            else
            {
                h.IMG("/my.webp", circle: true, css: "uk-width-small");
            }
            h._TOPBARXL();

            // buy orders
            if (arr == null)
            {
                h.ALERT("尚无消费订单");
            }
            else
            {
                h.MAINGRID(arr, o =>
                {
                    h.HEADER_("uk-card-header").H4_().T(o.id, 10).SP().T(o.name)._H4().SPAN_("uk-badge").T(o.adapted, time: 0).SP().MARK(Buy.Statuses[o.status])._SPAN()._HEADER();
                    h.UL_("uk-card-body uk-list-small uk-list-divider");
                    foreach (var ln in o.lns)
                    {
                        h.LI_("uk-flex");

                        h.SPAN_("uk-width-expand").T(ln.name);
                        if (ln.unitip != null)
                        {
                            h.SP().SMALL_().T(ln.unitip).T(ln.unit)._SMALL();
                        }
                        // h.SP().CNY(it.RealPrice);
                        h._SPAN();

                        h.SPAN_("uk-width-1-5 uk-flex-right").CNY(ln.RealPrice)._SPAN();
                        h.SPAN_("uk-width-1-6 uk-flex-right").T(ln.qty).SP().T(ln.unit)._SPAN();
                        h.SPAN_("uk-width-1-5 uk-flex-right").CNY(ln.SubTotal)._SPAN();
                        h._LI();
                    }
                    h._UL();

                    h.FOOTER_("uk-card-footer");
                    h.SPAN_("uk-width-expand").SMALL_().T(o.uarea).T(o.uaddr)._SMALL()._SPAN();
                    if (o.fee > 0)
                    {
                        h.SMALL_().T("运费 +").T(o.fee)._SMALL();
                    }
                    h.SPAN_("uk-width-1-5 uk-flex-right").CNY(o.topay)._SPAN();
                    h._FOOTER();
                });
            }

            h.TOOLBAR(bottom: true, status: prin.Status, state: prin.ToState());
        }, false, 12);
    }

    [Ui("设置", "我的账号信息显示及设置", status: 7), Tool(ButtonShow)]
    public async Task setg(WebContext wc)
    {
        const string PASSWORD_MASK = "t#0^0z4R4pX7";

        int uid = wc[0];

        string name;
        string tel;
        string password;
        var prin = (User)wc.Principal;
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE id = @1");
            var o = await dc.QueryTopAsync<User>(p => p.Set(uid));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("身份权限");

                var any = 0;

                if (o.admly > 0)
                {
                    h.LI_().SPAN(Application.Nodal.name, css: "uk-label").SPAN(User.Roles[o.admly], "uk-margin-auto-left")._LI();
                    any++;
                }

                if (o.mktly > 0 && o.mktid > 0)
                {
                    var org = GrabTwin<int, Org>(o.mktid);

                    h.LI_().SPAN(org.name, css: "uk-label").SPAN(User.Roles[o.mktly], "uk-margin-auto-left")._LI();
                    any++;
                }

                if (o.suply > 0 && o.supid > 0)
                {
                    var org = GrabTwin<int, Org>(o.supid);

                    h.LI_().SPAN(org.name, css: "uk-label").SPAN(User.Roles[o.suply], "uk-margin-auto-left")._LI();
                    any++;
                }

                var vip = o.vip;
                for (var i = 0; i < vip?.Length; i++)
                {
                    var orgid = vip[i];

                    var org = GrabTwin<int, Org>(orgid);
                    if (org != null)
                    {
                        h.LI_().SPAN(org.name, css: "uk-label").SPAN("大客户", "uk-margin-auto-left")._LI();
                        any++;
                    }
                }

                if (any == 0)
                {
                    h.LI_().SPAN(Application.Nodal.name, css: "uk-label").SPAN("普通消费者", "uk-margin-auto-left")._LI();
                }

                h._FIELDSUL()._FORM();

                // info
                //

                name = prin.name;
                tel = prin.tel;
                password = string.IsNullOrEmpty(prin.credential) ? null : PASSWORD_MASK;

                h.FORM_().FIELDSUL_("账号信息");
                h.LI_().TEXT("姓名", nameof(name), name, max: 12, min: 2, required: true, @readonly: prin.IsStationOp)._LI();
                h.LI_().TEXT("登录手机号", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true, @readonly: true);
                if (prin.IsStationOp)
                {
                    h.LI_().PASSWORD("终端密码", nameof(password), password, max: 12, min: 3)._LI();
                }
                h._FIELDSUL().BOTTOM_BUTTON("确定", nameof(setg))._FORM();
            }, false, 12);

            // resend token cookie
            wc.SetTokenCookies(o);
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            name = f[nameof(name)];

            tel = f[nameof(tel)];

            password = f[nameof(password)];
            var credential = string.IsNullOrEmpty(password) ? null :
                password == PASSWORD_MASK ? prin.credential :
                MainUtility.ComputeCredential(tel, password);

            using var dc = NewDbContext();
            dc.Sql("UPDATE users SET name = @1, tel = @2, credential = @3 WHERE id = @4 RETURNING ").collst(User.Empty);
            prin = await dc.QueryTopAsync<User>(p => p.Set(name).Set(tel).Set(credential).Set(prin.id));

            // refresh cookies
            wc.SetTokenCookies(prin);

            wc.GivePane(200); // close
        }
    }

    [Ui("须知", "本系统的使用条款", status: 7), Tool(ButtonShow)]
    public async Task agmt(WebContext wc, int dt)
    {
        wc.GivePane(200, h =>
        {
            h.ARTICLE_("uk-card uk-card-primary");
            h.H2("平台用户协议", css: "uk-card-header");
            h.SECTION_("uk-card-body")._SECTION();
            h._ARTICLE();
        }, false, 3600);
    }
}