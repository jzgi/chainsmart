namespace CoSupply
{
    public static class Global
    {
#if ZHNT
        public const string MRT = "市场";
        public const string BIZ = "商户";
#else
        public const string MRT = "城市";
        public const string BIZ = "驿站";
#endif
    }
}