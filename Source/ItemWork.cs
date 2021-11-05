using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public class PublyItemWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PubItemVarWork>();
        }
    }

    [UserAuthorize(admly: User.ADMLY_SYS)]
    [Ui("商品管理")]
    public class AdmlyItemWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyItemVarWork>();
        }

        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items ORDER BY typ, status DESC, id LIMIT 40 OFFSET 40 * @1");
            var arr = dc.Query<Item>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.typ != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(Item.Typs[o.typ])._TD()._TR();
                    }
                    h.TR_();
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyItemVarWork.upd), caption: o.name)._TD();
                    h.TD(Item.Statuses[o.status]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();
                    last = o.typ;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("新建"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var prin = (User) wc.Principal;
            if (wc.IsGet)
            {
                var o = new Item
                {
                    status = Item.STA_ENABLED
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("品目信息");
                    h.LI_().SELECT("分类", nameof(o.typ), o.typ, Item.Typs)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 10)._LI();
                    h.LI_().TEXT("计量单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true)._LI();
                    h.LI_().TEXT("计量脚注", nameof(o.unitip), o.unitip, max: 8)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, Item.Statuses, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, inst: new Item
                {
                    created = DateTime.Now,
                    creator = prin.name
                });
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO items ").colset(Item.Empty, 0)._VALUES_(Item.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }

        [Ui("✕", "删除"), Tool(ButtonPickShow, Appear.Small)]
        public async Task rm(WebContext wc)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.ALERT("删除标品？");
                    h.FORM_().HIDDEN(string.Empty, true)._FORM();
                });
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("DELETE FROM items WHERE id = @1");
                await dc.ExecuteAsync(p => p.Set(id));

                wc.GivePane(200);
            }
        }
    }
}