using System.Collections.Concurrent;

namespace ChainMart
{
    public class BookMsg
    {
        private ConcurrentDictionary<int, Org> orgs = null;
    }
}