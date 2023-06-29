using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using WebUtility = System.Net.WebUtility;

namespace ChainSmart;

public class WebConnectorByIcbc : WebConnector
{
    private const string
        APP_ID = "app_id",
        MER_ID = "mer_id",
        MSG_ID = "msg_id",
        OUT_TRADE_NO = "out_trade_no",
        MER_PRTCL_NO = "mer_prtcl_no",
        ORDER_AMT = "order_amt",
        ICBC_APPID = "icbc_appid",
        SHOP_APPID = "shop_appid",
        OPENID = "openId",
        SALEDEPNAME = "saledepname",
        BODY = "body",
        ATTACH = "attach",
        RET_TOTAL_AMT = "ret_total_amt",
        TRNSC_CCY = "trnsc_ccy",
        SIGN_TYPE = "sign_type",
        SIGN_TYPE_RSA2 = "RSA2",
        ENCRYPT_TYPE_AES = "AES",
        FORMAT = "format",
        TIMESTAMP = "timestamp",
        SIGN = "sign",
        APP_AUTH_TOKEN = "app_auth_token",
        CHARSET = "charset",
        NOTIFY_URL = "notify_url",
        RETURN_URL = "return_url",
        ENCRYPT_TYPE = "encrypt_type",
        BIZ_CONTENT_KEY = "biz_content",
        DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss",
        CHARSET_UTF8 = "UTF-8",
        FORMAT_JSON = "json",
        CA = "ca",
        RESPONSE_BIZ_CONTENT = "response_biz_content",
        RETURN_CODE = "return_code",
        TOTAL_AMT = "total_amt",
        OUTTRX_SERIAL_NO = "outtrx_serial_no",
        DEAL_FLAG = "deal_flag";

    private string appId;
    private string privateKey;
    private string gwPublicKey;
    private string merId;
    private string shopAppid;

    public WebConnectorByIcbc(string baseUri, string appId, string merId, string privateKey, string gwPublicKey,
        string shopAppid,
        WebClientHandler handler = null) : base(baseUri, handler)
    {
        this.appId = appId;
        this.merId = merId;
        this.privateKey = privateKey;
        this.gwPublicKey = gwPublicKey;
        this.shopAppid = shopAppid;
    }

    /// <summary>
    /// 退款
    /// </summary>
    /// <param name="sup"></param>
    /// <param name="out_trade_no"></param>
    /// <param name="total"></param>
    /// <param name="refund"></param>
    /// <param name="refoundno"></param>
    /// <param name="descr"></param>
    /// <returns></returns>
    public async Task<string> PostRefundAsync(string out_trade_no, decimal refund,
        string refoundno)
    {
        string msgId = DateTime.Now.ToString("yyyyMMddHHmmssms");
        const string path = "/api/cardbusiness/aggregatepay/b2c/online/merrefund/V1";
        var biz = new JObj
        {
            { MER_ID, merId }, //商户编号
            { OUTTRX_SERIAL_NO, refoundno }, //商户系统退货时生成的退款编号
            { RET_TOTAL_AMT, refund.ToString() }, //商户系统退货时生成的退款编号
            { TRNSC_CCY, "001" }, //商户系统订单号
            { OUT_TRADE_NO, out_trade_no }, //商户系统订单号
            { DEAL_FLAG, "0" }, //商户系统订单号
            { ICBC_APPID, appId }, //商户系统订单号
            { MER_PRTCL_NO, merId }, //商户系统订单号
        };
        var bizStr = biz.ToString();
        Dictionary<String, String> param = prepareParams(path, msgId, bizStr);

        Dictionary<String, String> urlQueryParams = buildUrlQueryParams(param);

        String url = BuildGetUrl(path, urlQueryParams, CHARSET_UTF8);
        FormBuilder builder = new FormBuilder(true, 4096);
        builder.Put(BIZ_CONTENT_KEY, bizStr);
        var (_, jo) = await PostAsync<JObj>(url, builder);
        JObj responseBiz = jo[RESPONSE_BIZ_CONTENT];
        if (responseBiz != null)
        {
            //verif sign
            string sign = jo[SIGN];
            var result = responseBiz.ToString();
            if (VerifySign(result, gwPublicKey, sign))
            {
                if (responseBiz[RETURN_CODE] == "0")
                {
                    string serial_no = responseBiz["outtrx_serial_no"];
                    return serial_no;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 退款查詢
    /// </summary>
    /// <param name="sup">供应商</param>
    /// <param name="trade_no">商户订单编号</param>
    /// <param name="outtrx_serial_no">商户退款编号</param>
    /// <returns></returns>
    public async Task<decimal> PostRefundQueryAsync(string trade_no, string outtrx_serial_no)
    {
        string msgId = DateTime.Now.ToString("yyyyMMddHHmmssms");
        const string path = "/api/cardbusiness/aggregatepay/b2c/online/refundqry/V1";
        var biz = new JObj
        {
            { MER_ID, merId }, //商户编号
            { OUT_TRADE_NO, trade_no }, //商户系统订单号
            { OUTTRX_SERIAL_NO, outtrx_serial_no }, //商户系统退货时生成的退款编号
        };
        var bizStr = biz.ToString();
        Dictionary<String, String> param = prepareParams(path, msgId, bizStr);

        Dictionary<String, String> urlQueryParams = buildUrlQueryParams(param);

        String url = BuildGetUrl(path, urlQueryParams, CHARSET_UTF8);
        FormBuilder builder = new FormBuilder(true, 4096);
        builder.Put(BIZ_CONTENT_KEY, bizStr);
        var (_, jo) = await PostAsync<JObj>(url, builder);
        JObj responseBiz = jo[RESPONSE_BIZ_CONTENT];
        if (responseBiz != null)
        {
            //verif sign
            string sign = jo[SIGN];
            var result = responseBiz.ToString();
            if (VerifySign(result, gwPublicKey, sign))
            {
                if (responseBiz[RETURN_CODE] == "0")
                {
                    string amt = responseBiz["real_reject_amt"];
                    return Convert.ToDecimal(amt);
                }
            }
        }

        return -1;
    }

    /**
     * 订单查询 
     */
    public async Task<decimal> PostOrderQueryAsync(string trade_no)
    {
        string msgId = DateTime.Now.ToString("yyyyMMddHHmmssms");
        const string path = "/api/cardbusiness/aggregatepay/b2c/online/orderqry/V1";
        var biz = new JObj
        {
            { MER_ID, merId }, //商户编号
            { OUT_TRADE_NO, trade_no }, //商户系统订单号
            { DEAL_FLAG, "0" }, //0-查询；1-关单 2-关单（不支持二次支付）
        };
        var bizStr = biz.ToString();
        Dictionary<String, String> param = prepareParams(path, msgId, bizStr);

        Dictionary<String, String> urlQueryParams = buildUrlQueryParams(param);

        String url = BuildGetUrl(path, urlQueryParams, CHARSET_UTF8);
        FormBuilder builder = new FormBuilder(true, 4096);
        builder.Put(BIZ_CONTENT_KEY, bizStr);
        var (_, jo) = await PostAsync<JObj>(url, builder);
        JObj responseBiz = jo[RESPONSE_BIZ_CONTENT];
        if (responseBiz != null)
        {
            //verif sign
            string sign = jo[SIGN];
            var result = responseBiz.ToString();
            if (VerifySign(result, gwPublicKey, sign))
            {
                if (responseBiz[RETURN_CODE] == "0")
                {
                    string totalAmount = responseBiz[TOTAL_AMT];
                    return Convert.ToDecimal(totalAmount);
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// 公众号聚合支付
    /// </summary>
    /// <param name="sup"></param>
    /// <param name="trade_no">商户订单编号</param>
    /// <param name="amount">支付金额</param>
    /// <param name="openid">用户openid</param>
    /// <param name="notifyurl">异步通知url</param>
    /// <param name="descr">订单描述</param>
    /// <returns></returns>
    public async Task<(string, string)> PostUnifiedOrderAsync(string trade_no, decimal amount,
        string openid, string notifyurl, string descr)
    {
        string msgId = DateTime.Now.ToString("yyyyMMddHHmmssms");
        const string path = "/ui/cardbusiness/aggregatepay/b2c/online/ui/consumepurchaseshowpay/V1";
        var biz = new JObj
        {
            { MER_ID, merId },
            { OUT_TRADE_NO, trade_no },
            { MER_PRTCL_NO, merId },
            { ORDER_AMT, amount.ToString() },
            { NOTIFY_URL, notifyurl },
            { ICBC_APPID, appId },
            { SHOP_APPID, shopAppid },
            { OPENID, openid },
            { SALEDEPNAME, "中惠农通" },
            { BODY, descr },
            { ATTACH, descr },
        };
        var bizStr = biz.ToString();
        Dictionary<String, String> param = prepareParams(path, msgId, bizStr);

        Dictionary<String, String> urlQueryParams = buildUrlQueryParams(param);

        String url =
            BuildGetUrl(
                "https://gw.open.icbc.com.cn/ui/cardbusiness/aggregatepay/b2c/online/ui/consumepurchaseshowpay/V1",
                urlQueryParams, CHARSET_UTF8);
        return ("_", buildForm(url, new Dictionary<string, string>() { { "biz_content", bizStr } }));
    }

    private static String buildForm(String baseUrl, Dictionary<String, String> parameters)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<form name=\"auto_submit_form\" method=\"post\" action=\"");
        sb.Append(baseUrl);
        sb.Append("\">\n");
        sb.Append(buildHiddenFields(parameters));

        sb.Append("<input type=\"submit\" value=\"立刻提交\" style=\"display:none\" >\n");
        sb.Append("</form>\n");
        sb.Append("<script>document.forms[0].submit();</script>");
        String form = sb.ToString();
        return form;
    }

    private static String buildHiddenFields(Dictionary<String, String> parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return "";
        }

        StringBuilder sb = new StringBuilder();

        foreach (var s in parameters)
        {
            String key = s.Key;
            String value = s.Value;
            // 除去参数中的空值
            if (key == null || value == null)
            {
                continue;
            }

            sb.Append(buildHiddenField(key, value));
        }

        String result = sb.ToString();
        return result;
    }

    private static String buildHiddenField(String key, String value)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<input type=\"hidden\" name=\"");
        sb.Append(key);

        sb.Append("\" value=\"");
        // 转义双引号
        String a = value.Replace("\"", "&quot;");
        sb.Append(a).Append("\">\n");
        return sb.ToString();
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
            if (v == null || k == null || v.Equals(""))
            {
                continue;
            }

            if (num > 0)
            {
                sb.Append('&');
            }

            sb.Append(k).Append('=').Append(v);
            num++;
        }

        return sb.ToString();
    }

    public static String BuildGetUrl(String path, Dictionary<String, String> param, String charset)
    {
        StringBuilder sb = new StringBuilder(path);
        if (!path.EndsWith("?"))
        {
            sb.Append('?');
        }

        int num = 0;
        foreach (var (k, v) in param)
        {
            if (v == null || k == null || v.Equals(""))
            {
                continue;
            }

            if (num > 0)
            {
                sb.Append('&');
            }

            sb.Append(k).Append("=").Append(WebUtility.UrlEncode(v));
            num++;
        }

        return sb.ToString();
    }

    public string Sign(string content, string privateKey)
    {
        _ = privateKey ?? throw new ArgumentException($"{nameof(privateKey)} 不能为 null！");

        byte[] keyData = Convert.FromBase64String(privateKey);

        using (var rsa = RSA.Create())
        {
            rsa.ImportPkcs8PrivateKey(keyData, out _);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(content);
            return Convert.ToBase64String(rsa.SignData(data, 0, data.Length, HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1));
        }
    }

    public static bool VerifySign(String contentForSign, String publicKey, String signedString)
    {
        bool result = false;
        byte[] Data = Encoding.GetEncoding(CHARSET_UTF8).GetBytes(contentForSign);
        byte[] data = Convert.FromBase64String(signedString);
        RSAParameters paraPub = ConvertFromPublicKey(publicKey);
        RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
        rsaPub.ImportParameters(paraPub);
        SHA1 sh = new SHA1CryptoServiceProvider();
        result = rsaPub.VerifyData(Data, sh, data);
        return result;
    }

    private static RSAParameters ConvertFromPublicKey(string pemFileConent)
    {
        byte[] keyData = Convert.FromBase64String(pemFileConent);
        if (keyData.Length < 162)
        {
            throw new ArgumentException("pem file content is incorrect.");
        }

        byte[] pemModulus = new byte[128];
        byte[] pemPublicExponent = new byte[3];
        Array.Copy(keyData, 29, pemModulus, 0, 128);
        Array.Copy(keyData, 159, pemPublicExponent, 0, 3);
        RSAParameters para = new RSAParameters();
        para.Modulus = pemModulus;
        para.Exponent = pemPublicExponent;
        return para;
    }

    protected Dictionary<String, String> prepareParams(String path, String msgId,
        String bizContentStr = "",
        String encryptType = null,
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

        var strToSign = buildOrderedSignStr(path, param);
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