﻿using CoChain.Web;

namespace Revital
{
    public class PublyWork : WebWork
    {
    }

    public class PublyShpWork : PublyWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyBizVarWork>();
        }
    }
}