using System.Net.Http.Headers;
using System.Text.Json;
using VirtualPM.Models;

namespace VirtualPM.Services;

public class AsanaService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;
    private readonly string _workspaceGid;

    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AsanaService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _accessToken = configuration["Asana:AccessToken"] ?? throw new InvalidOperationException("Asana access token not configured");
        _workspaceGid = configuration["Asana:WorkspaceGid"];
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://app.asana.com/api/1.0/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<Dictionary<string, AsanaUser>> GetAllUsersAsDictionaryAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("users?opt_fields=gid,name,email");

        if (!response.IsSuccessStatusCode)
        {
            return new Dictionary<string, AsanaUser>();
        }

        string content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<AsanaApiResponse<List<AsanaUser>>>(content, _jsonOptions);

        if (result?.Data == null || result.Data.Count == 0)
        {
            return new Dictionary<string, AsanaUser>();
        }

        return result.Data.ToDictionary(u => u.Email, u =>  u);
    }

    public async Task<UserTaskMetrics> GetUserTaskMetricsAsync(string asanaUserGid, string userName, string email)
    {
        UserTaskMetrics metrics = new ()
        {
            UserId = asanaUserGid,
            UserName = userName,
            Email = email
        };
        
        string tasksUrl = $"tasks?assignee={asanaUserGid}&workspace={_workspaceGid}&completed_since=now&opt_fields=gid,name,completed,due_on&limit=100";
        HttpResponseMessage response = await _httpClient.GetAsync(tasksUrl);

        if (!response.IsSuccessStatusCode)
        {
            return metrics;
        }

        string content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AsanaApiResponse<List<AsanaTask>>>(content, _jsonOptions);
        var tasks = result?.Data ?? new List<AsanaTask>();

        metrics.TotalTaskCount = tasks.Count;

        DateTime today = DateTime.Today;

        foreach (AsanaTask task in tasks)
        {
            if (task.DueOn.HasValue)
            {
                DateTime dueDate = task.DueOn.Value;
                if (dueDate >= today)
                    metrics.HasFutureDueDateTasks = true;
                else
                    metrics.OverdueTaskCount++;
            }
        }

        return metrics;
    }

    private class AsanaApiResponse<T>
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
