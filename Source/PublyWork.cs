﻿using CoChain.Web;

namespace CoBiz
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