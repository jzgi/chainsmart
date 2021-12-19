using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Web.Modal;
using static Revital.User;

namespace Revital
{
    public abstract class PieceWork : WebWork
    {
    }


    [UserAuthorize(Org.TYP_PRD, ORGLY_OP)]
    public abstract class PrdlyPieceWork : PieceWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PrdlyPieceVarWork>();
        }

        [Ui("当前"), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Piece.Empty).T(" FROM pieces WHERE frmid = @1 AND status > 0 ORDER BY id");
            await dc.QueryAsync<Piece>(p => p.Set(org.id));
            wc.GivePage(200, h => { h.TOOLBAR(caption: "来自平台的订单"); });
        }

        [Ui("禁用"), Tool(Anchor)]
        public async Task disabled(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Piece.Empty).T(" FROM pieces WHERE frmid = @1 AND status = 0 ORDER BY id");
            await dc.QueryAsync<Piece>(p => p.Set(org.id));
            wc.GivePage(200, h => { h.TOOLBAR(caption: "来自平台的订单"); });
        }

        [Ui("✚", "新建"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            if (wc.IsGet)
            {
                var o = new Piece
                {
                    status = _Art.STA_ENABLED
                };
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("填写区域属性");
                    h.LI_().NUMBER("区域编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                    h.LI_().SELECT("类型", nameof(o.typ), o.typ, Item.Typs)._LI();
                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(o.status), o.status, _Art.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Piece>();
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO product ").colset(Piece.Empty)._VALUES_(Piece.Empty);
                await dc.ExecuteAsync(p => o.Write(p));

                wc.GivePane(200); // close dialog
            }
        }
    }

    [Ui("产品管理")]
    public class PrdlyAgriPieceWork : PrdlyPieceWork
    {
    }

    [Ui("产品管理")]
    public class PrdlyDietPieceWork : PrdlyPieceWork
    {
    }
}