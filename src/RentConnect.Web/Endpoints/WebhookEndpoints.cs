using System.Text.Json;
using RentConnect.Data.Models;
using RentConnect.Data.UnitOfWork;
using RentConnect.Web.Services;

namespace RentConnect.Web.Endpoints;

// يستقبل أحداث الرسائل الواردة من Evolution API (Webhook) - نستخدمه فقط لمتابعة رد صاحب
// العقار على سؤال "هل تم تأجير العقار؟" اللي بينرسل تلقائياً لما مستخدم يلغي رسم كشف رقم.
public static class WebhookEndpoints
{
    private static readonly string[] YesWords = ["نعم", "ايوه", "أيوه", "اي", "أي", "yes", "y"];
    private static readonly string[] NoWords = ["لا", "لأ", "no", "n"];

    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/webhooks/evolution", async (
            HttpContext http,
            IUnitOfWork unitOfWork,
            IEvolutionApiClient evoClient) =>
        {
            JsonDocument doc;
            try
            {
                doc = await JsonDocument.ParseAsync(http.Request.Body);
            }
            catch (JsonException)
            {
                return Results.Ok();
            }

            using (doc)
            {
                var root = doc.RootElement;

                // Evolution API بيبعت أنواع أحداث كتيرة (connection.update، contacts.upsert، chats.upsert...)
                // على نفس رابط الـ webhook العام - نهتم فقط بأحداث الرسائل الواردة، وأي حدث تاني نتجاهله
                // بهدوء (Results.Ok) بدل ما نحاول نفسّره كرسالة ونطيح بـ exception.
                if (root.TryGetProperty("event", out var eventEl) && eventEl.ValueKind == JsonValueKind.String &&
                    !string.Equals(eventEl.GetString(), "messages.upsert", StringComparison.OrdinalIgnoreCase))
                {
                    return Results.Ok();
                }

                if (!root.TryGetProperty("data", out var data)) return Results.Ok();

                // شكل "data" ممكن يكون رسالة وحدة (object) أو أكتر من رسالة دفعة وحدة (array) -
                // نعالج كل رسالة بنفس المنطق ونوقف عند أول ردّ متطابق نعالجه.
                IEnumerable<JsonElement> messages = data.ValueKind switch
                {
                    JsonValueKind.Object => [data],
                    JsonValueKind.Array => data.EnumerateArray(),
                    _ => []
                };

                foreach (var message in messages)
                {
                    if (await ProcessMessageAsync(message, unitOfWork, evoClient))
                    {
                        break;
                    }
                }
            }

            return Results.Ok();
        });

        return endpoints;
    }

    // بيعالج رسالة واحدة واردة؛ بيرجّع true لو لقى ردّ "نعم/لا" متطابق وعالجه فعلياً
    private static async Task<bool> ProcessMessageAsync(JsonElement data, IUnitOfWork unitOfWork, IEvolutionApiClient evoClient)
    {
        if (data.ValueKind != JsonValueKind.Object) return false;

        if (!data.TryGetProperty("key", out var key) || key.ValueKind != JsonValueKind.Object) return false;

        // ملاحظة: تجاهلنا فلترة "fromMe" عمداً - لو صاحب العقار يستخدم نفس رقم الـ instance
        // (محادثة مع نفسه وقت الاختبار)، واتساب بيعتبر كل الرسائل fromMe=true بغض النظر
        // مين كتبها فعلياً. الحماية الحقيقية من مطابقة رسائلنا نحن هي إن ردّ "نعم/لا" لازم
        // يكون مطابقة تامة لكلمة واحدة فقط - رسائل البوت دائماً جمل كاملة فما بتطابق أبداً.
        var remoteJid = key.TryGetProperty("remoteJid", out var jidEl) ? jidEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(remoteJid)) return false;

        var ownerPhone = remoteJid.Split('@')[0];

        var text = ExtractMessageText(data);
        if (string.IsNullOrWhiteSpace(text)) return false;

        var normalized = text.Trim().TrimEnd('.', '!', '؟', '?');

        bool? answer = YesWords.Any(w => string.Equals(w, normalized, StringComparison.OrdinalIgnoreCase)) ? true
            : NoWords.Any(w => string.Equals(w, normalized, StringComparison.OrdinalIgnoreCase)) ? false
            : null;

        if (answer is null) return false;

        var listingId = await unitOfWork.OwnerConfirmations.AnswerLatestPendingAsync(ownerPhone, answer.Value);
        if (listingId is null) return false;

        string replyText;
        if (answer.Value)
        {
            await unitOfWork.Listings.UpdateStatusAsync(listingId.Value, ListingStatus.Rented);
            replyText = "تم تسجيل أن العقار مؤجّر، وتحديث حالة الإعلان تلقائياً. شكراً لك.";
        }
        else
        {
            // تأكيد إنه لسا متوفر - بنجدّد تاريخ "آخر تأكيد" حتى ما تنعاد نفس رسالة
            // التحقق الدوري بعد قليل بدون داعي
            await unitOfWork.Listings.ConfirmStillAvailableAsync(listingId.Value);
            replyText = "تم استلام ردّك، شكراً لك.";
        }

        await unitOfWork.CompleteAsync();

        var settings = await unitOfWork.Settings.GetAsync();
        if (!string.IsNullOrWhiteSpace(settings.EvolutionApiBaseUrl) &&
            !string.IsNullOrWhiteSpace(settings.EvolutionApiKey) &&
            !string.IsNullOrWhiteSpace(settings.EvolutionApiInstanceName))
        {
            await evoClient.SendTextMessageAsync(
                settings.EvolutionApiBaseUrl!, settings.EvolutionApiKey!, settings.EvolutionApiInstanceName!,
                ownerPhone, replyText);
        }

        return true;
    }

    // بيدوّر عن نص الرسالة بأكتر من مكان محتمل - Evolution API/Baileys بيرجّع أنواع مختلفة
    // من الرسائل (conversation، extendedTextMessage، إلخ)
    private static string? ExtractMessageText(JsonElement data)
    {
        if (!data.TryGetProperty("message", out var message)) return null;

        if (message.TryGetProperty("conversation", out var conversation) && conversation.ValueKind == JsonValueKind.String)
            return conversation.GetString();

        if (message.TryGetProperty("extendedTextMessage", out var extended) &&
            extended.TryGetProperty("text", out var extendedText) && extendedText.ValueKind == JsonValueKind.String)
            return extendedText.GetString();

        return null;
    }
}
