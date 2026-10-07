using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TICKETSAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WhatsAppController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpPost("ultramsgSend")]
        public async Task<IActionResult> SendWhatsAppMessageultramsg([FromBody] WhatsAppRequestDto dto)
        {
            try
            {
                var instance = _configuration["UltraMsg:InstanceID"];
                var token = _configuration["UltraMsg:Token"];

                if (string.IsNullOrEmpty(instance) || string.IsNullOrEmpty(token))
                {
                    return StatusCode(500, new { Success = false, Error = "Configuración de UltraMsg incompleta en appsettings.json" });
                }

                var url = $"https://api.ultramsg.com/{instance}/messages/chat";

                var formData = new Dictionary<string, string>
                    {
                        { "token", token },
                        { "to", dto.ToNumber },
                        { "body", dto.Message }
                    };

                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        Success = false,
                        Error = string.IsNullOrWhiteSpace(responseContent) ? "Error al comunicarse con UltraMsg" : responseContent
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Response = responseContent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }

    public class WhatsAppRequestDto
    {
        /// <summary>
        /// Número de teléfono en formato E.164 (ej: +5215512345678)
        /// </summary>
        [Required]
        public string ToNumber { get; set; } = string.Empty;

        /// <summary>
        /// Texto del mensaje a enviar
        /// </summary>
        [Required]
        [StringLength(1600, ErrorMessage = "El mensaje es demasiado largo.")]
        public string Message { get; set; } = string.Empty;
    }
}
