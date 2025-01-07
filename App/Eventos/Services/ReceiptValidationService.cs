using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Eventos.Common.Interfaces;

namespace Eventos.Services
{
    /// <summary>
    /// Servicio para validar recibos con el servidor de Apple.
    /// </summary>
    public class ReceiptValidationService : IReceiptValidationService
    {
        // Cambia esta URL a la de producción en el entorno real.
        private readonly string receiptValidationUrl = "https://sandbox.itunes.apple.com/verifyReceipt";

        /// <summary>
        /// Valida un recibo con el servidor de Apple.
        /// </summary>
        /// <param name="receiptData">El recibo codificado en base64.</param>
        /// <returns>True si la suscripción está activa; false en caso contrario.</returns>
        public async Task<bool> ValidateReceiptAsync(string receiptData)
        {
            // El payload que enviaremos al servidor de Apple.
            var payload = new
            {
                receipt_data = receiptData, // Recibo codificado en base64.
                password = "b6fbf8ac69cb49fda2fa244a5808fe04" // Clave compartida de App Store Connect.
            };

            // Serializar el payload en JSON.
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(receiptValidationUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Deserializamos la respuesta de Apple.
                var responseData = await response.Content.ReadAsStringAsync();
                var receiptResponse = JsonSerializer.Deserialize<ReceiptValidationResponse>(responseData);

                // Si el estado es 0, significa que la validación fue exitosa.
                if (receiptResponse?.Status == 0)
                {
                    // Verificamos la fecha de expiración del recibo.
                    var latestReceipt = receiptResponse?.LatestReceiptInfo?.FirstOrDefault();
                    if (latestReceipt != null && DateTime.TryParse(latestReceipt.ExpirationDate, out var expirationDate))
                    {
                        return expirationDate > DateTime.UtcNow; // Retornamos true si la suscripción sigue activa.
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Modelo para deserializar la respuesta de validación de Apple.
        /// </summary>
        public class ReceiptValidationResponse
        {
            public int Status { get; set; }
            public List<ReceiptInfo>? LatestReceiptInfo { get; set; }
        }

        /// <summary>
        /// Modelo para la información de los recibos.
        /// </summary>
        public class ReceiptInfo
        {
            [JsonPropertyName("expires_date")]
            public string? ExpirationDate { get; set; }
        }
    }
}
