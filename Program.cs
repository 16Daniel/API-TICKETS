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
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


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

builder.Services.AddScoped<FuncionesNomina>();
builder.Services.AddScoped<FuncionesInventario>();
builder.Services.AddScoped<MailC>();

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

    //options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    //{
    //    Description = "API Key requerida en el header: x-api-key",
    //    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    //    Name = "x-api-key",
    //    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    //});

    //options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    //{
    //    {
    //        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    //        {
    //            Reference = new Microsoft.OpenApi.Models.OpenApiReference
    //            {
    //                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
    //                Id = "ApiKey"
    //            }
    //        },
    //        new string[] {}
    //    }
    //});
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

app.UseAuthorization();

app.MapControllers();

app.Run();
