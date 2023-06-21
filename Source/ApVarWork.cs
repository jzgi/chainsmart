﻿using System.Threading.Tasks;
using ChainFx.Web;

 namespace ChainSmart; 

 public abstract class ApVarWork : WebWork
 {
 }

 public class AdmlyPurApVarWork : ApVarWork
 {
 }

 public class AdmlyBuyApVarWork : ApVarWork
 {
 }

 public class PtylyApVarWork : ApVarWork
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