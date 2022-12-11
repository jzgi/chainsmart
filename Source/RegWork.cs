using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainMart.User;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainMart
{
    public abstract class RegWork : WebWork
    {
    }

    [AdmlyAuthorize(ROL_MGT)]
    [Ui("区域设置", "业务")]
    public class AdmlyRegWork : RegWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<AdmlyRegVarWork>();
        }

        [Ui("省份", group: 1), Tool(Anchor)]
        public void @default(WebContext wc)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = ").T(Reg.TYP_PROVINCE).T(" ORDER BY id, status DESC");
            var arr = dc.Query<Reg>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Reg.TYP_PROVINCE);

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");
                    h.PIC("/void.webp", css: "uk-width-1-5");
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();
                    h._A();
                });
            }, false, 15);
        }

        [Ui("地市", group: 2), Tool(Anchor)]
        public void city(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Reg.TYP_CITY);
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = ").T(Reg.TYP_CITY).T(" ORDER BY id, status DESC");
                var arr = dc.Query<Reg>();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");
                    h.PIC("/void.webp", css: "uk-width-1-5");
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();
                    h._A();
                });
            }, false, 15);
        }

        [Ui("场区", group: 4), Tool(Anchor)]
        public void mrtdiv(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Reg.TYP_SECTION);

                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE typ = ").T(Reg.TYP_SECTION).T(" ORDER BY id, status DESC");
                var arr = dc.Query<Reg>();

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, css: "uk-card-body uk-flex");
                    h.PIC("/void.webp", css: "uk-width-1-5");
                    h.DIV_("uk-width-expand uk-padding-left");
                    h.H5(o.name);
                    h.P(o.tip);
                    h._DIV();
                    h._A();
                });
            }, false, 15);
        }

        [Ui("新建", "新建区域", icon: "plus", group: 7), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            var o = new Reg
            {
                typ = (short) typ,
                state = Entity.STA_NORMAL,
                created = DateTime.Now,
                creator = prin.name,
            };
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("区域属性");
                    h.LI_().NUMBER("区域编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                    h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                    h.LI_().NUMBER("资源数", nameof(o.num), o.num, min: 0, max: 9999)._LI();
                    h.LI_().SELECT("状态", nameof(o.state), o.state, Entity.States)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                o = await wc.ReadObjectAsync(instance: o);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO regs ").colset(Reg.Empty)._VALUES_(Ware.Empty);
                await dc.ExecuteAsync(p => o.Write(p));

                wc.GivePane(200); // close dialog
            }
        }
    }
}