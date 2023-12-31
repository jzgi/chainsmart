using System.Threading.Tasks;
using ChainFX;
using ChainFX.Web;
using static ChainFX.Nodal.Nodality;
using static ChainFX.Web.Modal;

namespace ChainSmart;

[MyAuthorize]
[Ui("我的个人账号")]
[Help("用户可以查看自己的消费订单，获取平台发放的资源奖励、以及管理个人账号信息")]
public class MyVarWork : BuyWork<MyBuyVarWork>
{
    public async Task @default(WebContext wc)
    {
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE uid = @1 AND status > -1 ORDER BY id DESC LIMIT 20");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(prin.id));

        wc.GivePage(200, h =>
        {
            h.TOPBARXL_();

            h.HEADER_("uk-width-expand uk-col uk-padding-left");
            h.H1(prin.name);
            h.H4_().T(prin.tel)._H4();
            h.P_().T(prin.created, time: 0).SP().T("注册");
            if (prin.typ > 0)
            {
                h.T('，').T(User.Typs[prin.typ]);
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
                    h.HEADER_("uk-card-header").H4(o.name).SPAN_("uk-badge").T(o.created, time: 0).SP().T(Buy.Statuses[o.status])._SPAN()._HEADER();
                    h.UL_("uk-card-body uk-list-divider");
                    foreach (var it in o.items)
                    {
                        h.LI_("uk-flex");

                        h.SPAN_("uk-width-expand").T(it.name);
                        if (it.unitw > 0)
                        {
                            h.SP().SMALL_().T(it.unitw).T(it.unit)._SMALL();
                        }
                        // h.SP().CNY(it.RealPrice);
                        h._SPAN();

                        h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.RealPrice)._SPAN();
                        h.SPAN_("uk-width-1-6 uk-flex-right").T(it.qty).SP().T(it.unit)._SPAN();
                        h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.SubTotal)._SPAN();
                        h._LI();
                    }
                    h._UL();

                    h.FOOTER_("uk-card-footer");
                    h.SPAN_("uk-width-expand").SMALL_().T(o.ucom).T(o.uaddr)._SMALL()._SPAN();
                    if (o.fee > 0)
                    {
                        h.SMALL_().T("派送到楼下 +").T(o.fee)._SMALL();
                    }
                    h.SPAN_("uk-width-1-5 uk-flex-right").CNY(o.topay)._SPAN();
                    h._FOOTER();
                });
            }

            h.TOOLBAR(bottom: true, status: prin.Status, state: prin.ToState());
        }, false, 12);
    }

    [Ui("身份", "刷新我的身份权限", status: 7), Tool(ButtonShow)]
    public async Task access(WebContext wc)
    {
        int uid = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE id = @1");
        var o = await dc.QueryTopAsync<User>(p => p.Set(uid));

        wc.GivePane(200, h =>
        {
            h.ARTICLE_("uk-card uk-card-primary");
            h.H2("我的身份和权限", css: "uk-card-header");
            h.UL_("uk-card-body uk-list uk-list-divider");

            var any = 0;

            if (o.admly > 0)
            {
                h.LI_().T(Application.Node.name).SPAN(User.Roles[o.admly], "uk-margin-auto-left")._LI();
                any++;
            }

            if (o.rtlly > 0 && o.rtlid > 0)
            {
                var org = GrabTwin<int, Org>(o.rtlid);

                h.LI_().T(org.name).SPAN(User.Roles[o.rtlly], "uk-margin-auto-left")._LI();
                any++;
            }

            if (o.suply > 0 && o.supid > 0)
            {
                var org = GrabTwin<int, Org>(o.supid);

                h.LI_().T(org.name).SPAN(User.Roles[o.suply], "uk-margin-auto-left")._LI();
                any++;
            }

            var vip = o.vip;
            for (var i = 0; i < vip?.Length; i++)
            {
                var orgid = vip[i];

                var org = GrabTwin<int, Org>(orgid);
                if (org != null)
                {
                    h.LI_().T(org.name).SPAN("VIP", "uk-margin-auto-left")._LI();
                    any++;
                }
            }

            if (any == 0)
            {
                h.LI_().T(Application.Node.name).SPAN("普通消费者", "uk-margin-auto-left")._LI();
            }

            h._UL();
            h._ARTICLE();
        }, false, 12);

        // resend token cookie
        wc.SetTokenCookies(o);
    }


    [Ui("设置", "设置我的账号信息", status: 7), Tool(ButtonShow)]
    public async Task setg(WebContext wc)
    {
        const string PASSWORD_MASK = "t#0^0z4R4pX7";

        string name;
        string tel;
        string password;
        var prin = (User)wc.Principal;
        if (wc.IsGet)
        {
            name = prin.name;
            tel = prin.tel;
            password = string.IsNullOrEmpty(prin.credential) ? null : PASSWORD_MASK;
            wc.GivePane(200, h =>
            {
                h.FORM_("uk-card uk-card-primary");
                h.H2("我的账号信息设置", css: "uk-card-header");
                h.FIELDSUL_(css: "uk-card-body");
                h.LI_().TEXT("姓名", nameof(name), name, max: 12, min: 2, required: true, @readonly: prin.IsStationOp)._LI();
                h.LI_().TEXT("登录手机号", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true, @readonly: true);
                if (prin.IsStationOp)
                {
                    h.LI_().PASSWORD("终端密码", nameof(password), password, max: 12, min: 3)._LI();
                }
                h._FIELDSUL();
                h.BOTTOM_BUTTON("确定", nameof(setg));
                h._FORM();
            });
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