using System.Threading.Tasks;

namespace Byteology.TypedHttpClients.Tests.TestServices
{
    internal interface ITestService
    {
        [HttpEndpoint("POST", "/noresultaction")]
        Task NoResultActionAsync();

        [HttpEndpoint("POST", "/action")]
        Task<TestServiceResult> ActionAsync();

        [HttpEndpoint("POST", "/actionbody")]
        Task BodyActionAsync([HttpBody]TestServiceResult body);

        [HttpEndpoint("POST", "/simpleuri")]
        Task SimpleUriAsync();

        [HttpEndpoint("POST", "simpleuri")]
        Task SimpleUriNoDashAsync();

        [HttpEndpoint("POST", "/paramuri/{param}")]
        Task ParamUriAsync(object param);

        [HttpEndpoint("POST", "/query")]
        Task QueryAsync(int i, string s, bool b, float f, object n, int?[] a);

        [HttpEndpoint("VERB", "/verb")]
        Task VerbAsync();

        [HttpEndpoint("POST", "/uri")]
        Task OutParamAsync(out int param);

        [HttpEndpoint("POST", "/uri")]
        Task MultipleBodyAsync([HttpBody] int param, [HttpBody] int param2);

        Task NotDecoratedAsync();

        [HttpEndpoint("POST", "/uri")]
        int NotAsync();
    }

}
