using ChainFx.Web;

namespace ChainSmart;

public abstract class TwinWork<V> : WebWork where V : TwinVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }
}

[Ui("智能物联")]
public class MktlyTwinWork : TwinWork<MktlyTwinVarWork>
{
}

[Ui("智能物联")]
public class CtrlyTwinWork : TwinWork<CtrlyTwinVarWork>
{
}