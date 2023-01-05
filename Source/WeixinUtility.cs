using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.CryptoUtility;
using static ChainFx.Application;
using WebUtility = System.Net.WebUtility;

namespace ChainMart
{
    /// <summary>
    /// A hub of operation that has its own weixin official acount.
    /// </summary>
    public static class WeixinUtility
    {
        static readonly WebClient OpenApi = new WebClient("https://api.weixin.qq.com");

        static readonly WebClient ScPayApi, RtlPayApi;

        static readonly WebClient SmsApi = new WebClient("https://sms.tencentcloudapi.com");

        public static readonly string
            appid,
            appsecret;

        public static readonly string
            scmchid,
            rtlmchid,
            noncestr;
        // spbillcreateip;

        public static readonly string key;

        public static readonly string
            smssecretid,
            smssecretkey,
            smssdkappid,
            smstemplateid;


        static WeixinUtility()
        {
            var s = Prog;
            s.Get(nameof(appid), ref appid);
            s.Get(nameof(appsecret), ref appsecret);
            s.Get(nameof(scmchid), ref scmchid);
            s.Get(nameof(rtlmchid), ref rtlmchid);
            s.Get(nameof(noncestr), ref noncestr);
            // s.Get(nameof(spbillcreateip), ref spbillcreateip);
            s.Get(nameof(key), ref key);

            s.Get(nameof(smssecretid), ref smssecretid);
            s.Get(nameof(smssecretkey), ref smssecretkey);
            s.Get(nameof(smssdkappid), ref smssdkappid);
            s.Get(nameof(smstemplateid), ref smstemplateid);

            try
            {
                ScPayApi = Set("sc_apiclient_cert.p12", scmchid);

                RtlPayApi = Set("rtl_apiclient_cert.p12", rtlmchid);
            }
            catch (Exception e)
            {
                War("Failed to load Weixin Pay certificate: " + e.Message);
            }
        }

        static WebClient Set(string fileName, string password)
        {
            var handler = new WebClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual
            };
            var cert = new X509Certificate2(fileName, password, X509KeyStorageFlags.MachineKeySet);
            handler.ClientCertificates.Add(cert);
            return new WebClient("https://api.mch.weixin.qq.com", handler);
        }

        static string accessToken;

        static int tick;

        public static string GetAccessToken()
        {
            int now = Environment.TickCount;
            if (accessToken == null || now < tick || now - tick > 3600000)
            {
                var (_, jo) = OpenApi.GetAsync<JObj>("/cgi-bin/token?grant_type=client_credential&appid=" + appid + "&secret=" + appsecret, null).Result;
                string access_token = jo?[nameof(access_token)];
                accessToken = access_token;
                tick = now;
            }

            return accessToken;
        }

        public static void GiveRedirectWeiXinAuthorize(WebContext wc, string listenAddr, bool userinfo = false)
        {
            string redirect_url = WebUtility.UrlEncode(listenAddr + wc.Uri);
            wc.SetHeader(
                "Location",
                "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appid + "&redirect_uri=" + redirect_url + "&response_type=code&scope=" + (userinfo ? "snsapi_userinfo" : "snsapi_base") + "&state=wxauth#wechat_redirect"
            );
            wc.Give(303);
        }

        public static async Task<(string access_token, string openid)> GetAccessorAsync(string code)
        {
            string url = "/sns/oauth2/access_token?appid=" + appid + "&secret=" + appsecret + "&code=" + code + "&grant_type=authorization_code";
            var (_, jo) = await OpenApi.GetAsync<JObj>(url, null);
            if (jo == null)
            {
                return default((string, string));
            }

            string access_token = jo[nameof(access_token)];
            if (access_token == null)
            {
                return default((string, string));
            }

            string openid = jo[nameof(openid)];
            return (access_token, openid);
        }

        public static async Task<User> GetUserInfoAsync(string access_token, string openid)
        {
            var (_, jo) = await OpenApi.GetAsync<JObj>("/sns/userinfo?access_token=" + access_token + "&openid=" + openid + "&lang=zh_CN", null);
            string nickname = jo[nameof(nickname)];
            return new User {im = openid, name = nickname};
        }


        static readonly DateTime EPOCH = new DateTime(1970, 1, 1);

        public static long NowMillis => (long) (DateTime.Now - EPOCH).TotalMilliseconds;

        public static IContent BuildPrepayContent(string prepay_id)
        {
            string package = "prepay_id=" + prepay_id;
            string timeStamp = ((int) (DateTime.Now - EPOCH).TotalSeconds).ToString();
            var jo = new JObj
            {
                {"appId", appid},
                {"nonceStr", noncestr},
                {"package", package},
                {"signType", "MD5"},
                {"timeStamp", timeStamp}
            };
            jo.Add("paySign", Sign(jo, "paySign"));
            return jo.Dump();
        }

        public static async Task<(string ticket, string url)> PostQrSceneAsync(int uid)
        {
            var j = new JsonBuilder(true, 1024);
            try
            {
                j.OBJ_();
                j.Put("expire_seconds", 604800);
                j.Put("action_name", "QR_SCENE");
                j.OBJ_("action_info");
                j.OBJ_("scene");
                j.Put("scene_id", uid);
                j._OBJ();
                j._OBJ();
                j._OBJ();
                var (_, jo) =
                    await OpenApi.PostAsync<JObj>("/cgi-bin/qrcode/create?access_token=" + GetAccessToken(), j);
                return (jo["ticket"], jo["url"]);
            }
            finally
            {
                j.Clear();
            }
        }

        public static async Task PostSendAsync(string openid, string text)
        {
            var bdr = new JsonBuilder(true, 1024);
            try
            {
                bdr.OBJ_();
                bdr.Put("touser", openid);
                bdr.Put("msgtype", "text");
                bdr.OBJ_("text");
                bdr.Put("content", text);
                bdr._OBJ();
                bdr._OBJ();
                await OpenApi.PostAsync<JObj>("/cgi-bin/message/custom/send?access_token=" + GetAccessToken(), bdr);
            }
            finally
            {
                bdr.Clear();
            }
        }

        public static async Task PostSendAsync(string openid, string title, string descr, string url, string picurl = null)
        {
            var bdr = new JsonBuilder(true, 1024);
            try
            {
                bdr.OBJ_();
                bdr.Put("touser", openid);
                bdr.Put("msgtype", "news");
                bdr.OBJ_("news").ARR_("articles").OBJ_();
                bdr.Put("title", title);
                bdr.Put("description", descr);
                bdr.Put("url", url);
                bdr.Put("picurl", picurl);
                bdr._OBJ()._ARR()._OBJ();
                bdr._OBJ();
                await OpenApi.PostAsync<JObj>("/cgi-bin/message/custom/send?access_token=" + GetAccessToken(), bdr);
            }
            finally
            {
                bdr.Clear();
            }
        }

        public static async Task<(string, string)> PostUnifiedOrderAsync(bool sc, string trade_no, decimal amount, string openid, string ip, string notifyurl, string descr)
        {
            var mchid = sc ? scmchid : rtlmchid;
            var api = sc ? ScPayApi : RtlPayApi;

            var x = new XElem("xml")
            {
                {"appid", appid},
                {"body", descr},
                {"mch_id", mchid},
                {"nonce_str", noncestr},
                {"notify_url", notifyurl},
                {"openid", openid},
                {"out_trade_no", trade_no},
                {"spbill_create_ip", ip},
                {"total_fee", ((int) (amount * 100)).ToString()},
                {"trade_type", "JSAPI"}
            };
            string sign = Sign(x);
            x.Add("sign", sign);

            var (_, xe) = (await api.PostAsync<XElem>("/pay/unifiedorder", x.Dump()));
            string prepay_id = xe.Child(nameof(prepay_id));
            string err_code = null;
            if (prepay_id == null)
            {
                err_code = xe.Child("err_code");
            }

            return (prepay_id, err_code);
        }

        public static bool OnNotified(bool sc, XElem xe, out string out_trade_no, out decimal total)
        {
            var mchid = sc ? scmchid : rtlmchid;

            total = 0;
            out_trade_no = null;

            string appid = xe.Child(nameof(appid));
            string mch_id = xe.Child(nameof(mch_id));
            string nonce_str = xe.Child(nameof(nonce_str));

            if (appid != WeixinUtility.appid || mch_id != mchid || nonce_str != noncestr) return false;

            string result_code = xe.Child(nameof(result_code));

            if (result_code != "SUCCESS")
            {
                return false;
            }

            string sign = xe.Child(nameof(sign));
            xe.Sort();
            if (sign != Sign(xe, "sign"))
            {
                return false;
            }

            int total_fee = xe.Child(nameof(total_fee)); // in cent
            total = ((decimal) total_fee) / 100;
            out_trade_no = xe.Child(nameof(out_trade_no)); // order no

            return true;
        }

        public static async Task<decimal> PostOrderQueryAsync(bool sc, string orderno)
        {
            var mchid = sc ? scmchid : rtlmchid;
            var api = sc ? ScPayApi : RtlPayApi;

            var x = new XElem("xml")
            {
                {"appid", appid},
                {"mch_id", mchid},
                {"nonce_str", noncestr},
                {"out_trade_no", orderno}
            };
            string sign = Sign(x);
            x.Add("sign", sign);

            var (_, xe) = (await api.PostAsync<XElem>("/pay/orderquery", x.Dump()));
            sign = xe.Child(nameof(sign));
            xe.Sort();
            if (sign != Sign(xe, "sign"))
            {
                return 0;
            }

            string return_code = xe.Child(nameof(return_code));
            if (return_code != "SUCCESS")
            {
                return 0;
            }

            decimal cash_fee = xe.Child(nameof(cash_fee));

            return cash_fee;
        }

        public static async Task<string> PostRefundAsync(bool SC, string orderno, decimal total, decimal refund, string refoundno, string descr = null)
        {
            var mchid = SC ? scmchid : rtlmchid;
            var api = SC ? ScPayApi : RtlPayApi;


            var x = new XElem("xml")
            {
                {"appid", appid},
                {"mch_id", mchid},
                {"nonce_str", noncestr},
                {"op_user_id", mchid},
                {"out_refund_no", refoundno},
                {"out_trade_no", orderno},
                {"refund_desc", descr},
                {"refund_fee", ((int) (refund * 100)).ToString()},
                {"total_fee", ((int) (total * 100)).ToString()}
            };
            string sign = Sign(x);
            x.Add("sign", sign);

            var (_, xe) = (await api.PostAsync<XElem>("/secapi/pay/refund", x.Dump()));
            if (xe == null)
            {
                return "TIMEOUT";
            }
            string return_code = xe.Child(nameof(return_code));
            if (return_code != "SUCCESS")
            {
                string return_msg = xe.Child(nameof(return_msg));
                return return_msg;
            }

            string result_code = xe.Child(nameof(result_code));
            if (result_code != "SUCCESS")
            {
                string err_code_des = xe.Child(nameof(err_code_des));
                return err_code_des;
            }

            return null;
        }


        public static async Task<string> PostRefundQueryAsync(bool SC, long orderid)
        {
            var mchid = SC ? scmchid : rtlmchid;
            var api = SC ? ScPayApi : RtlPayApi;

            var x = new XElem("xml")
            {
                {"appid", appid},
                {"mch_id", mchid},
                {"nonce_str", noncestr},
                {"out_trade_no", orderid.ToString()}
            };
            string sign = Sign(x);
            x.Add("sign", sign);
            var (_, xe) = (await api.PostAsync<XElem>("/pay/refundquery", x.Dump()));

            sign = xe.Child(nameof(sign));
            xe.Sort();
            if (sign != Sign(xe, "sign")) return "返回结果签名错误";

            string return_code = xe.Child(nameof(return_code));
            if (return_code != "SUCCESS")
            {
                string return_msg = xe.Child(nameof(return_msg));
                return return_msg;
            }

            string result_code = xe.Child(nameof(result_code));
            if (result_code != "SUCCESS")
            {
                return "退款订单查询失败";
            }

            string refund_status_0 = xe.Child(nameof(refund_status_0));
            if (refund_status_0 != "SUCCESS")
            {
                return refund_status_0 == "PROCESSING" ? "退款处理中" :
                    refund_status_0 == "REFUNDCLOSE" ? "退款关闭" : "退款异常";
            }

            return null;
        }

        static string Sign(XElem xe, string exclude = null)
        {
            var bdr = new TextBuilder(false, 1024);
            try
            {
                for (int i = 0; i < xe.Count; i++)
                {
                    var child = xe[i];
                    // not include the sign field
                    if (exclude != null && child.Tag == exclude) continue;
                    if (bdr.Count > 0)
                    {
                        bdr.Add('&');
                    }

                    bdr.Add(child.Tag);
                    bdr.Add('=');
                    bdr.Add(child.Text);
                }

                bdr.Add("&key=");
                bdr.Add(key);

                return MD5(bdr.ToString(), true);
            }
            finally
            {
                bdr.Clear();
            }
        }

        static string Sign(JObj jo, string exclude = null)
        {
            var bdr = new TextBuilder(false, 1024);
            try
            {
                for (int i = 0; i < jo.Count; i++)
                {
                    var mbr = jo.ValueAt(i);

                    // not include the sign field
                    if (exclude != null && mbr.Key == exclude) continue;

                    if (bdr.Count > 0)
                    {
                        bdr.Add('&');
                    }

                    bdr.Add(mbr.Key);
                    bdr.Add('=');
                    bdr.Add((string) mbr);
                }

                bdr.Add("&key=");
                bdr.Add(key);
                return MD5(bdr.ToString(), true);
            }
            finally
            {
                bdr.Clear();
            }
        }

        public static string Sign(string signKey, string secret)
        {
            using var mac = new HMACSHA1(Encoding.UTF8.GetBytes(signKey));

            byte[] hash = mac.ComputeHash(Encoding.UTF8.GetBytes(secret));

            return Convert.ToBase64String(hash);
        }

        public static string MakeSignPlainText(SortedDictionary<string, string> dict, string host)
        {
            var url = new StringBuilder("GET").Append(host).Append("/?");
            int num = 0;
            foreach (var (k, v) in dict)
            {
                if (num > 0)
                {
                    url.Append('&');
                }
                url.Append(k).Append('=').Append(v);
                num++;
            }
            return url.ToString();
        }

        public static long ToTimestamp()
        {
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime nowTime = DateTime.UtcNow;
            long unixTime = (long) Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }

        public static async Task<string> SendSmsAsync(string[] phoneNumberSet, string signName, params string[] templateParamSet)
        {
            const string endpoint = "sms.tencentcloudapi.com";

            long timestamp = ToTimestamp() / 1000;
            var dict = new Dictionary<string, string>
            {
                {"Action", "SendSms"},
                {"Nonce", "11886"},
                {"Timestamp", timestamp.ToString()},
                {"Version", "2021-01-11"},
                {"SecretId", smssecretid},
                {"Region", "ap-guangzhou"},
                {"SmsSdkAppId", smssdkappid},
                {"SignName", signName},
                {"TemplateId", smstemplateid}
            };

            for (var i = 0; i < templateParamSet.Length; i++)
            {
                dict.Add("TemplateParamSet." + i, templateParamSet[i]);
            }
            for (var i = 0; i < phoneNumberSet.Length; i++)
            {
                dict.Add("PhoneNumberSet." + i, phoneNumberSet[i]);
            }

            var sorted = new SortedDictionary<string, string>(dict, StringComparer.Ordinal);
            string sigInParam = MakeSignPlainText(sorted, endpoint);
            string sigOutParam = Sign(smssecretkey, sigInParam);

            // 获取 curl 命令串
            dict.Add("Signature", sigOutParam);

            var url = new StringBuilder("/?");
            int num = 0;
            foreach (var (k, v) in dict)
            {
                if (num > 0)
                {
                    url.Append('&');
                }
                url.Append(k).Append('=').Append(WebUtility.UrlEncode(v));
                num++;
            }

            //
            // send request
            var (_, jo) = await SmsApi.GetAsync<JObj>(url.ToString(), null);
            return jo.ToString();
        }
    }
}