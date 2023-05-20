using ChainFx;

namespace ChainSmart;

public class Unit
{
    public static readonly Map<string, short> Typs = new()
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

    public static readonly Map<short, string> Metrics = new()
    {
        { 0, null },
        { 50, "１两" },
        { 100, "２两" },
        { 150, "３两" },
        { 200, "４两" },
        { 250, "５两" },
        { 300, "６两" },
        { 350, "７两" },
        { 400, "８两" },
        { 450, "９两" },
        { 500, "１斤" },
        { 1000, "１公斤" },
        { 2000, "２公斤" },
        { 3000, "３公斤" },
        { 4000, "４公斤" },
        { 5000, "５公斤" },
        { 6000, "６公斤" },
        { 7000, "７公斤" },
        { 8000, "８公斤" },
        { 9000, "９公斤" },
        { 10000, "１０公斤" },
        { 11000, "１１公斤" },
        { 12000, "１２公斤" },
        { 13000, "１３公斤" },
        { 14000, "１４公斤" },
        { 15000, "１５公斤" },
    };

    public static int Convert(string u1, int v, string u2, short pieceful)
    {
        return 0;
    }
}