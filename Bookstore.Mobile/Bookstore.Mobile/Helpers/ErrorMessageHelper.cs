using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace Bookstore.Mobile.Helpers
{
    public static class ErrorMessageHelper
    {
        public static string ToFriendlyErrorMessage(string? errorContent)
        {
            if (string.IsNullOrWhiteSpace(errorContent))
                return "An unknown error occurred.";

            // Try to parse as JSON
            try
            {
                var doc = JsonNode.Parse(errorContent);
                // ASP.NET Core validation error format
                if (doc is JsonObject obj)
                {
                    // 1. ValidationProblemDetails: { errors: { field: [msg] }, ... }
                    if (obj.TryGetPropertyValue("errors", out var errorsNode) && errorsNode is JsonObject errorsObj)
                    {
                        var friendlyMessages = new List<string>();
                        foreach (var kv in errorsObj)
                        {
                            var field = kv.Key;
                            if (kv.Value is JsonArray arr)
                            {
                                foreach (var msgNode in arr)
                                {
                                    var msg = msgNode?.ToString() ?? "";
                                    friendlyMessages.Add(MapToFriendlyMessage(field, msg));
                                }
                            }
                        }
                        return string.Join("\n", friendlyMessages);
                    }
                    // 2. ProblemDetails: { title, detail }
                    if (obj.TryGetPropertyValue("detail", out var detailNode) && detailNode is JsonValue)
                        return detailNode.ToString();
                    if (obj.TryGetPropertyValue("title", out var titleNode) && titleNode is JsonValue)
                        return titleNode.ToString();
                }
            }
            catch { /* Not JSON, fall through */ }

            // Fallback: try to map known error phrases
            return MapToFriendlyMessage(null, errorContent);
        }

        private static string MapToFriendlyMessage(string? field, string msg)
        {
            if (field == "Email" && msg.Contains("valid e-mail address"))
                return "Invalid email address. Please enter a valid email.";
            if (field == "Password" && msg.Contains("minimum length"))
                return "Password must be at least 6 characters.";
            if (field == "Password" && msg.Contains("maximum length"))
                return "Password cannot exceed 100 characters.";
            if (field == "UserName" && msg.Contains("minimum length"))
                return "Username must be at least 3 characters.";
            if (field == "UserName" && msg.Contains("maximum length"))
                return "Username cannot exceed 50 characters.";
            if (msg.Contains("already exists"))
                return "Username or email already exists. Please choose another.";
            if (msg.Contains("do not match"))
                return "Password and confirm password do not match.";
            if (msg.Contains("required"))
                return "Please fill in all required fields.";
            // Add more mappings as needed
            return msg;
        }
    }
} 