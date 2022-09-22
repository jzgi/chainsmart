using System;

namespace ChainMart
{
    public interface IFlowable
    {
        string Oker { get; }

        DateTime Oked { get; }

        short State { get; }
    }
}