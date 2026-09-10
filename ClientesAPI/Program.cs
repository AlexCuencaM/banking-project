using ClientesAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
string? conn = builder.Configuration.GetConnectionString("ClientesDb");
builder.Services.AddDbContext<ClientesDbContext>(options =>
    options.UseSqlServer(
        conn));
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.MapControllers();

app.Run();