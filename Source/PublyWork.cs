﻿using ChainFx.Web;

namespace ChainMart
{
    public class PublyWork : WebWork
    {
    }

    public class PublyShpWork : PublyWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<PublyShpVarWork>();
        }
    }
}