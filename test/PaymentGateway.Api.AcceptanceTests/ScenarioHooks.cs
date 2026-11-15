using Reqnroll;

namespace PaymentGateway.Api.AcceptanceTests;

[Binding]
public sealed class ScenarioHooks
{
    private const string BaseUrl = "http://localhost:5100";

    private readonly ScenarioContext _scenarioContext;

    public ScenarioHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _scenarioContext.Set(BaseUrl, "BaseUrl");
    }
}
