using System.Threading.Tasks;
using ChainFx.Fabric;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class PublyWork : WebWork
    {
    }

    public class PublyTagWork : PublyWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyTagVarWork>();
        }
    }

    public class PublyTagVarWork : PublyWork
    {
        public async Task @default(WebContext wc)
        {
            int tagid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE nend >= @1 AND nstart <= @1");
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(tagid));

            wc.GivePage(200, h =>
            {
                if (lot == null)
                {
                    h.ALERT("没有找到产品");
                    return;
                }

                var item = GrabMap<int, int, Item>(lot.srcid)[lot.itemid];

                var src = GrabObject<int, Org>(lot.srcid);

                h.TOPBARXL_();
                h.PIC("/item/", lot.itemid, "/icon", circle: true, css: "uk-width-small");
                h.DIV_("uk-width-expand uk-col uk-padding-small-left").H2(item.name)._DIV();
                h._TOPBARXL();

                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("品名", item.name)._LI();
                h.LI_().FIELD("描述", item.tip)._LI();
                h.LI_().FIELD("产源", src.name)._LI();
                h.LI_().FIELD("批次号码", lot.id)._LI();
                h.LI_().FIELD("批次创建", lot.created)._LI();
                h.LI_().FIELD2("批次供量", lot.cap, item.unitpkg, true)._LI();
                h._UL();
            }, true, 3600, title: Self.Name + "产品溯源信息");
        }
    }
}