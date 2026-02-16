using Faceleads.Leads.Application.CreateConsultor;
using Faceleads.Leads.Application.GetConsultorById;
using Faceleads.Leads.Application.ListConsultores;
using Faceleads.Leads.Application.UpdateConsultor;
using Faceleads.Leads.Application.DeleteConsultor;
using Faceleads.Leads.Application.DeactivateConsultor;
using Faceleads.Leads.Application.ActivateConsultor;
using Faceleads.Leads.Api.Requests;
using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Api.Extensions;
using Faceleads.Leads.Domain;
using Faceleads.Leads.Api.Authorization;

namespace Faceleads.Leads.Api.Endpoints;

public static class ConsultoresEndpoints
{
    public static WebApplication MapConsultoresEndpoints(this WebApplication app)
    {
        // Endpoint para criação de consultor
        app.MapPost("/api/v1/consultores", async (
            CreateConsultorCommand request,
            CreateConsultorHandler handler,
            CancellationToken cancellationToken) =>
        {
            Result<Consultor> result = await handler.HandleAsync(request, cancellationToken).ConfigureAwait(false);

            if (!result.Success)
            {
                return result.ToIResult();
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
        }).RequireAuthorization(Permissions.Consultor.Create);

        // Versão API v1: listar consultores (usa mesmo handler e padrão Result)
        app.MapGet("/api/v1/consultores", async (
            ListConsultoresHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new ListConsultoresQuery(), cancellationToken).ConfigureAwait(false);

            if (!result.Success)
            {
                return result.ToIResult();
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
        }).RequireAuthorization(Permissions.Consultor.Get);

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
                return result.ToIResult();
            }

            return Results.Ok(Result.Ok());
        }).RequireAuthorization(Permissions.Consultor.Update);

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
                return result.ToIResult();
            }

            return Results.Ok(Result.Ok());
        }).RequireAuthorization(Permissions.Consultor.Update);

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
                return result.ToIResult();
            }

            return Results.Ok(Result.Ok());
        }).RequireAuthorization(Permissions.Consultor.Update);

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
                return result.ToIResult();
            }

            return Results.Ok(Result.Ok());
        }).RequireAuthorization(Permissions.Consultor.Delete);

        // Endpoint para obter consultor por id
        app.MapGet("/api/v1/consultores/{id:guid}", async (
            Guid id,
            GetConsultorByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            Result<Consultor> result = await handler.HandleAsync(id, cancellationToken).ConfigureAwait(false);

            if (!result.Success)
            {
                return result.ToIResult();
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
        }).RequireAuthorization(Permissions.Consultor.List);

        return app;
    }
}
