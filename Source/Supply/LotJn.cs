using System;
using System.Text;
using SkyChain;

namespace Zhnt.Supply
{
    public class LotJn : IData, IKeyable<(int, int)>
    {
        public static readonly LotJn Empty = new LotJn();

        public const byte ID = 1, LATER = 2;

        // status
        public const short
            STATUS_INIT = 0,
            STATUS_JOINED = 1,
            STATUS_APPLIED = 2,
            STATUS_APPROVED = 3,
            STATUS_ARCHIVED = 4;

        internal int lotid;
        internal int uid;
        internal short status;

        internal string uname;
        internal string utel;
        internal string uim;
        internal string uacct;
        internal string uaddr;
        internal short ptid;
        internal short qty;
        internal DateTime inited;
        internal decimal pay;
        internal decimal credit;

        public void Read(ISource s, byte proj = 15)
        {
            s.Get(nameof(lotid), ref lotid);
            s.Get(nameof(uid), ref uid);
            s.Get(nameof(status), ref status);

            s.Get(nameof(uname), ref uname);
            s.Get(nameof(utel), ref utel);
            s.Get(nameof(uim), ref uim);
            s.Get(nameof(uacct), ref uacct);
            s.Get(nameof(uaddr), ref uaddr);
            s.Get(nameof(ptid), ref ptid);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(inited), ref inited);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(pay), ref pay);
                s.Get(nameof(credit), ref credit);
            }
        }

        public void Write(ISink s, byte proj = 15)
        {
            s.Put(nameof(lotid), lotid);
            s.Put(nameof(uid), uid);
            s.Put(nameof(status), status);

            s.Put(nameof(uname), uname);
            s.Put(nameof(utel), utel);
            s.Put(nameof(uim), uim);
            s.Put(nameof(uacct), uacct);
            s.Put(nameof(uaddr), uaddr);
            if (ptid > 0) s.Put(nameof(ptid), ptid);
            else s.PutNull(nameof(ptid));
            s.Put(nameof(qty), qty);
            s.Put(nameof(inited), inited);

            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(pay), pay);
                s.Put(nameof(credit), credit);
            }
        }

        public (int, int) Key => (lotid, uid);

        string tradeno;
        public string GetTradeNo => tradeno ??= BuildTradeNo(lotid, uid, inited);

        public static string BuildTradeNo(int lotid, int uid, DateTime inited)
        {
            var sb = new StringBuilder();
            sb.Append(lotid).Append('-').Append(uid).Append('-');
            var d = inited.Day;

            if (d < 10)
            {
                sb.Append('0');
            }
            sb.Append(d);

            var h = inited.Hour;
            if (h < 10)
            {
                sb.Append('0');
            }
            sb.Append(h);

            var m = inited.Minute;
            if (m < 10)
            {
                sb.Append('0');
            }
            sb.Append(m);

            return sb.ToString();
        }
    }
}