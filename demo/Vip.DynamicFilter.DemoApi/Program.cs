using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Vip.DynamicFilter;
using Vip.DynamicFilter.DemoApi.Data;
using Vip.DynamicFilter.DemoApi.Models;

var builder = WebApplication.CreateBuilder(args);

#region Configuration

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Serialização camelCase + enums como string (recomendado para receber/enviar os tokens do DynamicFilter)
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

#endregion

var app = builder.Build();

#region Middlewares

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#endregion

#region Data

var clients = Seed.Clients;
var addresses = Seed.Addresses;

#endregion

#region Routes

app.MapGet("/", () => "Vip.DynamicFilter Demo API — consulte /swagger para testar.");

// Lista todos os clientes
app.MapGet("/clients", () => clients).WithOpenApi();

// Busca cliente por Id
app.MapGet("/clients/{id:guid}", (Guid id) => clients.FirstOrDefault(x => x.ClientId == id)).WithOpenApi();

// Aplica filtro + ordenação + paginação e retorna FilterResponse<T> (com total de registros)
app.MapPost("/clients", (FilterRequest filter) => clients.GetFilterResponse(filter)).WithOpenApi();

// Aplica filtro + ordenação + paginação e retorna apenas a lista (IQueryable materializado)
app.MapPost("/clients/query", (FilterRequest filter) => clients.ApplyFilterRequest(filter).ToList()).WithOpenApi();

// Aplica filtro sobre os endereços (demonstra uso com outro tipo)
app.MapPost("/addresses", (FilterRequest filter) => addresses.GetFilterResponse(filter)).WithOpenApi();

#endregion

app.Run();
