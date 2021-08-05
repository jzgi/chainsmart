using System;
using SkyChain;

namespace Zhnt
{
    /// <summary>
    /// A cash reconciliation or payment record.
    ///  </summary>
    public class Clear : IData, IKeyable<int>
    {
        public static readonly Clear Empty = new Clear();

        public const byte ID = 1, LATER = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {1, "厨房（调养）"},
            {2, "服务站（调养）"},
            {3, "商家（走服务站）"},
            {4, "商家（走三方）"},
            {5, "服务站（拼团）"},
        };

        public const short
            STATUS_CALCED = 0,
            STATUS_RECKONED = 1,
            STATUS_QUED = 3,
            STATUS_ARCHIVED = 4;

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STATUS_CALCED, "初算"},
            {STATUS_RECKONED, "已结"},
            {STATUS_QUED, "已付"},
        };

        internal int id;
        internal short typ;
        internal short status;

        internal short orgid;
        internal DateTime till;
        internal short serv;
        internal short compl;
        internal decimal amt;
        internal decimal pay;
        internal string payer;


        public void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(status), ref status);

            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(till), ref till);
            s.Get(nameof(serv), ref serv);
            s.Get(nameof(compl), ref compl);
            s.Get(nameof(amt), ref amt);
            s.Get(nameof(pay), ref pay);
            s.Get(nameof(payer), ref payer);
        }

        public void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(typ), typ);
            s.Put(nameof(status), status);

            s.Put(nameof(orgid), orgid);
            s.Put(nameof(till), till);
            s.Put(nameof(serv), serv);
            s.Put(nameof(compl), compl);
            s.Put(nameof(amt), amt);
            s.Put(nameof(pay), pay);
            s.Put(nameof(payer), payer);
        }

        public int Key => id;

        public bool IsNewlyCalced => status == 0;

        public bool IsSettled => status == STATUS_QUED;
    }
}