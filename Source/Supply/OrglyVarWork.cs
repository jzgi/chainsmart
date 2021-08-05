using System;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using Zhnt.Mart;
using static SkyChain.Web.Modal;
using static Zhnt.User;

namespace Zhnt.Supply
{
    [UserAuthorize(orgly: ORGLY_OP)]
    [Ui("账号")]
    public class OrglyVarWork : WebWork
    {
        protected override void OnMake()
        {
            MakeWork<OrglyAccessWork>("acc");

            MakeWork<OrglyClearWork>("clear");


            // bases

            MakeWork<OrglyOrderWork>("pur");

            // centers


            MakeWork<OrglyJobWork>("job");

            // transport

            MakeWork<OrglyDistWork>("dist");

            // points

            MakeWork<OrglyBuyWork>("buy");
        }

        public async Task @default(WebContext wc)
        {
            bool inner = wc.Query[nameof(inner)];
            short id = wc[0];
            var orgs = Fetch<Map<short, Org>>();
            var o = orgs[id];
            if (!inner)
            {
                wc.GiveFrame(200, false, 60, title: "供应方操作", group: (byte) o.typ);
            }
            else
            {
                using var dc = NewDbContext();

                wc.GivePage(200, h =>
                {
                    h.TOOLBAR(caption: "本方账号信息");

                    h.FORM_("uk-card uk-card-default");
                    h.HEADER_("uk-card-header").T("基本信息").SPAN_("uk-badge").T("状态：").T(_Art.Statuses[o.status])._SPAN()._HEADER();
                    h.UL_("uk-card-body");
                    h.LI_().FIELD("本方名称", o.Name)._LI();
                    h.LI_().FIELD("标语", o.tip)._LI();
                    h.LI_().FIELD("类型", Org.Typs[o.typ])._LI();
                    h.LI_().FIELD("地址", o.addr)._LI();
                    if (o.IsPt)
                    {
                        var shop = o.parent > 0 ? orgs[o.parent]?.name : null;
                        h.LI_().FIELD("关联厨房", shop)._LI();
                    }
                    if (o.IsMerchant)
                    {
                        h.LI_().FIELD("派递模式", o.@extern ? "全网包邮" : "同城服务站")._LI();
                    }
                    h.LI_().FIELD2("负责人", o.mgrname, o.mgrtel)._LI();
                    h.LI_().FIELD2("联络员", o.cttname, o.ctttel)._LI();
                    h._UL();
                    h.FOOTER_("uk-card-footer uk-flex-center").TOOL(nameof(setg), css: "uk-button-secondary")._FOOTER();
                    h._FORM();

                    if (o.IsProvider)
                    {
                        string url = ServerEnviron.extcfg[nameof(url)];
                        h.SECTION_("uk-section uk-flex-middle uk-col");
                        h.P("本方主页");
                        h.QRCODE(url + "/org/" + o.id + "/", css: "uk-width-medium");
                        h._SECTION();
                    }
                }, false, 3);
            }
        }

        [UserAuthorize(orgly: ORGLY_MGR)]
        [Ui("协议"), Tool(ButtonOpen)]
        public void agrmt(WebContext wc)
        {
            short orgid = wc[0];
            wc.GivePage(200, h =>
            {
                h.ARTICLE_("uk-section");
                h.H2_("uk-flex-center uk-width-1-1").T("平台入驻合作协议")._H2();
                h.H4_().T("平台介绍")._H4();
                h.P("穹苍家园是一个互联网＋大健康信息服务平台，利用自身在饮食健康方面的经验学识，借助自研的区块链技术整合优质资源，在多城市开展健康生活服务。");
                h.P("平台设有膳食调养和健康拼团两大业务版块，分别针对居民个人身体调养和城市健康生态圈的建设。");
                h.P("江西穹苍科技有限公司是平台的研发和运营方，是注重服务民生的社会企业");
                h.H4_().T("合作方式")._H4();
                h.P("平台上所进行的日常业务由入驻的服务方来开展，服务方有四种基本职能，分别是：调养厨房、服务站、健康商家、社工机构。每个服务方视情况可以同时担当一种或多种职能。");
                h.P("平台欲与有见识有责任心的服务方保持长期的合作伙伴关系，联手共进。平台须保障网络的平稳运行和升级迭代，全力开拓市场、吸纳更多的健康用户；服务方须诚信经营，全力提供优质的健康产品和服务。");
                h.UL_();
                h.LI_().H5("调养厨房");
                h.P("　　按照平台统一的配方和工艺要求，向客户供应功效性调养方案以及相应的客户服务。");
                h._LI();
                h.LI_().H5("服务站");
                h.P("　　为同城的调养和拼团业务做终端环节的货品派递。按货款的１０％提取服务佣金。");
                h._LI();
                h.LI_().H5("健康商家");
                h.P("　　通过拼团的方式供应健康产品或服务，内容涵盖全粮谷物、果蔬、餐料、特产、医养、护理等等。商家的经营可以面向同城或是全局，发布推广可以是临时性或是常规性。");
                h._LI();
                h.LI_().H5("社工机构");
                h.P("　　举办社工活动，线上线下聚结参与人员和志愿者，通过时间银行保存公益价值。时间银行自有监督公正性的机制，但仍需要各环节秉公操作。");
                h._LI();
                h._UL();
                h.H4_().T("代收结款")._H4();
                h.P("入驻服务方的部分线上业务需要由平台代收款项（通过微信支付）");
                h.P("平台以七天为一个结款周期，每周日固定为结算日——计算并支付在该日之前发生的代收或应付款项。这样在每次结款后，平台对各服务方的应付数额都归零（无押款）");
                h.P("每笔代收款项的２％将被平台扣除，其中０.６％是给微信公司的费用，１.４％作为平台自身区块链网络的运维费。");
                h.H4_().T("信用资产")._H4();
                h.P("服务方通过经营在平台上获得的信用积分，是一种数字资产（类似比特币），其价值与平台的市场价值成正比，并且可以在与平台联盟的所有平台之间流转使用。");
                h._ARTICLE();
            });
        }

        [UserAuthorize(orgly: ORGLY_MGR)]
        [Ui("图片"), Tool(ButtonCrop, Appear.Small)]
        public async Task icon(WebContext wc)
        {
            short id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                if (dc.QueryTop("SELECT icon FROM orgs WHERE id = @1", p => p.Set(id)))
                {
                    dc.Let(out byte[] bytes);
                    if (bytes == null) wc.Give(204); // no content 
                    else wc.Give(200, new StaticContent(bytes), shared: false, 60);
                }
                else wc.Give(404, shared: true, maxage: 3600 * 24); // not found
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];
                using var dc = NewDbContext();
                if (await dc.ExecuteAsync("UPDATE orgs SET icon = @1 WHERE id = @2", p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else wc.Give(500); // internal server error
            }
        }

        [UserAuthorize(orgly: ORGLY_MGR)]
        [Ui("设置", group: 1), Tool(ButtonShow)]
        public async Task setg(WebContext wc)
        {
            short orgid = wc[0];
            var obj = Fetch<Map<short, Org>>()[orgid];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Empty).T(" FROM users WHERE orgid = @1 AND orgly > 0");
                var map = dc.Query<int, User>(p => p.Set(orgid));
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改基本设置");
                    h.LI_().TEXT("标语", nameof(obj.tip), obj.tip, max: 16)._LI();
                    h.LI_().SELECT("联络员", nameof(obj.cttid), obj.cttid, map)._LI();
                    h.LI_().SELECT("状态", nameof(obj.status), obj.status, _Art.Statuses, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                var o = await wc.ReadObjectAsync(inst: obj); // use existing object
                using var dc = NewDbContext();
                // update the db record
                await dc.ExecuteAsync("UPDATE orgs SET tip = @1, cttid = CASE WHEN @2 = 0 THEN NULL ELSE @2 END, status = @3 WHERE id = @4",
                    p => p.Set(o.tip).Set(o.cttid).Set(o.status).Set(orgid));

                wc.GivePane(200);
            }
        }
    }
}