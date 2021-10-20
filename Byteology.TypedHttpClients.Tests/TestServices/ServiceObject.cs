using System;
using System.Text.Json;

namespace Byteology.TypedHttpClients.Tests.TestServices
{
    internal record TestServiceResult(string StringData, int IntData, bool BoolData, float FloatData, DateTime DateTimeData)
    {
        public TestServiceResult() : this("string", 5, true, 5.8f, DateTime.Now) { }

        public override string ToString()
            => JsonSerializer.Serialize(this, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
