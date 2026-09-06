using System.Linq;
#nullable enable
using System;
// Required for JsonPropertyOrderAttribute when compiling against .NET Standard 2.1 in Unity,
// matching the IsExternalInit.cs polyfill precedent in this same directory.
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
