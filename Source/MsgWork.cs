using System.Collections.Concurrent;
using System.Collections.Generic;
using SkyChain;
using SkyChain.Web;

namespace Revital
{
    public abstract class MsgWork : WebWork
    {
    }

    public class OrglyMsgWork : MsgWork
    {
        static readonly ConcurrentDictionary<int, Queue<Msg>> queues = new ConcurrentDictionary<int, Queue<Msg>>();

        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            var q = queues[org.id];
            if (q != null)
            {
                var jc = new JsonContent(true, 16);
                lock (q)
                {
                    Msg o;
                    jc.ARR_();
                    while ((o = q.Dequeue()) != null)
                    {
                        jc.OBJ(j => { j.Put(nameof(o.typ), o.typ); });
                    }
                    jc._ARR();
                }
                wc.Give(200, jc);
            }
            else
            {
                wc.Give(204); // no content
            }
        }

        public static void Append(int orgid, Msg msg)
        {
            var q = queues[orgid];
            if (q == null)
            {
                q = new Queue<Msg>();
                q.Enqueue(msg);
                queues.TryAdd(orgid, q);
            }
            else
            {
                lock (q)
                {
                    q.Enqueue(msg);
                }
            }
        }
    }
}