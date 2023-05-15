using ChainFx;

namespace ChainSmart;

public class Unit
{
    public static Map<string, short> Typs = new()
    {
        { "两", 50 },
        { "斤", 500 },
        { "公斤", 1000 },
        { "个", 0 },
        { "毫升", 1 },
        { "升", 1000 },
    };

    public static int Convert(string u1, int v, string u2, short pieceful)
    {
        return 0;
    }
}