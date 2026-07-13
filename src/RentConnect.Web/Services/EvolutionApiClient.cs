using System.Text;
using System.Text.Json;

namespace RentConnect.Web.Services;

public record EvolutionInstanceInfo(string Name, string ConnectionStatus, string? OwnerNumber, string? ProfileName);

public interface IEvolutionApiClient
{
    Task<(bool Success, List<EvolutionInstanceInfo> Instances, string? Error)> ListInstancesAsync(string baseUrl, string apiKey);

    Task<(bool Success, string? QrCodeBase64, string? Error)> CreateInstanceAsync(string baseUrl, string apiKey, string instanceName);

    Task<(bool Success, string? QrCodeBase64, string? Error)> GetQrCodeAsync(string baseUrl, string apiKey, string instanceName);

    Task<(bool Success, string? Error)> DeleteInstanceAsync(string baseUrl, string apiKey, string instanceName);

    Task<(bool Success, string? Error)> SendTextMessageAsync(string baseUrl, string apiKey, string instanceName, string whatsAppNumber, string text);
}

// عميل HTTP بسيط لـ Evolution API (بوابة واتساب مفتوحة المصدر تعمل بحاوية Docker خاصة بالمستخدم).
// مبني ومتحقّق منه فعلياً مقابل نسخة 2.3.7 لعمليتي fetchInstances وmessage/sendText، أما
// create/connect/delete فمبنية على توثيق Evolution API v2 وتُعيد نص الاستجابة الخام عند أي
// شكل غير متوقّع حتى تظهر تفاصيل مفيدة بلوحة الأدمن بدل فشل صامت.
internal class EvolutionApiClient : IEvolutionApiClient
{
    private readonly HttpClient _http;

    public EvolutionApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, List<EvolutionInstanceInfo> Instances, string? Error)> ListInstancesAsync(string baseUrl, string apiKey)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Get, baseUrl, "/instance/fetchInstances", apiKey);
            using var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return (false, new List<EvolutionInstanceInfo>(), $"({(int)response.StatusCode}) {body}");
            }

            using var doc = JsonDocument.Parse(body);
            var instances = new List<EvolutionInstanceInfo>();

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    var name = GetString(item, "name") ?? GetString(item, "instanceName") ?? "?";
                    var status = GetString(item, "connectionStatus") ?? GetString(item, "state") ?? "unknown";
                    var ownerJid = GetString(item, "ownerJid");
                    var ownerNumber = ownerJid?.Split('@').FirstOrDefault();
                    var profileName = GetString(item, "profileName");

                    instances.Add(new EvolutionInstanceInfo(name, status, ownerNumber, profileName));
                }
            }

            return (true, instances, null);
        }
        catch (Exception ex)
        {
            return (false, new List<EvolutionInstanceInfo>(), ex.Message);
        }
    }

    public async Task<(bool Success, string? QrCodeBase64, string? Error)> CreateInstanceAsync(string baseUrl, string apiKey, string instanceName)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Post, baseUrl, "/instance/create", apiKey);
            request.Content = JsonContent(new { instanceName, qrcode = true, integration = "WHATSAPP-BAILEYS" });

            using var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return (false, null, $"({(int)response.StatusCode}) {body}");
            }

            var qr = ExtractQrCode(body);
            return (true, qr, qr is null ? $"تم إنشاء الـ instance لكن ما لقيت رمز QR بالاستجابة: {body}" : null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? QrCodeBase64, string? Error)> GetQrCodeAsync(string baseUrl, string apiKey, string instanceName)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Get, baseUrl, $"/instance/connect/{Uri.EscapeDataString(instanceName)}", apiKey);
            using var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return (false, null, $"({(int)response.StatusCode}) {body}");
            }

            var qr = ExtractQrCode(body);
            return (true, qr, qr is null ? $"الـ instance متصلة أصلاً أو ما رجّعت رمز QR: {body}" : null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> DeleteInstanceAsync(string baseUrl, string apiKey, string instanceName)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Delete, baseUrl, $"/instance/delete/{Uri.EscapeDataString(instanceName)}", apiKey);
            using var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode) return (true, null);

            var body = await response.Content.ReadAsStringAsync();
            return (false, $"({(int)response.StatusCode}) {body}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SendTextMessageAsync(string baseUrl, string apiKey, string instanceName, string whatsAppNumber, string text)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Post, baseUrl, $"/message/sendText/{Uri.EscapeDataString(instanceName)}", apiKey);
            request.Content = JsonContent(new { number = whatsAppNumber, text });

            using var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode) return (true, null);

            var body = await response.Content.ReadAsStringAsync();
            return (false, $"({(int)response.StatusCode}) {body}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static HttpRequestMessage BuildRequest(HttpMethod method, string baseUrl, string path, string apiKey)
    {
        var request = new HttpRequestMessage(method, $"{baseUrl.TrimEnd('/')}{path}");
        request.Headers.Add("apikey", apiKey);
        return request;
    }

    private static StringContent JsonContent(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    // بيدوّر عن رمز الـ QR بأكتر من مكان محتمل بالاستجابة (تختلف بنية الاستجابة بين إصدارات Evolution API)
    private static string? ExtractQrCode(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("qrcode", out var qrcodeObj) && qrcodeObj.ValueKind == JsonValueKind.Object)
            {
                var nested = GetString(qrcodeObj, "base64");
                if (nested is not null) return nested;
            }

            var direct = GetString(root, "base64");
            if (direct is not null) return direct;

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
