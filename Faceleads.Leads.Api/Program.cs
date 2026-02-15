using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Application.CreateConsultor;
using Faceleads.Leads.Application.Repositories;
using Faceleads.Leads.Infrastructure.Repositories;
using Faceleads.Leads.Application.GetConsultorById;
using Faceleads.Leads.Application.ListConsultores;
using Faceleads.Leads.Domain;
using Faceleads.Leads.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Faceleads.Leads.Api.Requests;
using Faceleads.Leads.Api.Services;
using Microsoft.OpenApi.Models;
using Faceleads.Leads.Application.GetTenantName;
using Faceleads.Leads.Application.UpdateConsultor;
using Faceleads.Leads.Application.DeleteConsultor;
using Faceleads.Leads.Application.DeactivateConsultor;
using Faceleads.Leads.Application.ActivateConsultor;
using Faceleads.Leads.Infrastructure.Interceptors;
using Microsoft.AspNetCore.Authorization;
using Faceleads.Leads.Api.Authorization;
using Faceleads.Leads.Application.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Ensure HttpContextAccessor and tenant service are available early so the
// auditing interceptor can resolve the current tenant when AddDbContext runs.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
builder.Services.AddScoped<AuditingSaveChangesInterceptor>();

// DbContext configurado para SQL Server. A connection string deve ser configurada em appsettings.
builder.Services.AddDbContext<LeadsDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("LeadsDatabase"));
    // Add interceptor resolved from the DI container
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditingSaveChangesInterceptor>());
});

// Repositórios
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IConsultorRepository, ConsultorRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissaoRepository, PermissaoRepository>();
builder.Services.AddScoped<IRolePermissaoRepository, RolePermissaoRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
// Identity helpers - use ASP.NET Identity hasher adapter for compatibility with existing hashes
builder.Services.AddScoped<Faceleads.Leads.Application.Services.IPasswordHasher<Usuario>, Faceleads.Leads.Api.Adapters.PasswordHasherAdapter<Usuario>>();
// Also register open-generic adapter for other types if needed
builder.Services.AddScoped(typeof(Faceleads.Leads.Application.Services.IPasswordHasher<>), typeof(Faceleads.Leads.Api.Adapters.PasswordHasherAdapter<>));

// Casos de uso / handlers
builder.Services.AddScoped<CreateConsultorHandler>();
builder.Services.AddScoped<GetConsultorByIdHandler>();
builder.Services.AddScoped<ListConsultoresHandler>();
builder.Services.AddScoped<GetTenantNameHandler>();
builder.Services.AddScoped<UpdateConsultorHandler>();
builder.Services.AddScoped<DeleteConsultorHandler>();
builder.Services.AddScoped<ActivateConsultorHandler>();
builder.Services.AddScoped<DeactivateConsultorHandler>();
// Auth handlers
builder.Services.AddScoped<Faceleads.Leads.Application.Auth.LoginHandler>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];
var jwtKey = jwtSettings["Key"];

// Provide hard-coded defaults for development if configuration is missing.
// These are intended for dev only — prefer setting secure values in App Settings in production.
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = "KcAxkN5Xn/lTyAqvY3wfgGSThIwpSIZED0XE95R3I1Q="; // dev fallback key
    builder.Configuration["Jwt:Key"] = jwtKey;
    Console.WriteLine("Warning: Jwt:Key not configured. Using hard-coded development key. Set Jwt__Key in App Settings for production.");
}
if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    jwtIssuer = "Faceleads";
    builder.Configuration["Jwt:Issuer"] = jwtIssuer;
}
if (string.IsNullOrWhiteSpace(jwtAudience))
{
    jwtAudience = "FaceleadsAudience";
    builder.Configuration["Jwt:Audience"] = jwtAudience;
}

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
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<AuditingSaveChangesInterceptor>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("consultor.create", p => p.Requirements.Add(new PermissionRequirement("consultor.create")));
    options.AddPolicy("consultor.delete", p => p.Requirements.Add(new PermissionRequirement("consultor.delete")));
    options.AddPolicy("consultor.update", p => p.Requirements.Add(new PermissionRequirement("consultor.update")));
    options.AddPolicy("consultor.list", p => p.Requirements.Add(new PermissionRequirement("consultor.list")));
});

// Register permission handler and cache
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
// Register adapter to satisfy application ITokenService without making Application depend on Api project
builder.Services.AddScoped<Faceleads.Leads.Application.Services.ITokenService, Faceleads.Leads.Api.Adapters.TokenServiceAdapter>();
// CurrentTenantService already registered above (implements application interface)

// CORS - permitir o frontend React em desenvolvimento
builder.Services.AddCors(options =>
{
    // Temporary: allow all origins to diagnose CORS issues. This will echo the request origin
    // and allow credentials. Remove or restrict in production.
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
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

// If the app is deployed under a virtual application path in Azure (or another reverse proxy),
// you can set the environment variable or configuration key "PATH_BASE" to that prefix
// (for example "/faceleads-api"). When set, the app will use that as PathBase so routing
// matches incoming requests that include the prefix.
var pathBase = builder.Configuration["PATH_BASE"];
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
    Console.WriteLine($"Using PATH_BASE='{pathBase}'");
}

app.UseHttpsRedirection();

// Ensure routing is enabled so CORS middleware can evaluate requests correctly.
app.UseRouting();

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
        consultor.CreatedOn,
        consultor.CreatedBy
    };

    return Results.Created($"/api/v1/consultores/{consultor.Id}", Result<object>.Ok(createdPayload));
}).RequireAuthorization("consultor.create");

// Endpoint para atualizar o consultor
app.MapPut("/api/v1/consultores/{id:guid}", async (
    Guid id,
    UpdateConsultorRequest request,
    UpdateConsultorHandler handler,
    CancellationToken cancellationToken) =>
{
    var cmd = new UpdateConsultorCommand
    {
        Id = id,
        NomeCompleto = request.NomeCompleto,
        Email = request.Email,
        Telefone = request.Telefone
    };

    var result = await handler.HandleAsync(cmd, cancellationToken).ConfigureAwait(false);

    if (!result.Success)
    {
        return result.ErrorCode switch
        {
            "CONSULTOR_ID_INVALIDO" => Results.BadRequest(Result.Fail(result.ErrorCode!, result.ErrorMessage!)),
            "CONSULTOR_NAO_ENCONTRADO" => Results.NotFound(Result.Fail(result.ErrorCode!, result.ErrorMessage!)),
            _ => Results.BadRequest(Result.Fail(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty))
        };
    }

    return Results.Ok(Result.Ok());
}).RequireAuthorization("consultor.update");

// Fallback diagnostic endpoint: returns 404 with request path and registered endpoints list.
// Useful to diagnose routing issues in environments where the app may be mounted under a path.
app.MapFallback(async (HttpContext ctx, EndpointDataSource eds) =>
{
    var endpoints = eds.Endpoints
        .Select(e => e.DisplayName ?? e.ToString())
        .Where(s => !string.IsNullOrEmpty(s))
        .ToArray();

    var info = new
    {
        message = "No route matched the request",
        path = ctx.Request.Path.Value,
        method = ctx.Request.Method,
        registeredEndpoints = endpoints
    };

    return Results.Json(info, statusCode: 404);
});

// Simple unauthenticated health endpoints for deployment/routing checks
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/", () => Results.Ok("Faceleads Leads API is running"));

// Fallback diagnostic endpoint will be registered at the end of the routing pipeline.

// Backward-compatible route: api versioned login
app.MapPost("/api/v1/login", async (
    LoginRequest login,
    LoginHandler handler,
    CancellationToken cancellationToken) =>
{
    var cmd = new LoginCommand { Username = login.Username, Password = login.Password };
    var result = await handler.HandleAsync(cmd, cancellationToken).ConfigureAwait(false);
    if (!result.Success)
    {
        return Results.Json(Result.Fail(result.ErrorCode ?? "AUTH_INVALID", result.ErrorMessage ?? "Credenciais inválidas."), statusCode: 401);
    }

    var value = result.Value!;
    var payload = new { access_token = value.AccessToken, refresh_token = value.RefreshToken, tenant_name = value.TenantName, username = value.Username };
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
        CreatedOn = c.CreatedOn,
        CreatedBy = c.CreatedBy
    }).ToList();

    return Results.Ok(Result<IEnumerable<object>>.Ok(dto.Cast<object>()));
}).RequireAuthorization("consultor.list");

// Endpoint para ativar consultor
app.MapPatch("/api/v1/consultores/{id:guid}/ativar", async (
    Guid id,
    ActivateConsultorHandler handler,
    CancellationToken cancellationToken) =>
{
    var cmd = new ActivateConsultorCommand { Id = id };
    var result = await handler.HandleAsync(cmd, cancellationToken).ConfigureAwait(false);

    if (!result.Success)
    {
        return result.ErrorCode switch
        {
            "CONSULTOR_ID_INVALIDO" => Results.BadRequest(Result.Fail(result.ErrorCode!, result.ErrorMessage!)),
            "CONSULTOR_NAO_ENCONTRADO" => Results.NotFound(Result.Fail(result.ErrorCode!, result.ErrorMessage!)),
            _ => Results.BadRequest(Result.Fail(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty))
        };
    }

    return Results.Ok(Result.Ok());
}).RequireAuthorization("consultor.update");

// Endpoint para desativar consultor
app.MapPatch("/api/v1/consultores/{id:guid}/desativar", async (
    Guid id,
    DeactivateConsultorHandler handler,
    CancellationToken cancellationToken) =>
{
    var cmd = new DeactivateConsultorCommand { Id = id };
    var result = await handler.HandleAsync(cmd, cancellationToken).ConfigureAwait(false);

    if (!result.Success)
    {
        return result.ErrorCode switch
        {
            "CONSULTOR_ID_INVALIDO" => Results.BadRequest(Result.Fail(result.ErrorCode!, result.ErrorMessage!)),
            "CONSULTOR_NAO_ENCONTRADO" => Results.NotFound(Result.Fail(result.ErrorCode!, result.ErrorMessage!)),
            _ => Results.BadRequest(Result.Fail(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty))
        };
    }

    return Results.Ok(Result.Ok());
}).RequireAuthorization("consultor.update");

// Endpoint para soft-delete (exclusão lógica)
app.MapDelete("/api/v1/consultores/{id:guid}", async (
    Guid id,
    DeleteConsultorHandler handler,
    CancellationToken cancellationToken) =>
{
    var cmd = new DeleteConsultorCommand { Id = id };

    var result = await handler.HandleAsync(cmd, cancellationToken).ConfigureAwait(false);

    if (!result.Success)
    {
        return result.ErrorCode switch
        {
            "CONSULTOR_ID_INVALIDO" => Results.BadRequest(Result.Fail(result.ErrorCode!, result.ErrorMessage!)),
            "CONSULTOR_NAO_ENCONTRADO" => Results.NotFound(Result.Fail(result.ErrorCode!, result.ErrorMessage!)),
            _ => Results.BadRequest(Result.Fail(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty))
        };
    }

    return Results.Ok(Result.Ok());
}).RequireAuthorization("consultor.delete");

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
        // Diferenciar Id inválido (400) de não encontrado (404) pelo código de erro
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
        CreatedOn = consultor.CreatedOn,
        CreatedBy = consultor.CreatedBy
    };

    return Results.Ok(Result<object>.Ok(payload));
}).RequireAuthorization();

app.Run();
