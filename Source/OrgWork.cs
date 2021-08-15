using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static SkyChain.Web.Modal;

namespace Zhnt
{
    public class AdmlyOrgWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<AdmlyOrgVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            var regs = Fetch<Map<short, Reg>>();

            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw ORDER BY regid, id, status DESC");
            var arr = await dc.QueryAsync<Org>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: "服务方");

                h.TABLE_();
                short last = 0;
                foreach (var o in arr)
                {
                    if (o.regid != last)
                    {
                        h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 4).T(o.regid > 0 ? regs[o.regid].name : "其它")._TD()._TR();
                    }
                    h.TR_();
                    h.TDCHECK(o.id);
                    h.TD_().VARTOOL(o.Key, nameof(AdmlyOrgVarWork.upd), caption: o.name).SP().SUB(Org.Typs[o.typ])._TD();
                    h.TD_("uk-visible@s").T(o.addr)._TD();
                    h.TD_().A_TEL(o.mgrname, o.Tel)._TD();
                    h.TD(_Art.Statuses[o.status]);
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                    h._TR();
                    last = o.regid;
                }
                h._TABLE();
            });
        }

        [Ui("新建"), Tool(ButtonShow)]
        public async Task @new(WebContext wc)
        {
            var regs = Fetch<Map<short, Reg>>();
            var orgs = Fetch<Map<short, Org>>();
            if (wc.IsGet)
            {
                var m = new Org();
                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("服务方属性");
                    h.LI_().SELECT("服务类型", nameof(m.typ), m.typ, Org.Typs)._LI();
                    h.LI_().TEXT("名称", nameof(m.name), m.name, max: 8, required: true)._LI();
                    h.LI_().TEXT("标语", nameof(m.tip), m.tip, max: 16)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("状态", nameof(m.status), m.status, _Art.Statuses)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync<Org>(0);
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, 0)._VALUES_(Org.Empty, 0);
                await dc.ExecuteAsync(p => o.Write(p));
                wc.GivePane(201); // created
            }
        }
    }

    [Ui("商户")]
    public class GrplyBizWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MrtlyBizVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE parent = @1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Org>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().T(o.name).SP().SUB(o.addr)._TD();
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                });
            }, false, 3);
        }
    }

    [Ui("群成员")]
    public class GrplySrcWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MrtlyBizVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            using var dc = NewDbContext();

            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE parent = @1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Org>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TDCHECK(o.id);
                    h.TD_().T(o.name).SP().SUB(o.addr)._TD();
                    h.TDFORM(() => h.VARTOOLS(o.Key));
                });
            }, false, 3);
        }
    }
}