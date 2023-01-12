using System.Threading.Tasks;

namespace Byteology.TypedHttpClients.Tests.TestServices
{
	internal interface ITestService
	{
		[HttpEndpoint("POST", "/noResultAction")]
		Task NoResultActionAsync();

		[HttpEndpoint("POST", "/action", Tags = new[] { "tag" })]
		Task<TestServiceResult> ActionAsync();

		[HttpEndpoint("POST", "/actionBody")]
		Task BodyActionAsync([HttpBody] TestServiceResult body);

		[HttpEndpoint("POST", "/simpleUri")]
		Task SimpleUriAsync();

		[HttpEndpoint("POST", "simpleUri")]
		Task SimpleUriNoDashAsync();

		[HttpEndpoint("POST", "/paramUri/{param}")]
		Task ParamUriAsync(object param);

		[HttpEndpoint("POST", "/query")]
		Task QueryAsync(int i, string s, bool b, float f, object? n, int?[] a);

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
