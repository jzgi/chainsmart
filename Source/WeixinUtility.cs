﻿using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Fabric;
using ChainFx.Web;
using static ChainFx.CryptoUtility;
using static ChainFx.Application;
using WebUtility = System.Net.WebUtility;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ChainMart
{
    /// <summary>
    /// A hub of operation that has its own weixin official acount.
    /// </summary>
    public static class WeixinUtility
    {
        static readonly WebClient OpenApi = new WebClient("https://api.weixin.qq.com");

        static readonly WebClient PayApi;

        static readonly WebClient SmsApi = new WebClient("https://sms.tencentcloudapi.com");

        public static readonly string
            url,
            appid,
            appsecret;

        public static readonly string
            mchid,
            noncestr,
            spbillcreateip;

        public static readonly string key;
        public static readonly string watchurl;

        public static readonly string
            smssecretid,
            smssecretkey,
            smssmssdkappid;


        static WeixinUtility()
        {
            var s = Prog;
            s.Get(nameof(url), ref url);
            s.Get(nameof(appid), ref appid);
            s.Get(nameof(appsecret), ref appsecret);
            s.Get(nameof(mchid), ref mchid);
            s.Get(nameof(noncestr), ref noncestr);
            s.Get(nameof(spbillcreateip), ref spbillcreateip);
            s.Get(nameof(key), ref key);
            s.Get(nameof(watchurl), ref watchurl);

            s.Get(nameof(smssecretid), ref smssecretid);
            s.Get(nameof(smssecretkey), ref smssecretkey);
            s.Get(nameof(smssmssdkappid), ref smssmssdkappid);

            try
            {
                // load and init client certificate
                var handler = new WebClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual
                };
                var cert = new X509Certificate2("apiclient_cert.p12", mchid, X509KeyStorageFlags.MachineKeySet);
                handler.ClientCertificates.Add(cert);
                PayApi = new WebClient("https://api.mch.weixin.qq.com", handler);
            }
            catch (Exception e)
            {
                War("failed to load certificate: " + e.Message);
            }
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
            return new User { im = openid, name = nickname };
        }


        static readonly DateTime EPOCH = new DateTime(1970, 1, 1);

        public static long NowMillis => (long)(DateTime.Now - EPOCH).TotalMilliseconds;

        public static IContent BuildPrepayContent(string prepay_id)
        {
            string package = "prepay_id=" + prepay_id;
            string timeStamp = ((int)(DateTime.Now - EPOCH).TotalSeconds).ToString();
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

        public static async Task<string> PostTransferAsync(int id, string openid, string username, decimal cash, string desc)
        {
            var x = new XElem("xml")
            {
                {"amount", ((int) (cash * 100)).ToString()},
                {"check_name", "FORCE_CHECK"},
                {"desc", desc},
                {"mch_appid", appid},
                {"mchid", mchid},
                {"nonce_str", noncestr},
                {"openid", openid},
                {"partner_trade_no", id.ToString()},
                {"re_user_name", username},
                {"spbill_create_ip", spbillcreateip}
            };
            string sign = Sign(x);
            x.Add("sign", sign);

            var (_, xe) = (await PayApi.PostAsync<XElem>("/mmpaymkttransfers/promotion/transfers", x.Dump()));
            string return_code = xe.Child(nameof(return_code));
            if ("SUCCESS" == return_code)
            {
                string result_code = xe.Child(nameof(result_code));
                if ("SUCCESS" != result_code)
                {
                    string err_code_des = xe.Child(nameof(err_code_des));
                    return err_code_des;
                }
            }
            else
            {
                string return_msg = xe.Child(nameof(return_msg));
                return return_msg;
            }

            return null;
        }

        public static async Task<(string, string)> PostUnifiedOrderAsync(string trade_no, decimal amount, string openid, string ip, string notifyurl, string descr)
        {
            var x = new XElem("xml")
            {
                {"appid", appid},
                {"body", descr},
                {"detail", descr},
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

            var (_, xe) = (await PayApi.PostAsync<XElem>("/pay/unifiedorder", x.Dump()));
            string prepay_id = xe.Child(nameof(prepay_id));
            string err_code = null;
            if (prepay_id == null)
            {
                err_code = xe.Child("err_code");
            }

            return (prepay_id, err_code);
        }

        public static bool OnNotified(XElem xe, out string out_trade_no, out decimal total)
        {
            total = 0;
            out_trade_no = null;
            string appid = xe.Child(nameof(appid));
            string mch_id = xe.Child(nameof(mch_id));
            string nonce_str = xe.Child(nameof(nonce_str));

            if (appid != WeixinUtility.appid || mch_id != mchid || nonce_str != noncestr) return false;

            string result_code = xe.Child(nameof(result_code));

            if (result_code != "SUCCESS") return false;

            string sign = xe.Child(nameof(sign));
            xe.Sort();
            if (sign != Sign(xe, "sign")) return false;

            int total_fee = xe.Child(nameof(total_fee)); // in cent
            total = ((decimal)total_fee) / 100;
            out_trade_no = xe.Child(nameof(out_trade_no)); // 商户订单号
            return true;
        }

        public static async Task<decimal> PostOrderQueryAsync(string orderno)
        {
            var x = new XElem("xml")
            {
                {"appid", appid},
                {"mch_id", mchid},
                {"nonce_str", noncestr},
                {"out_trade_no", orderno}
            };
            string sign = Sign(x);
            x.Add("sign", sign);
            var (_, xe) = (await PayApi.PostAsync<XElem>("/pay/orderquery", x.Dump()));
            sign = xe.Child(nameof(sign));
            xe.Sort();
            if (sign != Sign(xe, "sign")) return 0;

            string return_code = xe.Child(nameof(return_code));
            if (return_code != "SUCCESS") return 0;

            decimal cash_fee = xe.Child(nameof(cash_fee));

            return cash_fee;
        }

        public static async Task<string> PostRefundAsync(string orderno, decimal total, decimal refund, string refoundno, string descr = null)
        {
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

            var (_, xe) = (await PayApi.PostAsync<XElem>("/secapi/pay/refund", x.Dump()));
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

        public static async Task<string> PostRefundQueryAsync(long orderid)
        {
            var x = new XElem("xml")
            {
                {"appid", appid},
                {"mch_id", mchid},
                {"nonce_str", noncestr},
                {"out_trade_no", orderid.ToString()}
            };
            string sign = Sign(x);
            x.Add("sign", sign);
            var (_, xe) = (await PayApi.PostAsync<XElem>("/pay/refundquery", x.Dump()));

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
                    bdr.Add((string)mbr);
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
            string signRet = string.Empty;
            using (HMACSHA1 mac = new HMACSHA1(Encoding.UTF8.GetBytes(signKey)))
            {
                byte[] hash = mac.ComputeHash(Encoding.UTF8.GetBytes(secret));
                signRet = Convert.ToBase64String(hash);
            }
            return signRet;
        }
        public static string MakeSignPlainText(SortedDictionary<string, string> requestParams, string requestMethod, string requestHost, string requestPath)
        {
            string retStr = "";
            retStr += requestMethod;
            retStr += requestHost;
            retStr += requestPath;
            retStr += "?";
            string v = "";
            foreach (string key in requestParams.Keys)
            {
                v += string.Format("{0}={1}&", key, requestParams[key]);
            }
            retStr += v.TrimEnd('&');
            return retStr;
        }

        public static long ToTimestamp()
        {
            DateTime startTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime nowTime = DateTime.UtcNow;
            long unixTime = (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;

        }

        public static string GetSmsRequestV1(string[] PhoneNumberSet, string SignName, string TemplateId, string[] TemplateParamSet)
        {

            string SECRET_ID = "AKIDOaWBZhZAUYNwEH3YlhOoxpRmqdAQP7Xi";
            string SECRET_KEY = "vZw3yd18yXpqa4GBqEblfIelbCWZMt86";
            string SmsSdkAppId = "1400730065";
            string endpoint = "sms.tencentcloudapi.com";
            string region = "ap-guangzhou";
            string action = "SendSms";
            string version = "2021-01-11";
            long timestamp = ToTimestamp() / 1000;
            string requestTimestamp = timestamp.ToString();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("Action", action);
            // param.Add("Nonce", "11886");
            param.Add("Nonce", Math.Abs(new Random().Next()).ToString());

            param.Add("Timestamp", requestTimestamp.ToString());
            param.Add("Version", version);

            param.Add("SecretId", SECRET_ID);
            param.Add("Region", region);
            // 实际调用需要更新参数，这里仅作为演示签名验证通过的例子
            param.Add("SmsSdkAppId", SmsSdkAppId);
            param.Add("SignName", SignName);
            param.Add("TemplateId", TemplateId);
            for (var i = 0; i < TemplateParamSet.Length; i++)
            {
                param.Add("TemplateParamSet." + i, TemplateParamSet[i]);
            }
            for (var i = 0; i < PhoneNumberSet.Length; i++)
            {
                param.Add("PhoneNumberSet." + i, PhoneNumberSet[i]);
            }
            // param.Add("SessionContext", "test");
            SortedDictionary<string, string> headers = new SortedDictionary<string, string>(param, StringComparer.Ordinal);
            string sigInParam = MakeSignPlainText(headers, "GET", endpoint, "/");
            string sigOutParam = Sign(SECRET_KEY, sigInParam);
            Console.WriteLine(sigOutParam); // 输出签名

            // 获取 curl 命令串
            param.Add("Signature", sigOutParam);
            StringBuilder urlBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in param)
            {
                urlBuilder.Append($"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}&");
            }
            string query = urlBuilder.ToString().TrimEnd('&');

            string url = "https://" + endpoint + "/?" + query;
            string curlcmd = "curl \"" + url + "\"";
            Console.WriteLine(curlcmd);
            return url;
        }

        public static async Task<string> SendSmsAsync(string[] PhoneNumberSet, string SignName, string TemplateId, string[] TemplateParamSet)
        {
            var url = GetSmsRequestV1(PhoneNumberSet, SignName, TemplateId, TemplateParamSet);
            var (_, jo) = await SmsApi.GetAsync<JObj>(url, null);
            Console.WriteLine(jo);
            return jo.ToString();
        }
    }
}