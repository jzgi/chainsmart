using ChainFx;

namespace ChainMart
{
    public class SetgUtility
    {
        public static readonly decimal
            rtlbasic,
            rtlfee,
            rtlpayrate,
            suppayrate;

        static SetgUtility()
        {
            var jo = Application.Setg;
            jo.Get(nameof(rtlbasic), ref rtlbasic);
            jo.Get(nameof(rtlfee), ref rtlfee);
            jo.Get(nameof(rtlpayrate), ref rtlpayrate);
            jo.Get(nameof(suppayrate), ref suppayrate);
        }
    }
}