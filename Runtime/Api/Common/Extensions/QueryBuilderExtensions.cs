using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Instasounds.Api
{
    public static class QueryBuilderExtensions
    {
        public static string GenerateQueryString<T>(this T queryParams)
        {
            var properties = queryParams.GetType().GetProperties()
                .Where(property => property.GetValue(queryParams, null) != null)
                .ToDictionary(GetPropertyName, property => property.GetValue(queryParams, null));

            if (properties.Count == 0)
                return string.Empty;

            var queryString = new StringBuilder();
            queryString.Append('?');

            foreach (var (key, value) in properties)
            {
                if (value is IDictionary dictionary)
                {
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        queryString
                            .Append($"{Uri.EscapeDataString(entry.Key.ToString())}={Uri.EscapeDataString(entry.Value.ToString())}&");
                    }
                }
                else if (value is IEnumerable<string> stringArray)
                {
                    // Handle arrays of strings by appending multiple key-value pairs
                    foreach (var stringValue in stringArray)
                    {
                        queryString.Append($"{Uri.EscapeDataString(key.ToString())}={Uri.EscapeDataString(stringValue)}&");
                    }
                }
                else
                {
                    queryString.Append($"{Uri.EscapeDataString(key.ToString())}={Uri.EscapeDataString(value.ToString())}&");
                }
            }

            return queryString.ToString();
        }

        private static string GetPropertyName(MemberInfo prop)
        {
            return prop.GetCustomAttribute<JsonPropertyAttribute>() != null
                ? prop.GetCustomAttribute<JsonPropertyAttribute>().PropertyName
                : prop.Name;
        }
    }
}
