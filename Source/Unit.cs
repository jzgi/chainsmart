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
        { "只", 0 },
        { "根", 0 },
        { "份", 0 },
        { "包", 0 },
        { "瓶", 0 },
        { "桶", 0 },
    };

    public static int Convert(string u1, int v, string u2, short pieceful)
    {
        return 0;
    }
}