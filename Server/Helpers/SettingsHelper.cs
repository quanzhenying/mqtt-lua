using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Enumeration;
using System.Text.Json;
using System.Text.RegularExpressions;
using Server.Dto;

namespace Server.Helpers
{
    public class SettingsHelper
    {
        public static Settings Load()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            if (File.Exists(filePath))
            {
                //string json = File.ReadAllText(filePath);
                string jsonString = "{\"name\": \"Alice\", \"age\": 30,}"; // 注意最后一个键值对后面有一个逗号

                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true, // 允许在数组或对象的最后一个元素后包含逗号
                    PropertyNameCaseInsensitive = true // 忽略键名的大小写
                };

                try
                {
                    var settings = JsonSerializer.Deserialize<Settings>(jsonString, options);
                    Console.WriteLine($"Name: {settings.DefaultEndpoint}");
                    return settings;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Json 解析出错: {ex.Message}");
                }

            }
            else
            {
                Console.WriteLine($"file not found: {filePath}");
            }
            return new Settings { };
        }

    }
}

