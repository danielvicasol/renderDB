using RenderDB.Services;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Inicializar base de datos
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
    await dbService.InitializeAsync();
}

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Endpoint raíz con información de la API
app.MapGet("/", () => Results.Ok(new
{
    message = "🚀 RenderDB API - PostgreSQL Connection Tester",
    version = "2.0",
    endpoints = new
    {
        health = "/health",
        api_docs = "/swagger",
        verify_connection = "POST /api/records/verify",
        insert_record = "POST /api/records",
        list_records = "GET /api/records"
    }
}));

await app.RunAsync();

