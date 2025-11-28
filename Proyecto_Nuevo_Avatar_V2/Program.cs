using Proyecto_Nuevo_Avatar_V2.Entities;
using Proyecto_Nuevo_Avatar_V2.Services;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Add services to the container.
builder.Services.AddRazorPages();
// Contraseña Parametrizable

builder.Services.Configure<ContrasenaSettings>(
    builder.Configuration.GetSection("PasswordPolicy"));

// Cliente Login

builder.Services.AddHttpClient<ILoginApiClient, LoginApiClient>(client =>
{
    var baseUrl = builder.Configuration["LoginApi:BaseUrl"]
                  ?? throw new InvalidOperationException("LoginApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente Usuarios

builder.Services.AddHttpClient<IUsuarioApiClient, UsuarioApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuarioApi:BaseUrl"]
                  ?? throw new InvalidOperationException("UsuarioApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});
// Cliente para PARÁMETROS

builder.Services.AddHttpClient<IParametrosApiClient, ParametrosApiClient>(client =>
{
    var baseUrl = builder.Configuration["ParametrosApi:BaseUrl"]
                  ?? throw new InvalidOperationException("ParametrosApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});
// Cliente para BITÁCORA
builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(client =>
{
    var baseUrl = builder.Configuration["BitacoraApi:BaseUrl"]
                  ?? throw new InvalidOperationException("BitacoraApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente para Roles

builder.Services.AddHttpClient<IRolesApiClient, RolesApiClient>(client =>
{
    var baseUrl = builder.Configuration["RolesApi:BaseUrl"]
                  ?? throw new InvalidOperationException("RolesApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente para Módulos
builder.Services.AddHttpClient<IModuloApiClient, ModuloApiClient>(client =>
{
    var baseUrl = builder.Configuration["ModulosApi:BaseUrl"]
                  ?? throw new InvalidOperationException("ModulosApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});


// Cliente para Notificaciones

builder.Services.AddHttpClient<INotificacionApiClient, NotificacionApiClient>(client =>
{
    var baseUrl = builder.Configuration["NotificacionesApi:BaseUrl"]
                  ?? throw new InvalidOperationException("ModulosApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Promedios
builder.Services.AddHttpClient<INotasAPIClient, NotasAPIClient>(client =>
{
    var baseUrl = builder.Configuration["PromedioApi:BaseUrl"]
                  ?? throw new InvalidOperationException("PromedioApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Listado Estudiantes Periodo
builder.Services.AddHttpClient<IListadoPeriodoApiClient, ListadoPeriodoApiClient>(client =>
{
    var baseUrl = builder.Configuration["ListadoApi:BaseUrl"]
                  ?? throw new InvalidOperationException("ListadoApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Facturas
builder.Services.AddHttpClient<IFacturaApiClient, FacturaApiClient>(client =>
{
    var baseUrl = builder.Configuration["FacturaApi:BaseUrl"]
                  ?? throw new InvalidOperationException("FacturaApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Pagos
builder.Services.AddHttpClient<IPagoApiClient, PagoApiClient>(client =>
{
    var baseUrl = builder.Configuration["PagoApi:BaseUrl"]
                  ?? throw new InvalidOperationException("PagoApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente para Instituciones
builder.Services.AddHttpClient<IInstitucionApiClient, InstitucionApiClient>(client =>
{
    var baseUrl = builder.Configuration["InstitucionApi:BaseUrl"]
                  ?? throw new InvalidOperationException("InstitucionApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente para Carreras
builder.Services.AddHttpClient<ICarreraApiClient, CarreraApiClient>(client =>
{
    var baseUrl = builder.Configuration["CarreraApi:BaseUrl"]
                  ?? throw new InvalidOperationException("CarreraApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente para Profesores
builder.Services.AddHttpClient<IProfesorApiClient, ProfesorApiClient>(client =>
{
    var baseUrl = builder.Configuration["ProfesorApi:BaseUrl"]
                  ?? throw new InvalidOperationException("ProfesorApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente para Cursos
builder.Services.AddHttpClient<ICursoApiClient, CursoApiClient>(client =>
{
    var baseUrl = builder.Configuration["CursoApi:BaseUrl"]
                  ?? throw new InvalidOperationException("CursoApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente para Periodos
builder.Services.AddHttpClient<IPeriodoApiClient, PeriodoApiClient>(client =>
{
    var baseUrl = builder.Configuration["PeriodoApi:BaseUrl"]
                  ?? throw new InvalidOperationException("PeriodoApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente para Grupos
builder.Services.AddHttpClient<IGrupoApiClient, GrupoApiClient>(client =>
{
    var baseUrl = builder.Configuration["GrupoApi:BaseUrl"]
                  ?? throw new InvalidOperationException("GrupoApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente para Prematriculas
builder.Services.AddHttpClient<IPrematriculaApiClient, PrematriculaApiClient>(client =>
{
    var baseUrl = builder.Configuration["PrematriculaApi:BaseUrl"]
                  ?? throw new InvalidOperationException("PrematriculaApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});


// Cliente para Expedientes
builder.Services.AddHttpClient<IExpedientesApiClient, ExpedienteApiClient>(client =>
{
    var baseUrl = builder.Configuration["ExpedienteApi:BaseUrl"]
                  ?? throw new InvalidOperationException("ExpedienteApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente para Direcciones
builder.Services.AddHttpClient<IDireccionesApiClient, DireccionesApiClient>(client =>
{
    var baseUrl = builder.Configuration["DireccionApi:BaseUrl"]
                  ?? throw new InvalidOperationException("DireccionApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseSession();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Login/Login");
    return Task.CompletedTask;
});

app.Run();
