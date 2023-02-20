using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Application;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart
{
    public class LotVarWork : WebWork
    {
        public virtual async Task @default(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();

            const short msk = 255 | MSK_EXTRA;
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty, msk).T(" FROM lots_vw WHERE id = @1 AND srcid = @2");
            var m = await dc.QueryTopAsync<Lot>(p => p.Set(id).Set(org.id), msk);

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", m.name)._LI();
                h.LI_().FIELD("简介", string.IsNullOrEmpty(m.tip) ? "无" : m.tip)._LI();
                h.LI_().FIELD("交货条款", Lot.Terms[m.term]);
                if (m.term > 0) h.FIELD("交货日期", m.dated);
                h._LI();

                h.LI_();
                if (m.targs == null) h.FIELD("限域投放", "不限");
                else h.FIELD("限域投放", m.targs, topOrgs, capt: v => v.Ext);
                h._LI();
                h.LI_().FIELD("基准单位", m.unit).FIELD2("批发件含量", m.unitx, m.unit)._LI();
                h.LI_().FIELD("基准单价", m.price, true).FIELD("促销立减", m.off, true)._LI();
                h.LI_().FIELD("起订件数", m.min).FIELD("限订件数", m.max)._LI();
                h.LI_().FIELD("批次总件数", m.cap).FIELD("剩余件数", m.AvailX)._LI();
                h.LI_().FIELD2("溯源编号", m.nstart, m.nend, "－")._LI();
                h.LI_().FIELD("处理状态", m.status, Lot.Statuses)._LI();
                h.LI_().FIELD2("创建", m.created, m.creator, "&nbsp;")._LI();
                if (m.adapter != null) h.LI_().FIELD2("调整", m.adapted, m.adapter, "&nbsp;")._LI();
                if (m.fixer != null) h.LI_().FIELD2("上线", m.@fixed, m.fixer, "&nbsp;")._LI();
                h._UL();

                h.TABLE(m.ops, o =>
                {
                    h.TD_().T(o.dt, time: 1)._TD();
                    h.TD(o.qty, right: true);
                    h.TD(StockOp.Typs[o.typ]);
                    h.TD(o.avail, right: true);
                    h.TD(o.by);
                }, caption: "库存操作记录", reverse: true);

                h.TOOLBAR(bottom: true, status: m.status, state: m.state);
            });
        }

        protected async Task doimg(WebContext wc, string col, bool shared, int maxage)
        {
            int id = wc[0];
            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").T(col).T(" FROM lots WHERE id = @1");
                if (dc.QueryTop(p => p.Set(id)))
                {
                    dc.Let(out byte[] bytes);
                    if (bytes == null) wc.Give(204); // no content 
                    else wc.Give(200, new WebStaticContent(bytes), shared, maxage);
                }
                else
                {
                    wc.Give(404, null, shared, maxage); // not found
                }
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                ArraySegment<byte> img = f[nameof(img)];

                using var dc = NewDbContext();
                dc.Sql("UPDATE lots SET ").T(col).T(" = @1 WHERE id = @2");
                if (await dc.ExecuteAsync(p => p.Set(img).Set(id)) > 0)
                {
                    wc.Give(200); // ok
                }
                else
                    wc.Give(500); // internal server error
            }
        }
    }

    public class PublyLotVarWork : LotVarWork
    {
        public override async Task @default(WebContext wc)
        {
            int lotid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1");
            var o = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            wc.GivePage(200, h =>
            {
                if (o == null)
                {
                    h.ALERT("无效的溯源产品批次");
                    return;
                }

                // var asset = GrabObject<int, Asset>(o.assetid);

                var src = GrabObject<int, Org>(o.srcid);

                h.TOPBARXL_();
                h.HEADER_("uk-width-expand uk-col uk-padding-small-left").H2(o.name)._HEADER();
                if (o.icon)
                {
                    h.PIC("/lot/", o.id, "/icon", circle: true, css: "uk-width-small");
                }
                else
                    h.PIC("/void.webp", circle: true, css: "uk-width-small");
                h._TOPBARXL();

                h.ARTICLE_("uk-card uk-card-primary");
                h.H4("批次信息", "uk-card-header");
                h.UL_("uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("产品名", o.name)._LI();
                h.LI_().FIELD("简介", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
                h.LI_().FIELD("总件数", o.cap)._LI();
                h.LI_().FIELD("批次编号", o.id, digits: 8)._LI();
                h.LI_().LABEL("产源／供应").A_("/org/", src.id, "/", css: "uk-button-link uk-active").T(src.legal)._A()._LI();

                if (o.nstart > 0 && o.nend > 0) h.LI_().FIELD2("溯源编号", $"{o.nstart:0000 0000}", $"{o.nend:0000 0000}", "－")._LI();
                h.LI_().FIELD2("创建", o.created, o.creator)._LI();
                if (o.adapter != null) h.LI_().FIELD2("制码", o.adapted, o.adapter)._LI();
                if (o.fixer != null) h.LI_().FIELD2("上线", o.@fixed, o.fixer)._LI();
                h._UL();
                h._ARTICLE();

                h.ARTICLE_("uk-card uk-card-primary");
                h.H4("批次检验", "uk-card-header");
                if (o.m1)
                {
                    h.PIC("/lot/", o.id, "/m-1", css: "uk-width-1-1 uk-card-body");
                }
                if (o.m2)
                {
                    h.PIC("/lot/", o.id, "/m-2", css: "uk-width-1-1 uk-card-body");
                }
                if (o.m3)
                {
                    h.PIC("/lot/", o.id, "/m-3", css: "uk-width-1-1 uk-card-body");
                }
                if (o.m4)
                {
                    h.PIC("/lot/", o.id, "/m-4", css: "uk-width-1-1 uk-card-body");
                }
                h._ARTICLE();

                h.FOOTER_("uk-col uk-flex-middle uk-margin-large-top uk-margin-bottom");
                h.SPAN("金中关（北京）信息技术研究院", css: "uk-padding-small");
                h.SPAN("江西同其成科技有限公司", css: "uk-padding-small");
                h._FOOTER();
            }, true, 3600, title: "中惠农通产品溯源信息");
        }

        public async Task icon(WebContext wc)
        {
            await doimg(wc, nameof(icon), true, 3600 * 6);
        }

        public async Task pic(WebContext wc)
        {
            await doimg(wc, nameof(pic), true, 3600 * 6);
        }

        public async Task m(WebContext wc, int sub)
        {
            await doimg(wc, nameof(m) + sub, true, 3600);
        }
    }

    public class SrclyLotVarWork : LotVarWork
    {
        static readonly string[] Units = {"斤", "包", "箱", "桶"};

        [Ui(tip: "修改产品批次", icon: "pencil"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
        public async Task edit(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();
            var cats = Grab<short, Cat>();
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1 AND srcid = @2");
                var o = dc.QueryTop<Lot>(p => p.Set(lotid).Set(org.id));

                await dc.QueryAsync("SELECT id, name FROM assets_vw WHERE orgid = @1 AND status = 4", p => p.Set(org.id));
                var assets = dc.ToIntMap();

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_();

                    h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 12)._LI();
                    h.LI_().SELECT("类别", nameof(o.typ), o.typ, cats, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, tip: "可选", max: 40)._LI();
                    h.LI_().SELECT("相关设施", nameof(o.assetid), o.assetid, assets, required: true)._LI();
                    h.LI_().SELECT("限域投放", nameof(o.targs), o.targs, topOrgs, filter: (k, v) => v.EqCenter, capt: v => v.Ext, size: 2, required: true)._LI();
                    h.LI_().SELECT("交货条款", nameof(o.term), o.term, Lot.Terms, required: true).DATE("交货日期", nameof(o.dated), o.dated)._LI();
                    h.LI_().TEXT("基准单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true, datalst: Units).NUMBER("批发件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                    h.LI_().NUMBER("基准单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("优惠立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订件数", nameof(o.min), o.min).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("批次总件数", nameof(o.cap), o.cap)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
                });
            }
            else // POST
            {
                const short msk = MSK_EDIT;
                // populate 
                var o = await wc.ReadObjectAsync(msk, instance: new Lot
                {
                    adapted = DateTime.Now,
                    adapter = prin.name,
                });

                var item = GrabObject<int, Asset>(o.assetid);
                o.name = item.name;
                o.typ = item.typ;

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots ")._SET_(Lot.Empty, msk).T(" WHERE id = @1 AND srcid = @2");
                await dc.ExecuteAsync(p =>
                {
                    o.Write(p, msk);
                    p.Set(lotid).Set(org.id);
                });

                wc.GivePane(200); // close dialog
            }
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("资料", icon: "album"), Tool(ButtonCrop, size: 3, subs: 4)]
        public async Task m(WebContext wc, int sub)
        {
            await doimg(wc, nameof(m) + sub, false, 3);
        }

        [OrglyAuthorize(0, User.ROL_RVW)]
        [Ui("溯源", "溯源码绑定或印制", icon: "tag"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
        public async Task tag(WebContext wc, int cmd)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                if (cmd == 0)
                {
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("溯源标签方式");
                        h.LI_().AGOTO("Ａ）绑定预制标签", "tag-1")._LI();
                        h.LI_().AGOTO("Ｂ）现场印制专属贴标", "tag-2")._LI();
                        h._FIELDSUL()._FORM();
                    });
                    return;
                }

                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1 AND srcid = @2");
                var o = dc.QueryTop<Lot>(p => p.Set(lotid).Set(org.id));

                var item = GrabObject<int, Asset>(o.assetid);

                if (cmd == 1)
                {
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("绑定预制标签号码");

                        h.LI_().NUMBER("起始号码", nameof(o.nstart), o.nstart)._LI();
                        h.LI_().NUMBER("截至号码", nameof(o.nend), o.nend)._LI();

                        h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(tag), subscript: cmd)._FORM();
                    });
                }
                else // cmd = (page - 1)
                {
                    const short NUM = 90;

                    wc.GivePane(200, h =>
                    {
                        h.UL_(css: "uk-grid uk-child-width-1-6");

                        var today = DateTime.Today;
                        var idx = (cmd - 2) * NUM;
                        for (var i = 0; i < NUM; i++)
                        {
                            h.LI_("height-1-15");

                            h.HEADER_();
                            h.QRCODE(MainApp.WwwUrl + "/lot/" + o.id + "/", css: "uk-width-1-3");
                            h.ASIDE_().H6_().T(Self.name).T("溯源")._H6().SMALL_().T(today, date: 3, time: 0)._SMALL()._ASIDE();
                            h._HEADER();

                            h.H6_("uk-flex").T(lotid, digits: 8).T('-').T(idx + 1).SPAN(Asset.States[item.state], "uk-margin-auto-left")._H6();

                            h._LI();

                            if (++idx >= o.cap)
                            {
                                break;
                            }
                        }
                        h._UL();

                        h.PAGINATION(idx < o.cap, begin: 2, print: true);
                    });
                }
            }
            else // POST
            {
                if (cmd == 1)
                {
                    var f = await wc.ReadAsync<Form>();
                    int nstart = f[nameof(nstart)];
                    int nend = f[nameof(nend)];

                    // update
                    using var dc = NewDbContext();
                    dc.Sql("UPDATE lots SET nstart = @1, nend = @2, state = 2, status = 2, adapted = @3, adapter = @4 WHERE id = @5");
                    await dc.ExecuteAsync(p => p.Set(nstart).Set(nend).Set(DateTime.Now).Set(prin.name).Set(lotid));
                }
                else if (cmd == 2)
                {
                }

                wc.GivePane(200); // close
            }
        }

        [OrglyAuthorize(0, User.ROL_MGT)]
        [Ui("上线", "上线投入使用", icon: "cloud-upload"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
        public async Task ok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("UPDATE lots SET status = 4, fixed = @1, fixer = @2 WHERE id = @3 AND srcid = @4");
            await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

            wc.Give(200);
        }

        [OrglyAuthorize(0, User.ROL_MGT)]
        [Ui("下线", "下线以便修改", icon: "cloud-download"), Tool(ButtonConfirm, status: STU_OKED)]
        public async Task unok(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("UPDATE lots SET status = 2, fixed = NULL, fixer = NULL WHERE id = @1 AND srcid = @2")._MEET_(wc);
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.Give(200);
        }

        [OrglyAuthorize(0, User.ROL_LOG)]
        [Ui("库存", icon: "database"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED | STU_OKED)]
        public async Task stock(WebContext wc)
        {
            int itemid = wc[0];
            var org = wc[-2].As<Org>();
            var prin = (User) wc.Principal;

            short typ = 0;
            decimal qty = 0.0M;

            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("库存操作");
                    h.LI_().SELECT("操作类型", nameof(typ), typ, StockOp.Typs, required: true)._LI();
                    h.LI_().NUMBER("数量", nameof(qty), qty, money: false)._LI();
                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(stock))._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                typ = f[nameof(typ)];
                qty = f[nameof(qty)];

                // update
                using var dc = NewDbContext();

                var now = DateTime.Now;
                if (typ < 5) // add
                {
                    dc.Sql("UPDATE lots SET ops[coalesce(array_length(ops,1),0) + 1] = ROW(@1, @2, @3, (avail + @3::NUMERIC(6,1)), @4), avail = avail + @3::NUMERIC(6,1) WHERE id = @5 AND srcid = @6");
                    await dc.ExecuteAsync(p => p.Set(now).Set(typ).Set(qty).Set(prin.name).Set(itemid).Set(org.id));
                }
                else // reduce
                {
                    dc.Sql("UPDATE lots SET ops[coalesce(array_length(ops,1),0) + 1] = ROW(@1, @2, @3, (avail - @3::NUMERIC(6,1)), @4), avail = avail - @3::NUMERIC(6,1) WHERE id = @5 AND srcid = @6");
                    await dc.ExecuteAsync(p => p.Set(now).Set(typ).Set(qty).Set(prin.name).Set(itemid).Set(org.id));
                }

                wc.GivePane(200); // close dialog
            }
        }


        [OrglyAuthorize(0, User.ROL_MGT)]
        [Ui(tip: "删除或者作废该批次", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED | STU_ADAPTED)]
        public async Task rm(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            try
            {
                dc.Sql("DELETE FROM lots WHERE id = @1 AND srcid = @2");
                await dc.ExecuteAsync(p => p.Set(id).Set(org.id));
            }
            catch (Exception e)
            {
                dc.Sql("UPDATE lots SET status = 0 WHERE id = @1 AND srcid = @2");
                await dc.ExecuteAsync(p => p.Set(id).Set(org.id));
            }

            wc.Give(204); // no content
        }
    }

    public class CtrlyLotVarWork : LotVarWork
    {
    }

    public class ShplyBookLotVarWork : LotVarWork
    {
        //
        // NOTE: this page is made publicly cacheable, though under variable path
        //
        public override async Task @default(WebContext wc)
        {
            int lotid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1");
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            var item = GrabObject<int, Asset>(lot.assetid);

            wc.GivePane(200, h =>
            {
                h.ARTICLE_("uk-card uk-card-primary");
                if (item.pic)
                {
                    h.PIC_(MainApp.WwwUrl, "/item/", lot.assetid, "/pic")._PIC();
                }
                h.H4("产品详情", "uk-card-header");
                h.SECTION_("uk-card-body");
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", item.name)._LI();
                h.LI_().FIELD("产品描述", string.IsNullOrEmpty(item.tip) ? "无" : item.tip)._LI();
                if (!string.IsNullOrEmpty(item.reserve))
                {
                    h.LI_().FIELD("生产基地", item.reserve)._LI();
                }
                h.LI_().FIELD2("创建", item.created, lot.creator)._LI();
                h.LI_().FIELD2("上线", item.@fixed, lot.fixer)._LI();
                h._UL();
                h._SECTION();
                h._ARTICLE();

                // bottom bar
                //
                decimal realprice = lot.RealPrice;
                short qtyx = lot.min;
                decimal unitx = lot.unitx;
                decimal qty = qtyx * unitx;
                decimal topay = decimal.Round(qty * lot.RealPrice, 2); // round to money 2 decimal digits

                h.BOTTOMBAR_();
                h.FORM_("uk-flex uk-width-1-1", oninput: $"qty.value = (qtyx.value * {unitx}).toFixed(1); topay.value = ({realprice} * qty.value).toFixed(2);");

                h.HIDDEN(nameof(realprice), realprice);

                h.SELECT_(null, nameof(qty), css: "uk-width-small");
                for (int i = lot.min; i < lot.max; i += 1)
                {
                    h.OPTION_(i).T(i)._OPTION();
                }
                h._SELECT().SP().SPAN_("uk-width-expand").T("件，共").SP();
                h.OUTPUT(nameof(qty), qty).SP().T(lot.unit)._SPAN();

                // pay button
                h.BUTTON_(nameof(book), onclick: "return call_book(this);", css: "uk-button-danger uk-width-medium").CNYOUTPUT(nameof(topay), topay)._BUTTON();

                h._FORM();
                h._BOTTOMBAR();
            }, shared: true, maxage: 60);
        }

        public async Task book(WebContext wc, int cmd)
        {
            var shp = wc[-3].As<Org>();
            int lotid = wc[0];

            var prin = (User) wc.Principal;

            // submitted values
            var f = await wc.ReadAsync<Form>();
            short qtyx = f[nameof(qtyx)];

            using var dc = NewDbContext(IsolationLevel.ReadCommitted);
            try
            {
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1");
                var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

                var m = new Book
                {
                    typ = lot.typ,
                    name = lot.name,
                    tip = lot.tip,
                    created = DateTime.Now,
                    creator = prin.name,

                    shpid = shp.id,
                    shpname = shp.Name,
                    mktid = shp.MarketId,
                    srcid = lot.srcid,
                    srcname = lot.srcname,
                    zonid = lot.zonid,
                    ctrid = shp.ctrid,

                    itemid = lot.assetid,
                    lotid = lot.id,
                    unit = lot.unit,
                    unitx = lot.unitx,
                    price = lot.price,
                    off = lot.off,
                    qty = qtyx * lot.unitx,
                    topay = lot.RealPrice * qtyx * lot.unitx,
                };

                // make use of any existing abandoned record
                const short msk = MSK_BORN | MSK_EDIT;
                dc.Sql("INSERT INTO books ").colset(Book.Empty, msk)._VALUES_(Book.Empty, msk).T(" ON CONFLICT (shpid, status) WHERE status = 0 DO UPDATE ")._SET_(Book.Empty, msk).T(" RETURNING id, topay");
                await dc.QueryTopAsync(p => m.Write(p));
                dc.Let(out int bookid);
                dc.Let(out decimal topay);

                // call WeChatPay to prepare order there
                string trade_no = (bookid + "-" + topay).Replace('.', '-');
                var (prepay_id, err_code) = await WeixinUtility.PostUnifiedOrderAsync(sup: true,
                    trade_no,
                    topay,
                    prin.im, // the payer
                    wc.RemoteIpAddress.ToString(),
                    MainApp.MgtUrl + "/" + nameof(MgtService.onpay),
                    m.ToString()
                );

                if (prepay_id != null)
                {
                    wc.Give(200, WeixinUtility.BuildPrepayContent(prepay_id));
                }
                else
                {
                    dc.Rollback();
                    wc.Give(500);
                }
            }
            catch (Exception e)
            {
                dc.Rollback();
                Err(e.Message);
                wc.Give(500);
            }
        }
    }
}