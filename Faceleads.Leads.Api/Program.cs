using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Application.CreateConsultor;
using Faceleads.Leads.Application.GetConsultorById;
using Faceleads.Leads.Application.ListConsultores;
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
using Faceleads.Leads.Api.Services;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// DbContext configurado para SQL Server. A connection string deve ser configurada em appsettings.
builder.Services.AddDbContext<LeadsDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("LeadsDatabase"));
    // Add interceptor resolved from the DI container
    options.AddInterceptors(serviceProvider.GetRequiredService<Faceleads.Leads.Infrastructure.Interceptors.AuditingSaveChangesInterceptor>());
});

// Repositórios
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IConsultorRepository, ConsultorRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Casos de uso / handlers
builder.Services.AddScoped<CreateConsultorHandler>();
builder.Services.AddScoped<GetConsultorByIdHandler>();
builder.Services.AddScoped<ListConsultoresHandler>();

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

// Register auditing support
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Faceleads.Leads.Application.Common.ICurrentUserService, Faceleads.Leads.Api.Services.CurrentUserService>();
builder.Services.AddScoped<Faceleads.Leads.Infrastructure.Interceptors.AuditingSaveChangesInterceptor>();

builder.Services.AddAuthorization();

// CORS - permitir o frontend React em desenvolvimento
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger/ OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Faceleads Leads API",
        Version = "v1",
        Description = "API de gestão de leads e consultores"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Faceleads Leads API v1");
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Habilita CORS antes de autenticação/autorização
app.UseCors("AllowReactDev");

app.UseAuthentication();
app.UseAuthorization();

// Endpoint para criação de consultor
app.MapPost("/api/v1/consultores", async (
    CreateConsultorCommand request,
    CreateConsultorHandler handler,
    CancellationToken cancellationToken) =>
{
    Result<Consultor> result = await handler.HandleAsync(request, cancellationToken).ConfigureAwait(false);

    if (!result.Success)
    {
        return Results.BadRequest(Result.Fail(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty));
    }

    var consultor = result.Value!;

    var createdPayload = new
    {
        consultor.Id,
        consultor.NomeCompleto,
        consultor.Email,
        consultor.Telefone,
        consultor.Ativo,
        consultor.CriadoEmUtc
    };

    return Results.Created($"/api/v1/consultores/{consultor.Id}", Result<object>.Ok(createdPayload));
}).RequireAuthorization();

// Backward-compatible route: api versioned login
app.MapPost("/api/v1/login", async (LoginRequest login, ITokenService tokenService) =>
{
    // Credenciais de teste hard-coded (não usar em produção)
    if (login.Username != "admin" || login.Password != "password")
    {
        return Results.Json(Result.Fail("AUTH_INVALID", "Credenciais inválidas."), statusCode: 401);
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

    var issueResult = await tokenService.IssueTokensAsync(login.Username);
    if (!issueResult.Success)
    {
        return Results.BadRequest(Result.Fail(issueResult.ErrorCode ?? "ERROR", issueResult.ErrorMessage ?? string.Empty));
    }

    var (accessToken, refreshToken) = issueResult.Value!;

    var payload = new { access_token = accessToken, refresh_token = refreshToken };
    return Results.Ok(Result<object>.Ok(payload));
});

// Versão API v1: listar consultores (usa mesmo handler e padrão Result)
app.MapGet("/api/v1/consultores", async (
    ListConsultoresHandler handler,
    CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(new ListConsultoresQuery(), cancellationToken).ConfigureAwait(false);

    if (!result.Success)
    {
        return Results.BadRequest(Result.Fail(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty));
    }

    var dto = result.Value!.Select(c => new
    {
        c.Id,
        c.NomeCompleto,
        c.Email,
        c.Telefone,
        c.Ativo,
        c.CriadoEmUtc
    }).ToList();

    return Results.Ok(Result<IEnumerable<object>>.Ok(dto.Cast<object>()));
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

// Endpoint para renovar tokens usando refresh token
app.MapPost("/api/v1/refresh", async (RefreshRequest request, ITokenService tokenService) =>
{
    try
    {
        var refreshResult = await tokenService.RefreshWithTokenAsync(request.RefreshToken);
        if (!refreshResult.Success)
        {
            return Results.Json(Result.Fail(refreshResult.ErrorCode ?? "REFRESH_INVALID", refreshResult.ErrorMessage ?? string.Empty), statusCode: 401);
        }

        var (accessToken, refreshToken) = refreshResult.Value!;
        var payload = new { access_token = accessToken, refresh_token = refreshToken };
        return Results.Ok(Result<object>.Ok(payload));
    }
    catch
    {
        return Results.Unauthorized();
    }
});

// Endpoint para logout: revoga um refresh token, retornando Result padrão
app.MapPost("/logout", async (RefreshRequest request, ITokenService tokenService) =>
{
    var result = await tokenService.RevokeRefreshTokenAsync(request.RefreshToken).ConfigureAwait(false);

    if (!result.Success)
    {
        return Results.BadRequest(new { result.ErrorCode, result.ErrorMessage });
    }

    return Results.NoContent();
});

// Endpoint para obter consultor por id
app.MapGet("/api/v1/consultores/{id:guid}", async (
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
            "CONSULTOR_ID_INVALIDO" => Results.BadRequest(Result.Fail(result.ErrorCode, result.ErrorMessage ?? string.Empty)),
            "CONSULTOR_NAO_ENCONTRADO" => Results.NotFound(Result.Fail(result.ErrorCode, result.ErrorMessage ?? string.Empty)),
            _ => Results.BadRequest(Result.Fail(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty))
        };
    }

    var consultor = result.Value!;

    var payload = new
    {
        consultor.Id,
        consultor.NomeCompleto,
        consultor.Email,
        consultor.Telefone,
        consultor.Ativo,
        consultor.CriadoEmUtc
    };

    return Results.Ok(Result<object>.Ok(payload));
}).RequireAuthorization();

app.Run();
