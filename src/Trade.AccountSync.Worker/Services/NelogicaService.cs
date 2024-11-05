namespace Warren.Trade.Risk.ClientV2.Services
{
    public abstract class NelogicaService
    {
        protected string? NelogicaToken { get; set; }
        protected const string NelogicaTokenPathKey = "nelogica:AccessToken";
    }
}
