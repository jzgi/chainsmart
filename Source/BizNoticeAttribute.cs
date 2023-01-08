using System;
using ChainFx.Web;

namespace ChainMart
{
    public class BizNoticeAttribute : NoticeAttribute
    {
        public BizNoticeAttribute(short typ) : base(typ)
        {
        }

        public override int DoCheck()
        {
            return 0;
        }

        public override void DoClear(WebContext wc)
        {
            throw new NotImplementedException();
        }
    }
}