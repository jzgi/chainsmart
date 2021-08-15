using System;
using SkyChain;
using SkyChain.Web;
using Zhnt;
using static SkyChain.CryptoUtility;

namespace Zhnt
{
    public static class ZhntUtility
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
            url = ServerEnviron.extcfg[nameof(url)];
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


        public static HtmlContent TOPBAR_DIET(this HtmlContent h, ref int curtyp)
        {
            h.NAV_("uk-top-bar");

            h.DIV_("uk-button-group uk-flex"); // tabbed subnav of item groups
        
            // h.T("<span class=\"uk-margin-left-auto\">功效见证&nbsp;<button class=\"uk-icon-button uk-danger\" formaction=\"cases\" onclick=\"return dialog(this,8,false,4,'功效见证')\"><span uk-icon=\"users\"></span></button></span>");

            h._NAV();
            h.DIV_("uk-top-placeholder")._DIV();
            return h;
        }

        static readonly string[] Subnavs = {"产品", "服务", "社工"};

        public static HtmlContent TOPBAR_LOT(this HtmlContent h, Reg reg, int subscript, Map<short, Reg> regs)
        {
            h.NAV_("uk-top-bar");

            h.SECTION_("uk-flex");
            var regid = reg.id;
            h.FORM_("uk-width-xsmall").SELECT(null, nameof(regid), regid, regs, refresh: true)._FORM();

            h.SUBNAV(Subnavs, string.Empty, subscript);

            h._SECTION();

            h.T("<button class=\"uk-button uk-icon-button uk-danger uk-margin-left-auto\" formaction=\"/org/?regid=").T(regid).T("\" onclick=\"return dialog(this,8,false,4,'供应方')\">供方</button>");

            h._NAV();
            h.DIV_("uk-top-placeholder")._DIV();
            return h;
        }

        public static HtmlContent SELECT_ITEM(this HtmlContent h, string label, string name, short v, Map<short, Item> opt, Map<short, string> typs, bool required = true)
        {
            h.SELECT_(label, name, false, required);
            if (opt != null)
            {
                short last = 0; // last typ
                for (int i = 0; i < opt.Count; i++)
                {
                    var it = opt.ValueAt(i);
                    if (it.typ != last)
                    {
                        if (last > 0)
                        {
                            h.T("</optgroup>");
                        }
                        h.T("<optgroup label=\"").T(typs[it.typ]).T("\">");
                    }
                    h.OPTION(it.id, it.name);

                    last = it.typ;
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
            long hash = 0;
            Digest(v, ref hash);
            return ToHex(hash);
        }

        public static void SetTokenCookie(this WebContext wc, User o)
        {
            const byte proj_all_but_privacy = 0x0f ^ User.PRIVACY;
            string token = AuthenticateAttribute.EncryptPrincipal(o, proj_all_but_privacy);
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
    }
}