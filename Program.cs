using DashboardApi.Mail;
using DashboardApi.ModelsBD1;
using DashboardApi.ModelsBD2;
using DashboardApi.ModelsDBRebel;
using Microsoft.EntityFrameworkCore;
using Quartz;
using TICKETSAPI.ModelsTickets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


var defualtconnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = builder.Configuration.GetConnectionString("DBREBELWINGS");
var connectionStringBD1 = builder.Configuration.GetConnectionString("DB1");
var connectionStringBD2 = builder.Configuration.GetConnectionString("DB2");

builder.Services.AddDbContext<DBRebelContext>(options => options.UseSqlServer(connectionString))
    .AddDbContext<BD1Context>(options => options.UseSqlServer(connectionStringBD1))
    .AddDbContext<BD2Context>(options => options.UseSqlServer(connectionStringBD2))
     .AddDbContext<TicketsContext>(options => options.UseSqlServer(defualtconnectionString));


builder.Services.AddCors(policyBuilder =>
    policyBuilder.AddDefaultPolicy(policy =>
        policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod())
);


builder.Services.AddScoped<MailC>();


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

app.UseSwaggerUI(c =>
{
    app.UseSwagger().UseDeveloperExceptionPage();
#if DEBUG
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API TICKETS v1");
#else
    c.SwaggerEndpoint("/back/api_planeacion/swagger/v1/swagger.json", "API_PEDIDOS v1");
#endif
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
