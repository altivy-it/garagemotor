using Microsoft.AspNetCore.Http;

namespace GarageMotor.Api.Infrastructure;

public static class ProblemResponses
{
    private static readonly Dictionary<int, (string Title, string Detail)> Messages =
        new Dictionary<int, (string Title, string Detail)>
        {
            [400] = ("Requisição inválida", "Confira os dados enviados."),
            [401] = ("Sessão necessária", "Autentique-se para continuar."),
            [403] = ("Ação não permitida", "Você não pode executar esta ação."),
            [404] = ("Recurso não encontrado", "O recurso solicitado não está disponível."),
            [409] = ("Conflito", "Os dados foram alterados ou a operação conflita com o estado atual."),
            [413] = ("Conteúdo muito grande", "O conteúdo excede o limite permitido."),
            [415] = ("Tipo de conteúdo não aceito", "O formato enviado não é aceito."),
            [422] = ("Regra não atendida", "Os dados não atendem às regras desta operação."),
            [429] = ("Limite de tentativas atingido", "Tente novamente mais tarde."),
            [503] = ("Serviço temporariamente indisponível", "Tente novamente mais tarde."),
        };

    public static IResult Create(HttpContext context, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (!Messages.TryGetValue(statusCode, out var message))
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode), "Status is not a public problem response.");
        }

        var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = "about:blank",
            Title = message.Title,
            Status = statusCode,
            Detail = message.Detail,
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        return Results.Problem(
            type: problem.Type,
            title: problem.Title,
            statusCode: problem.Status,
            detail: problem.Detail,
            extensions: problem.Extensions);
    }
}
