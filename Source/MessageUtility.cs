using System.Collections.Concurrent;
using System.Threading;

namespace ChainMart
{
    public class MessageUtility
    {
        static ConcurrentDictionary<int, OrgBag> orgs;


        static readonly Thread cycler = new Thread(SendCycle);


        static async void SendCycle(object state)
        {
            while (true)
            {
                Thread.Sleep(60 * 1000);

                if (orgs == null)
                {
                    orgs = new ConcurrentDictionary<int, OrgBag>();

                    // loading
                }
                else
                {
                }
            }
        }

        public static void Add(int orgid, string msg)
        {
        }
    }
}