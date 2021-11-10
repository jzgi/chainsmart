using System;
using SkyChain;
using SkyChain.Web;
using static Revital.Org;

namespace Revital
{
    /// <summary>
    /// The formatives of posts and acts.
    /// </summary>
    public class PostDescr : IKeyable<short>
    {
        public static readonly Map<short, PostDescr> All = new Map<short, PostDescr>()
        {
            new PostDescr
            {
                typ = FRK_AGRI,
                name = "生态农产",
                CardView = (h, descr, m, org) =>
                {
                    h.DL_("uk-description-list");
                    h.P_("uk-text-truncate").T(m.tip)._P();
                    h.DD("价格").DT_().CNY(m.price, em: true).T('／').T(m.unit);
                    h._DT();
                    h.DD("派递").DT_();
                    h.T("服务站");
                    h._DT()._DL();
                },
                FormView = (h, descr, m, org) =>
                {
                    if (m.min <= 0) m.min = 1;
                    if (m.max <= 0) m.max = 100;
                    if (m.step <= 0) m.step = 1;

                    h.LI_().TEXT("名称", nameof(m.name), m.name, min: 2, max: 16, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, min: 10, max: 30, required: true)._LI();
                    h.LI_().NUMBER("单价", nameof(m.price), m.price, min: 0.01M, max: 9999.00M, required: true)._LI();
                    h.LI_().NUMBER("成团量", nameof(m.min), m.min, min: 1, max: 2000, required: true).NUMBER("封顶量", nameof(m.max), m.max, min: 1, max: 2000, required: true)._LI();
                    h.LI_()._LI();
                }
            },
            new PostDescr
            {
                typ = FRK_DIETARY,
                name = "调养膳食"
            },
            new PostDescr
            {
                typ = FRK_HOME,
                name = "工业产品"
            },
            new PostDescr
            {
                typ = FRK_CARE,
                name = "家政陪护"
            },
            new PostDescr
            {
                typ = FRK_AD,
                name = "广告宣传"
            },
            new PostDescr
            {
                typ = FRK_CHARITY,
                name = "志愿公益"
            }
        };

        short typ;

        string name;

        public Action<HtmlContent, PostDescr, Post, Org> CardView;

        public Action<HtmlContent, PostDescr, Post, Org> FormView;

        public short Key => typ;

        public override string ToString() => name;
    }
}