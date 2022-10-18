using System.Collections.Concurrent;
using System.Collections.Generic;
using ChainFx;
using ChainFx.Web;

namespace ChainMart
{
    public abstract class EventWork : WebWork
    {
    }

    [Ui("事件管理", "业务")]
    public class AdmlyEventWork : EventWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<AdmlyEventVarWork>();
        }

        static readonly ConcurrentDictionary<int, Queue<Event>> queues = new ConcurrentDictionary<int, Queue<Event>>();

        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            var q = queues[org.id];
            if (q != null)
            {
                var jc = new JsonBuilder(true, 16);
                lock (q)
                {
                    Event o;
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

        public static void Append(int orgid, Event @event)
        {
            var q = queues[orgid];
            if (q == null)
            {
                q = new Queue<Event>();
                q.Enqueue(@event);
                queues.TryAdd(orgid, q);
            }
            else
            {
                lock (q)
                {
                    q.Enqueue(@event);
                }
            }
        }
    }
}