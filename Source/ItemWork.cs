using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class ItemWork : WebWork
    {
    }

    public class PublyItemWork : ItemWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyItemVarWork>();
        }
    }

    [UserAuthorize(admly: User.ADMLY_MGT)]
    [Ui("平台｜统一品目设置")]
    public class AdmlyItemWork : ItemWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyItemVarWork>();
        }

        [Ui("农副产", group: 1), Tool(Anchor)]
        public void @default(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE typ = ").T(Item.TYP_AGRI).T(" ORDER BY cat, status DESC LIMIT 40 OFFSET 40 * @1");
            var arr = dc.Query<Item>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Item.TYP_AGRI);
                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.cat != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(Item.Cats[o.cat])._TD()._TR();
                    }
                    h.TR_();
                    h.TDCHECK(o.id);
                    h.TDVAR(o.Key, o.name);
                    h.TD(_Info.Symbols[o.status]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();

                    last = o.cat;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("制造品", group: 2), Tool(Anchor)]
        public void fact(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE typ = ").T(Item.TYP_FACT).T(" ORDER BY cat, status DESC LIMIT 40 OFFSET 40 * @1");
            var arr = dc.Query<Item>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Item.TYP_FACT);
                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.cat != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(Item.Cats[o.cat])._TD()._TR();
                    }
                    h.TR_();
                    h.TDCHECK(o.id);
                    h.TDVAR(o.Key, o.name);
                    h.TD(_Info.Symbols[o.status]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();

                    last = o.cat;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("服务类", group: 4), Tool(Anchor)]
        public void svrc(WebContext wc, int page)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE typ = ").T(Item.TYP_SRVC).T(" ORDER BY cat, status DESC LIMIT 40 OFFSET 40 * @1");
            var arr = dc.Query<Item>(p => p.Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Item.TYP_SRVC);
                if (arr == null) return;

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.cat != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(Item.Cats[o.cat])._TD()._TR();
                    }
                    h.TR_();
                    h.TDCHECK(o.id);
                    h.TDVAR(o.Key, o.name);
                    h.TD(_Info.Symbols[o.status]);
                    h.TD_("uk-visible@l").T(o.tip)._TD();
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();

                    last = o.cat;
                }
                h._TABLE();
                h.PAGINATION(arr.Length == 40);
            });
        }

        [Ui("✚", "新建品目", group: 7), Tool(ButtonShow)]
        public async Task @new(WebContext wc, int typ)
        {
            var prin = (User) wc.Principal;
            if (wc.IsGet)
            {
                var o = new Item
                {
                    status = _Info.STA_ENABLED
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("品目信息");
                    h.LI_().SELECT("类型", nameof(o.typ), o.typ, Item.Typs, filter: (k, v) => k == typ, required: true)._LI();
                    h.LI_().SELECT("分属", nameof(o.cat), o.cat, Item.Cats, filter: (k, v) => k < typ * 20 && k > (typ - 1) * 20, required: true)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, max: 10, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 30)._LI();
                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true)._LI();
                    h.LI_().TEXT("单位脚注", nameof(o.unitip), o.unitip, max: 8)._LI();
                    h.LI_().NUMBER("单位运费", nameof(o.fee), o.fee, min: 0.00M, max: 999.99M)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Info.Statuses, required: true)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(0, new Item
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

        // [Ui("✕", "删除"), Tool(ButtonPickShow, Appear.Small)]
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