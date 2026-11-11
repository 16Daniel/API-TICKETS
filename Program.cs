using DashboardApi.Mail;
using DashboardApi.ModelsBD1;
using DashboardApi.ModelsBD2;
using DashboardApi.ModelsDBRebel;
using Microsoft.EntityFrameworkCore;
using Quartz;
using TICKETSAPI.Funciones;
using TICKETSAPI.ModelsTickets;
using TICKETSAPI.Jobs;
using TICKETSAPI.ModelsBD2Prueba;
using TICKETSAPI.Repositories;
using Microsoft.OpenApi.Models;
using TICKETSAPI.Middlewares;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();

var TicketsConnection = builder.Configuration.GetConnectionString("TicketsConnection");
var RebelWingsConnection = builder.Configuration.GetConnectionString("RebelWingsConnection");
var DB1Connection = builder.Configuration.GetConnectionString("DB1Connection");
var DB2Connection = builder.Configuration.GetConnectionString("DB2Connection");
var DB2PruebaConnection = builder.Configuration.GetConnectionString("DB2PruebaConnection");

builder.Services.AddDbContext<DBRebelContext>(options => options.UseSqlServer(RebelWingsConnection))
    .AddDbContext<BD1Context>(options => options.UseSqlServer(DB1Connection))
    .AddDbContext<BD2Context>(options => options.UseSqlServer(DB2Connection))
     .AddDbContext<BD2ContextPrueba>(options => options.UseSqlServer(DB2PruebaConnection))
     .AddDbContext<TicketsContext>(options => options.UseSqlServer(TicketsConnection));


builder.Services.AddCors(policyBuilder =>
    policyBuilder.AddDefaultPolicy(policy =>
        policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod())
);


// 1. Obtener rutas y credenciales del appsettings.json
string projectId = builder.Configuration["Firebase:ProjectId"]!;
string credentialsPath = builder.Configuration["Firebase:CredentialsPath"]!;
string fullCredentialsPath = Path.Combine(builder.Environment.ContentRootPath, credentialsPath);

GoogleCredential credential = GoogleCredential.FromFile(fullCredentialsPath);

// 2. Inicializar Firebase Admin SDK (Requerido para Cloud Messaging / Notificaciones Push)
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = credential,
        ProjectId = projectId
    });
}

// 3. Registrar FirestoreDb (Tu configuración actual reutilizando la credencial)
builder.Services.AddSingleton(sp =>
{
    return new FirestoreDbBuilder
    {
        ProjectId = projectId,
        Credential = credential
    }.Build();
});

// 4. (Opcional recomendado) Registrar FirebaseMessaging para inyectarlo en Controllers o Jobs
builder.Services.AddSingleton(sp => FirebaseMessaging.DefaultInstance);


builder.Services.AddScoped<FuncionesNomina>();
builder.Services.AddScoped<FuncionesInventario>();
builder.Services.AddScoped<MailC>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IWorkforceRepository, WorkforceRepository>();

//Configurar Quartz
builder.Services.AddQuartz(q =>
{
    var remisionesKey = new JobKey("remisionesJob");
    q.AddJob<JobRemisiones>(opts => opts.WithIdentity(remisionesKey));
    q.AddTrigger(opts => opts
        .ForJob(remisionesKey)
        .WithIdentity("remisionesJob-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(5)
            .RepeatForever())
    );

    var faltasKey = new JobKey("faltaspersonalJob");
    q.AddJob<JobFaltasPersonal>(opts => opts.WithIdentity(faltasKey));
    q.AddTrigger(opts => opts
        .ForJob(faltasKey)
        .WithIdentity("faltaspersonalJob-trigger")
        .WithCronSchedule("0 0 8 ? * WED *")
    );


    //// Define la clave única para la tarea
    //var jobnotificacionesKey = new JobKey("RevisarTicketsVencidosJob");

    //q.AddJob<RevisarTicketsVencidosJob>(opts => opts.WithIdentity(jobnotificacionesKey));

    //q.AddTrigger(opts => opts
    //    .ForJob(jobnotificacionesKey)
    //    .WithIdentity("RevisarTicketsVencidosTrigger")
    //    .StartNow() // <--- Ejecuta el job inmediatamente al iniciar la aplicación
    //    .WithSimpleSchedule(x => x
    //        .WithIntervalInMinutes(60)
    //        .RepeatForever()) // Repetición continua cada 60 minutos
    //);

    //var jobWorkforceKey = new JobKey("jobWorkforce");
    //q.AddJob<JobWorkforce>(opts => opts.WithIdentity(jobWorkforceKey));
    //q.AddTrigger(opts => opts
    //    .ForJob(jobWorkforceKey)
    //    .WithIdentity("jobWorkforce-trigger")
    //    .WithCronSchedule("0 5 9 * * ?")
    //);

});

// Quartz como hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RW Tickets API",
        Version = "v0.0.1",
        Description = "API para administración del sistema de tickets"
    });

    options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key requerida en el header: x-api-key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "x-api-key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
#if DEBUG
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API TICKETS v1");
#else
    c.SwaggerEndpoint("/back/api_tickets/swagger/v1/swagger.json", "API TICKETS v1");
#endif
});

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
