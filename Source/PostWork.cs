using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.Item;
using static SkyChain.Web.Modal;

namespace Revital
{
    public abstract class PostWork : WebWork
    {
    }

    public class PublyPostWork : PostWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyPostVarWork>();
        }

        public void @default(WebContext wc, int page)
        {
        }
    }

    [Ui("［经营户］展页管理")]
    public class BizlyPostWork : PostWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyPostVarWork>();
        }

        [Ui("在展示", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Post.Empty).T(" FROM posts WHERE orgid = @1 AND status >= 2 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Post>(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.name, o.tip)._TD();
                    h.TD(o.price, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }

        [Ui("未展示", group: 2), Tool(Anchor)]
        public async Task off(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Post.Empty).T(" FROM posts WHERE orgid = @1 AND status <= 1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Post>(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().VARTOOL(o.Key, nameof(BizlyPostVarWork.upd), caption: o.name).SP()._TD();
                    h.TD(o.price, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }

        [Ui("✚", "新建供应展项", group: 3), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var items = ObtainMap<short, Item>();
            if (wc.IsGet)
            {
                var o = new Post
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    status = _Art.STA_DISABLED,
                    unitx = 1,
                    min = 1,
                    max = 100,
                    step = 1,
                    cap = 100
                };
                wc.GivePane(200, h =>
                {
                    // var ctr = Obtain<int, Org>(org.ctrid);
                    // var plans = ObtainSub<int, int, Plan>(org.ctrid);
                    //
                    // h.FORM_().FIELDSUL_(ctr.name);
                    //
                    // h.LI_().SELECT_PLAN("供应项目", nameof(o.planid), o.planid, plans, Cats, required: true)._LI();
                    // h.LI_().TEXTAREA("特色描述", nameof(o.tip), o.tip, max: 40)._LI();
                    // h.LI_().SELECT("状态", nameof(o.status), o.status, _Article.Statuses, required: true)._LI();
                    //
                    // h._FIELDSUL().FIELDSUL_("规格参数");
                    //
                    // h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("标准比", nameof(o.unitx), o.unitx, min: 1, max: 1000, required: true)._LI();
                    // h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    // h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    // h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("最大容量", nameof(o.cap), o.cap)._LI();
                    //
                    // h._FIELDSUL();
                    //
                    // h.BOTTOM_BUTTON("确定");
                    //
                    // h._FORM();
                });
            }
            else // POST
            {
                // populate 
                var o = await wc.ReadObjectAsync(0, new Post
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    orgid = org.id,
                });
                var item = items[o.itemid];
                o.cat = item.cat;
                o.name = item.name + '（' + o.ext + '）';

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO plans ").colset(Plan.Empty, 0)._VALUES_(Plan.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }

        [Ui("自定义", "新建自定义展项", group: 3), Tool(ButtonOpen)]
        public async Task cust(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var items = ObtainMap<short, Item>();
            if (wc.IsGet)
            {
                var o = new Post
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    status = _Art.STA_DISABLED,
                    unitx = 1,
                    min = 1,
                    max = 10,
                    step = 1,
                    cap = 100
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");

                    h.LI_().SELECT_ITEM("标准品目", nameof(o.itemid), o.itemid, items, Cats, filter: x => x.typ == org.fork, required: true).TEXT("附加名", nameof(o.ext), o.ext, max: 10)._LI();
                    h.LI_().TEXTAREA("特色描述", nameof(o.tip), o.tip, max: 40)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Art.Statuses, required: true)._LI();

                    h._FIELDSUL().FIELDSUL_("规格参数");

                    h.LI_().TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("标准比", nameof(o.unitx), o.unitx, min: 1, max: 1000, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订量", nameof(o.min), o.min).NUMBER("限订量", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增量", nameof(o.step), o.step).NUMBER("最大容量", nameof(o.cap), o.cap)._LI();

                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确定");

                    h._FORM();
                });
            }
            else // POST
            {
                // populate 
                var o = await wc.ReadObjectAsync(0, new Post
                {
                    created = DateTime.Now,
                    creator = prin.name,
                    orgid = org.id,
                });
                var item = items[o.itemid];
                o.cat = item.cat;
                o.name = item.name + '（' + o.ext + '）';

                // insert
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO posts ").colset(Post.Empty, 0)._VALUES_(Post.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p, 0));

                wc.GivePane(200); // close dialog
            }
        }
    }
}