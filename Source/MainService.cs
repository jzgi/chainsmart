﻿using System;
using System.Threading.Tasks;
using System.Web;
using ChainFX;
using ChainFX.Nodal;
using ChainFX.Web;
using static ChainFX.Entity;

namespace ChainSmart;

public abstract class MainService : WebService
{
    public void @catch(WebContext wc)
    {
        var err = wc.Error;

        if (err is ForbiddenException ex)
        {
            wc.GivePage(403,
                m => m.ALERT(head: "⛔ 没有访问权限", p: "此功能需要系统授权后才能使用。"),
                title: "权限保护",
                shared: false, maxage: 30
            );
        }
        else
        {
            wc.GiveMsg(500,
                err.Message, err.StackTrace,
                shared: false, maxage: 30
            );
        }
    }

    public async Task login(WebContext wc)
    {
        string tel = null;
        string password = null;
        string url;
        if (wc.IsGet)
        {
            wc.GivePage(200, h =>
            {
                url = wc.Query[nameof(url)];
                url = HttpUtility.UrlDecode(url);

                h.TOPBARXL_();
                h.NAV_(css: "uk-flex-center").ICON("user", ratio: 3, "uk-circle uk-background-muted uk-padding-large")._NAV();
                h._TOPBARXL();

                h.ALERT("注：新用户请从中惠农通微信公众号来注册", css: "uk-alert-warning");

                h.FORM_().FIELDSUL_();
                h.LI_().TEXT("手机号", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                h.LI_().PASSWORD("密码", nameof(password), password, max: 12, min: 3)._LI();
                h._FIELDSUL();

                h.HIDDEN(nameof(url), url);
                h.BOTTOMBAR_().BUTTON("登录", nameof(login))._BOTTOMBAR();
                h._FORM();
            }, title: "用户登录");
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            tel = f[nameof(tel)];
            password = f[nameof(password)];
            url = f[nameof(url)];

            var credential = MainUtility.ComputeCredential(tel, password);

            using var dc = Nodality.NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
            var prin = dc.QueryTop<User>(p => p.Set(tel));

            if (prin == null || !credential.Equals(prin.credential))
            {
                wc.GiveRedirect(nameof(login));
                return;
            }

            // successfully signed in
            wc.Principal = prin;

            wc.SetTokenCookies(prin);

            wc.GiveRedirect(url ?? "/");
        }
    }

    public async Task signup(WebContext wc)
    {
        // must have openid
        string openid = wc.Cookies[nameof(openid)];

        string name = null;
        string tel = null;
        string vcode = null; // verification code
        string url;
        if (wc.IsGet) // GET
        {
            url = wc.Query[nameof(url)];
            url = HttpUtility.UrlDecode(url);

            RenderPage(wc);
        }
        else // POST
        {
            var f = await wc.ReadAsync<Form>();
            url = f[nameof(url)];
            name = f[nameof(name)];
            tel = f[nameof(tel)];
            vcode = f[nameof(vcode)];

            var m = new User
            {
                typ = 0,
                name = name,
                tel = tel,
                im = openid,
                created = DateTime.Now,
                creator = name
            };

            // check
            if (vcode != CryptoUtility.ComputeVCode(m.tel))
            {
                RenderPage(wc, error: true);
            }
            else
            {
                // insert or update
                using var dc = Nodality.NewDbContext();
                const short msk = MSK_BORN | MSK_EDIT;
                dc.Sql("INSERT INTO users ").colset(m, msk)._VALUES_(m, msk).T(" ON CONFLICT (tel) DO UPDATE SET im = @1 RETURNING ").collst(User.Empty);
                m = await dc.QueryTopAsync<User>(p =>
                {
                    m.Write(p);
                    p.Set(openid);
                });

                // refresh cookie
                wc.Principal = m;
                wc.SetTokenCookies(m);
                wc.GiveRedirect(url);
            }
        }

        void RenderPage(WebContext webctx, bool error = false)
        {
            webctx.GivePage(200, h =>
            {
                h.NAV_("uk-top-placeholder")._NAV();
                h.HEADER_(css: "uk-flex-center").ICON("user", ratio: 4, "uk-circle uk-background-muted uk-padding-large")._HEADER();

                h.ALERT("为了服务更便利，请尽量照实填写", css: "uk-alert-success");

                h.FORM_();

                h.FIELDSUL_();
                h.LI_().TEXT("姓名", nameof(name), name, max: 10, min: 2, required: true)._LI();
                h.LI_().LABEL("手机号").TEXT(null, nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true, css: "uk-width-expand").BUTTON("获取验证码", action: nameof(smsvcode), onclick: "return $smsvcode(this);", css: "uk-button-secondary")._LI();
                h.LI_().TEXT("验证码", nameof(vcode), vcode, tip: "短信收到的验证码", pattern: "[0-9]+", max: 4, min: 4)._LI();
                h._FIELDSUL();

                h.HIDDEN(nameof(url), url);

                if (error)
                {
                    h.ALERT("验证码错误", css: "uk-alert-danger");
                }
                else
                {
                    h.BOTTOMBAR_().BUTTON("注册", nameof(signup))._BOTTOMBAR();
                }

                h._FORM();
            }, title: "注册新用户");
        }
    }

    public async Task smsvcode(WebContext wc)
    {
        var f = await wc.ReadAsync<Form>();
        string tel = f[nameof(tel)];
        string name = f[nameof(name)];

        string vcode = CryptoUtility.ComputeVCode(tel);

        string ret = await WeixinUtility.SendVCodeSmsAsync(tel, vcode);
    }
}