using TP_ITSM.Models;
using TP_ITSM.Services.Execon;
using TP_ITSM.Services.Trackpoint;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(option =>
    option.AddPolicy("NewPolicy", app =>
    {
        app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    }
    )
);

builder.Services.AddScoped<IExeconServices, TP_ITSM.Services.Execon.Services>();
builder.Services.AddScoped<ITrackpointServices, TP_ITSM.Services.Trackpoint.Service>();

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions( o =>
        {
            o.JsonSerializerOptions.Converters.Add(new ItemValueJsonConverter());
            o.JsonSerializerOptions.Converters.Add(new StringToIntConverter());
        }
    );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseCors("NewPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
