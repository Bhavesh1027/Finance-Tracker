using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NBomber;
using NBomber.CSharp;
using NBomber.Http;

var baseAddress = Environment.GetEnvironmentVariable("FINANCETRACKER_BASE_URL")
                  ?? "http://localhost:5000";

var jwtToken = await GetJwtTokenAsync(baseAddress);

var httpFactory = HttpClientFactory.Create(
    name: "api_client",
    clientFactory: () =>
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(baseAddress),
            Timeout = TimeSpan.FromSeconds(30)
        };

        if (!string.IsNullOrWhiteSpace(jwtToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    });

// ---------------- Scenario 1: Transaction Creation Load Test ----------------

var createTransactionStep = HttpStep.Create(
    name: "create_transaction",
    httpClientFactory: httpFactory,
    request: context =>
    {
        var payload = new
        {
            UserId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "INR",
            Category = 0, // e.g., Food
            Description = "Perf test transaction",
            Date = DateTime.UtcNow,
            TransactionType = 1 // Expense
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = Http.CreateRequest("POST", "/api/v1/transactions")
            .WithBody(content);

        return Task.FromResult(request);
    });

var createTransactionsScenario =
    ScenarioBuilder
        .CreateScenario("transaction_creation_load", createTransactionStep)
        .WithWarmUpDuration(TimeSpan.FromSeconds(10))
        .WithLoadSimulations(
            Simulation.KeepConstant(
                copies: 50,
                during: TimeSpan.FromSeconds(60)))
        .WithAssertions(
            Assertion.ForStep(
                stepName: "create_transaction",
                assertion: stats => stats.Response.TimePercentile95Ms < 500,
                name: "p95_latency_lt_500ms"),
            Assertion.ForStep(
                stepName: "create_transaction",
                assertion: stats => stats.Fail.Request.Count <= stats.Ok.Request.Count * 0.01,
                name: "error_rate_lt_1_percent"));

// ---------------- Scenario 2: Monthly Summary Read Test (cached) ----------------

var getSummaryStep = HttpStep.Create(
    name: "get_monthly_summary",
    httpClientFactory: httpFactory,
    request: context =>
    {
        var month = DateTime.UtcNow.Month;
        var year = DateTime.UtcNow.Year;

        var request = Http.CreateRequest("GET", $"/api/v1/transactions/summary?month={month}&year={year}");
        return Task.FromResult(request);
    });

var summaryReadScenario =
    ScenarioBuilder
        .CreateScenario("monthly_summary_read", getSummaryStep)
        .WithWarmUpDuration(TimeSpan.FromSeconds(10))
        .WithLoadSimulations(
            Simulation.KeepConstant(
                copies: 200,
                during: TimeSpan.FromSeconds(60)))
        .WithAssertions(
            Assertion.ForStep(
                stepName: "get_monthly_summary",
                assertion: stats => stats.Response.TimePercentile95Ms < 100,
                name: "p95_latency_lt_100ms"),
            Assertion.ForStep(
                stepName: "get_monthly_summary",
                assertion: stats => stats.Ok.Request.RPS >= 500,
                name: "throughput_gt_500_rps"));

// ---------------- Scenario 3: Mixed workload ----------------

var getTransactionsStep = HttpStep.Create(
    name: "get_transactions",
    httpClientFactory: httpFactory,
    request: context =>
    {
        var month = DateTime.UtcNow.Month;
        var year = DateTime.UtcNow.Year;

        var request = Http.CreateRequest("GET", $"/api/v1/transactions?month={month}&year={year}");
        return Task.FromResult(request);
    });

var mixedScenario =
    ScenarioBuilder
        .CreateScenario("mixed_workload",
            // use a sequence with probabilistic branching
            Step.Create("mixed_step", async context =>
            {
                // 70% reads (summary or transactions), 30% writes
                var rnd = Random.Shared.NextDouble();

                if (rnd < 0.7)
                {
                    var readStep = rnd < 0.35 ? getSummaryStep : getTransactionsStep;
                    return await readStep.Run(context);
                }

                return await createTransactionStep.Run(context);
            }))
        .WithWarmUpDuration(TimeSpan.FromSeconds(10))
        .WithLoadSimulations(
            Simulation.KeepConstant(
                copies: 100,
                during: TimeSpan.FromMinutes(2)));

// ---------------- NBomber runner ----------------

var testResultsDirectory = Path.Combine(
    Directory.GetCurrentDirectory(),
    "TestResults");

Directory.CreateDirectory(testResultsDirectory);

NBomberRunner
    .RegisterScenarios(
        createTransactionsScenario,
        summaryReadScenario,
        mixedScenario)
    .WithTestSuite("FinanceTracker")
    .WithTestName("PerformanceTests")
    .WithReportFormats(
        ReportFormat.Html,
        ReportFormat.Md)
    .WithReportFileName("performance-report")
    .WithReportFolder(testResultsDirectory)
    .Run();

static async Task<string> GetJwtTokenAsync(string baseAddress)
{
    // TODO: Implement real JWT token retrieval for your auth setup.
    // For now, this can be a static token or a call to an auth endpoint.
    await Task.CompletedTask;
    return string.Empty;
}

