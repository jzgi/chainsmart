using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.User;
using static SkyChain.Web.Modal;

namespace Revital.Main
{
    public abstract class RegWork : WebWork
    {
    }

    [UserAuthorize(admly: ADMLY_MGT)]
    [Ui("平台｜地理区域设置")]
    public class AdmlyRegWork : RegWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<AdmlyRegVarWork>();
        }

        [Ui("省份", group: 1), Tool(Anchor)]
        public void @default(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Reg.TYP_PROV);
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = ").T(Reg.TYP_PROV).T(" ORDER BY id, status DESC");
                var arr = dc.Query<Reg>();
                h.TABLE(arr, o =>
                    {
                        h.TDCHECK(o.Key);
                        h.TDAVAR(o.Key, o.name);
                        h.TDFORM(() => h.TOOLGROUPVAR(o.Key, subscript: Reg.TYP_PROV));
                    }
                );
            });
        }

        [Ui("地市", group: 2), Tool(Anchor)]
        public void dist(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Reg.TYP_DIST);
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = ").T(Reg.TYP_DIST).T(" ORDER BY id, status DESC");
                var arr = dc.Query<Reg>();
                h.TABLE(arr, o =>
                    {
                        h.TDCHECK(o.Key);
                        h.TDAVAR(o.Key, o.name);
                        h.TDFORM(() => h.TOOLGROUPVAR(o.Key, subscript: Reg.TYP_DIST));
                    }
                );
            });
        }

        [Ui("场地", group: 4), Tool(Anchor)]
        public void sect(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Reg.TYP_SECT);
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = ").T(Reg.TYP_SECT).T(" ORDER BY id, status DESC");
                var arr = dc.Query<Reg>();
                h.TABLE(arr, o =>
                    {
                        h.TDCHECK(o.Key);
                        h.TDAVAR(o.Key, o.name);
                        h.TDFORM(() => h.TOOLGROUPVAR(o.Key, subscript: Reg.TYP_SECT));
                    }
                );
            });
        }

        [Ui("✚", "新建区域", group: 7), Tool(ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            var o = new Reg
            {
                typ = (short) typ,
                status = Info.STA_ENABLED,
                created = DateTime.Now,
                creator = prin.name,
            };
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("区域属性");
                    h.LI_().NUMBER("区域编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                    h.LI_().SELECT("类型", nameof(o.typ), o.typ, Reg.Typs, filter: (k, v) => k == typ, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                    h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, Info.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                o = await wc.ReadObjectAsync(instance: o);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO regs ").colset(Reg.Empty)._VALUES_(Item.Empty);
                await dc.ExecuteAsync(p => o.Write(p));

                wc.GivePane(200); // close dialog
            }
        }
    }
}