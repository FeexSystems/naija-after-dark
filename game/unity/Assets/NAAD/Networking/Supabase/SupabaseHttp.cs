using System;
using System.Text;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Networking.Config;
using UnityEngine;
using UnityEngine.Networking;

namespace NAAD.Networking.Supabase
{
    /// <summary>
    /// Minimal HTTP helper for Supabase Auth + PostgREST.
    /// Uses anon key + optional user JWT only.
    /// </summary>
    public sealed class SupabaseHttp
    {
        private readonly SupabaseConfig _config;
        private readonly INAADLogger _log;

        public SupabaseHttp(SupabaseConfig config, INAADLogger log)
        {
            _config = config;
            _log = log;
        }

        public async Task<HttpResult> SendAsync(
            string url,
            string method,
            string? jsonBody,
            string? accessToken,
            params (string key, string value)[] extraHeaders)
        {
            using var req = new UnityWebRequest(url, method);
            if (!string.IsNullOrEmpty(jsonBody))
            {
                var body = Encoding.UTF8.GetBytes(jsonBody);
                req.uploadHandler = new UploadHandlerRaw(body);
            }
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("apikey", _config.anonKey);
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(accessToken))
                req.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            else
                req.SetRequestHeader("Authorization", $"Bearer {_config.anonKey}");

            foreach (var (key, value) in extraHeaders)
                req.SetRequestHeader(key, value);

            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            var code = (int)req.responseCode;
            var text = req.downloadHandler?.text ?? string.Empty;

            if (req.result != UnityWebRequest.Result.Success)
            {
                _log.Warn("Http", $"{method} {url} → {code} {text}");
                return new HttpResult(false, code, text);
            }

            return new HttpResult(true, code, text);
        }
    }

    public readonly struct HttpResult
    {
        public readonly bool Ok;
        public readonly int StatusCode;
        public readonly string Body;

        public HttpResult(bool ok, int statusCode, string body)
        {
            Ok = ok;
            StatusCode = statusCode;
            Body = body;
        }
    }
}
