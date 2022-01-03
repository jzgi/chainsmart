using System;
using System.Text;
using SkyChain;
using SkyChain.Web;
using static SkyChain.CryptoUtility;

namespace Revital
{
    public static class RevitalUtility
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
            url = Application.extcfg[nameof(url)];
            if (uri == null)
            {
                return url;
            }
            return url + uri;
        }


        public static HtmlContent A_POI(this HtmlContent h, double x, double y, string title, string addr, string tel = null)
        {
            h.T("<a class=\"uk-icon-link\" uk-icon=\"location\" href=\"http://apis.map.qq.com/uri/v1/marker?marker=coord:").T(y).T(',').T(x).T(";title:").T(title).T(";addr:").T(addr);
            if (tel != null)
            {
                h.T(";tel:").T(tel);
            }
            h.T("\" onclick=\"return dialog(this,8,false,1,'')\"></a>");
            return h;
        }


        public static HtmlContent SELECT_ITEM(this HtmlContent h, string label, string name, short v, Map<short, Item> opts, Map<short, string> cats, Func<Item, bool> filter = null, bool required = false)
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
                    if (it.cat != last)
                    {
                        if (last > 0)
                        {
                            h.T("</optgroup>");
                        }
                        h.T("<optgroup label=\"").T(cats[it.cat]).T("\">");
                    }
                    h.OPTION(it.id, it.name);

                    last = it.cat;
                }
                h.T("</optgroup>");
            }
            h._SELECT();
            return h;
        }

        public static HtmlContent SELECT_ORG(this HtmlContent h, string label, string name, int v, Map<int, Org> opts, Map<short, Reg> regs, Func<Org, bool> filter = null, bool required = false)
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

        public static HtmlContent SELECT_PLAN(this HtmlContent h, string label, string name, int v, Map<int, Product> opts, Map<short, string> cats, Func<Product, bool> filter = null, bool required = false)
        {
            h.SELECT_(label, name, false, required);
            if (opts != null)
            {
                short last = 0; // last typ
                for (int i = 0; i < opts.Count; i++)
                {
                    var plan = opts.ValueAt(i);
                    if (filter != null && !filter(plan))
                    {
                        continue;
                    }
                    if (plan.cat != last)
                    {
                        if (last > 0)
                        {
                            h.T("</optgroup>");
                        }
                        h.T("<optgroup label=\"").T(cats[plan.cat]).T("\">");
                    }
                    h.OPTION(plan.id, plan.name);

                    last = plan.cat;
                }
                h.T("</optgroup>");
            }
            h._SELECT();
            return h;
        }


        public static HtmlContent RECEIVER(this HtmlContent h, string tel)
        {
            h.T("<a class=\"uk-icon-button uk-light\" href=\"tel:").T(tel).T("\" uk-icon=\"icon: receiver\"></a>");
            return h;
        }

        public static HtmlContent A_ICON(this HtmlContent h, string url, string icon)
        {
            h.T("<a class=\"uk-icon-button uk-light\" href=\"").T(url).T("\" uk-icon=\"icon: \"").T(icon).T("\"></a>");
            return h;
        }

        public static HtmlContent A_TEL(this HtmlContent h, string name, string tel, string css = null)
        {
            h.T("<a ");
            if (css != null)
            {
                h.T(" class=\"").T(css).T("\" ");
            }
            h.T("href=\"tel:").T(tel).T("\">").T(name).T("</a>");
            return h;
        }

        public static HtmlContent A_TEL_ICON(this HtmlContent h, string name, string tel = null)
        {
            h.T("<a class=\"uk-icon-link\" uk-icon=\"receiver\" href=\"tel:").T(tel).T("\"></a>");
            return h;
        }

        public static HtmlContent POI_(this HtmlContent h, double x, double y, string title, string addr, string tel = null)
        {
            h.T("<a class=\"\" href=\"http://apis.map.qq.com/uri/v1/marker?marker=coord:").T(y).T(',').T(x).T(";title:").T(title).T(";addr:").T(addr);
            if (tel != null)
            {
                h.T(";tel:").T(tel);
            }

            h.T("&referer=\">");
            return h;
        }

        public static HtmlContent _POI(this HtmlContent h)
        {
            h.T("</a>");
            return h;
        }

        public static HtmlContent MASKNAME(this HtmlContent h, string name)
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

        public static void SetTokenCookie(this WebContext wc, User o)
        {
            string token = AuthenticateAttribute.EncryptPrincipal(o, 0x0fff);
            wc.SetCookie(nameof(token), token);
        }

        public static void ViewAgrmt(this HtmlContent h, JObj jo)
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