using System;
using System.Text;
using UnityEngine;

namespace Novels.Diagnostics
{
    internal sealed class SmokeTelemetry
    {
        internal const string Prefix = "[NOVELS_SMOKE] ";

        private readonly Action<(LogType type, string message)> _onLog;
        private readonly string _runId = Guid.NewGuid().ToString("N");
        private int _sequence;

        internal SmokeTelemetry(Action<(LogType type, string message)> onLog)
        {
            _onLog = onLog;
        }

        internal void Emit(
            string eventName,
            params (string key, string value)[] fields)
        {
            if (_onLog == null || string.IsNullOrWhiteSpace(eventName))
                return;

            var json = new StringBuilder(160);
            json.Append("{\"v\":1,\"seq\":")
                .Append(++_sequence)
                .Append(",\"runId\":");
            AppendString(json, _runId);
            json.Append(",\"event\":");
            AppendString(json, eventName);
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    if (string.IsNullOrWhiteSpace(field.key)
                        || string.IsNullOrWhiteSpace(field.value))
                        continue;
                    json.Append(',');
                    AppendString(json, field.key);
                    json.Append(':');
                    AppendString(json, field.value);
                }
            }
            json.Append('}');
            _onLog((LogType.Log, Prefix + json));
        }

        private static void AppendString(StringBuilder target, string value)
        {
            target.Append('"');
            foreach (var character in value ?? string.Empty)
            {
                switch (character)
                {
                    case '"': target.Append("\\\""); break;
                    case '\\': target.Append("\\\\"); break;
                    case '\n': target.Append("\\n"); break;
                    case '\r': target.Append("\\r"); break;
                    case '\t': target.Append("\\t"); break;
                    default:
                        if (character < ' ')
                            target.Append("\\u").Append(((int)character).ToString("x4"));
                        else
                            target.Append(character);
                        break;
                }
            }
            target.Append('"');
        }
    }
}
