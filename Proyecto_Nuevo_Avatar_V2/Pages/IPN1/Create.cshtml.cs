using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Nuevo_Avatar_V2.Services;

namespace Proyecto_Nuevo_Avatar_V2.Pages.IPN1
{
    public class CreateModel : PageModel
    {
        private readonly IFacturaApiClient _facturaService;
        private readonly ILoginApiClient _loginApiClient;
        public CreateModel(IFacturaApiClient facturaService, ILoginApiClient loginApiClient)
        {
            _facturaService = facturaService;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        [Required(ErrorMessage = "La identificación es obligatoria.")]
        public string NumeroIdentificacion { get; set; }

        public string PeriodoActivo { get; set; }
        public int TotalCursos { get; set; }
        public bool MostrarInfo { get; set; }

        [TempData]
        public string ModalTitle { get; set; }

        [TempData]
        public string ModalMessage { get; set; }

        [TempData]
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

            MostrarInfo = false;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            MostrarInfo = false;

            if (string.IsNullOrWhiteSpace(NumeroIdentificacion))
            {
                ModalTitle = "Error";
                ModalMessage = "Debe ingresar un número de identificación";
                ModalType = "error";
                return RedirectToPage();
            }

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            try
            {

                var periodos = await _facturaService.ObtenerPeriodosAsync(token);

                if (!periodos.Any())
                {
                    ModalTitle = "Error";
                    ModalMessage = "No se encontraron periodos disponibles";
                    ModalType = "error";
                    return RedirectToPage();
                }

                var añoActual = DateTime.Now.Year;
                var periodoActivo = periodos
                    .Where(p => !string.IsNullOrEmpty(p.Estado)
                             && p.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase)
                             && p.Año == añoActual)
                    .FirstOrDefault();

                if (periodoActivo == null)
                {
                    ModalTitle = "Error";
                    ModalMessage = $"No se encontró un periodo activo para el año {añoActual}";
                    ModalType = "error";
                    return RedirectToPage();
                }

                var idPeriodoActivo = periodoActivo.ID_Periodo;

                var matriculas = await _facturaService.ObtenerMatriculasAsync(token);

                if (!matriculas.Any())
                {
                    ModalTitle = "Error";
                    ModalMessage = "No se encontraron matrículas registradas en el sistema";
                    ModalType = "error";
                    return RedirectToPage();
                }

                var matriculasEstudiante = matriculas
                    .Where(m => m.Numero_Identificacion.Equals(NumeroIdentificacion.Trim(), StringComparison.OrdinalIgnoreCase)
                             && m.Id_Periodo.Equals(idPeriodoActivo, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!matriculasEstudiante.Any())
                {
                    ModalTitle = "Sin Matrículas";
                    ModalMessage = $"El estudiante con identificación <strong>{NumeroIdentificacion}</strong> no tiene cursos matriculados en el periodo activo actual <strong>{idPeriodoActivo}</strong> ({periodoActivo.Año})";
                    ModalType = "error";
                    return RedirectToPage();
                }

                var totalCursos = matriculasEstudiante
                    .Select(m => m.Curso)
                    .Distinct()
                    .Count();

                var facturaCreada = await _facturaService.CrearFacturaAsync(
                    token,
                    NumeroIdentificacion.Trim(),
                    idPeriodoActivo,
                    totalCursos
                );

                if (facturaCreada == null)
                {
                    ModalTitle = "Error";
                    ModalMessage = "Este estudiante ya cuanta con una factura pendiente para el periodo activo actual.";
                    ModalType = "error";
                    return RedirectToPage();
                }

                var detalles = await _facturaService.ObtenerDetalleFacturaAsync(token, facturaCreada.Numero_Factura);

                var sb = new StringBuilder();
                sb.AppendLine($"<strong>Factura #:</strong> {facturaCreada.Numero_Factura}<br>");
                sb.AppendLine($"<strong>Estudiante:</strong> {HtmlEncoder.Default.Encode(NumeroIdentificacion)}<br>");
                sb.AppendLine($"<strong>Periodo:</strong> {HtmlEncoder.Default.Encode(idPeriodoActivo)}<br>");
                sb.AppendLine("<hr>");
                sb.AppendLine("<h6 class='mb-3'>Servicios estudiantiles</h6>");

                if (detalles != null && detalles.Any())
                {
                    sb.AppendLine("<div class='table-responsive'>");
                    sb.AppendLine("<table class='table table-striped align-middle w-100'>");
                    sb.AppendLine("<thead><tr>");
                    sb.AppendLine("<th style='width:50%'>Curso</th>");
                    sb.AppendLine("<th class='text-end' style='width:50%'>Monto Base</th>");
                    sb.AppendLine("</tr></thead>");
                    sb.AppendLine("<tbody>");

                    foreach (var d in detalles)
                    {
                        var curso = HtmlEncoder.Default.Encode(d.Nombre_Curso ?? string.Empty);
                        sb.AppendLine("<tr>");
                        sb.AppendLine($"<td>{curso}</td>");
                        sb.AppendLine($"<td class='text-end'>{d.MontoBase:N2}</td>");
                        sb.AppendLine("</tr>");
                    }

                    sb.AppendLine("</tbody>");


                    var totalBase = detalles.Sum(x => x.MontoBase);
                    var totalIVA = detalles.Sum(x => x.IVA);
                    var totalGeneral = detalles.Sum(x => x.MontoTotal);

                    sb.AppendLine("<tfoot>");
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td colspan='2'><hr class='my-2' /></td>");
                    sb.AppendLine("</tr>");

                    sb.AppendLine("<tr class='fw-bold'>");
                    sb.AppendLine("<td class='text-end'>Total Base:</td>");
                    sb.AppendLine($"<td class='text-end text-success'>{totalBase:N2}</td>");
                    sb.AppendLine("</tr>");

                    sb.AppendLine("<tr class='fw-bold'>");
                    sb.AppendLine("<td class='text-end'>Total IVA:</td>");
                    sb.AppendLine($"<td class='text-end text-success'>{totalIVA:N2}</td>");
                    sb.AppendLine("</tr>");

                    sb.AppendLine("<tr class='fw-bold'>");
                    sb.AppendLine("<td class='text-end'>Total general:</td>");
                    sb.AppendLine($"<td class='text-end text-success'>{totalGeneral:N2}</td>");
                    sb.AppendLine("</tr>");

                    sb.AppendLine("</tfoot>");
                    sb.AppendLine("</table>");
                    sb.AppendLine("</div>");

                }
                else
                {
                    sb.AppendLine("<div class='text-muted'>No hay detalle disponible.</div>");
                }


                PeriodoActivo = $"{idPeriodoActivo} ({periodoActivo.Año})";
                TotalCursos = totalCursos;
                MostrarInfo = true;

                ModalTitle = "Factura Generada";
                ModalMessage = sb.ToString();
                ModalType = "success";

                return Page();
            }
            catch (Exception ex)
            {
                ModalTitle = "Error";
                ModalMessage = $"Error al procesar la solicitud: {ex.Message}";
                ModalType = "error";
                return Page();
            }
        }

    }
}