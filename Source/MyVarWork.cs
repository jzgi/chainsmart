using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

[MyAuthorize]
[Ui("我的个人账号")]
public class MyVarWork : WebWork
{
    protected override void OnCreate()
    {
        CreateWork<MyBuyWork>("buy");

        CreateWork<MyCarbWork>("cer");

        CreateWork<MyAgmtVarWork>("agmt");
    }

    public void @default(WebContext wc)
    {
        var prin = (User)wc.Principal;
        wc.GivePage(200, h =>
        {
            h.TOPBARXL_();

            h.HEADER_("uk-width-expand uk-col uk-padding-left");
            h.H2(prin.name);
            h.H4(prin.tel);
            if (prin.typ > 0) h.P(User.Typs[prin.typ]);
            h._HEADER();

            if (prin.icon)
            {
                h.PIC("/user/", prin.id, "/icon/", circle: true, css: "uk-width-medium");
            }
            else
                h.PIC("/my.webp", circle: true, css: "uk-width-small");

            h._TOPBARXL();

            h.WORKBOARD(compact: false);

            h.TOOLBAR(bottom: true, status: prin.Status, state: prin.State);
        }, false, 120);
    }

    [Ui("身份", "刷新我的身份权限", icon: "user"), Tool(ButtonShow, status: 7)]
    public async Task access(WebContext wc)
    {
        int uid = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE id = @1");
        var o = await dc.QueryTopAsync<User>(p => p.Set(uid));

        // resend token cookie
        wc.SetTokenCookies(o);

        wc.GivePane(200, h =>
        {
            h.ARTICLE_("uk-card uk-card-default");
            h.H4("我的最新身份和权限", css: "uk-card-header");
            h.UL_("uk-card-body uk-list uk-list-divider");

            var any = 0;
            var vip = o.vip;
            if (vip != null)
            {
                h.LI_().LABEL("大客户").SPAN_("uk-static");
                for (int i = 0; i < vip.Length; i++)
                {
                    if (i > 0)
                    {
                        h.BR();
                    }

                    var org = GrabTwin<int, Org>(vip[i]);
                    if (org != null)
                    {
                        h.T(org.name);
                    }
                }

                h._SPAN();
                h._LI();

                any++;
            }

            if (o.admly > 0)
            {
                h.LI_().FIELD(User.Admly[o.admly], "平台")._LI();

                any++;
            }

            if (o.suply > 0 && o.supid > 0)
            {
                var org = GrabTwin<int, Org>(o.supid);

                h.LI_().FIELD(User.Orgly[o.suply], org.name)._LI();

                any++;
            }

            if (o.rtlly > 0 && o.rtlid > 0)
            {
                var org = GrabTwin<int, Org>(o.rtlid);

                h.LI_().FIELD(User.Orgly[o.rtlly], org.name)._LI();

                any++;
            }

            if (any == 0)
            {
                h.LI_().FIELD(null, "暂无特殊权限")._LI();
            }

            h._UL();
            h._ARTICLE();
        }, false, 60);
    }


    [Ui("设置", icon: "cog"), Tool(ButtonShow, status: 7)]
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
                h.FORM_().FIELDSUL_("基本信息");

                h.LI_().TEXT("姓名", nameof(name), name, max: 12, min: 2, required: true, @readonly: prin.IsStationOp)._LI();
                h.LI_().TEXT("登录手机号", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true, @readonly: true);

                h._FIELDSUL();

                if (prin.IsStationOp)
                {
                    h.FIELDSUL_("工作台操作密码");
                    h.LI_().PASSWORD("密码", nameof(password), password, max: 12, min: 3)._LI();
                    h._FIELDSUL();
                }
                h.BOTTOM_BUTTON("确定", nameof(setg))._FORM();
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
}