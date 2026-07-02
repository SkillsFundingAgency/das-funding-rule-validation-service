using System.Net;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace SFA.DAS.FundingRuleValidation.UnitTests;

public class FakeHttpRequestData(FunctionContext functionContext) : HttpRequestData(functionContext)
{
    public override HttpResponseData CreateResponse()
    {
        return new FakeHttpResponseData(functionContext)
        {
            StatusCode = HttpStatusCode.Accepted
        };
    }

    public override Stream Body { get; } = new MemoryStream();
    public override HttpHeadersCollection Headers => [];
    public override IReadOnlyCollection<IHttpCookie> Cookies => [];
    public override Uri Url => new Uri("http://localhost/api/fundingRules");
    public override IEnumerable<ClaimsIdentity> Identities => [];
    public override string Method => "POST";
}