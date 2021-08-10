using System;
using System.Text;
using SkyChain.Web;

namespace Zhnt.Market
{
    public static class MarketUtility
    {
        public const string DIETS = "膳食调养";

        public static readonly string[] Numbers =
        {
            "零", "一", "二", "三", "四", "五", "六", "七", "八", "九",
            "十", "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九",
            "二十", "二十一", "二十二", "二十三", "二十四",
        };

        public static readonly string[] WeekDays =
        {
            "日", "一", "二", "三", "四", "五", "六",
        };

        public static HtmlContent CN_NUM(this HtmlContent h, int num)
        {
            h.Add(Numbers[num]);
            return h;
        }

        public static HtmlContent CN_WEEKDAY(this HtmlContent h, int num)
        {
            h.Add(WeekDays[num]);
            return h;
        }

        public static readonly string[] Percents =
        {
            "００％", "１０％", "２０％", "３０％", "４０％", "５０％", "６０％", "７０％", "８０％", "９０％",
            "１０＆", "１１０％", "１２０％", "１３０％", "１４０％", "１５０％", "１６０％", "１７０％", "１８０％", "１９０％",
            "２００＆", "２１０％", "２２０％", "２３０％", "２４０％"
        };

        public static HtmlContent PERCENT(this HtmlContent h, int num)
        {
            if (num > 0)
            {
                h.Add(Percents[num]);
            }
            return h;
        }


        public static HtmlContent DAY(this HtmlContent h, DateTime dt, bool cond = true)
        {
            if (cond)
            {
                var day = dt.Day;
                if (day < 10)
                {
                    h.T('0');
                }
                h.T(day).T("日");
                h.T(WeekDays[(short) dt.DayOfWeek]);
            }
            return h;
        }

        public static HtmlContent DATE_WEEK(this HtmlContent h, DateTime dt, bool cond = true, bool year = true)
        {
            if (cond)
            {
                byte y = year ? (byte) 3 : (byte) 2;
                h.T(dt, y, 0).SP();
                h.T(WeekDays[(short) dt.DayOfWeek]);
            }
            return h;
        }

        public static string GetCircledDateString(DateTime date, DateTime today)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            // h.T(year).T("年");

            var sb = new StringBuilder();
            if (today == date)
            {
                sb.Append("&#10084;&#65039;");
            }
            else if (today > date)
            {
                sb.Append("&#128420;");
            }
            else
            {
                sb.Append("&#128154;");
            }
            sb.Append("&nbsp;").Append(year).Append('-');
            if (month < 10)
            {
                sb.Append('0');
            }
            sb.Append(month).Append('-');
            if (day < 10)
            {
                sb.Append('0');
            }
            sb.Append(day);

            var dt = new DateTime(year, month, day);
            sb.Append(' ').Append(WeekDays[(short) dt.DayOfWeek]);

            return sb.ToString();
        }

        public static DateTime ToDateTime(int dt)
        {
            int year = dt / 10000;
            int month = dt % 10000 / 100;
            int day = dt % 100;
            return new DateTime(year, month, day);
        }

        public static int ToDt(DateTime date)
        {
            return date.Year * 10000 + date.Month * 100 + date.Day;
        }

        public const int MAX_WORKING_DAYS = 21;
    }
}