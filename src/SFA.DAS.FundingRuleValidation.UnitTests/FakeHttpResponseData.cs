using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

[assembly: ExcludeFromCodeCoverage]
namespace SFA.DAS.FundingRuleValidation.UnitTests;

public class FakeHttpResponseData(FunctionContext functionContext) : HttpResponseData(functionContext)
{
    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; } = [];
    public override Stream Body { get; set; } = new MemoryStream();
    public override HttpCookies Cookies { get; }
}