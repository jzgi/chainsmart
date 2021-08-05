using System;
using SkyChain;
using SkyChain.Web;
using static System.String;

namespace Zhnt.Supply
{
    /// <summary>
    /// A factory descriptor for lot records.
    /// </summary>
    public class LotDescr : IKeyable<short>
    {
        public static readonly Map<short, string> ShipSpans = new Map<short, string>
        {
            {1, "成团后一天内"},
            {2, "成团后两天内"},
            {3, "成团后三天内"},
            {4, "截止后一天内"},
            {5, "截止后两天内"},
            {6, "截止后三天内"},
        };

        // spans
        public static readonly Map<short, string> EventSpans = new Map<short, string>
        {
            {1, "一小时"},
            {2, "两小时"},
            {3, "三小时"},
            {4, "四小时"},
            {5, "五小时"},
            {6, "六小时"},
            {7, "七小时"},
            {8, "八小时"},

            {11, "两天，各一小时"},
            {12, "两天，各两小时"},
            {13, "两天，各三小时"},
            {14, "两天，各四小时"},
            {15, "两天，各五小时"},
            {16, "两天，各六小时"},
            {17, "两天，各七小时"},
            {18, "两天，各八小时"},

            {21, "三天，各一小时"},
            {22, "三天，各两小时"},
            {23, "三天，各三小时"},
            {24, "三天，各四小时"},
            {25, "三天，各五小时"},
            {26, "三天，各六小时"},
            {27, "三天，各七小时"},
            {28, "三天，各八小时"},
        };

        public static readonly Map<short, LotDescr> All = new Map<short, LotDescr>()
        {
            new LotDescr(Purchase.TYP_PRODUCT)
            {
                name = "产品拼团",
                tip = "本方提供生态产品，允许多个买家拼单，预付款项，成功后通过适当的方式派送给买家",
                act = "拼单",
                addrlbl = "收货地址",
                CanDo = (org) => org.IsMerchant,
                CardView = (h, descr, m, org) =>
                {
                    h.DL_("uk-description-list");
                    h.P_("uk-text-truncate").T(m.tip)._P();
                    h.DD("价格").DT_().CNY(m.price, em: true).T('／').T(m.unit);
                    if (!IsNullOrEmpty(m.unitip))
                    {
                        h.T('（').T(m.unitip).T('）');
                    }
                    h._DT();
                    h.DD("派递").DT_();
                    if (org.IsInternal)
                    {
                        h.T("服务站");
                    }
                    else if (IsNullOrEmpty(m.addr))
                    {
                        h.T("包邮到户");
                    }
                    else
                    {
                        h.T(m.addr);
                    }
                    h.T("，").T(ShipSpans[m.span]).T("发货");
                    h._DT();
                    h.DD("进度").DT_();
                    if (m.qtys >= m.min)
                    {
                        h.T("已成团，已拼 <em>").T(m.qtys).T("</em> ").T(m.unit);
                    }
                    else
                    {
                        h.T("差 <em>").T(m.min - m.qtys).T("</em> ").T(m.unit).T("成团");
                    }
                    var left = (m.ended - DateTime.Today).Days + 1;
                    if (left > 1)
                    {
                        h.T("，剩 <em>").T(left).T("</em> 天截止");
                    }
                    else if (left == 1)
                    {
                        h.T("，最后一天");
                    }
                    else
                    {
                        h.T("，已截止");
                    }
                    h._DT()._DL();
                },
                FormView = (h, descr, m, org) =>
                {
                    if (m.ended == default) m.ended = DateTime.Today.AddDays(3);
                    if (m.min <= 0) m.min = 1;
                    if (m.max <= 0) m.max = 100;
                    if (m.least <= 0) m.least = 1;
                    if (m.step <= 0) m.step = 1;

                    h.LI_().TEXT("名称", nameof(m.name), m.name, min: 2, max: 16, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, min: 10, max: 30, required: true)._LI();
                    h.LI_().TEXT("单位", nameof(m.unit), m.unit, min: 1, max: 4, required: true).TEXT("单位提示", nameof(m.unitip), m.unitip, max: 8)._LI();
                    h.LI_().NUMBER("单价", nameof(m.price), m.price, min: 0.01M, max: 9999.00M, required: true)._LI();
                    h.LI_().NUMBER("起订", nameof(m.least), m.least, min: 1, max: 200, required: true).NUMBER("递增", nameof(m.step), m.step, min: 1, max: 200, required: true)._LI();
                    h.LI_().DATE("截止日期", nameof(m.ended), m.ended).SELECT("发货", nameof(m.span), m.span, ShipSpans)._LI();
                    h.LI_().NUMBER("成团量", nameof(m.min), m.min, min: 1, max: 2000, required: true).NUMBER("封顶量", nameof(m.max), m.max, min: 1, max: 2000, required: true)._LI();
                    h.LI_()._LI();
                }
            },

            new LotDescr(Purchase.TYP_SERVICE)
            {
                name = "服务拼团",
                tip = "本方提供健康服务，允许多个客户预约，不预付，成功后在指定场地或者应邀上门实施",
                act = "预约",
                addrlbl = "服务场址",
                CanDo = (org) => org.IsMerchant,
                CardView = (h, descr, m, org) =>
                {
                    h.DL_("uk-description-list");
                    h.P_("uk-text-truncate uk-background-muted").T(m.tip)._P();
                    h.DD("价格").DT_().CNY(m.price, em: true).T('／').T(m.unit);
                    if (!IsNullOrEmpty(m.unitip))
                    {
                        h.SUB_().T('（').T(m.unitip).T('）')._SUB();
                    }
                    h._DT();
                    h.DD("场址").DT_();
                    if (IsNullOrEmpty(m.addr))
                    {
                        h.T("由客户指定");
                    }
                    else
                    {
                        h.T(m.addr);
                    }
                    h._DT();
                    h.DD("情况").DT_();
                    if (m.qtys > 0)
                    {
                        h.T(m.qtys >= m.min ? "已成团" : "未成团");
                        h.T("，预约 <em>").T(m.qtys).T("</em> ").T(m.unit);
                    }
                    else
                    {
                        h.T("尚无预约");
                    }
                    var left = (m.ended - DateTime.Today).Days + 1;
                    if (left > 1)
                    {
                        h.T("，剩 <em>").T(left).T("</em> 天截止");
                    }
                    else if (left == 1)
                    {
                        h.T("，最后一天");
                    }
                    else
                    {
                        h.T("，已截止");
                    }
                    h._DT()._DL();
                },
                FormView = (h, descr, m, org) =>
                {
                    if (m.ended == default) m.ended = DateTime.Today.AddMonths(1);
                    if (m.min <= 0) m.min = 1;
                    if (m.max <= 0) m.max = 100;

                    h.LI_().TEXT("名称", nameof(m.name), m.name, min: 2, max: 16, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, min: 10, max: 30, tip: "１０～３０字", required: true)._LI();
                    h.LI_().TEXT("单位", nameof(m.unit), m.unit, min: 1, max: 4, required: true).TEXT("单位提示", nameof(m.unitip), m.unitip, max: 8)._LI();
                    h.LI_().NUMBER("单价", nameof(m.price), m.price, min: 0.01M, max: 9999.00M, required: true)._LI();
                    h.LI_().NUMBER("起订", nameof(m.least), m.least, min: 1, max: 200, required: true).NUMBER("递增", nameof(m.step), m.step, min: 1, max: 200, required: true)._LI();
                    h.LI_().TEXT("服务场址", nameof(m.addr), m.addr, min: 4, max: 20, tip: "空白表示场址由" + descr.addrlbl + "指定")._LI();
                    h.LI_().DATE("截止日期", nameof(m.ended), m.ended)._LI();
                    h.LI_().NUMBER("成团量", nameof(m.min), m.min, min: 1, max: 200, required: true).NUMBER("封顶量", nameof(m.max), m.max, min: 1, max: 200, required: true)._LI();
                }
            },

            new LotDescr(Purchase.TYP_EVENT)
            {
                name = "社工活动",
                tip = "组织社工活动，邀请参与者线上报名，使用时间银行奖励志愿者",
                act = "报名",
                addrlbl = "活动场址",
                CanDo = (org) => org.IsSocial,
                CardView = (h, descr, m, org) =>
                {
                    h.DL_("uk-description-list");
                    h.P_("uk-text-truncate").T(m.tip)._P();
                    h.DD("时间").DT_().T(m.start, 3, 2).T("（").T(EventSpans[m.span]).T("）")._DT();
                    if (!IsNullOrEmpty(m.addr))
                    {
                        h.DD("场址").DT(m.addr);
                    }
                    h.DD("情况").DT_();
                    h.T("线上已报名 <em>").T(m.qtys).T("</em> ").T(m.unit);
                    var left = (m.ended - DateTime.Today).Days + 1;
                    if (left > 1)
                    {
                        h.T("，剩 <em>").T(left).T("</em> 天截止");
                    }
                    else if (left == 1)
                    {
                        h.T("，最后一天");
                    }
                    else
                    {
                        h.T("，已截止");
                    }
                    h._DT()._DL();
                },
                FormView = (h, descr, m, org) =>
                {
                    if (IsNullOrEmpty(m.unit)) m.unit = "人";
                    if (m.ended == default) m.ended = DateTime.Today;
                    if (m.min <= 0) m.min = 1;
                    if (m.max <= 0) m.max = 100;

                    h.LI_().TEXT("标题", nameof(m.name), m.name, min: 2, max: 16, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, min: 10, max: 30, tip: "１０～３０字", required: true)._LI();
                    h.LI_().TEXT("单位", nameof(m.unit), m.unit, min: 1, max: 4, required: true).TEXT("单位提示", nameof(m.unitip), m.unitip, max: 8)._LI();
                    h.HIDDEN(nameof(m.least), m.least);
                    h.LI_().DATETIME("开始时间", nameof(m.start), m.start)._LI();
                    h.LI_().SELECT("", nameof(m.span), m.span, EventSpans)._LI();
                    h.LI_().TEXT("活动场址", nameof(m.addr), m.addr, min: 4, max: 20, required: true)._LI();
                    h.LI_().DATE("报名截止", nameof(m.ended), m.ended)._LI();
                    h.LI_().NUMBER("成团量", nameof(m.min), m.min, min: 1, max: 200, required: true).NUMBER("满载量", nameof(m.max), m.max, min: 1, max: 200, required: true)._LI();
                    h._FIELDSUL();
                }
            }
        };

        readonly short typ;

        internal string name;

        internal string tip;

        internal string act;

        internal string addrlbl;

        public short Key => typ;

        public Action<HtmlContent, LotDescr, Purchase, Org> CardView;

        public Action<HtmlContent, LotDescr, Purchase, Org> FormView;

        public Func<Org, bool> CanDo;

        LotDescr(short typ) => this.typ = typ;

        public short Id => typ;

        public string Name => name;

        public string Tip => tip;

        public string Act => act;

        public bool IsPersonal => typ > 1;

        public bool IsPayBefore => typ == Purchase.TYP_PRODUCT;
        public bool IsPayAfter => typ == Purchase.TYP_SERVICE;
        public bool IsNoPay => typ == Purchase.TYP_EVENT;
    }
}