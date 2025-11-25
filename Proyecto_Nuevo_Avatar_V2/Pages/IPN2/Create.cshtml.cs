using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.IPN2
{
    public class CreateModel : PageModel
    {
        private readonly IPagoApiClient _pagosService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILoginApiClient _loginApiClient;
        public CreateModel(IPagoApiClient pagosService, IWebHostEnvironment environment, ILoginApiClient loginApiClient)
        {
            _pagosService = pagosService;
            _environment = environment;
            _loginApiClient = loginApiClient;
        }
        [BindProperty]
        public decimal? MontoTotal { get; set; }

        [BindProperty]
        public long NumeroFacturaBuscado { get; set; }

        public string ModalTitle { get; set; }
        public string ModalMessage { get; set; }
        public string ModalType { get; set; }

        #region "Validar Token"

        private async Task<string?> GetValidAccessTokenAsync()
        {
            var accessToken = HttpContext.Session.GetString("AccessToken");
            var refreshToken = HttpContext.Session.GetString("RefreshToken");

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return null;

            bool valido = await _loginApiClient.ValidateTokenAsync(accessToken);
            if (!valido)
            {
                var nuevoToken = await _loginApiClient.RefreshTokenAsync(refreshToken);
                if (nuevoToken != null)
                {
                    HttpContext.Session.SetString("AccessToken", nuevoToken.Access_Token);
                    HttpContext.Session.SetString("RefreshToken", nuevoToken.Refresh_Token);
                    HttpContext.Session.SetString("Expiresin", nuevoToken.Expires_In.ToString());
                    accessToken = nuevoToken.Access_Token;
                }
                else
                {
                    return null;
                }
            }

            return accessToken;
        }

        #endregion

        public IActionResult OnGet()
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var accessToken = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostBuscarFacturaAsync(long NumeroFactura)
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            try
            {
                var montoTotal = await _pagosService.BuscarFacturaParaPagoAsync(token, NumeroFactura);

                if (montoTotal.HasValue)
                {
                    MontoTotal = montoTotal.Value;
                    NumeroFacturaBuscado = NumeroFactura;
                }
                else
                {
                    ModalTitle = "No encontrada";
                    ModalMessage = $"No se encontró la factura #{NumeroFactura}";
                    ModalType = "error";
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Anulada"))
                {
                    ModalTitle = "Factura Anulada";
                    ModalMessage = "La factura se encuentra en estado Anulada y no puede ser pagada";
                    ModalType = "error";
                }
                else if (ex.Message.Contains("Cancelada"))
                {
                    ModalTitle = "Factura Cancelada";
                    ModalMessage = "La factura ya fue pagada";
                    ModalType = "error";
                }
                else if (ex.Message.Contains("no existe"))
                {
                    ModalTitle = "Factura no existente";
                    ModalMessage = "La factura no existe";
                    ModalType = "error";
                }
                else
                {
                    ModalTitle = "Error";
                    ModalMessage = ex.Message;
                    ModalType = "error";
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRegistrarPagoAsync(long NumeroFactura, decimal MontoPago, bool GenerarComprobante = false)
        {
     
            NumeroFacturaBuscado = NumeroFactura;

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {
                try
                {
                    MontoTotal = await _pagosService.BuscarFacturaParaPagoAsync(token, NumeroFactura);
                }
                catch { }
                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }


            if (string.IsNullOrEmpty(token))
            {
                ModalTitle = "Sin sesión";
                ModalMessage = "Tu sesión ha expirado. Por favor inicia sesión nuevamente.";
                ModalType = "error";
                return Page();
            }

            try
            {
                string rutaComprobante = null;

                if (GenerarComprobante)
                {
                    try
                    {
                        var pdfBytes = ComprobantePagoPdf.GenerarComprobante(null, NumeroFactura, MontoPago);
                        rutaComprobante = await GuardarPDF(pdfBytes, NumeroFactura);
                    }
                    catch (Exception pdfEx)
                    {
                        ModalTitle = "Error al generar PDF";
                        ModalMessage = $"No se pudo generar el comprobante: {pdfEx.Message}";
                        ModalType = "error";
                        return Page();
                    }
                }

                var pago = await _pagosService.CrearPagoFacturaAsync(token, NumeroFactura, MontoPago, rutaComprobante);

                if (pago != null)
                {
                    ModalTitle = "Pago registrado";
                    ModalMessage = $"Pago registrado exitosamente.<br/>Número de Pago: <strong>#{pago.Numero_Pago}</strong>";
                    ModalType = "success";
                    return Page();
                }
                else
                {
                    ModalTitle = "Error";
                    ModalMessage = "No se pudo registrar el pago";
                    ModalType = "error";
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                if (errorMessage.Contains("{\"mensaje\""))
                {
                    try
                    {
                        int startIndex = errorMessage.IndexOf("{\"mensaje\"");
                        int endIndex = errorMessage.IndexOf("}", startIndex) + 1;
                        string jsonPart = errorMessage.Substring(startIndex, endIndex - startIndex);

                        using (var doc = System.Text.Json.JsonDocument.Parse(jsonPart))
                        {
                            errorMessage = doc.RootElement.GetProperty("mensaje").GetString();
                        }
                    }
                    catch { }
                }

                ModalTitle = "Error al registrar pago";
                ModalMessage = errorMessage;
                ModalType = "error";
            }

            return Page();
        }

        private async Task<string> GuardarPDF(byte[] pdfBytes, long numeroFactura)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "comprobantes");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = $"Comprobante_Factura_{numeroFactura}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(uploadsFolder, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            return $"/uploads/comprobantes/{fileName}";
        }
    }
}
