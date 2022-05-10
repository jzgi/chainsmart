using System;
using Chainly;

namespace Revital
{
    public class BuyAct : IData
    {
        int bookid;
        short status;
        int orgid;
        string label;
        string tip;
        string uid;
        string uname;
        string utel;

        DateTime stamp;

        public void Read(ISource s, short msk = 255)
        {
        }

        public void Write(ISink s, short msk = 255)
        {
        }
    }
}