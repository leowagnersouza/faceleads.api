using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Application.CreateConsultor;
using Faceleads.Leads.Application.GetConsultorById;
using Faceleads.Leads.Domain;
using Faceleads.Leads.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Faceleads.Leads.Api.Requests;

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

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured. Set Jwt:Key in configuration or environment.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

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
}).RequireAuthorization();

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
}).RequireAuthorization();

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
}).RequireAuthorization();

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
}).RequireAuthorization();

// Endpoint de login simples para emitir JWT (credenciais em memória para testes)
app.MapPost("/login", (LoginRequest login) =>
{
    // Credenciais de teste hard-coded (não usar em produção)
    if (login.Username != "admin" || login.Password != "password")
    {
        return Results.Unauthorized();
    }

    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var jwtIssuer = jwtSettings["Issuer"]!;
    var jwtAudience = jwtSettings["Audience"]!;
    var jwtKey = jwtSettings["Key"]!;

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, login.Username),
        new Claim(ClaimTypes.Role, "Admin")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds);

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new { access_token = tokenString });
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
}).RequireAuthorization();

app.Run();
