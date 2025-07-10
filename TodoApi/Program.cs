using Microsoft.EntityFrameworkCore;
using TodoApi.Services.Services;
using TodoApi.SyncServices.ExternalAPI;
using TodoApi.SyncServices.ExternalAPI.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddDbContext<TodoContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext"))
    )
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddScoped<ITodoListService, TodoListService>();
builder.Services.AddScoped<ITodoItemService, TodoItemService>();
builder.Services.AddScoped<IExternalAPI, FakeExternalAPI>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<IExternalAPI, ExternalTodoApiClient>(client =>
{
    client.BaseAddress = new Uri("https://external-api-url.com");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
