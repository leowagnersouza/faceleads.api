using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Application.CreateConsultor;
using Faceleads.Leads.Application.GetConsultorById;
using Faceleads.Leads.Domain;
using Faceleads.Leads.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// DbContext configurado para SQL Server. A connection string deve ser configurada em appsettings.
builder.Services.AddDbContext<LeadsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LeadsDatabase")));

// Repositórios
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IConsultorRepository, ConsultorRepository>();

// Casos de uso / handlers
builder.Services.AddScoped<CreateConsultorHandler>();
builder.Services.AddScoped<GetConsultorByIdHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoint para criação de consultor
app.MapPost("/consultores", async (
    CreateConsultorCommand request,
    CreateConsultorHandler handler,
    CancellationToken cancellationToken) =>
{
    Result<Consultor> result = await handler.HandleAsync(request, cancellationToken).ConfigureAwait(false);

    if (!result.Success)
    {
        return Results.BadRequest(new
        {
            result.ErrorCode,
            result.ErrorMessage
        });

// Endpoint para listar consultores (exclui soft-deleted automaticamente)
app.MapGet("/consultores", async (
    IConsultorRepository consultorRepository,
    CancellationToken cancellationToken) =>
{
    var consultores = await consultorRepository.ListAsync(cancellationToken).ConfigureAwait(false);

    var dto = consultores.Select(c => new
    {
        c.Id,
        c.NomeCompleto,
        c.Email,
        c.Telefone,
        c.Ativo,
        c.CriadoEmUtc
    });

    return Results.Ok(dto);
});

// Endpoint para ativar consultor
app.MapPatch("/consultores/{id:guid}/ativar", async (
    Guid id,
    IConsultorRepository consultorRepository,
    CancellationToken cancellationToken) =>
{
    var success = await consultorRepository.ActivateAsync(id, cancellationToken).ConfigureAwait(false);

    return success
        ? Results.NoContent()
        : Results.NotFound(new { Error = "CONSULTOR_NAO_ENCONTRADO" });
});

// Endpoint para desativar consultor
app.MapPatch("/consultores/{id:guid}/desativar", async (
    Guid id,
    IConsultorRepository consultorRepository,
    CancellationToken cancellationToken) =>
{
    var success = await consultorRepository.DeactivateAsync(id, cancellationToken).ConfigureAwait(false);

    return success
        ? Results.NoContent()
        : Results.NotFound(new { Error = "CONSULTOR_NAO_ENCONTRADO" });
});

// Endpoint para soft-delete (exclusão lógica)
app.MapDelete("/consultores/{id:guid}", async (
    Guid id,
    IConsultorRepository consultorRepository,
    CancellationToken cancellationToken) =>
{
    var success = await consultorRepository.SoftDeleteAsync(id, cancellationToken).ConfigureAwait(false);

    return success
        ? Results.NoContent()
        : Results.NotFound(new { Error = "CONSULTOR_NAO_ENCONTRADO" });
});
    }

    var consultor = result.Value!;

    return Results.Created($"/consultores/{consultor.Id}", new
    {
        consultor.Id,
        consultor.NomeCompleto,
        consultor.Email,
        consultor.Telefone,
        consultor.Ativo,
        consultor.CriadoEmUtc
    });
});

// Endpoint para obter consultor por id
app.MapGet("/consultores/{id:guid}", async (
    Guid id,
    GetConsultorByIdHandler handler,
    CancellationToken cancellationToken) =>
{
    Result<Consultor> result = await handler.HandleAsync(id, cancellationToken).ConfigureAwait(false);

    if (!result.Success)
    {
        // Diferenciar ID inválido (400) de não encontrado (404) pelo código de erro
        return result.ErrorCode switch
        {
            "CONSULTOR_ID_INVALIDO" => Results.BadRequest(new
            {
                result.ErrorCode,
                result.ErrorMessage
            }),
            "CONSULTOR_NAO_ENCONTRADO" => Results.NotFound(new
            {
                result.ErrorCode,
                result.ErrorMessage
            }),
            _ => Results.BadRequest(new
            {
                result.ErrorCode,
                result.ErrorMessage
            })
        };
    }

    var consultor = result.Value!;

    return Results.Ok(new
    {
        consultor.Id,
        consultor.NomeCompleto,
        consultor.Email,
        consultor.Telefone,
        consultor.Ativo,
        consultor.CriadoEmUtc
    });
});

app.Run();
