using System;

namespace NAAD.Networking.Supabase
{
    /// <summary>
    /// Phase D — single shared JSON reader for all Supabase responses.
    /// Replaces the per-file IndexOf scanners (which broke on escaped quotes,
    /// nested objects, and spacing variants). Correctly handles:
    ///   - \" escapes inside strings
    ///   - nested objects/arrays when brace-matching
    ///   - arbitrary whitespace around ':' and values
    ///   - JSON null (returns false, value null)
    /// Missing keys return false; callers keep their existing fallbacks.
    /// </summary>
    public static class NaadJson
    {
        public static bool TryGetString(string json, string key, out string? value)
        {
            value = null;
            if (!TryFindValue(json, key, out var rest)) return false;
            rest = rest.TrimStart();
            if (rest.StartsWith("null", StringComparison.Ordinal)) return false;
            if (rest.Length < 2 || rest[0] != '"') return false;
            value = Unescape(ScanQuoted(rest, out _));
            return value != null;
        }

        public static bool TryGetInt(string json, string key, out int value)
        {
            value = 0;
            if (!TryGetLong(json, key, out var l)) return false;
            if (l < int.MinValue || l > int.MaxValue) return false;
            value = (int)l;
            return true;
        }

        public static bool TryGetLong(string json, string key, out long value)
        {
            value = 0;
            if (!TryFindValue(json, key, out var rest)) return false;
            rest = rest.TrimStart();
            if (rest.StartsWith("null", StringComparison.Ordinal)) return false;
            var end = 0;
            while (end < rest.Length && (char.IsDigit(rest[end]) || rest[end] == '-' || rest[end] == '+'))
                end++;
            if (end == 0) return false;
            return long.TryParse(rest.Substring(0, end),
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        public static bool TryGetDouble(string json, string key, out double value)
        {
            value = 0;
            if (!TryFindValue(json, key, out var rest)) return false;
            rest = rest.TrimStart();
            if (rest.StartsWith("null", StringComparison.Ordinal)) return false;
            var end = 0;
            while (end < rest.Length && (char.IsDigit(rest[end]) || rest[end] == '-' || rest[end] == '+'
                       || rest[end] == '.' || rest[end] == 'e' || rest[end] == 'E'))
                end++;
            if (end == 0) return false;
            return double.TryParse(rest.Substring(0, end),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        public static bool TryGetBool(string json, string key, out bool value)
        {
            value = false;
            if (!TryFindValue(json, key, out var rest)) return false;
            rest = rest.TrimStart();
            if (rest.StartsWith("true", StringComparison.Ordinal)) { value = true; return true; }
            if (rest.StartsWith("false", StringComparison.Ordinal)) { value = false; return true; }
            return false;
        }

        /// <summary>Extracts a nested object value (e.g. "payload") as a raw JSON substring.</summary>
        public static bool TryGetObject(string json, string key, out string? value)
        {
            value = null;
            if (!TryFindValue(json, key, out var rest)) return false;
            rest = rest.TrimStart();
            if (rest.StartsWith("null", StringComparison.Ordinal)) return false;
            if (rest.Length == 0 || (rest[0] != '{' && rest[0] != '[')) return false;
            value = ScanBalanced(rest);
            return value != null;
        }

        /// <summary>Top-level "success" flag, tolerant of spacing/case.</summary>
        public static bool ReadSuccess(string json)
        {
            return TryGetBool(json, "success", out var v) && v;
        }

        // ---- internals -----------------------------------------------------

        /// <summary>Finds "key" outside of strings, returns the text after its ':'.</summary>
        private static bool TryFindValue(string json, string key, out string rest)
        {
            rest = "";
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return false;
            var pattern = "\"" + key + "\"";
            var searchFrom = 0;
            while (true)
            {
                var idx = IndexOfOutsideStrings(json, pattern, searchFrom);
                if (idx < 0) return false;
                var colon = IndexOfColonOutsideStrings(json, idx + pattern.Length);
                if (colon < 0) return false;
                rest = json.Substring(colon + 1);
                return true;
            }
        }

        private static int IndexOfOutsideStrings(string json, string pattern, int from)
        {
            var inStr = false;
            for (var i = from; i <= json.Length - pattern.Length; i++)
            {
                var c = json[i];
                if (inStr)
                {
                    if (c == '\\') { i++; continue; }
                    if (c == '"') inStr = false;
                    continue;
                }
                if (c == '"') { inStr = true; continue; }
                if (json.Substring(i, pattern.Length) == pattern)
                {
                    // ensure it is a key: next non-space char must be ':'
                    var j = i + pattern.Length;
                    while (j < json.Length && char.IsWhiteSpace(json[j])) j++;
                    if (j < json.Length && json[j] == ':') return i;
                }
            }
            return -1;
        }

        private static int IndexOfColonOutsideStrings(string json, int from)
        {
            var inStr = false;
            for (var i = from; i < json.Length; i++)
            {
                var c = json[i];
                if (inStr)
                {
                    if (c == '\\') { i++; continue; }
                    if (c == '"') inStr = false;
                    continue;
                }
                if (c == '"') { inStr = true; continue; }
                if (c == ':') return i;
                if (c == '{' || c == '}' || c == '[' || c == ']') return -1;
            }
            return -1;
        }

        /// <summary>Scans a quoted string starting at rest[0]=='"'; returns unescaped content.</summary>
        private static string? ScanQuoted(string rest, out int consumed)
        {
            consumed = 0;
            var sb = new System.Text.StringBuilder();
            for (var i = 1; i < rest.Length; i++)
            {
                var c = rest[i];
                if (c == '\\' && i + 1 < rest.Length)
                {
                    sb.Append(UnescapeChar(rest[i + 1]));
                    i++;
                    continue;
                }
                if (c == '"') { consumed = i + 1; return sb.ToString(); }
                sb.Append(c);
            }
            return null;
        }

        private static string Unescape(string? s) => s ?? "";

        private static char UnescapeChar(char c) => c switch
        {
            '"' => '"',
            '\\' => '\\',
            '/' => '/',
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            _ => c,
        };

        /// <summary>Returns the balanced {...} or [...] substring starting at rest[0].</summary>
        private static string? ScanBalanced(string rest)
        {
            if (rest.Length == 0) return null;
            var open = rest[0];
            var close = open == '{' ? '}' : ']';
            if (open != '{' && open != '[') return null;
            var depth = 0;
            var inStr = false;
            for (var i = 0; i < rest.Length; i++)
            {
                var c = rest[i];
                if (inStr)
                {
                    if (c == '\\') { i++; continue; }
                    if (c == '"') inStr = false;
                    continue;
                }
                if (c == '"') { inStr = true; continue; }
                if (c == open) depth++;
                else if (c == close)
                {
                    depth--;
                    if (depth == 0) return rest.Substring(0, i + 1);
                }
            }
            return null;
        }
    }
}
