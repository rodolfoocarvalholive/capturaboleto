using Refit;
using System.Threading.Tasks;
using System.Net.Http;

namespace CapturaBoleto.Api.Services
{
    public interface IPublicApiService
    {
        // Exemplo: Consumir a API pública de cat facts
        [Get("/api/credentials")]
        Task<List<MallCredential>> GetMallCredentialAsync();

        // Novo método para capturar o conteúdo bruto
        [Get("/api/credentials")]
        Task<HttpResponseMessage> GetMallCredentialRawAsync();
    }
    public class MallCredential
    {
        public string Id { get; set; }
        public string NameMall { get; set; }
        public string Cnpj { get; set; }
        public string UrlPortal { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool Active { get; set; }
    }
}
