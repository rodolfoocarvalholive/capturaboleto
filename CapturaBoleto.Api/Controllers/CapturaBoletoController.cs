using CapturaBoleto.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CapturaBoleto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CapturaBoletoController : ControllerBase
    {
        private readonly IPublicApiService _publicApiService;
        private readonly IHttpClientFactory _httpClientFactory;

        public CapturaBoletoController(IPublicApiService publicApiService, IHttpClientFactory httpClientFactory)
        {
            _publicApiService = publicApiService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("credentials")]
        [ProducesResponseType(typeof(List<MallCredential>), 200)]
        public async Task<IActionResult> GetMallCredentials()
        {
            var result = await _publicApiService.GetMallCredentialAsync();
            return Ok(result);
        }

        [HttpGet("credentials/html")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> GetMallCredentialsHtml()
        {
            var response = await _publicApiService.GetMallCredentialRawAsync();
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, response.Content.Headers.ContentType?.ToString() ?? "text/plain");
        }

        [HttpGet("credentials/html-multiplan")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> GetMultiplanCredentialsHtml()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://canallojista.multiplan.com.br/login");
            var content = await response.Content.ReadAsStringAsync();

            // Carrega o HTML usando HtmlAgilityPack
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(content);

            // Aqui você pode manipular o HTML se desejar, por exemplo:
            // var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;
            // doc.DocumentNode.SelectSingleNode("//body").AppendChild(HtmlAgilityPack.HtmlNode.CreateNode("<div>Conteúdo injetado!</div>"));

            // Retorna o HTML processado
            return Content(doc.DocumentNode.OuterHtml, "text/html");
        }

        [HttpGet("credentials/html-multiplan-viewer")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult GetMultiplanCredentialsHtmlViewer()
        {
            var html = @"<!DOCTYPE html>
                        <html lang='pt-br'>
                        <head>
                            <meta charset='utf-8'>
                            <title>Visualizador Multiplan</title>
                            <style>body,html{margin:0;padding:0;height:100%;}iframe{border:none;width:100vw;height:100vh;}</style>
                        </head>
                        <body>
                            <iframe src='/api/capturaboleto/credentials/html-multiplan'></iframe>
                        </body>
                        </html>";
            return Content(html, "text/html");
        }

        [HttpPost("login-multiplan")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> LoginMultiplan([FromBody] MultiplanLoginRequest loginRequest)
        {
            var client = _httpClientFactory.CreateClient();
            var url = "https://api-g-p.multiplan.com.br/canallojista/loginmultiplan";
            var payload = new
            {
                operacao = "login",
                usuario = loginRequest.Usuario,
                senha = loginRequest.Senha
            };
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            // Headers obrigatórios
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            client.DefaultRequestHeaders.Add("Accept-Language", "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "9d934cf475764f61a8eb158939cdeae5");
            client.DefaultRequestHeaders.Add("Origin", "https://canallojista.multiplan.com.br");
            client.DefaultRequestHeaders.Add("Pragma", "no-cache");
            client.DefaultRequestHeaders.Add("Referer", "https://canallojista.multiplan.com.br/");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-site");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("cl_company", "SVO");
            client.DefaultRequestHeaders.Add("cl_contract", "0000020000487");
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Google Chrome\";v=\"138\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");

            var response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            return Content(responseBody, response.Content.Headers.ContentType?.ToString() ?? "application/json");
        }

        [HttpPost("boletos-cobranca")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> GetBoletosCobranca([FromBody] BoletosCobrancaRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var url = "https://api-g-p.multiplan.com.br/canallojista/boletoscobranca";

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.Token);

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("empresa", request.Empresa),
                new KeyValuePair<string, string>("contrato", request.Contrato),
                new KeyValuePair<string, string>("dtVencimento", request.DtVencimento ?? string.Empty),
                new KeyValuePair<string, string>("status", request.Status ?? string.Empty),
                new KeyValuePair<string, string>("isRescindidos", request.IsRescindidos?.ToString() ?? "0")
            };
            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Filtra apenas os itens com idSituacao == "ACC"
            using var doc = System.Text.Json.JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var filtered = root.EnumerateArray()
                    .Where(x => x.TryGetProperty("idSituacao", out var prop) && prop.GetString() == "ACC")
                    .ToList();
                return Content(System.Text.Json.JsonSerializer.Serialize(filtered), "application/json");
            }
            else if (root.TryGetProperty("boletos", out var boletos) && boletos.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var filtered = boletos.EnumerateArray()
                    .Where(x => x.TryGetProperty("idSituacao", out var prop) && prop.GetString() == "ACC")
                    .ToList();
                return Content(System.Text.Json.JsonSerializer.Serialize(filtered), "application/json");
            }
            // Se não for array, retorna o body original
            return Content(responseBody, response.Content.Headers.ContentType?.ToString() ?? "application/json");
        }

        [HttpPost("boletos-cobranca-arquivos")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> GetBoletosCobrancaArquivos([FromBody] BoletosCobrancaArquivosRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var url = "https://api-g-p.multiplan.com.br/canallojista/boletoscobranca";

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.Token);

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("operacao", "arquivos"),
                new KeyValuePair<string, string>("ids", request.Ids)
            };
            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            return Content(responseBody, response.Content.Headers.ContentType?.ToString() ?? "application/json");
        }
    }
}
