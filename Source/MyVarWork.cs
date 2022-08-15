using System.Threading.Tasks;
using CoChain;
using CoChain.Web;
using static System.String;
using static CoChain.Web.Modal;
using static CoChain.Nodal.Store;

namespace CoSupply
{
    [UserAuthorize]
    [Ui("账号信息")]
    public class MyVarWork : WebWork
    {
        protected override void OnCreate()
        {
            CreateWork<MyBuyWork>("buy");
        }

        [UserAuthorize]
        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            wc.GivePage(200, h =>
            {
                h.FORM_("uk-card uk-card-default uk-card-body uk-flex");
                if (prin.icon)
                {
                    h.PIC("/user/", prin.id, "/icon/", circle: true, css: "uk-width-medium");
                }
                else
                {
                    h.PIC("/user.png", circle: true, css: "uk-width-small");
                }
                h.DIV_("uk-width-expand uk-col uk-padding-small-left");
                h.SPAN(prin.name);
                h.SPAN(prin.tel);
                h.SPAN(User.Typs[prin.typ]);
                h._DIV();
                h._FORM();

                h.TASKLIST();
            }, false, 3, title: "我的账户");
        }

        [Ui("设置"), Tool(ButtonShow)]
        public async Task setg(WebContext wc)
        {
            const string PASSMASK = "t#0^0z4R4pX7";
            string name;
            string tel;
            string password;
            var prin = (User) wc.Principal;
            if (wc.IsGet)
            {
                name = prin.name;
                tel = prin.tel;
                password = IsNullOrEmpty(prin.credential) ? null : PASSMASK;
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");
                    h.LI_().TEXT("姓名", nameof(name), name, max: 8, min: 2, required: true)._LI();
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h._FIELDSUL().FIELDSUL_("操作密码（可选）");
                    h.LI_().PASSWORD("密码", nameof(password), password, max: 12, min: 3)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                name = f[nameof(name)];
                tel = f[nameof(tel)];
                password = f[nameof(password)];
                string credential =
                    IsNullOrEmpty(password) ? null :
                    password == PASSMASK ? prin.credential :
                    RevitalUtility.ComputeCredential(tel, password);

                using var dc = NewDbContext();
                dc.Sql("UPDATE users SET name = CASE WHEN @1 IS NULL THEN name ELSE @1 END , tel = @2, credential = @3 WHERE id = @4 RETURNING ").collst(User.Empty);
                prin = await dc.QueryTopAsync<User>(p => p.Set(name).Set(tel).Set(credential).Set(prin.id));
                // refresh cookie
                wc.SetTokenCookie(prin);
                wc.GivePane(200); // close
            }
        }
    }
}