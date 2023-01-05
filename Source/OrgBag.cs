using System.Collections.Concurrent;
using ChainFx;

namespace ChainMart
{
    public class OrgBag : IKeyable<int>
    {
        public const short 
            TYP_NEW = 1;
        
        int orgid;
        
        string orgname;
        
        


        public int Key { get; }
    }
}