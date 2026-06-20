using System.Text;
using System.Threading.Tasks;
using MirraCloud.Core;
using MirraCloud.Core.Realtime.Protocol;
using Plugins.MirraCloud.Core.General.AsyncOperations;

namespace MirraCloud.Example.Sandbox
{
    /// <summary>
    /// Awaits the SDK's uniform <see cref="AsyncOperation{T}"/> + <see cref="RestApiResult"/>
    /// shape and normalizes it into an <see cref="OpResult"/>. The SDK never throws on HTTP —
    /// failures are values — so everything here is branch-on-value, not try/catch.
    /// </summary>
    public static class SandboxOps
    {
        public static async Task<OpResult> Run<T>(AsyncOperation<RestApiResult<T>> op)
        {
            if (op == null)
            {
                return new OpResult { Ok = false, Status = "FAIL · no operation", Body = "(operation was null)" };
            }

            await op.Task();
            return From(op.Result);
        }

        public static async Task<OpResult> Run(AsyncOperation<RestApiResult> op)
        {
            if (op == null)
            {
                return new OpResult { Ok = false, Status = "FAIL · no operation", Body = "(operation was null)" };
            }

            await op.Task();
            return From(op.Result);
        }

        // ---- realtime (Chats): RealtimeResult instead of RestApiResult ----

        public static async Task<OpResult> RunRt(AsyncOperation<RealtimeResult> op)
        {
            if (op == null)
            {
                return new OpResult { Ok = false, Status = "FAIL · no operation", Body = "(operation was null)" };
            }

            await op.Task();
            return FromRt(op.Result, null);
        }

        public static async Task<OpResult> RunRt<T>(AsyncOperation<RealtimeResult<T>> op)
        {
            if (op == null)
            {
                return new OpResult { Ok = false, Status = "FAIL · no operation", Body = "(operation was null)" };
            }

            await op.Task();
            var r = op.Result;
            return FromRt(r, r != null ? (object)r.Data : null);
        }

        private static OpResult FromRt(RealtimeResult r, object data)
        {
            if (r == null)
            {
                return new OpResult { Ok = false, Status = "FAIL · null result", Body = "(null result)" };
            }

            string status = (r.IsSuccess ? "OK" : "FAIL") + " · realtime"
                + (string.IsNullOrEmpty(r.Code) ? "" : " · " + r.Code);
            string body;
            if (r.IsSuccess)
            {
                body = data != null ? SafeJson(data) : (string.IsNullOrEmpty(r.Message) ? "(ok)" : r.Message);
            }
            else
            {
                body = "Code: " + r.Code + "\nMessage: " + r.Message;
            }

            return new OpResult { Ok = r.IsSuccess, Status = status, Body = body, Method = "WS" };
        }

        public static string SafeJson(object o)
        {
            if (o == null) return "(null)";
            try { return JsonPretty.Format(UnityEngine.JsonUtility.ToJson(o)); }
            catch { return o.ToString(); }
        }

        public static OpResult From(RestApiResult r)
        {
            if (r == null)
            {
                return new OpResult { Ok = false, Status = "FAIL · null result", Body = "(null result)" };
            }

            string http = r.HttpStatusCode.HasValue ? r.HttpStatusCode.Value.ToString() : "-";
            string status = $"{(r.IsSuccess ? "OK" : "FAIL")} · HTTP {http} · {r.DurationMs}ms · retries {r.RetryCount}";

            string body;
            if (r.IsSuccess)
            {
                body = JsonPretty.Format(r.ResponseBody);
            }
            else
            {
                var sb = new StringBuilder();
                var e = r.Error;
                if (e != null)
                {
                    sb.Append("Type: ").Append(e.Type).Append('\n');
                    if (!string.IsNullOrEmpty(e.Message))
                    {
                        sb.Append("Message: ").Append(e.Message).Append('\n');
                    }
                }
                sb.Append("\n--- response body ---\n").Append(JsonPretty.Format(r.ResponseBody));
                body = sb.ToString();
            }

            return new OpResult
            {
                Ok = r.IsSuccess,
                Status = status,
                Body = body,
                Method = r.Method,
                Route = r.Route,
                Http = r.HttpStatusCode,
                DurationMs = r.DurationMs,
            };
        }
    }

    /// <summary>
    /// Dependency-free JSON re-indenter for display. Works directly on the raw server
    /// <c>ResponseBody</c> so QA sees exactly what the backend returned.
    /// </summary>
    public static class JsonPretty
    {
        public static string Format(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return "(empty body)";
            }

            var sb = new StringBuilder(json.Length + 64);
            int indent = 0;
            bool inStr = false;
            bool esc = false;

            foreach (char c in json)
            {
                if (esc)
                {
                    sb.Append(c);
                    esc = false;
                    continue;
                }

                if (inStr)
                {
                    sb.Append(c);
                    if (c == '\\') esc = true;
                    else if (c == '"') inStr = false;
                    continue;
                }

                switch (c)
                {
                    case '"':
                        inStr = true;
                        sb.Append(c);
                        break;
                    case '{':
                    case '[':
                        sb.Append(c).Append('\n').Append(' ', (++indent) * 2);
                        break;
                    case '}':
                    case ']':
                        indent = indent > 0 ? indent - 1 : 0;
                        sb.Append('\n').Append(' ', indent * 2).Append(c);
                        break;
                    case ',':
                        sb.Append(c).Append('\n').Append(' ', indent * 2);
                        break;
                    case ':':
                        sb.Append(": ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(c)) sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Tolerant string→primitive parsing for field values (invariant culture, never throws).
    /// </summary>
    public static class SandboxParse
    {
        public static int Int(string s)
        {
            return int.TryParse(s, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
        }

        public static double Double(string s)
        {
            return double.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0d;
        }

        public static bool Bool(string s)
        {
            return bool.TryParse(s, out var v) && v;
        }
    }
}
