﻿using System.Threading.Tasks;
using ChainFx.Web;

namespace ChainSMart
{
    public abstract class ClearVarWork : WebWork
    {
    }

    public class AdmlyBookClearVarWork : ClearVarWork
    {
    }

    public class AdmlyBuyClearVarWork : ClearVarWork
    {
    }

    public class PtylyClearVarWork : ClearVarWork
    {
        [Ui("￥", "微信领款"), Tool(Modal.ButtonOpen)]
        public async Task rcv(WebContext wc, int dt)
        {
            int orderid = wc[0];
            if (wc.IsGet)
            {
            }
            else // POST
            {
                wc.GivePane(200); // close
            }
        }
    }
}