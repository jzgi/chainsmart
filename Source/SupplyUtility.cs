using System;
using System.Text;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Db;
using SkyChain.Web;
using static SkyChain.CryptoUtility;
using static Revital.Supply.Book;

namespace Revital.Supply
{
    public static class SupplyUtility
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

        public static HtmlContent SELECT_ITEM(this HtmlContent h, string label, string name, int v, Map<int, Item> opt, Map<short, string> typs, bool required = true)
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

        public static void SetTokenCookie(this WebContext wc, User_ o)
        {
            const byte proj_all_but_privacy = 0x0f ^ User_.PRIVACY;
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


        public const string LOTS = "健康拼团";

        public static int[] GetRelatedOrgs(this Map<short, Org> orgs, Reg reg)
        {
            var lst = new ValueList<int>(32);
            for (int i = 0; i < orgs.Count; i++)
            {
                var org = orgs.ValueAt(i);
                if (org.IsMerchantTo(reg) || org.IsSocialTo(reg))
                {
                    lst.Add(org.id);
                }
            }
            return lst.ToArray();
        }

        public static void ViewLotTop(this HtmlContent h, Purchase off, string icon, string img)
        {
            h.SECTION_("uk-flex");
            h.PIC_("uk-width-1-2 uk-margin-auto-vertical");
            h._PIC();
            h._SECTION();
        }

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

        public static async Task<bool> AddLotJnAsync(this DbContext dc, int lotid, int uid, decimal cash, Map<short, Org> orgs)
        {
            dc.Sql("UPDATE lotjns SET status =  pay = @1 WHERE lotid = @2 AND uid = @3 AND status = RETURNING qty, pay");
            if (!await dc.QueryTopAsync(p => p.Set(cash).Set(lotid).Set(uid)))
            {
                return false;
            }
            dc.Let(out short qty);
            dc.Let(out decimal pay);

            dc.Sql("UPDATE lots SET qtys = qtys + @1, pays = pays + @2 WHERE id = @3 AND status = ").T(STATUS_CREATED).T(" RETURNING typ, orgid, name, min, qtys");
            if (!await dc.QueryTopAsync(p => p.Set(qty).Set(pay).Set(lotid)))
            {
                return false;
            }
            dc.Let(out short typ);
            dc.Let(out short orgid);
            dc.Let(out string name);
            dc.Let(out short min);
            dc.Let(out short qtys);


            return true;
        }


        private const string
            LOTJN_NOT_FOUND = "LOTJN_NOT_FOUND",
            LOT_NOT_FOUND = "LOT_NOT_FOUND";

        public static async Task<string> RemoveLotJnAsync(this DbContext dc, int lotid, int uid, string reason = null)
        {
            dc.Sql("DELETE FROM lotjns WHERE lotid = @1 AND uid = @2 AND status = RETURNING qty, inited, pay");
            if (!await dc.QueryTopAsync(p => p.Set(lotid).Set(uid)))
            {
                return LOTJN_NOT_FOUND;
            }
            dc.Let(out short qty);
            dc.Let(out DateTime inited);
            dc.Let(out decimal pay);

            dc.Sql("UPDATE lots SET qtys = qtys - @1, pays = pays - @2 WHERE id = @3 AND status < ").T(STATUS_SUBMITTED);
            if (await dc.ExecuteAsync(p => p.Set(qty).Set(pay).Set(lotid)) < 1)
            {
                return LOT_NOT_FOUND;
            }

            // refund
            if (pay > 0.00M)
            {
            }
            return null;
        }

        public static async Task<bool> SucceedLotAsync(this DbContext dc, int lotid, Map<short, Org> orgs)
        {
            // update status of the master record
            dc.Sql("UPDATE lots SET status = ").T(STATUS_SUBMITTED).T(" WHERE id = @1 AND status = ").T(STATUS_CREATED).T(" RETURNING typ, orgid, name");
            if (!await dc.QueryTopAsync(p => p.Set(lotid)))
            {
                return false;
            }
            dc.Let(out short typ);
            dc.Let(out short orgid);
            dc.Let(out string name);

            var org = orgs[orgid];


            return true;
        }

        public static async Task<bool> AbortLotAsync(this DbContext dc, int lotid, string reason, Map<short, Org> orgs)
        {
            // update master status
            //
            dc.Sql("UPDATE lots SET status = ").T(STATUS_ABORTED).T(", qtys = 0, pays = 0 WHERE id = @1 AND status < ").T(STATUS_CLOSED).T(" RETURNING typ, orgid, name");
            if (!await dc.QueryTopAsync(p => p.Set(lotid)))
            {
                return false;
            }
            dc.Let(out short typ);
            dc.Let(out short orgid);
            dc.Let(out string name);

            var org = orgs[orgid];

            await WeChatUtility.PostSendAsync(org.Im, "【" + LOTS + "】" + name + reason + "（" + org.name + "）");

            // delete all joiners
            //
            dc.Sql("DELETE FROM lotjns WHERE lotid = @1 AND status = RETURNING uid, uim, inited, pay");
            await dc.QueryAsync(p => p.Set(lotid));
            while (dc.Next())
            {
                dc.Let(out int uid);
                dc.Let(out string uim);
                dc.Let(out DateTime inited);
                dc.Let(out decimal pay);

                var sb = new StringBuilder();
                sb.Append("【").Append(LOTS).Append("】");
                sb.Append(name).Append(reason);
                sb.Append("（").Append(org.name).Append("）");

                // refund
                if (pay > 0.00M)
                {
                }

                await WeChatUtility.PostSendAsync(uim, sb.ToString());
            }

            return true;
        }
    }
}