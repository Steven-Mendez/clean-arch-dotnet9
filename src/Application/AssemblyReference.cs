using System.Reflection;

namespace Application;

/// <summary>
///     Provides a stable access point for the application assembly metadata.
/// </summary>
public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}