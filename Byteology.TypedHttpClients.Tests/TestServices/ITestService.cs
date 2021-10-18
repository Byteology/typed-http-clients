using System.Threading.Tasks;

namespace Byteology.TypedHttpClients.Tests.TestServices
{
    internal interface ITestService
    {
        [HttpMethod("POST", "/noresultaction")]
        Task NoResultActionAsync();

        [HttpMethod("POST", "/action")]
        Task<TestServiceResult> ActionAsync();

        [HttpMethod("POST", "/actionbody")]
        Task BodyActionAsync([HttpBody]TestServiceResult body);

        [HttpMethod("POST", "/simpleuri")]
        Task SimpleUriAsync();

        [HttpMethod("POST", "simpleuri")]
        Task SimpleUriNoDashAsync();

        [HttpMethod("POST", "/paramuri/{param}")]
        Task ParamUriAsync(object param);

        [HttpMethod("POST", "/query")]
        Task QueryAsync(int i, string s, bool b, float f, object n, int?[] a);

        [HttpMethod("VERB", "/verb")]
        Task VerbAsync();

        [HttpMethod("POST", "/uri")]
        Task OutParamAsync(out int param);

        [HttpMethod("POST", "/uri")]
        Task MultipleBodyAsync([HttpBody] int param, [HttpBody] int param2);

        Task NotDecoratedAsync();

        [HttpMethod("POST", "/uri")]
        int NotAsync();
    }

}
