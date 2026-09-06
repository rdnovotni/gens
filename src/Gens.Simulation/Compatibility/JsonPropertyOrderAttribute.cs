using System.Linq;
#nullable enable
using System;
// Required for JsonPropertyOrderAttribute when compiling under Unity, whose asmdef compilation
// never sees this project's System.Text.Json 9.0.0 PackageReference (Unity ignores csproj
// PackageReferences entirely) and may bundle an older System.Text.Json lacking this attribute.
// Guarded to Unity only: under `dotnet build`/`dotnet test`, the real NuGet-provided attribute of
// the same name is already in scope, and an unconditional duplicate here is a hard CS0436 conflict
// under this project's TreatWarningsAsErrors.
#if UNITY_2021_1_OR_NEWER
namespace System.Text.Json.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class JsonPropertyOrderAttribute : Attribute
    {
        public JsonPropertyOrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }
}
#endif
