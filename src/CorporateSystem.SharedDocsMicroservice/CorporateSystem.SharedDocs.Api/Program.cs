using CorporateSystem.SharedDocs.Infrastructure.Extensions;
using CorporateSystem.SharedDocs.Services.Extensions;
using CorporateSystem.SharedDocs.Services.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSharedDocsInfrastructure();
builder.Services.AddSharedDocsServices();
builder.Services.Configure<AuthMicroserviceOptions>(builder.Configuration.GetSection("AuthMicroserviceOptions"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();