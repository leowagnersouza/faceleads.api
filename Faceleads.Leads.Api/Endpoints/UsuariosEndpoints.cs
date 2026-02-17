using Faceleads.Leads.Application.CreateUsuario;
using Faceleads.Leads.Application.UpdateUsuario;
using Faceleads.Leads.Application.ListUsuarios;
using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Api.Extensions;
using Faceleads.Leads.Api.Authorization;

namespace Faceleads.Leads.Api.Endpoints;

public static class UsuariosEndpoints
{
    public static WebApplication MapUsuariosEndpoints(this WebApplication app)
    {
        // Create usuario
        app.MapPost("/api/v1/usuarios", async (
            CreateUsuarioCommand request,
            CreateUsuarioHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken).ConfigureAwait(false);
            if (!result.Success) return result.ToIResult();
            var u = result.Value!;
            var payload = new { u.Id, u.NomeUsuario, u.Email, u.Ativo, u.CreatedOn, u.CreatedBy };
            return Results.Created($"/api/v1/usuarios/{u.Id}", Result<object>.Ok(payload));
        }).RequireAuthorization(Permissions.Usuario.Create);

        // List usuarios
        app.MapGet("/api/v1/usuarios", async (
            ListUsuariosHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new ListUsuariosQuery(), cancellationToken).ConfigureAwait(false);
            if (!result.Success) return result.ToIResult();
            var dto = result.Value!.Select(u => new { u.Id, u.NomeUsuario, u.Email, u.Ativo, CreatedOn = u.CreatedOn, CreatedBy = u.CreatedBy }).ToList();
            return Results.Ok(Result<IEnumerable<object>>.Ok(dto.Cast<object>()));
        }).RequireAuthorization(Permissions.Usuario.List);

        // Patch usuario (partial update)
        app.MapPatch("/api/v1/usuarios/{id:guid}", async (
            Guid id,
            UpdateUsuarioCommand request,
            UpdateUsuarioHandler handler,
            HttpContext ctx,
            CancellationToken cancellationToken) =>
        {
            var cmd = new UpdateUsuarioCommand { 
                Id = id, 
                NomeUsuario = request.NomeUsuario, 
                Email = request.Email, 
                ConsultorId = request.ConsultorId, 
                Password = request.Password, 
                Ativo = request.Ativo 
            };

            var result = await handler.HandleAsync(cmd, cancellationToken).ConfigureAwait(false);
            if (!result.Success) return result.ToIResult();
            return Results.Ok(Result.Ok());
        }).RequireAuthorization(Permissions.Usuario.Update);

        return app;
    }
}
