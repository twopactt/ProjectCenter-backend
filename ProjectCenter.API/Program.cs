using ProjectCenter.API.Extensions;
using ProjectCenter.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Регистрация всех сервисов одной строкой
builder.Services.AddAllServices(builder.Configuration);

// Добавляем контроллеры
builder.Services.AddControllers();

// Добавляем Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithBearer();

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseCors();

// Настройка статических файлов
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:5173");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");
    }
});

app.UseMiddleware<UserContextMiddleware>();
app.MapControllers();

app.Run();