using System;
using System.Data;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Application;
using static ChainFx.Entity;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.Modal;

namespace ChainMart
{
    public class LotVarWork : WebWork
    {
        public virtual async Task @default(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1 AND srcid = @2");
            var o = await dc.QueryTopAsync<Lot>(p => p.Set(lotid).Set(org.id));

            wc.GivePane(200, h =>
            {
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", o.name)._LI();
                h.LI_().FIELD("简介", o.tip)._LI();
                h.LI_().FIELD("限投放", o.targs, topOrgs, capt: v => v.Ext)._LI();
                h.LI_().FIELD("计价单位", o.unit).FIELD("每件含量", o.unitx, false)._LI();
                h.LI_().FIELD("单价", o.price).FIELD("立减", o.off)._LI();
                h.LI_().FIELD("起订件数", o.min).FIELD("限订件数", o.max)._LI();
                h.LI_().FIELD("递增", o.step)._LI();
                h.LI_().FIELD("总件数", o.cap).FIELD("剩余件数", o.avail)._LI();
                h.LI_().FIELD2("溯源编号", o.nstart, o.nend, "－")._LI();
                h.LI_().FIELD("处理进展", o.status, Lot.Statuses).FIELD("应用状况", Lot.States[o.state])._LI();
                h.LI_().FIELD2("创建", o.created, o.creator, "&nbsp;")._LI();
                if (o.adapter != null) h.LI_().FIELD2("调整", o.adapted, o.adapter, "&nbsp;")._LI();
                if (o.oker != null) h.LI_().FIELD2("上线", o.oked, o.oker, "&nbsp;")._LI();
                h._UL();

                h.TOOLBAR(bottom: true, status: o.status, state: o.state);
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
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            wc.GivePage(200, h =>
            {
                if (lot == null)
                {
                    h.ALERT("无效的溯源产品批次");
                    return;
                }

                var item = GrabObject<int, Item>(lot.itemid);

                var src = GrabObject<int, Org>(lot.srcid);

                h.TOPBARXL_();
                h.HEADER_("uk-width-expand uk-col uk-padding-small-left").H2(item.name)._HEADER();
                if (item.icon)
                {
                    h.PIC("/item/", item.id, "/icon", circle: true, css: "uk-width-small");
                }
                else
                    h.PIC("/void.webp", circle: true, css: "uk-width-small");
                h._TOPBARXL();

                h.ARTICLE_("uk-card uk-card-primary");
                h.H4("批次信息", "uk-card-header");
                h.UL_("uk-card-body uk-list uk-list-divider");
                h.LI_().FIELD("产品名", lot.name)._LI();
                h.LI_().FIELD("简介", string.IsNullOrEmpty(lot.tip) ? "无" : lot.tip)._LI();
                h.LI_().FIELD2("总件数", lot.cap, lot.unit)._LI();
                h.LI_().FIELD2("溯源编号", $"{lot.nstart:0000 0000}", $"{lot.nend:0000 0000}", "－")._LI();
                h.LI_().FIELD2("创建", lot.created, lot.creator)._LI();
                h.LI_().FIELD2("制码", lot.adapted, lot.adapter)._LI();
                h.LI_().FIELD2("上线", lot.oked, lot.oker)._LI();
                h._UL();
                h._ARTICLE();

                h.ARTICLE_("uk-card uk-card-primary");
                h.H4("批次检验", "uk-card-header");
                if (lot.m1)
                {
                    h.PIC("/lot/", lot.id, "/m-1", css: "uk-width-1-1 uk-card-body");
                }
                if (lot.m2)
                {
                    h.PIC("/lot/", lot.id, "/m-2", css: "uk-width-1-1 uk-card-body");
                }
                if (lot.m3)
                {
                    h.PIC("/lot/", lot.id, "/m-3", css: "uk-width-1-1 uk-card-body");
                }
                if (lot.m4)
                {
                    h.PIC("/lot/", lot.id, "/m-4", css: "uk-width-1-1 uk-card-body");
                }
                h._ARTICLE();

                h.ARTICLE_("uk-card uk-card-primary");
                h.H4("产品详情", "uk-card-header");
                h.SECTION_("uk-card-body");
                if (item.pic)
                {
                    h.PIC("/item/", lot.itemid, "/pic", css: "uk-width-1-1");
                }
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", item.name)._LI();
                h.LI_().FIELD("产品描述", string.IsNullOrEmpty(item.tip) ? "无" : item.tip)._LI();
                if (!string.IsNullOrEmpty(item.origin))
                {
                    h.LI_().FIELD("生产基地", item.origin)._LI();
                }
                h.LI_().LABEL("产源／供应").A_("/org/", src.id, "/", css: "uk-button-link uk-active").T(src.legal)._A()._LI();
                h.LI_().FIELD2("创建", item.created, lot.creator)._LI();
                h.LI_().FIELD2("上线", item.oked, lot.oker)._LI();
                h._UL();
                h._SECTION();
                h._ARTICLE();

                h.ARTICLE_("uk-card uk-card-primary");
                h.H4("产品证照", "uk-card-header");
                if (item.m1)
                {
                    h.PIC("/item/", item.id, "/m-1", css: "uk-width-1-1 uk-card-body");
                }
                if (item.m2)
                {
                    h.PIC("/item/", item.id, "/m-2", css: "uk-width-1-1 uk-card-body");
                }
                if (item.m3)
                {
                    h.PIC("/item/", item.id, "/m-3", css: "uk-width-1-1 uk-card-body");
                }
                if (item.m4)
                {
                    h.PIC("/item/", item.id, "/m-4", css: "uk-width-1-1 uk-card-body");
                }
                h._ARTICLE();

                h.FOOTER_("uk-col uk-flex-middle uk-margin-large-top uk-margin-bottom");
                h.SPAN("金中关（北京）信息技术研究院", css: "uk-padding-small");
                h.SPAN("江西同其成科技有限公司", css: "uk-padding-small");
                h._FOOTER();
            }, true, 3600, title: "中惠农通产品溯源信息");
        }

        public async Task m(WebContext wc, int sub)
        {
            await doimg(wc, nameof(m) + sub, true, 3600);
        }
    }

    public class SrclyLotVarWork : LotVarWork
    {
        [Ui(tip: "修改产品销售批次", icon: "pencil"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
        public async Task edit(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();
            var prin = (User) wc.Principal;

            if (wc.IsGet)
            {
                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots_vw WHERE id = @1 AND srcid = @2");
                var o = dc.QueryTop<Lot>(p => p.Set(lotid).Set(org.id));

                await dc.QueryAsync("SELECT id, name FROM items_vw WHERE srcid = @1 AND status = 4", p => p.Set(org.id));
                var items = dc.ToIntMap();

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("产品销售批次信息");

                    h.LI_().SELECT("已上线产品", nameof(o.itemid), o.itemid, items, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, tip: "可选", max: 40)._LI();
                    h.LI_().DATE("预售交割", nameof(o.dated), o.dated)._LI();
                    h.LI_().TEXT("计价单位", nameof(o.unit), o.unit, min: 1, max: 4, required: true).NUMBER("每件含量", nameof(o.unitx), o.unitx, min: 1, money: false)._LI();
                    h.LI_().NUMBER("单价", nameof(o.price), o.price, min: 0.00M, max: 99999.99M).NUMBER("立减", nameof(o.off), o.off, min: 0.00M, max: 99999.99M)._LI();
                    h.LI_().NUMBER("起订件数", nameof(o.min), o.min).NUMBER("限订件数", nameof(o.max), o.max, min: 1, max: 1000)._LI();
                    h.LI_().NUMBER("递增", nameof(o.step), o.step)._LI();
                    h.LI_().NUMBER("批次总件数", nameof(o.cap), o.cap).NUMBER("可售件数", nameof(o.avail), o.avail)._LI();

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

                var item = GrabObject<int, Item>(o.itemid);
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

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("限域", icon: "crosshairs"), Tool(ButtonShow, status: STU_CREATED | STU_ADAPTED)]
        public async Task targ(WebContext wc)
        {
            int lotid = wc[0];
            var org = wc[-2].As<Org>();
            var topOrgs = Grab<int, Org>();

            int[] targs;

            if (wc.IsGet)
            {
                using var dc = NewDbContext();

                await dc.QueryTopAsync("SELECT targs FROM lots WHERE id = @1 AND srcid = @2", p => p.Set(lotid).Set(org.id));
                dc.Let(out targs);

                wc.GivePane(200, h =>
                {
                    h.FORM_();

                    h.FIELDSUL_("该批次限定销售区域");
                    h.LI_().SELECT("销售区域", nameof(targs), targs, topOrgs, filter: (k, v) => v.EqCenter || v.EqMarket, capt: v => v.Ext, size: 12, required: false)._LI();
                    h._FIELDSUL();

                    h.BOTTOM_BUTTON("确认", nameof(targ));
                    h._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                targs = f[nameof(targs)];
                targs = targs.RemovedOf(0);

                // update
                using var dc = NewDbContext();
                dc.Sql("UPDATE lots SET targs = @1 WHERE id = @2 AND srcid = @3");
                await dc.ExecuteAsync(p =>
                {
                    if (targs == null || targs.Length == 0)
                    {
                        p.SetNull();
                    }
                    else
                        p.Set(targs);

                    p.Set(lotid).Set(org.id);
                });

                wc.GivePane(200); // close dialog
            }
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui(tip: "删除该产品批次", icon: "trash"), Tool(ButtonConfirm, status: STU_CREATED)]
        public async Task rm(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("DELETE FROM lots WHERE id = @1 AND srcid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.Give(204); // no content
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
                    var src = GrabObject<int, Org>(o.srcid);

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
                            h.ASIDE_().H6_("uk-margin-small-bottom").T(Self.name).T("溯源")._H6().SMALL(src.name)._ASIDE();
                            h._HEADER();

                            h.H6_("uk-flex").SPAN(idx + 1).SPAN_("uk-margin-auto-left").T(today, date: 2, time: 0)._SPAN()._H6();

                            h._LI();

                            if (++idx >= o.cap)
                            {
                                break;
                            }
                        }
                        h._UL();

                        h.PAGINATION(idx <= o.cap, print: true);
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
            dc.Sql("UPDATE lots SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND srcid = @4");
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
            dc.Sql("UPDATE lots SET status = 2, oked = NULL, oker = NULL WHERE id = @1 AND srcid = @2")._MEET_(wc);
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.Give(200);
        }

        [OrglyAuthorize(0, User.ROL_MGT)]
        [Ui("无效", "将批次设为无效", icon: "ban"), Tool(ButtonConfirm, status: STU_ADAPTED | STU_OKED)]
        public async Task @void(WebContext wc)
        {
            int id = wc[0];
            var org = wc[-2].As<Org>();

            using var dc = NewDbContext();
            dc.Sql("UPDATE lots SET status = 0 WHERE id = @1 AND srcid = @2");
            await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

            wc.Give(204); // no content
        }
    }

    public class CtrlyLotVarWork : LotVarWork
    {
    }

    public class ShplyBookLotVarWork : LotVarWork
    {
        public override async Task @default(WebContext wc)
        {
            int lotid = wc[0];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1");
            var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

            var item = GrabObject<int, Item>(lot.itemid);

            wc.GivePane(200, h =>
            {
                h.ARTICLE_("uk-card uk-card-primary");
                if (item.pic)
                {
                    h.PIC_().T(MainApp.WwwUrl).T("/item/").T(lot.itemid).T("/pic")._PIC();
                }
                h.H4("产品详情", "uk-card-header");
                h.SECTION_("uk-card-body");
                h.UL_("uk-list uk-list-divider");
                h.LI_().FIELD("产品名", item.name)._LI();
                h.LI_().FIELD("产品描述", string.IsNullOrEmpty(item.tip) ? "无" : item.tip)._LI();
                if (!string.IsNullOrEmpty(item.origin))
                {
                    h.LI_().FIELD("生产基地", item.origin)._LI();
                }
                // h.LI_().LABEL("产源／供应").A_("/org/", src.id, "/", css: "uk-button-link uk-active").T(src.fully)._A()._LI();
                h.LI_().FIELD2("创建", item.created, lot.creator)._LI();
                h.LI_().FIELD2("上线", item.oked, lot.oker)._LI();
                h._UL();
                h._SECTION();
                h._ARTICLE();

                // bottom bar
                //
                decimal realprice = lot.RealPrice;
                short qty = lot.min;
                decimal unitx = lot.unitx;
                decimal qtyx = qty * unitx;
                decimal topay = decimal.Round(qtyx * lot.RealPrice, 2); // round to money 2 decimal digits

                h.BOTTOMBAR_();
                h.FORM_("uk-flex uk-width-1-1", oninput: $"qtyx.value = (qty.value * {unitx}).toFixed(1); topay.value = ({realprice} * qtyx.value).toFixed(2);");

                h.HIDDEN(nameof(realprice), realprice);

                h.SELECT_(null, nameof(qty), css: "uk-width-small");
                for (int i = lot.min; i < lot.max; i += lot.step)
                {
                    h.OPTION_(i).T(i)._OPTION();
                }
                h._SELECT().SP().SPAN_("uk-width-expand").T("件，共").SP();
                h.OUTPUT(nameof(qtyx), qtyx).SP().T(lot.unit)._SPAN();

                // pay button
                h.BUTTON_(nameof(book), onclick: "return call_book(this);", css: "uk-button-danger uk-width-medium").CNYOUTPUT(nameof(topay), topay)._BUTTON();

                h._FORM();
                h._BOTTOMBAR();
            });
        }

        public async Task book(WebContext wc, int cmd)
        {
            var shp = wc[-3].As<Org>();
            int lotid = wc[0];

            var prin = (User) wc.Principal;

            // submitted values
            var f = await wc.ReadAsync<Form>();
            short qty = f[nameof(qty)];

            using var dc = NewDbContext(IsolationLevel.ReadCommitted);
            try
            {
                dc.Sql("SELECT ").collst(Lot.Empty).T(" FROM lots WHERE id = @1");
                var lot = await dc.QueryTopAsync<Lot>(p => p.Set(lotid));

                dc.Sql("SELECT ").collst(Item.Empty).T(" FROM items WHERE id = @1");
                var item = await dc.QueryTopAsync<Item>(p => p.Set(lot.itemid));

                var m = new Book
                {
                    typ = lot.typ,
                    name = lot.name,
                    created = DateTime.Now,
                    creator = prin.name,
                    shpid = shp.id,
                    shpname = shp.Name,
                    shptel = shp.tel,
                    mktid = shp.MarketId,
                    srcid = lot.srcid,
                    srcname = lot.srcname,
                    zonid = lot.zonid,
                    ctrid = shp.ctrid,
                    itemid = lot.itemid,
                    lotid = lot.id,
                    unit = lot.unit,
                    unitx = lot.unitx,
                    price = lot.price,
                    off = lot.off,
                    qty = qty,
                    topay = lot.RealPrice * qty * lot.unitx,
                };

                // make use of any existing abandoned record
                const short msk = MSK_BORN | MSK_EDIT;
                dc.Sql("INSERT INTO books ").colset(Book.Empty, msk)._VALUES_(Book.Empty, msk).T(" ON CONFLICT (shpid, status) WHERE status = 0 DO UPDATE ")._SET_(Book.Empty, msk).T(" RETURNING id, topay");
                await dc.QueryTopAsync(p => m.Write(p));
                dc.Let(out int bookid);
                dc.Let(out decimal topay);

                // call WeChatPay to prepare order there
                string trade_no = (bookid + "-" + topay).Replace('.', '-');
                var (prepay_id, err_code) = await WeixinUtility.PostUnifiedOrderAsync(sc: true,
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