namespace CapturaBoleto.Api.Controllers
{
    public class BoletosCobrancaRequest
    {
        public string Token { get; set; }
        public string Empresa { get; set; }
        public string Contrato { get; set; }
        public string DtVencimento { get; set; }
        public string Status { get; set; }
        public int? IsRescindidos { get; set; }
    }
}
