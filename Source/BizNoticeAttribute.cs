using ChainFx.Web;

namespace ChainMart
{
    public class BizNoticeAttribute : NoticeAttribute
    {
        public BizNoticeAttribute(short slot) : base(slot)
        {
        }

        public override int DoCheck(int noticeId, bool clear = false)
        {
            if (clear)
            {
                return NoticeBot.CheckAndClearPully(noticeId, slot);
            }
            else
            {
                return NoticeBot.CheckPully(noticeId, slot);
            }
        }
    }
}