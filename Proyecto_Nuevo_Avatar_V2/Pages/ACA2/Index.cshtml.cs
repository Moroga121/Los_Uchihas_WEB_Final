using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Proyecto_Nuevo_Avatar_V2.Pages.ACA2
{
    public class IndexModel : PageModel
    {
        private readonly IListadoPeriodoApiClient _listadoService;
        private readonly ILoginApiClient _loginApiClient;
        private readonly IConfiguration _configuration;

        public IndexModel(IListadoPeriodoApiClient listadoService, ILoginApiClient loginApiClient, IConfiguration configuration)
        {
            _listadoService = listadoService;
            _loginApiClient = loginApiClient;
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public string Periodo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FiltroCarrera { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FiltroCurso { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FiltroGrupo { get; set; }

        public List<ListadoPeriodoDto> Estudiantes { get; set; }
        public bool MostrarMensaje { get; set; }
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

            Estudiantes = new List<ListadoPeriodoDto>();
            MostrarMensaje = false;

            await CargarPeriodosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostBuscarAsync(int numeroPagina = 1)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(Periodo))
            {
                Estudiantes = new List<ListadoPeriodoDto>();
                MostrarMensaje = false;
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

                var estudiantes = await _listadoService.ObtenerMatriculadosAsync(token, Periodo)
                                 ?? new List<ListadoPeriodoDto>();


                if (!string.IsNullOrEmpty(FiltroCarrera))
                    estudiantes = estudiantes.Where(e => e.Carrera == FiltroCarrera).ToList();

                if (!string.IsNullOrEmpty(FiltroCurso))
                    estudiantes = estudiantes.Where(e => e.Curso == FiltroCurso).ToList();

                if (!string.IsNullOrEmpty(FiltroGrupo))
                    estudiantes = estudiantes.Where(e => e.Grupo == FiltroGrupo).ToList();

                TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");
                TotalRegistros = estudiantes.Count;
                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
                if (TotalPaginas <= 0) TotalPaginas = 1;

                PaginaActual = 1;

                Estudiantes = estudiantes
                    .Skip((PaginaActual - 1) * TamanoPagina)
                    .Take(TamanoPagina)
                    .ToList();

                MostrarMensaje = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Estudiantes = new List<ListadoPeriodoDto>();
                MostrarMensaje = true;
                TotalRegistros = 0;
                TotalPaginas = 1;
                PaginaActual = 1;
            }

            await CargarPeriodosAsync();
            return Page();
        }



        public async Task<IActionResult> OnGetBuscarAsync(int numeroPagina = 1)
        {
            ViewData["Nombre"] = HttpContext.Session.GetString("Nombre");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
            ViewData["Rol"] = HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(Periodo))
            {
                Estudiantes = new List<ListadoPeriodoDto>();
                MostrarMensaje = false;
                await CargarPeriodosAsync();
                return Page();
            }

            try
            {
                var token = await GetValidAccessTokenAsync();
                if (token == null)
                {
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Login/Login");
                }

                var estudiantes = await _listadoService.ObtenerMatriculadosAsync(token, Periodo)
                                 ?? new List<ListadoPeriodoDto>();

                if (!string.IsNullOrEmpty(FiltroCarrera))
                    estudiantes = estudiantes.Where(e => e.Carrera == FiltroCarrera).ToList();

                if (!string.IsNullOrEmpty(FiltroCurso))
                    estudiantes = estudiantes.Where(e => e.Curso == FiltroCurso).ToList();

                if (!string.IsNullOrEmpty(FiltroGrupo))
                    estudiantes = estudiantes.Where(e => e.Grupo == FiltroGrupo).ToList();

                TamanoPagina = _configuration.GetValue<int>("Pagination:PageSize");
                TotalRegistros = estudiantes.Count;
                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
                if (TotalPaginas <= 0) TotalPaginas = 1;

                PaginaActual = numeroPagina < 1 ? 1 : numeroPagina;
                PaginaActual = PaginaActual > TotalPaginas ? TotalPaginas : PaginaActual;

                Estudiantes = estudiantes
                    .Skip((PaginaActual - 1) * TamanoPagina)
                    .Take(TamanoPagina)
                    .ToList();

                MostrarMensaje = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Estudiantes = new List<ListadoPeriodoDto>();
                MostrarMensaje = true;
                TotalRegistros = 0;
                TotalPaginas = 1;
                PaginaActual = 1;
            }

            await CargarPeriodosAsync();
            return Page();
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

                var periodosData = await _listadoService.ObtenerPeriodosAsync(token);

                Periodos = periodosData.Select(p => new SelectListItem
                {
                    Value = p,
                    Text = p
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar periodos: {ex.Message}");
                Periodos = new List<SelectListItem>();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostExportCSVAsync()
        {
            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            var estudiantes = await _listadoService.ObtenerMatriculadosAsync(token, Periodo);

            if (!string.IsNullOrEmpty(FiltroCarrera))
                estudiantes = estudiantes.Where(e => e.Carrera == FiltroCarrera).ToList();

            if (!string.IsNullOrEmpty(FiltroCurso))
                estudiantes = estudiantes.Where(e => e.Curso == FiltroCurso).ToList();

            if (!string.IsNullOrEmpty(FiltroGrupo))
                estudiantes = estudiantes.Where(e => e.Grupo == FiltroGrupo).ToList();

            var csv = new StringBuilder();
            csv.AppendLine($"Periodo: {Periodo}");
            csv.AppendLine();
            csv.AppendLine("Tipo ID,Número ID,Nombre Completo,Carrera,Curso,Grupo");

            if (estudiantes != null && estudiantes.Any())
            {
                foreach (var est in estudiantes)
                {
                    csv.AppendLine($"{est.Tipo_Identificacion},{est.Numero_Identificacion},{est.Nombre_Completo},{est.Carrera},{est.Curso},{est.Grupo}");
                }
            }

            // UTF-8para que Excel reconozca las tildes
            var preamble = Encoding.UTF8.GetPreamble();
            var content = Encoding.UTF8.GetBytes(csv.ToString());
            var bytesWithBom = preamble.Concat(content).ToArray();

            return File(bytesWithBom, "text/csv; charset=utf-8", $"ListadoEstudiantes_{Periodo}_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        public async Task<IActionResult> OnPostExportPDFAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var token = await GetValidAccessTokenAsync();
            if (token == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Login/Login");
            }

            var estudiantes = await _listadoService.ObtenerMatriculadosAsync(token, Periodo);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(FiltroCarrera))
                estudiantes = estudiantes.Where(e => e.Carrera == FiltroCarrera).ToList();

            if (!string.IsNullOrEmpty(FiltroCurso))
                estudiantes = estudiantes.Where(e => e.Curso == FiltroCurso).ToList();

            if (!string.IsNullOrEmpty(FiltroGrupo))
                estudiantes = estudiantes.Where(e => e.Grupo == FiltroGrupo).ToList();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("Listado de Estudiantes Matriculados")
                                .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                            column.Item().PaddingTop(5)
                                .Text($"Periodo: {Periodo}")
                                .FontSize(12).FontColor(Colors.Grey.Darken2);

                            if (!string.IsNullOrEmpty(FiltroCarrera) || !string.IsNullOrEmpty(FiltroCurso) || !string.IsNullOrEmpty(FiltroGrupo))
                            {
                                var filtros = new List<string>();
                                if (!string.IsNullOrEmpty(FiltroCarrera)) filtros.Add($"Carrera: {FiltroCarrera}");
                                if (!string.IsNullOrEmpty(FiltroCurso)) filtros.Add($"Curso: {FiltroCurso}");
                                if (!string.IsNullOrEmpty(FiltroGrupo)) filtros.Add($"Grupo: {FiltroGrupo}");

                                column.Item().PaddingTop(3)
                                    .Text($"Filtros: {string.Join(", ", filtros)}")
                                    .FontSize(10).Italic().FontColor(Colors.Grey.Darken1);
                            }
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            if (estudiantes != null && estudiantes.Any())
                            {
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1.5f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten2)
                                            .Padding(8).Text("Tipo ID").SemiBold();
                                        header.Cell().Background(Colors.Grey.Lighten2)
                                            .Padding(8).Text("Número ID").SemiBold();
                                        header.Cell().Background(Colors.Grey.Lighten2)
                                            .Padding(8).Text("Nombre Completo").SemiBold();
                                        header.Cell().Background(Colors.Grey.Lighten2)
                                            .Padding(8).Text("Carrera").SemiBold();
                                        header.Cell().Background(Colors.Grey.Lighten2)
                                            .Padding(8).Text("Curso").SemiBold();
                                        header.Cell().Background(Colors.Grey.Lighten2)
                                            .Padding(8).Text("Grupo").SemiBold();
                                    });

                                    foreach (var estudiante in estudiantes)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(5).Text(estudiante.Tipo_Identificacion);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(5).Text(estudiante.Numero_Identificacion);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(5).Text(estudiante.Nombre_Completo);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(5).Text(estudiante.Carrera);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(5).Text(estudiante.Curso);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .Padding(5).Text(estudiante.Grupo);
                                    }
                                });

                                column.Item().PaddingTop(20)
                                    .AlignRight()
                                    .Text($"Total: {estudiantes.Count}")
                                    .FontSize(11).SemiBold();
                            }
                            else
                            {
                                column.Item().Text("No se encontraron estudiantes con los filtros aplicados.")
                                    .FontSize(12).Italic().FontColor(Colors.Grey.Medium);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Generado el: ");
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                            text.Span(" | Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf();

            var fileName = $"ListadoEstudiantes_{Periodo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(pdf, "application/pdf", fileName);
        }
    }
}

