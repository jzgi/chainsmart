using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt.Mart
{
    public class PubMatVarWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }

    public class AdmlyMatVarWork : WebWork
    {
        [Ui("✎", "✎ 修改", group: 2), Tool(AnchorShow)]
        public async Task upd(WebContext wc)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Mat.Empty).T(" FROM mats WHERE id = @1");
                var o = dc.QueryTop<Mat>(p => p.Set(id));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("填写资料");
                    h.LI_().SELECT("食材类型", nameof(o.typ), o.typ, Mat.Typs)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, max: 10, required: true).TEXT("单位", nameof(o.unit), o.unit, min: 1, max: 2, required: true)._LI();
                    h.LI_().NUMBER("热量", nameof(o.calory), o.calory, min: 0, required: true).NUMBER("碳水化合", nameof(o.carb), o.carb, min: 0, required: true)._LI();
                    h.LI_().NUMBER("脂肪", nameof(o.fat), o.fat, min: 0, required: true).NUMBER("蛋白质", nameof(o.protein), o.protein, min: 0, required: true)._LI();
                    h.LI_().NUMBER("糖", nameof(o.sugar), o.sugar, min: 0, required: true).NUMBER("维他命", nameof(o.vitamin), o.vitamin, min: 0, required: true)._LI();
                    h.LI_().NUMBER("无机盐", nameof(o.mineral), o.mineral, min: 0, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, Mat.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Mat>(0);
                using var dc = NewDbContext();
                dc.Sql("UPDATE mats")._SET_(Mat.Empty, 0).T(" WHERE id = @1");
                dc.Execute(p =>
                {
                    o.Write(p, 0);
                    p.Set(id);
                });
                wc.GivePane(200); // close
            }
        }

        [Ui("✕", "删除"), Tool(ButtonShow, Appear.Small)]
        public async Task rm(WebContext wc)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.ALERT("删除材料？");
                    h.FORM_().HIDDEN(string.Empty, true)._FORM();
                });
            }
            else
            {
                using var dc = NewDbContext();
                dc.Sql("DELETE FROM mats WHERE id = @1");
                await dc.ExecuteAsync(p => p.Set(id));

                wc.GivePane(200);
            }
        }
    }
}