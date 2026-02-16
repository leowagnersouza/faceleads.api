using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Faceleads.Leads.Api.Authorization;

/// <summary>
/// Centralized permission names for the API.
/// Add new resource groups as nested static classes and constants inside them.
/// Use <see cref="GetAll"/> to enumerate all permission strings for registration.
/// </summary>
public static class Permissions
{
    public static class Consultor
    {
        public const string List = "consultor.list";
        public const string Create = "consultor.create";
        public const string Get = "consultor.get";
        public const string Update = "consultor.update";
        public const string Delete = "consultor.delete";
    }

    // Example: add other resources here
    // public static class Usuario { public const string Manage = "usuario.manage"; }

    public static IEnumerable<string> GetAll()
    {
        var nested = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

        var values = nested.SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => f.GetValue(null) as string));

        return values.Where(v => !string.IsNullOrEmpty(v)).Distinct();
    }
}
