using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace ChainSmart;

public static class IcbcUtility
{
    static readonly string
        bankprov,
        bankcity,
        bankreg;

    static readonly string
        supbankacct,
        rtlbankacct,
        bankacctname;

    static IcbcUtility()
    {
        bankprov = Application.Prog[nameof(bankprov)];
        bankcity = Application.Prog[nameof(bankcity)];
        bankreg = Application.Prog[nameof(bankreg)];
        supbankacct = Application.Prog[nameof(supbankacct)];
        rtlbankacct = Application.Prog[nameof(rtlbankacct)];
        bankacctname = Application.Prog[nameof(bankacctname)];
    }

    static readonly Map<int, string> ColDefs = new()
    {
        { 0, "币种" },
        { 1, "日期" },
        { 2, "明细标志" },
        { 3, "顺序号" },
        { 4, "付款账号开户行" },

        { 5, "付款账号/卡号" },
        { 6, "付款账号名称/卡名称" },
        { 7, "收款账号开户行" },
        { 8, "收款账号省份" },
        { 9, "收款账号地市" },

        { 10, "收款账号地区码" },
        { 11, "收款账号" },
        { 12, "收款账号名称" },
        { 13, "金额" },
        { 14, "汇款用途" },

        { 15, "备注信息" },
        { 16, "汇款方式" },
        { 17, "收款账户短信通知手机号码" },
        { 18, "自定义序号" },
        { 19, "预先审批编号" },
    };

    static string GetDate()
    {
        var today = DateTime.Today;
        var sb = new StringBuilder();
        sb.Append(today.Year);

        var m = today.Month;
        if (m < 10)
        {
            sb.Append('0');
        }
        sb.Append(m);

        var d = today.Day;
        if (d < 10)
        {
            sb.Append('0');
        }
        sb.Append(d);

        return sb.ToString();
    }

    /// <summary>
    /// Gives a frame page.
    /// </summary>
    public static async Task GiveXls(this WebContext wc, short status, bool sup, int orgid, IList<Ap> aplst, Map<int, Org> orgmap)
    {
        var workbook = new HSSFWorkbook();
        var sheet = workbook.CreateSheet();

        var hdr = sheet.CreateRow(0);
        for (int i = 0; i < ColDefs.Count; i++)
        {
            hdr.CreateCell(i, CellType.String).SetCellValue(ColDefs[i]);
        }

        var date = GetDate();

        for (int i = 0; i < aplst?.Count; i++)
        {
            var ap = aplst[i];
            if (!orgmap.TryGetValue(ap.orgid, out var org)) return;

            lock (org)
            {
                var row = sheet.CreateRow(i + 1);

                row.CreateCell(0, CellType.String).SetCellValue("RMB");
                row.CreateCell(1, CellType.String).SetCellValue(date);
                row.CreateCell(3, CellType.String).SetCellValue(org.id.ToString());
                row.CreateCell(4, CellType.String).SetCellValue("工行");

                row.CreateCell(5, CellType.String).SetCellValue(sup ? supbankacct : rtlbankacct);
                row.CreateCell(6, CellType.String).SetCellValue(bankacctname);

                row.CreateCell(7, CellType.String).SetCellValue("工行");
                row.CreateCell(8, CellType.String).SetCellValue(bankprov);
                row.CreateCell(9, CellType.String).SetCellValue(bankcity);
                row.CreateCell(10, CellType.String).SetCellValue(bankreg);

                row.CreateCell(11, CellType.String).SetCellValue(org.bankacct);
                row.CreateCell(12, CellType.String).SetCellValue(org.bankacctname);
                row.CreateCell(13, CellType.String).SetCellValue("" + ap.topay);
                row.CreateCell(14, CellType.String).SetCellValue("其他应付款");
                row.CreateCell(16, CellType.String).SetCellValue("1");
                row.CreateCell(17, CellType.String).SetCellValue(org.tel);
            }
        }

        wc.SetHeader("Content-Type", "application/vnd.ms-excel");
        wc.StatusCode = status;

        var mem = new MemoryStream();
        workbook.Write(mem);
        workbook.Close();

        var buf = mem.ToArray();

        wc.SetHeader("Content-Length", buf.Length);
        wc.SetHeader("Content-Disposition", "attachment; filename=\"" + orgid + ".xls\"");
        await wc.ResponseStream.WriteAsync(buf);
    }
}