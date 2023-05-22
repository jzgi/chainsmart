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
        CreateWork<MyBuyWork>("buy");

        CreateWork<MyCarbWork>("cer");

        CreateWork<MyAccessVarWork>("access");

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
            
            h.FOOTER_("uk-card-footer uk-flex-center uk-margin-large-top").PIC("/qrcode.jpg", css: "uk-width-small")._FOOTER();

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
            wc.SetTokenCookies(prin);

            wc.GivePane(200); // close
        }
    }
}