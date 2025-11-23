using System.Text.Json.Serialization;

namespace MHServerEmu.ServerManagement.Serialization
{
    /// <summary>
    /// <see cref="JsonSerializerContext"/> implementation for AOT publish compatibility.
    /// </summary>
    [JsonSerializable(typeof(Dictionary<string, long>))]
    public partial class JsonContext : JsonSerializerContext
    {
    }
}
