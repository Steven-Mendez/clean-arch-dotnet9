using System;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Api.Extensions;

internal static class OpenApiOperationExtensions
{
    public static void SetParameterDescription(this OpenApiOperation operation, string name, string description, bool? required = null)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));

        var parameter = operation.Parameters?.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (parameter == null) return;

        parameter.Description = description;
        if (required.HasValue) parameter.Required = required.Value;
    }

    public static void SetRequestBodyDescription(this OpenApiOperation operation, string description, bool? required = null)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));
        if (operation.RequestBody == null) return;

        operation.RequestBody.Description = description;
        if (required.HasValue) operation.RequestBody.Required = required.Value;
    }
}
