using Faceleads.Leads.Application.Common;

namespace Faceleads.Leads.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToIResult(this Error error)
    {
        // Keep returning the same Result payload for consistency
        var payload = Result.Fail(error);

        return error.StatusCode switch
        {
            401 => Results.Json(payload, statusCode: 401),
            404 => Results.NotFound(payload),
            400 => Results.BadRequest(payload),
            _ => Results.BadRequest(payload)
        };
    }

    public static IResult ToIResult(this Result result)
    {
        if (result.Success)
        {
            return Results.Ok(Result.Ok());
        }

        if (result.Error is not null)
        {
            return result.Error.ToIResult();
        }

        var err = new Error(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty);
        return err.ToIResult();
    }

    public static IResult ToIResult<T>(this Result<T> result)
    {
        if (result.Success)
        {
            return Results.Ok(Result<T>.Ok(result.Value!));
        }

        if (result.Error is not null)
        {
            return result.Error.ToIResult();
        }

        var err = new Error(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty);
        return err.ToIResult();
    }
}
