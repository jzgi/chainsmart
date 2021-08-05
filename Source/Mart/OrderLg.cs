using System;
using SkyChain;
using SkyChain.Web;

namespace Zhnt.Mart
{
    /// <summary>
    /// A log record for one day of order execution.
    /// </summary>
    public class OrderLg : IData, IKeyable<int>
    {
        public static readonly OrderLg Empty = new OrderLg();

        public const byte BASIC = 1, LOG = 2;

        public static readonly Map<short, string> Digests = new Map<short, string>
        {
            {0, "无"},
            {1, "轻微改善（排泄有所改观）"},
            {2, "轻微改善（腹胀感觉转轻）"},
            {3, "明显改善（餐前有饥饿感）"},
            {4, "明显改善（嘴唇转红，气色向好）"},
            {5, "重大改善（肌肉力量有所恢复）"},
            {6, "重大改善（每天至少规律排便一次）"},
        };

        public static readonly Map<short, string> Rests = new Map<short, string>
        {
            {0, "无"},
            {1, "轻微改善（多睡半小时）"},
            {2, "轻微改善（睡眠深沉度略微好转）"},
            {3, "明显改善（多睡一小时）"},
            {4, "明显改善（睡眠深沉度明显好转）"},
            {5, "重大改善（多睡一个半小时）"},
            {6, "重大改善（睡眠深沉度很好）"},
        };

        public static readonly Map<short, string> Fits = new Map<short, string>
        {
            {0, "无"},
            {1, "轻微改善（体重减少０.５公斤）"},
            {2, "轻微改善（主要积脂部位轻微收缩）"},
            {3, "明显改善（体重减１公斤）"},
            {4, "明显改善（主要积脂部位明显收缩）"},
            {5, "重大改善（体重减１.５公斤以上）"},
            {6, "重大改善（体脂比例整体下降）"},
        };

        public static readonly Map<short, string> Bloods = new Map<short, string>
        {
            {0, "无"},
            {1, "轻微改善（血压降５）"},
            {2, "轻微改善（血脂单项有所下降）"},
            {3, "明显改善（血压降１０"},
            {4, "明显改善（血脂单项明显下降）"},
            {5, "重大改善（血压降１５以上）"},
            {6, "重大改善（血脂多项明显下降）"},
        };

        public static readonly Map<short, string> Sugars = new Map<short, string>
        {
            {0, "无"},
            {1, "轻微改善（血糖降１.０）"},
            {2, "明显改善（血糖降２.０）"},
            {3, "重大改善（血糖降３.０）"},
        };

        public static readonly Map<short, string> Styles = new Map<short, string>
        {
            {0, "无"},
            {1, "有所改善（接受了较清淡纯正、少毒少刺激的食物）"},
            {2, "很大改善（习惯了用粗加工的全粮谷物代替过度加工的残粮）"},
            {3, "完全改善（在食用全粮的同时，摒弃了高胆固醇、多激素等危害的肉食）"},
        };

        public static readonly Map<short, string> Statues = new Map<short, string>
        {
            {0, null},
            {1, "✔"},
        };


        internal int orderid;
        internal DateTime dt;
        internal short track;
        internal short status;
        internal short fit;
        internal short digest;
        internal short rest;
        internal short blood;
        internal short sugar;
        internal short style;

        public void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & BASIC) == BASIC)
            {
                s.Get(nameof(orderid), ref orderid);
                s.Get(nameof(dt), ref dt);
                s.Get(nameof(track), ref track);
                s.Get(nameof(status), ref status);
            }
            if ((proj & LOG) == LOG)
            {
                s.Get(nameof(fit), ref fit);
                s.Get(nameof(digest), ref digest);
                s.Get(nameof(rest), ref rest);
                s.Get(nameof(blood), ref blood);
                s.Get(nameof(sugar), ref sugar);
                s.Get(nameof(style), ref style);
            }
        }

        public void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & BASIC) == BASIC)
            {
                s.Put(nameof(orderid), orderid);
                s.Put(nameof(dt), dt);
                s.Put(nameof(track), track);
                s.Put(nameof(status), status);
            }
            if ((proj & LOG) == LOG)
            {
                s.Put(nameof(fit), fit);
                s.Put(nameof(digest), digest);
                s.Put(nameof(rest), rest);
                s.Put(nameof(blood), blood);
                s.Put(nameof(sugar), sugar);
                s.Put(nameof(style), style);
            }
        }

        public int Key => orderid;

        public void LogBrief(HtmlContent h, bool tip = false)
        {
            int c = 0;
            if (fit > 0)
            {
                if (c++ > 0) h.T('、');
                h.T("肠胃排泄");
            }
            if (digest > 0)
            {
                if (c++ > 0) h.T('、');
                h.T("睡眠精力");
            }
            if (rest > 0)
            {
                if (c++ > 0) h.T('、');
                h.T("体重体型");
            }
            if (blood > 0)
            {
                if (c++ > 0) h.T('、');
                h.T("血压血脂");
            }
            if (sugar > 0)
            {
                if (c++ > 0) h.T('、');
                h.T("血糖指标");
            }
            if (style > 0)
            {
                if (c++ > 0) h.T('、');
                h.T("饮食习惯");
            }
            if (tip && c == 0)
            {
                h.T("未填日志");
            }
        }
    }
}