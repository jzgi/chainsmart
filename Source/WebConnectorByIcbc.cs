using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using WebUtility = System.Net.WebUtility;

namespace ChainSmart;

public class WebConnectorByIcbc : WebConnector
{
    private const string SIGN_TYPE = "sign_type",
        SIGN_TYPE_RSA = "RSA",
        SIGN_TYPE_RSA2 = "RSA2",
        SIGN_TYPE_SM2 = "SM2",
        SIGN_TYPE_CA = "CA",
        SIGN_SHA1RSA_ALGORITHMS = "SHA1WithRSA",
        SIGN_SHA256RSA_ALGORITHMS = "SHA256WithRSA",
        ENCRYPT_TYPE_AES = "AES",
        APP_ID = "app_id",
        FORMAT = "format",
        TIMESTAMP = "timestamp",
        SIGN = "sign",
        APP_AUTH_TOKEN = "app_auth_token",
        CHARSET = "charset",
        NOTIFY_URL = "notify_url",
        RETURN_URL = "return_url",
        ENCRYPT_TYPE = "encrypt_type",
        // -----===-------///
        BIZ_CONTENT_KEY = "biz_content",
        /** 默认时间格式 **/
        DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss",
        /** Date默认时区 **/
        DATE_TIMEZONE = "GMT+8",
        /** UTF-8字符集 **/
        CHARSET_UTF8 = "UTF-8",
        /** GBK字符集 **/
        CHARSET_GBK = "GBK",
        /** JSON 应格式 */
        FORMAT_JSON = "json",
        /** XML 应格式 */
        FORMAT_XML = "xml",
        CA = "ca",
        PASSWORD = "password",
        RESPONSE_BIZ_CONTENT = "response_biz_content",
        /** 消息唯一编号 **/
        MSG_ID = "msg_id",
        /** sdk版本号在header中的key */
        VERSION_HEADER_NAME = "APIGW-VERSION";

    private string appId;
    private string privateKey;
    private string gwPublicKey;

    public WebConnectorByIcbc(string baseUri, string appId, string privateKey, string gwPublicKey,
        WebClientHandler handler = null) : base(baseUri, handler)
    {
        this.appId = appId;
        this.privateKey = privateKey;
        this.gwPublicKey = gwPublicKey;
    }

    /**
     * 退款
     */
    public async Task<string> PostRefundAsync(bool sup, string out_trade_no, decimal total, decimal refund,
        string refoundno, string descr = null)
    {
        return null;
    }

    /**
     * 退款查询
     */
    public async Task<string> PostRefundQueryAsync(bool sup, long orderid)
    {
        return null;
    }

    /**
     * 订单查询 
     */
    public async Task<decimal> PostOrderQueryAsync(bool sup, string orderno)
    {
        // request.setServiceUrl("https://gw.open.icbc.com.cn/api/mybank/pay/qrcode/scanned/paystatus/V2");
        //
        // bizContent.setMerId("150251930149");
        // bizContent.setCustId("");
        // bizContent.setOrderId("");
        // bizContent.setOutTradeNo("C201812180000****");
        string msgId = "12042699****";
        const string path = "https://gw.open.icbc.com.cn/api/mybank/pay/qrcode/scanned/paystatus/V2";
        //mer_id out_trade_no mer_prtcl_no order_amt notify_url icbc_appid saledepname
        var biz = new JObj
        {
            { "mer_id", "150251930149" },
            { "out_trade_no", "C201812180000****" },
            { "custId", "" },
            { "orderId", "" },
        };
        var bizStr = biz.ToString();
        Dictionary<String, String> param = prepareParams(path, msgId, bizStr);

        Dictionary<String, String> urlQueryParams = buildUrlQueryParams(param);

        String url = BuildGetUrl(path, urlQueryParams, CHARSET_UTF8);
        FormBuilder builder = new FormBuilder(true, 4096);
        builder.Put(BIZ_CONTENT_KEY, bizStr);
        var (_, jo) = await PostAsync<JObj>(url, builder);
        var v = jo.ToString();
        return 0;
    }

    /**
     * 公众号聚合支付
     */
    public async Task<(string, string)> PostUnifiedOrderAsync(bool sup, string trade_no, decimal amount,
        string openid, string ip, string notifyurl, string descr)
    {
        /**
         * 1.sign
         * 2.url
         * 3.form data
         */
        string msgId = "12042699****";
        const string path = "https://gw.open.icbc.com.cn/api/cardbusiness/aggregatepay/b2c/online/consumepurchase/V2";
        //mer_id out_trade_no mer_prtcl_no order_amt notify_url icbc_appid saledepname
        var biz = new JObj
        {
            { "mer_id", "150251930149" },
            { "out_trade_no", "02000015087" },
            { "mer_prtcl_no", "150251930149" },
            { "order_amt", "100" },
            { "notify_url", "http://www.zhnt-x.com" },
            { "icbc_appid", appId },
            { "saledepname", "中惠农通" },
        };
        var bizStr = biz.ToString();
        Dictionary<String, String> param = prepareParams(path, msgId, bizStr);

        Dictionary<String, String> urlQueryParams = buildUrlQueryParams(param);

        String url = BuildGetUrl(path, urlQueryParams, CHARSET_UTF8);
        FormBuilder builder = new FormBuilder(true, 4096);
        builder.Put(BIZ_CONTENT_KEY, bizStr);
        var (_, jo) = await PostAsync<JObj>(path, builder);
        return (null, null);
    }

    private Dictionary<string, string> buildUrlQueryParams(Dictionary<string, string> param)
    {
        Dictionary<String, String> urlQueryParams = new Dictionary<String, String>();
        string[] apiParamNames =
        {
            SIGN, APP_ID, SIGN_TYPE, CHARSET, FORMAT,
            ENCRYPT_TYPE, TIMESTAMP, MSG_ID
        };
        foreach (var s in param)
        {
            String key = s.Key;
            String value = s.Value;
            if (apiParamNames.Contains(key))
            {
                urlQueryParams.Add(key, value);
            }
        }

        return urlQueryParams;
    }


    protected string buildOrderedSignStr(String path, Dictionary<String, String> param)
    {
        var sorted = new SortedDictionary<string, string>(param, StringComparer.Ordinal);
        StringBuilder sb = new StringBuilder(path);
        sb.Append("?");
        int num = 0;
        foreach (var (k, v) in sorted)
        {
            if (num > 0)
            {
                sb.Append('&');
            }

            sb.Append(k).Append('=').Append(v);
            num++;
        }

        return sb.ToString();
    }

    public static String BuildGetUrl(String strUrl, Dictionary<String, String> param, String charset)
    {
        if (param == null)
        {
            return strUrl;
        }

        StringBuilder sb = new StringBuilder(strUrl);
        try
        {
            if (new Uri(strUrl).Query == null || new Uri(strUrl).Query.Equals(""))
            {
                if (!strUrl.EndsWith("?"))
                {
                    sb.Append('?');
                }
            }
        }
        catch (UriFormatException e)
        {
            throw new Exception("url exception. url: " + strUrl, e);
        }

        Dictionary<String, String> tmp = new Dictionary<string, string>();
        tmp = param;
        Boolean hasParam = false;

        Boolean shouldAddAnd = strUrl.Contains("?") ? !strUrl.EndsWith("&") : false;

        foreach (var s in tmp)
        {
            String name = s.Key;
            String value = s.Value;
            // 忽略参数名或参数值为空的参数

            if (value == null || name == null || value.Equals(""))
            {
                continue;
            }
            else
            {
                if (hasParam)
                {
                    sb.Append("&");
                }
                else
                {
                    if (shouldAddAnd)
                    {
                        sb.Append("&");
                    }

                    hasParam = true;
                }

                sb.Append(name).Append("=").Append(WebUtility.UrlEncode(value));
            }
        }

        return sb.ToString();
    }

    protected string Sign(string message, string privateKey)
    {
        _ = privateKey ?? throw new ArgumentException($"{nameof(privateKey)} 不能为 null！");

        byte[] keyData = Convert.FromBase64String(privateKey);

        using (var rsa = RSA.Create())
        {
            rsa.ImportPkcs8PrivateKey(keyData, out _);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            return Convert.ToBase64String(rsa.SignData(data, 0, data.Length, HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1));
        }
    }

    protected Dictionary<String, String> prepareParams(String path, String msgId, String bizContentStr = "", String encryptType = null,
        String encryptKey = null)
    {
        Dictionary<String, String> param = new Dictionary<String, String>();

        param.Add(APP_ID, appId);
        param.Add(SIGN_TYPE, SIGN_TYPE_RSA2);
        param.Add(CHARSET, CHARSET_UTF8);
        param.Add(FORMAT, FORMAT_JSON);
        param.Add(CA, "");
        param.Add(APP_AUTH_TOKEN, "");
        param.Add(MSG_ID, msgId);
        param.Add(TIMESTAMP, DateTime.Now.ToString(DATE_TIME_FORMAT));

        if (bizContentStr != null && encryptType != null && encryptKey != null)
        {
            param.Add(ENCRYPT_TYPE, encryptType);
            param.Add(BIZ_CONTENT_KEY,
                EncryptContent(bizContentStr, encryptType, encryptKey, CHARSET_UTF8));
        }
        else
        {
            param.Add("biz_content", bizContentStr);
        }

        String strToSign = buildOrderedSignStr(path, param);
        String signedStr = Sign(strToSign, privateKey);
        if (signedStr.Length < 3)
            throw new Exception("sign Exception!");
        param.Add(SIGN, signedStr);
        return param;
    }

    public static String DecryptContent(String content, String encryptType, String encryptKey, String charset)
    {
        if (encryptType.Equals(ENCRYPT_TYPE_AES))
            return AesDecrypt(content, encryptKey);
        else
            throw new Exception("当前不支持该算法类型：encrypeType=" + encryptType);
    }

    //AES内容加密
    public static String EncryptContent(String content, String encryptType, String encryptKey, String charset)
    {
        if (encryptType.Equals(ENCRYPT_TYPE_AES))
            return AesEncrypt(content, encryptKey);
        else
            throw new Exception("当前不支持该算法类型：encrypeType=" + encryptType);
    }

    /// <summary>
    ///AES加密（加密步骤）
    ///1，加密字符串得到2进制数组；
    ///2，将2禁止数组转为16进制；
    ///3，进行base64编码
    /// </summary>
    /// <param name="toEncrypt">要加密的字符串</param>
    /// <param name="key">密钥</param>
    public static String AesEncrypt(String toEncrypt, String key)
    {
        Byte[] _Key = Convert.FromBase64String(key);
        Byte[] _Source = Encoding.UTF8.GetBytes(toEncrypt);
        Aes aes = Aes.Create("AES");
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = _Key;
        aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        ICryptoTransform cTransform = aes.CreateEncryptor();
        Byte[] cryptData = cTransform.TransformFinalBlock(_Source, 0, _Source.Length);
        String CryptString = Convert.ToBase64String(cryptData);
        return CryptString;
    }

    /// <summary>
    /// AES解密（解密步骤）
    /// 1，将BASE64字符串转为16进制数组
    /// 2，将16进制数组转为字符串
    /// 3，将字符串转为2进制数据
    /// 4，用AES解密数据
    /// </summary>
    /// <param name="encryptedSource">已加密的内容</param>
    /// <param name="key">密钥</param>
    public static String AesDecrypt(string encryptedSource, string key)
    {
        Byte[] _Key = Convert.FromBase64String(key);
        Aes aes = Aes.Create("AES");
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = _Key;
        aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        ICryptoTransform cTransform = aes.CreateDecryptor();
        Byte[] encryptedData = Convert.FromBase64String(encryptedSource);
        Byte[] originalSrouceData = cTransform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        String originalString = Encoding.UTF8.GetString(originalSrouceData);
        return originalString;
    }
}