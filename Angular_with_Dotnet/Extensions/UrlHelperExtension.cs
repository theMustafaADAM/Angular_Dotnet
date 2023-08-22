using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Angular_with_Dotnet.Extensions
{
	public static class UrlHelperExtension
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
	{
		public static string AbsoluteUrl(this IHttpContextAccessor httpContextAccessor, string relativeUrl, object? parameters = null)
		{

            var request = httpContextAccessor.HttpContext.Request;
            var url = new Uri(new Uri($"{request.Scheme}://{request.Host.Value}"), relativeUrl).ToString();
            if (parameters != null) url = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(url, queryString: ToDictionary(parameters));
            return url;

		}

		private static Dictionary<string, string> ToDictionary(object obj)
		{
			var json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

    }
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
}

