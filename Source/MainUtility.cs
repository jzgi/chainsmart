using System;
using System.Text;
using ChainFx;
using ChainFx.Web;
using static ChainFx.CryptoUtility;

namespace ChainMart
{
    public static class MainUtility
    {
        public static double ComputeDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const int EARTH_RADIUS_KM = 6371;

            var dlat = ToRadians(lat2 - lat1);
            var dlng = ToRadians(lng2 - lng1);

            lat1 = ToRadians(lat1);
            lat2 = ToRadians(lat2);

            var a = Math.Sin(dlat / 2) * Math.Sin(dlat / 2) + Math.Sin(dlng / 2) * Math.Sin(dlng / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EARTH_RADIUS_KM * c;

            static double ToRadians(double degrees)
            {
                return degrees * Math.PI / 180;
            }
        }

        public static string GetUrlLink(string uri)
        {
            string url;
            url = Application.Prog[nameof(url)];
            if (uri == null)
            {
                return url;
            }
            return url + uri;
        }


        public static HtmlBuilder A_POI(this HtmlBuilder h, double x, double y, string title, string addr, string tel = null, bool active = true)
        {
            if (active)
            {
                h.T("<a class=\"uk-icon-link\" uk-icon=\"location\" href=\"http://apis.map.qq.com/uri/v1/marker?marker=coord:").T(y).T(',').T(x).T(";title:").T(title).T(";addr:").T(addr);
                if (tel != null)
                {
                    h.T(";tel:").T(tel);
                }
                h.T("\" onclick=\"return dialog(this,32,false,'").T(title).T("');\"></a>");
            }
            else
            {
                h.T("<span class=\"uk-icon-link uk-inactive\" uk-icon=\"location\"></span>");
            }
            return h;
        }


        public static HtmlBuilder SELECT_ITEM(this HtmlBuilder h, string label, string name, short v, Map<short, Ware> opts, Map<short, Cat> cats, Func<Ware, bool> filter = null, bool required = false)
        {
            h.SELECT_(label, name, false, required);
            if (opts != null)
            {
                short last = 0; // last typ
                for (int i = 0; i < opts.Count; i++)
                {
                    var it = opts.ValueAt(i);
                    if (filter != null && !filter(it))
                    {
                        continue;
                    }
                    if (it.typ != last)
                    {
                        if (last > 0)
                        {
                            h.T("</optgroup>");
                        }
                        h.T("<optgroup label=\"").T(cats[it.typ].name).T("\">");
                    }
                    h.OPTION(it.id, it.name);

                    last = it.typ;
                }
                h.T("</optgroup>");
            }
            h._SELECT();
            return h;
        }


        public static HtmlBuilder SELECT_ORG(this HtmlBuilder h, string label, string name, int v, Map<int, Org> opts, Map<short, Reg> regs, Func<Org, bool> filter = null, bool required = false)
        {
            h.SELECT_(label, name, false, required);
            if (opts != null)
            {
                short last = 0; // last typ
                for (int i = 0; i < opts.Count; i++)
                {
                    var org = opts.ValueAt(i);
                    if (filter != null && !filter(org))
                    {
                        continue;
                    }
                    if (org.regid != last)
                    {
                        if (last > 0)
                        {
                            h.T("</optgroup>");
                        }
                        h.T("<optgroup label=\"").T(regs[org.regid]?.name).T("\">");
                    }
                    h.OPTION(org.id, org.name);

                    last = org.regid;
                }
                h.T("</optgroup>");
            }
            h._SELECT();
            return h;
        }

        public static HtmlBuilder RECEIVER(this HtmlBuilder h, string tel)
        {
            h.T("<a class=\"uk-icon-button uk-light\" href=\"tel:").T(tel).T("\" uk-icon=\"icon: receiver\"></a>");
            return h;
        }

        public static HtmlBuilder A_ICON(this HtmlBuilder h, string url, string icon)
        {
            h.T("<a class=\"uk-icon-button uk-light\" href=\"").T(url).T("\" uk-icon=\"icon: \"").T(icon).T("\"></a>");
            return h;
        }

        public static HtmlBuilder A_TEL(this HtmlBuilder h, string name, string tel, string css = null)
        {
            h.T("<a ");
            if (css != null)
            {
                h.T(" class=\"").T(css).T("\" ");
            }
            h.T("href=\"tel:").T(tel).T("\">").T(name).T("</a>");
            return h;
        }

        public static HtmlBuilder A_TEL_ICON(this HtmlBuilder h, string name, string tel = null)
        {
            h.T("<a class=\"uk-icon-link\" uk-icon=\"receiver\" href=\"tel:").T(tel).T("\"></a>");
            return h;
        }

        public static HtmlBuilder POI_(this HtmlBuilder h, double x, double y, string title, string addr, string tel = null)
        {
            h.T("<a class=\"\" href=\"http://apis.map.qq.com/uri/v1/marker?marker=coord:").T(y).T(',').T(x).T(";title:").T(title).T(";addr:").T(addr);
            if (tel != null)
            {
                h.T(";tel:").T(tel);
            }

            h.T("&referer=\">");
            return h;
        }

        public static HtmlBuilder _POI(this HtmlBuilder h)
        {
            h.T("</a>");
            return h;
        }

        public static HtmlBuilder MASKNAME(this HtmlBuilder h, string name)
        {
            for (int i = 0; i < name?.Length; i++)
            {
                var c = name[i];
                if (i == 0)
                {
                    h.T(c);
                }
                else
                {
                    h.T('Ｘ');
                }
            }
            return h;
        }


        public static string ComputeCredential(string tel, string password)
        {
            string v = tel + ":" + password;
            return MD5(v);
        }

        public static void SetUserCookie(this WebContext wc, User o)
        {
            const int A_WEEK = 3600 * 24 * 7;

            // token cookie
            var token = AuthenticateAttribute.ToToken(o, 0x0fff);
            var tokenStr = WebUtility.FormatSetCookie(nameof(token), token, maxage: A_WEEK, httponly: true);

            // cookie for vip, o means none
            var vipStr = WebUtility.FormatSetCookie(nameof(o.vip), o.vip.ToString(), maxage: A_WEEK);

            var nameStr = WebUtility.FormatSetCookie(nameof(o.name), (o.name), maxage: A_WEEK);
            var telStr = WebUtility.FormatSetCookie(nameof(o.tel), o.tel, maxage: A_WEEK);

            // multiple cookie
            wc.SetHeader("Set-Cookie", tokenStr, vipStr, nameStr, telStr);
        }

        public static void ViewAgrmt(this HtmlBuilder h, JObj jo)
        {
            string title = null;
            string a = null, b = null;
            string[] terms = null;
            jo.Get(nameof(title), ref title);
            jo.Get(nameof(a), ref a);
            jo.Get(nameof(b), ref b);
            jo.Get(nameof(terms), ref terms);

            h.OL_();
            for (int i = 0; i < terms.Length; i++)
            {
                var o = terms[i];
                h.LI_().T(o)._LI();
            }
            h._OL();
            h.FIELD("甲方", a).FIELD("乙方", b);
        }


        public const string LOTS = "健康拼团";

        public static string FormatLotTime(DateTime t)
        {
            var sb = new StringBuilder();

            sb.Append(t.Year).Append('-');

            var mon = t.Month;
            if (mon < 10)
            {
                sb.Append('0');
            }
            sb.Append(mon).Append('-');

            var day = t.Day;
            if (day < 10)
            {
                sb.Append('0');
            }
            sb.Append(day).Append(' ');

            var hr = t.Hour;
            if (hr < 10)
            {
                sb.Append('0');
            }
            sb.Append(hr).Append(':');

            var min = t.Minute;
            if (min < 10)
            {
                sb.Append('0');
            }
            sb.Append(min);

            return sb.ToString();
        }
    }
}