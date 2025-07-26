namespace CapturaBoleto.Api.Controllers
{
    public class BoletosCobrancaArquivosRequest
    {
        public string Token { get; set; }
        public string Ids { get; set; } // pode ser um id ou ids separados por vírgula
    }
}
