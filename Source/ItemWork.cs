using System;
using System.Threading.Tasks;
using CoChain;
using CoChain.Web;
using static CoChain.Web.Modal;
using static CoChain.Nodal.Store;

namespace Revital
{
    public abstract class ItemWork : WebWork
    {
    }

    public class PublyItemWork : ItemWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyItemVarWork>();
        }
    }

    [UserAuthorize(admly: User.ADMLY_MGT)]
    [Ui("平台标准品目设置", icon: "album")]
    public class AdmlyItemWork : ItemWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<AdmlyItemVarWork>();
        }

        public void @default(WebContext wc, int typ)
        {
            var cats = Grab<short, Cat>();
            if (typ == 0)
            {
                wc.Subscript = typ = cats.KeyAt(0);
            }
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE typ = @1 ORDER BY state DESC");
            var arr = dc.Query<Item>(p => p.Set(typ));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(typ, opts: cats);

                if (arr == null) return;

                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TDAVAR(o.Key, o.name);
                    h.TD(o.state == 1 ? null : Info.States[o.state]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TDFORM(() => h.TOOLGROUPVAR(o.Key));
                });

                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("✚", "新建规范品目"), Tool(ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            var cats = Grab<short, Cat>();
            if (wc.IsGet)
            {
                var m = new Item
                {
                    typ = (short) typ,
                    created = DateTime.Now,
                    creator = prin.name,
                    state = Info.STA_ENABLED
                };
                wc.GivePane(200, h =>
                {
                    var typname = cats[m.typ];
                    h.FORM_().FIELDSUL_(typname + "品目信息");
                    h.LI_().TEXT("品目名", nameof(m.name), m.name, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 30)._LI();
                    h.LI_().TEXT("基本单位", nameof(m.unit), m.unit, min: 1, max: 4, required: true).TEXT("单位脚注", nameof(m.unitip), m.unitip, max: 8)._LI();
                    h.LI_().SELECT("状态", nameof(m.state), m.state, Info.States, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                const short proj = Info.BORN;
                var m = await wc.ReadObjectAsync(0, new Item
                {
                    typ = (short) typ,
                    created = DateTime.Now,
                    creator = prin.name
                });
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO items ").colset(Item.Empty, proj)._VALUES_(Item.Empty, proj);
                await dc.ExecuteAsync(p => m.Write(p, proj));

                wc.GivePane(200); // close dialog
            }
        }

        // [Ui("✕", "删除"), Tool(ButtonPickShow, Appear.Small)]
        // public async Task rm(WebContext wc)
        // {
        //     short id = wc[0];
        //     if (wc.IsGet)
        //     {
        //         wc.GivePane(200, h =>
        //         {
        //             h.ALERT("删除标品？");
        //             h.FORM_().HIDDEN(string.Empty, true)._FORM();
        //         });
        //     }
        //     else
        //     {
        //         using var dc = NewDbContext();
        //         dc.Sql("DELETE FROM items WHERE id = @1");
        //         await dc.ExecuteAsync(p => p.Set(id));
        //
        //         wc.GivePane(200);
        //     }
        // }
    }
}