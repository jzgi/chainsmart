using System.Threading.Tasks;
using ChainFx;
using ChainFx.Nodal;
using ChainFx.Web;

namespace ChainSmart;

[MyAuthorize]
[Ui("我的个人账号")]
public class MyVarWork : WebWork
{
    protected override void OnCreate()
    {
        CreateWork<MyAccessVarWork>("access");

        CreateWork<MyBuyWork>("buy");
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

            h.WORKBOARD();

            h.ARTICLE_("uk-card uk-card-primary");
            h.H3("服务约定", "uk-card-header");
            h.SECTION_("uk-card-body");
            h.P("「中惠农通」线上平台是实体市场的镜像，平台上所有商户都是在指定市场内有实体商户的合法经营户，受当地政府有关部门监管。");
            h.P("通过平台发生的线上和线下的商品交易行为，买卖双方应各自负有相应的责任。");
            h.P("按照平台的协议，市场内的「中惠农通体验中心」作为商盟的盟主，有权向盟友商户提出整改建议。");
            h.P("「中惠农通」是集体所有制平台，保障您的个人数据不作任何其它用途。");
            h._SECTION();
            h.FOOTER_("uk-card-footer uk-flex-center").PIC("/qrcode.jpg", css: "uk-width-small")._FOOTER();
            h._ARTICLE();
        }, false, 900);
    }

    [Ui("设置", icon: "pencil"), Tool(Modal.ButtonShow)]
    public async Task setg(WebContext wc)
    {
        const string PASSMASK = "t#0^0z4R4pX7";
        string name;
        string tel;
        // string password;
        var prin = (User)wc.Principal;
        if (wc.IsGet)
        {
            name = prin.name;
            tel = prin.tel;
            // password = string.IsNullOrEmpty(prin.credential) ? null : PASSMASK;
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("基本信息");
                h.LI_().TEXT("姓名", nameof(name), name, max: 12, min: 2, required: true)._LI();
                h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                // h._FIELDSUL();
                // h.FIELDSUL_("操作密码（可选）");
                // h.LI_().PASSWORD("密码", nameof(password), password, max: 12, min: 3)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确定", nameof(setg))._FORM();
            });
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            name = f[nameof(name)];
            tel = f[nameof(tel)];
            // password = f[nameof(password)];
            // var credential =
            //     string.IsNullOrEmpty(password) ? null :
            //     password == PASSMASK ? prin.credential :
            //     MainUtility.ComputeCredential(tel, password);

            using var dc = Nodality.NewDbContext();
            dc.Sql("UPDATE users SET name = CASE WHEN @1 IS NULL THEN name ELSE @1 END , tel = @2 WHERE id = @3 RETURNING ").collst(User.Empty);
            prin = await dc.QueryTopAsync<User>(p => p.Set(name).Set(tel).Set(prin.id));

            // refresh cookies
            wc.SetUserCookies(prin);

            wc.GivePane(200); // close
        }
    }
}