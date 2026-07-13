using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace RentConnect.Web.Infrastructure;

// بديل عن asp-append-version (Tag Helper خاص بـ MVC ولا يعمل داخل مكوّنات Blazor .razor) -
// بيحسب بصمة (hash) من محتوى الملف الثابت ويرفقها كـ query string، فيتغيّر رابط الملف
// تلقائياً كل ما يتغيّر محتواه فعلياً، فيضمن إن المتصفح يحمّل النسخة الجديدة دايماً.
internal static class AssetVersion
{
    private static readonly ConcurrentDictionary<string, string> Cache = new();

    public static string Get(IWebHostEnvironment env, string relativePath)
    {
        var fileInfo = env.WebRootFileProvider.GetFileInfo(relativePath);
        if (!fileInfo.Exists) return "0";

        var cacheKey = $"{relativePath}|{fileInfo.LastModified.UtcTicks}";
        return Cache.GetOrAdd(cacheKey, _ =>
        {
            using var stream = fileInfo.CreateReadStream();
            var hash = SHA256.HashData(stream);
            return Convert.ToHexString(hash)[..10].ToLowerInvariant();
        });
    }
}
