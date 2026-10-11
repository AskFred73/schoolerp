using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SchoolERP.Infrastructure;

namespace SchoolERP.Api.Tests;

public class ApiEndpointTests : IClassFixture<SchoolErpApiFactory>
{
    private readonly SchoolErpApiFactory _factory;
    private readonly HttpClient _client;

    public ApiEndpointTests(SchoolErpApiFactory factory)
    {
        _factory = factory;
        factory.EnsureInitialized();
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "accountant@school.local", password = "Demo@123" });
        login.EnsureSuccessStatusCode();
        var doc = await login.Content.ReadFromJsonAsync<JsonElement>();
        var token = doc.GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", DemoIds.TenantId.ToString());
    }

    [Fact]
    public async Task DashboardSummary_ReturnsKpis()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/analytics/dashboard-summary");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3420, json.GetProperty("enrollment").GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task Student360_ReturnsMaskedAadhaar()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/students/STU-2026-0842/360");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("XXXX-XXXX-4812", json.GetProperty("header").GetProperty("aadhaarMasked").GetString());
    }

    [Fact]
    public async Task Timetable_ReturnsConflicts()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync($"/api/v1/academics/timetable/{DemoIds.ClassXAId}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("conflicts").GetArrayLength() > 0);
    }

    [Fact]
    public async Task PosCollect_UpdatesBalance()
    {
        await AuthenticateAsync();
        var body = new
        {
            admissionNo = "STU-2026-0842",
            invoiceIds = new[] { DemoIds.Invoice4412Id },
            channel = "UPI",
            amount = 1000m,
            waiveLateFine = false
        };
        var response = await _client.PostAsJsonAsync("/api/v1/finance/pos/collect", body);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("amountPaid").GetDecimal() == 1000m);
    }

    [Fact]
    public async Task SubstitutionAllocate_SucceedsForAcademicCoordinator()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "academic@school.local", password = "Demo@123" });
        login.EnsureSuccessStatusCode();
        var loginDoc = await login.Content.ReadFromJsonAsync<JsonElement>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginDoc.GetProperty("token").GetString());
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", DemoIds.TenantId.ToString());

        var timetable = await _client.GetAsync($"/api/v1/academics/timetable/{DemoIds.ClassXAId}");
        timetable.EnsureSuccessStatusCode();
        var tt = await timetable.Content.ReadFromJsonAsync<JsonElement>();
        var slotId = tt.GetProperty("slots")[0].GetProperty("id").GetGuid();

        var response = await _client.PostAsJsonAsync("/api/v1/academics/substitutions/allocate", new
        {
            timetableSlotId = slotId,
            substituteTeacherId = DemoIds.TeacherNarayananId,
            reason = "Unit test allocation"
        });
        response.EnsureSuccessStatusCode();
    }
}
