using Api.Contracts;
using Api.Validation;
using FluentValidation;
using Api.Endpoints;
using Core.Abstractions;
using Core.Storage;
using Scalar.AspNetCore;
using Api.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<ICatalogStore>(
    _ => new InMemoryCatalogStore());

builder.Services.AddScoped<
    IValidator<SaveProductRequest>,
    SaveProductRequestValidator>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

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