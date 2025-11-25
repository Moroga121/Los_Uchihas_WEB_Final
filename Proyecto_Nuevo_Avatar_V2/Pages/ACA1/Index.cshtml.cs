using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ACA1
{
    public class IndexModel : PageModel
    {
        private readonly INotasAPIClient _notasService;
        private readonly IConfiguration _configuration;
        private readonly ILoginApiClient _loginApiClient;

        public IndexModel(INotasAPIClient notasService, IConfiguration configuration, ILoginApiClient loginApiClient)
        {
            _notasService = notasService;
            _configuration = configuration;
            _loginApiClient = loginApiClient;
        }

        [BindProperty]
        [Required(ErrorMessage = "El Tipo de Identificación es obligatorio.")]
        public string TipoIdentificacion { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La Identificación es obligatoria.")]
        public string NumeroIdentificacion { get; set; }

        [BindProperty]
        public int? Año { get; set; }

        [BindProperty]
        public string? Periodo { get; set; }

        public List<NotasDto> Notas { get; set; } = new();

        public List<SelectListItem> Periodos { get; set; } = new();

        public int TamanoPagina { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

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
        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {

                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            Notas = new List<NotasDto>();
            await CargarPeriodosAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetBuscarAsync(string tipoIdentificacion, string numeroIdentificacion, int? año, string? periodo, int numeroPagina = 1)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            // Rellenar propiedades para mantener valores en la UI
            TipoIdentificacion = tipoIdentificacion;
            NumeroIdentificacion = numeroIdentificacion;
            Año = año;
            Periodo = periodo;

            if (string.IsNullOrWhiteSpace(TipoIdentificacion) || string.IsNullOrWhiteSpace(NumeroIdentificacion))
            {
                await CargarPeriodosAsync();
                Notas = new List<NotasDto>();
                return Page();
            }

            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            try
            {
                var token = await GetValidAccessTokenAsync();
                if (token == null)
                {
                    await CargarPeriodosAsync();
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Login/Login");
                }

                var notas = await _notasService.ObtenerPromedio(
                    TipoIdentificacion,
                    NumeroIdentificacion,
                    token,
                    Año,
                    Periodo
                ) ?? new List<NotasDto>();

                notas = notas.OrderBy(n => n.Nombre_Curso).ToList();

                // Paginación
                TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");
                TotalRegistros = notas.Count;
                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
                if (TotalPaginas <= 0) TotalPaginas = 1;

                PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
                PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

                Notas = notas
                    .Skip((PaginaActual - 1) * TamanoPagina)
                    .Take(TamanoPagina)
                    .ToList();
            }
            catch
            {
                Notas = new List<NotasDto>();
                TotalRegistros = 0;
                TotalPaginas = 1;
                PaginaActual = 1;
            }

            await CargarPeriodosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostBuscarAsync(int numeroPagina = 1)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            if (!ModelState.IsValid)
            {
                await CargarPeriodosAsync();
                return Page();
            }

            try
            {
                var token = await GetValidAccessTokenAsync();
                if (token == null)
                {
                    await CargarPeriodosAsync();
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Login/Login");
                }

                var notas = await _notasService.ObtenerPromedio(
                    TipoIdentificacion,
                    NumeroIdentificacion,
                    token,
                    Año,
                    Periodo
                ) ?? new List<NotasDto>();

                notas = notas.OrderBy(n => n.Nombre_Curso).ToList();

                // Paginación
                TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");
                TotalRegistros = notas.Count;
                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
                if (TotalPaginas <= 0) TotalPaginas = 1;

                PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
                PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

                Notas = notas
                    .Skip((PaginaActual - 1) * TamanoPagina)
                    .Take(TamanoPagina)
                    .ToList();
            }
            catch
            {
                Notas = new List<NotasDto>();
                TotalRegistros = 0;
                TotalPaginas = 1;
                PaginaActual = 1;
            }

            await CargarPeriodosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostExportCSVAsync()
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {
                await CargarPeriodosAsync();
                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            var notas = await _notasService.ObtenerPromedio(
                TipoIdentificacion,
                NumeroIdentificacion,
                token,
                Año,
                Periodo
            ) ?? new List<NotasDto>();

            var csv = new StringBuilder();
            csv.AppendLine($"{TipoIdentificacion},{NumeroIdentificacion},{(Año.HasValue ? Año.ToString() : "-")},{(!string.IsNullOrEmpty(Periodo) ? Periodo : "-")}");
            csv.AppendLine();
            csv.AppendLine("Código de Curso,Nombre del Curso,Promedio");

            foreach (var nota in notas)
                csv.AppendLine($"{nota.Codigo_Curso},{nota.Nombre_Curso},{nota.Promedio:F2}");

            var preamble = Encoding.UTF8.GetPreamble();
            var content = Encoding.UTF8.GetBytes(csv.ToString());
            var bytesWithBom = preamble.Concat(content).ToArray();

            var fileName = $"Promedios_{NumeroIdentificacion}_{DateTime.Now:yyyyMMddHHmmss}.csv";
            return File(bytesWithBom, "text/csv; charset=utf-8", fileName);
        }

        public async Task<IActionResult> OnPostExportPDFAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {
                await CargarPeriodosAsync();
                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            var notas = await _notasService.ObtenerPromedio(
                TipoIdentificacion,
                NumeroIdentificacion,
                token,
                Año,
                Periodo
            ) ?? new List<NotasDto>();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text("Reporte de Promedios Académicos").SemiBold().FontSize(20);

                    page.Content().Column(x =>
                    {
                        x.Item().Text($"Tipo de Identificación: {TipoIdentificacion}");
                        x.Item().Text($"Número de Identificación: {NumeroIdentificacion}");
                        x.Item().Text($"Año: {(Año.HasValue ? Año.ToString() : "-")}");
                        x.Item().Text($"Periodo: {(!string.IsNullOrEmpty(Periodo) ? Periodo : "-")}");

                        x.Item().PaddingTop(20);

                        if (notas.Any())
                        {
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Padding(5).Text("Código").SemiBold();
                                    header.Cell().Padding(5).Text("Nombre del Curso").SemiBold();
                                    header.Cell().Padding(5).Text("Promedio").SemiBold();
                                });

                                foreach (var n in notas)
                                {
                                    table.Cell().Padding(5).Text(n.Codigo_Curso);
                                    table.Cell().Padding(5).Text(n.Nombre_Curso);
                                    table.Cell().Padding(5).Text(n.Promedio.ToString("F2"));
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            }).GeneratePdf();

            var fileName = $"Promedios_{NumeroIdentificacion}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        private async Task<IActionResult> CargarPeriodosAsync()
        {
            try
            {
                var token = await GetValidAccessTokenAsync();
                if (token == null)
                {
                    Periodos = new List<SelectListItem>();
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Login/Login");
                }

                var periodosData = await _notasService.ObtenerPeriodosAsync(token);
                Periodos = periodosData.Select(p => new SelectListItem { Value = p, Text = p }).ToList();
            }
            catch
            {
                Periodos = new List<SelectListItem>();
            }
            return Page();
        }
    }
}
