﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using ChainFx;
using ChainFx.Web;

namespace ChainMart
{
    public abstract class NoteWork : WebWork
    {
        
    }

    [Ui("平台通告管理", icon: "file-text")]
    public class AdmlyNoteWork : NoteWork
    {
        protected override void OnCreate()
        {
            CreateVarWork<AdmlyNoteVarWork>();
        }

        static readonly ConcurrentDictionary<int, Queue<Note>> queues = new ConcurrentDictionary<int, Queue<Note>>();

        public void @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            var q = queues[org.id];
            if (q != null)
            {
                var jc = new JsonContent(true, 16);
                lock (q)
                {
                    Note o;
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

        public static void Append(int orgid, Note note)
        {
            var q = queues[orgid];
            if (q == null)
            {
                q = new Queue<Note>();
                q.Enqueue(note);
                queues.TryAdd(orgid, q);
            }
            else
            {
                lock (q)
                {
                    q.Enqueue(note);
                }
            }
        }
    }
}