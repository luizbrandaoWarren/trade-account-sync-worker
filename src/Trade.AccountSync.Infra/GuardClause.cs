namespace Warren.Trade.Risk.Infra
{
    public static class GuardClause
    {
        public static bool IsNullOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}
