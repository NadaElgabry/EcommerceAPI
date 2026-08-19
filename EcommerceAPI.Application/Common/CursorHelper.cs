using EcommerceAPI.Application.Exceptions;
using System;
using System.Text;
using System.Text.Json;

namespace EcommerceAPI.Application.Common
{
    public static class CursorHelper
    {
        public static string Encode<T>(T payload)
        {
            var json = JsonSerializer.Serialize(payload);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public static T Decode<T>(string cursor)
        {
            var buffer = new byte[cursor.Length];

            if (!Convert.TryFromBase64String(cursor, buffer, out int bytesWritten))
            {
                throw new BadRequestException("Invalid cursor.");
            }

            var json = Encoding.UTF8.GetString(buffer, 0, bytesWritten);

            try
            {
                return JsonSerializer.Deserialize<T>(json)
                    ?? throw new BadRequestException("Invalid cursor.");
            }
            catch (JsonException)
            {
                throw new BadRequestException("Invalid cursor.");
            }
        }
    }
}