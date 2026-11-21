using Api.Contracts;
using Api.Validation;
using FluentValidation;
using Api.Endpoints;
using Core.Abstractions;
using Core.Storage;
using Scalar.AspNetCore;
using Api.Errors;
using Microsoft.Data.Sqlite;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var rawConnectionString = builder.Configuration.GetConnectionString("AppDb")
    ?? "Data Source=data/app.db";

var csb = new SqliteConnectionStringBuilder(rawConnectionString);

var databasePath = Path.GetFullPath(
    csb.DataSource,
    builder.Environment.ContentRootPath);

Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
csb.DataSource = databasePath;

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<
    IValidator<SaveProductRequest>,
    SaveProductRequestValidator>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(csb.ConnectionString));

builder.Services.AddScoped<ICatalogStore, EfCatalogStore>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapProducts();

app.Run();

public partial class Program { }