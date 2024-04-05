using System;
using ChainFX;
using ChainFX.Web;
using static ChainFX.CryptoUtility;

namespace ChainSmart;

public static class MainUtility
{
    /// <summary>
    /// based since 2020
    /// </summary>
    public static int ToInt2020(DateTime v)
    {
        return (v.Year - 2020) << 26 |
               v.Month << 22 | // 4 
               v.Day << 17 | // 5
               v.Hour << 12 | // 5
               v.Minute << 6 | // 6 
               v.Second; // 6
    }

    public static DateTime ToDateTime(int v2020)
    {
        return new DateTime(
            (v2020 >> 26) + 2020,
            v2020 >> 22 & 0b1111,
            v2020 >> 17 & 0b11111,
            v2020 >> 12 & 0b11111,
            v2020 >> 6 & 0b111111,
            v2020 & 0b111111
        );
    }

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

    public static HtmlBuilder SELECT_SPEC(this HtmlBuilder h, string name, JObj specs, string onchange = null, string css = null)
    {
        h.SELECT_(name, local: name, onchange: onchange, required: true, css: css);

        for (int i = 0; i < specs?.Count; i++)
        {
            var spec = specs.EntryAt(i);
            var v = spec.Value;
            if (v.IsObject)
            {
                h.OPTGROUP_(spec.Key);

                var sub = (JObj)v;
                for (int k = 0; k < sub.Count; k++)
                {
                    var e = sub.EntryAt(k);
                    h.OPTION(e.Key, e.Value);
                }

                h._OPTGROUP();
            }
            else
            {
                h.OPTION(spec.Key, spec.Value);
            }
        }

        h._SELECT();

        return h;
    }

    public static HtmlBuilder SELECT_ORG(this HtmlBuilder h, string name, int orgid, Org[] orgs, Map<short, Reg> regs, string onchange = null, string css = null)
    {
        if (orgs != null)
        {
            Array.Sort(orgs, (x, y) => x.regid - y.regid);
        }

        h.SELECT_(name, local: name, onchange: onchange, empty: string.Empty, css: css);

        short last = 0;
        for (int i = 0; i < orgs?.Length; i++)
        {
            var o = orgs[i];


            if (last != o.regid)
            {
                if (i > 0)
                {
                    h._OPTGROUP();
                }

                var reg = regs[o.regid];
                h.OPTGROUP_(reg.name);

                last = o.regid;
            }

            h.OPTION(o.Key, o.Name, selected: o.id == orgid);
        }
        h._OPTGROUP();

        h._SELECT();

        return h;
    }

    public static HtmlBuilder RECEIVER(this HtmlBuilder h, string tel)
    {
        h.T("<a class=\"uk-icon-button uk-light\" href=\"tel:").T(tel).T("\" uk-icon=\"icon: receiver\"></a>");
        return h;
    }

    public static HtmlBuilder AICON(this HtmlBuilder h, string url, string icon, string css = null)
    {
        h.T("<a class=\"uk-icon-link uk-circle");
        if (css != null)
        {
            h.T(' ');
            h.T(css);
        }
        h.T("\" target=\"_parent\" href=\"").T(url).T("\" uk-icon=\"icon: ").T(icon).T("\"></a>");
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

    public static HtmlBuilder ATEL(this HtmlBuilder h, string tel = null, string css = null)
    {
        h.T("<a uk-icon=\"icon: receiver; ratio: 2\" class=\"uk-icon-link");
        if (css != null)
        {
            h.T(' ');
            h.T(css);
        }
        h.T("\" href=\"tel:").T(tel).T("\"></a>");
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


    public static void SetTokenCookies(this WebContext wc, User o, int maxage = 3600 * 12)
    {
        // get root domain name for cookies
        var host = wc.Header("Host");

        string root = null;
        if (host != null)
        {
            var colon = host.LastIndexOf(':');
            var end = colon == -1 ? host.Length : colon;
            var dot = host.LastIndexOf('.', end - 1);

            if (dot != -1)
            {
                dot = host.LastIndexOf('.', dot - 1);
                root = dot == -1 ? null : host[(dot + 1)..end];
            }
        }

        // token cookie
        var token = AuthenticateAttribute.ToToken(o, 0x0fff);
        var tokenStr = WebUtility.BuildSetCookie(nameof(token), token, maxage: maxage, domain: root, httponly: true);

        // cookie for vip, o means none
        var vipStr = WebUtility.BuildSetCookie(nameof(o.vip), TextUtility.ToString(o.vip), domain: root);
        var nameStr = WebUtility.BuildSetCookie(nameof(o.name), (o.name), domain: root);
        var telStr = WebUtility.BuildSetCookie(nameof(o.tel), (o.tel), domain: root);

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
        foreach (var o in terms)
        {
            h.LI_().T(o)._LI();
        }

        h._OL();
        h.FIELD("甲方", a).FIELD("乙方", b);
    }
}