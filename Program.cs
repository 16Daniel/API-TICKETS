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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


var defualtconnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = builder.Configuration.GetConnectionString("DBREBELWINGS");
var connectionStringBD1 = builder.Configuration.GetConnectionString("DB1");
var connectionStringBD2 = builder.Configuration.GetConnectionString("DB2");
var connectionStringBD2P = builder.Configuration.GetConnectionString("DB2PRUEBA");

builder.Services.AddDbContext<DBRebelContext>(options => options.UseSqlServer(connectionString))
    .AddDbContext<BD1Context>(options => options.UseSqlServer(connectionStringBD1))
    .AddDbContext<BD2Context>(options => options.UseSqlServer(connectionStringBD2))
     .AddDbContext<BD2ContextPrueba>(options => options.UseSqlServer(connectionStringBD2P))
     .AddDbContext<TicketsContext>(options => options.UseSqlServer(defualtconnectionString));


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


    //var faltasKey = new JobKey("faltaspersonalJob");
    //q.AddJob<JobFaltasPersonal>(opts => opts.WithIdentity(faltasKey));
    //q.AddTrigger(opts => opts
    //    .ForJob(faltasKey)
    //    .WithIdentity("faltaspersonalJob-trigger")
    //    .WithCronSchedule("0 37 9 ? * * *")
    //);

});

// Quartz como hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

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
