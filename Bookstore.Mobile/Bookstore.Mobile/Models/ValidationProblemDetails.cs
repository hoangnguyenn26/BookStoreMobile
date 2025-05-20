using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bookstore.Mobile.Models
{
    /// <summary>
    /// Lớp biểu diễn thông báo lỗi validation theo chuẩn RFC 7807 (Problem Details)
    /// </summary>
    public class ValidationProblemDetails
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("errors")]
        public Dictionary<string, string[]>? Errors { get; set; }

        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        [JsonPropertyName("traceId")]
        public string? TraceId { get; set; }

        /// <summary>
        /// Phân tích chuỗi JSON thành đối tượng ValidationProblemDetails
        /// </summary>
        public static ValidationProblemDetails? Parse(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<ValidationProblemDetails>(json, options);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Chuyển đổi thông báo lỗi validation thành các thông báo thân thiện với người dùng
        /// </summary>
        public List<string> GetFriendlyMessages()
        {
            var messages = new List<string>();

            if (Errors == null || !Errors.Any())
            {
                if (!string.IsNullOrEmpty(Detail))
                {
                    messages.Add(Detail);
                }
                else if (!string.IsNullOrEmpty(Title))
                {
                    messages.Add(Title);
                }
                return messages;
            }

            foreach (var field in Errors)
            {
                string fieldName = GetFieldDisplayName(field.Key);

                foreach (var error in field.Value)
                {
                    string friendlyMessage = GetFriendlyErrorMessage(field.Key, error);
                    messages.Add(friendlyMessage);
                }
            }

            return messages;
        }

        /// <summary>
        /// Chuyển đổi tên trường thành dạng thân thiện với người dùng
        /// </summary>
        private string GetFieldDisplayName(string fieldName)
        {
            return fieldName switch
            {
                "UserName" => "Tên đăng nhập",
                "Email" => "Email",
                "Password" => "Mật khẩu",
                "ConfirmPassword" => "Xác nhận mật khẩu",
                "FirstName" => "Tên",
                "LastName" => "Họ",
                "PhoneNumber" => "Số điện thoại",
                _ => fieldName
            };
        }

        /// <summary>
        /// Chuyển đổi thông báo lỗi thành dạng thân thiện với người dùng
        /// </summary>
        private string GetFriendlyErrorMessage(string fieldName, string errorMessage)
        {
            string friendlyFieldName = GetFieldDisplayName(fieldName);

            // Xử lý các loại lỗi validation phổ biến
            if (errorMessage.Contains("is not a valid e-mail address"))
            {
                return "Email không hợp lệ. Vui lòng nhập đúng định dạng email.";
            }
            else if (errorMessage.Contains("minimum length") && fieldName == "Password")
            {
                return "Mật khẩu phải có ít nhất 6 ký tự.";
            }
            else if (errorMessage.Contains("maximum length") && fieldName == "Password")
            {
                return "Mật khẩu không được vượt quá 100 ký tự.";
            }
            else if (errorMessage.Contains("minimum length") && fieldName == "UserName")
            {
                return "Tên đăng nhập phải có ít nhất 3 ký tự.";
            }
            else if (errorMessage.Contains("maximum length") && fieldName == "UserName")
            {
                return "Tên đăng nhập không được vượt quá 50 ký tự.";
            }
            else if (errorMessage.Contains("already exists"))
            {
                return $"{friendlyFieldName} đã tồn tại trong hệ thống. Vui lòng chọn {friendlyFieldName.ToLower()} khác.";
            }
            else if (errorMessage.Contains("do not match"))
            {
                return "Mật khẩu và xác nhận mật khẩu không khớp.";
            }
            else if (errorMessage.Contains("required"))
            {
                return $"{friendlyFieldName} là trường bắt buộc.";
            }
            else
            {
                return $"{friendlyFieldName}: {errorMessage}";
            }
        }
    }
}