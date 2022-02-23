using SkyChain;
using SkyChain.Web;

namespace Revital
{
    public abstract class ItemVarWork : WebWork
    {
        const int PIC_MAX_AGE = 3600 * 24;

        public void icon(WebContext wc, int forced = 0)
        {
            short id = wc[this];
            using var dc = NewDbContext();
            if (dc.QueryTop("SELECT icon FROM items WHERE id = @1", p => p.Set(id)))
            {
                dc.Let(out byte[] bytes);
                if (bytes == null) wc.Give(204); // no content 
                else wc.Give(200, new StaticContent(bytes), shared: (forced == 0) ? true : (bool?) null, PIC_MAX_AGE);
            }
            else
                wc.Give(404, shared: true, maxage: 3600 * 24); // not found
        }
    }
}