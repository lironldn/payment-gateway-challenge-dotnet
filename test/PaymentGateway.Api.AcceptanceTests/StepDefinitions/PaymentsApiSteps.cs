using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Reqnroll;

using Xunit;

namespace PaymentGateway.Api.AcceptanceTests.StepDefinitions;

[Binding]
public class PaymentsApiSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly HttpClient _httpClient = new();
    private HttpResponseMessage? _lastResponse;
    private JsonElement? _lastPayment;

    public PaymentsApiSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private string BaseUrl => _scenarioContext.Get<string>("BaseUrl");

    [Given("the services are already running")]
    public void GivenServicesAreAlreadyRunning()
    {
        using var probe = _httpClient.GetAsync(BaseUrl + "/swagger/index.html").GetAwaiter().GetResult();
        Assert.True(probe.IsSuccessStatusCode || probe.StatusCode == HttpStatusCode.NotFound, $"Payment API not available at {BaseUrl}");
    }

    [When("a customer submits a payment")]
    public async Task WhenCustomerSubmitsPayment(Table table)
    {
        var payload = table.CreateInstance<PaymentRequestInput>();

        _lastResponse = await _httpClient.PostAsJsonAsync(BaseUrl + "/api/payments", payload);
        var body = await _lastResponse.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(body) && body.TrimStart().StartsWith("{"))
        {
            _lastPayment = JsonDocument.Parse(body).RootElement;
        }
    }

    [Then("the response status should be (.*)")]
    public void ThenResponseStatusShouldBe(int expectedStatus)
    {
        Assert.Equal(expectedStatus, (int)_lastResponse!.StatusCode);
    }

    [Then("the response payment status should be \"(.*)\"")]
    public void ThenResponsePaymentStatusShouldBe(string expectedStatus)
    {
        Assert.NotNull(_lastPayment);
        Assert.Equal(expectedStatus, _lastPayment!.Value.GetProperty("status").GetString());
    }

    [Then("the response details should be")]
    public void ThenResponseDetailsShouldBe(Table table)
    {
        Assert.NotNull(_lastPayment);

        var expected = table.CreateInstance<ResponseExpectation>();
        var actual = _lastPayment!.Value;

        Assert.Equal(expected.CardNumberLastFour, actual.GetProperty("cardNumberLastFour").GetString());
        Assert.Equal(expected.ExpiryMonth, actual.GetProperty("expiryMonth").GetInt32());
        Assert.Equal(expected.ExpiryYear, actual.GetProperty("expiryYear").GetInt32());
        Assert.Equal(expected.Currency, actual.GetProperty("currency").GetString());
        Assert.Equal(expected.Amount, actual.GetProperty("amount").GetInt32());
        Assert.Equal(expected.Status, actual.GetProperty("status").GetString());
    }

    private sealed class PaymentRequestInput
    {
        public string CardNumber { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
    }

    private sealed class ResponseExpectation
    {
        public string CardNumberLastFour { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
