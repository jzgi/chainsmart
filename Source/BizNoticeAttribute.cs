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
                return NoticeBox.CheckAndClearPully(noticeId, slot);
            }
            else
            {
                return NoticeBox.CheckPully(noticeId, slot);
            }
        }
    }
}