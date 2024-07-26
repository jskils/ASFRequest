using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.IPC.Responses;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web;
using ArchiSteamFarm.Web.Responses;
using ASFRequest.Request;

namespace ASFRequest.Web;

internal static class WebRequest {
	private static readonly Dictionary<string, string> UserAgentHeader = new(5) {
		{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36" },
		{ "Sec-Ch-Ua", "\"Not.A/Brand\";v=\"8\", \"Chromium\";v=\"114\", \"Google Chrome\";v=\"114\"" },
		{ "Sec-Ch-Ua-Mobile", "?0" },
		{ "Sec-Ch-Ua-Platform", "\"Windows\"" },
		{ "Accept-Language", "zh-CN,zh;q=0.9" }
	};

	private static Uri SteamApiURL => new("https://api.steampowered.com");
	private static JsonSerializerOptions JsonOptions => JsonUtilities.DefaultJsonSerialierOptions;

	internal static async Task<GenericResponse<HttpStatusCode>?> GetState(Bot bot, BaseRequest param) {
		Uri request = param.Url!;
		Uri referer = param.Referer ?? ArchiWebHandler.SteamCommunityURL;
		Dictionary<string, string> headers = MergeHeaders(param.Headers);
		HtmlDocumentResponse? response;

		if (param.WithSession) {
			response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request, referer: referer, headers: headers).ConfigureAwait(false);
		} else {
			response = await bot.ArchiWebHandler.WebBrowser.UrlGetToHtmlDocument(request, referer: referer, headers: headers).ConfigureAwait(false);
		}

		return new GenericResponse<HttpStatusCode>(response?.StatusCode ?? HttpStatusCode.InternalServerError);
	}

	internal static async Task<GenericResponse<string?>?> GetHtml(Bot bot, HtmlRequest param) {
		Uri request = param.Url!;
		Uri referer = param.Referer ?? ArchiWebHandler.SteamCommunityURL;
		Dictionary<string, string> headers = MergeHeaders(param.Headers);
		HtmlDocumentResponse? response;

		if (param.WithSession) {
			response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(request, referer: referer, headers: headers).ConfigureAwait(false);
		} else {
			response = await bot.ArchiWebHandler.WebBrowser.UrlGetToHtmlDocument(request, referer: referer, headers: headers).ConfigureAwait(false);
		}

		IDocument? document = response?.Content;

		if (document == null) {
			return new GenericResponse<string?>("");
		}

		if (string.IsNullOrEmpty(param.Xpath)) {
			return new GenericResponse<string?>(document.ToHtml());
		}

		INode? selectNode = document.SelectSingleNode(param.Xpath);

		return new GenericResponse<string?>(selectNode?.ToHtml());
	}

	internal static async Task<GenericResponse<JsonObject?>?> GetJson(Bot bot, BaseRequest param) {
		Uri request = param.Url!;
		Uri referer = param.Referer ?? ArchiWebHandler.SteamCommunityURL;
		Dictionary<string, string> headers = MergeHeaders(param.Headers);

		ObjectResponse<JsonObject>? response;

		if (param.WithSession) {
			response = await bot.ArchiWebHandler.UrlGetToJsonObjectWithSession<JsonObject>(request, referer: referer, headers: headers).ConfigureAwait(false);
		} else {
			response = await bot.ArchiWebHandler.WebBrowser.UrlGetToJsonObject<JsonObject>(request, referer: referer, headers: headers).ConfigureAwait(false);
		}

		return new GenericResponse<JsonObject?>(response?.Content);
	}

	internal static async Task<GenericResponse<string?>?> PostHtml(Bot bot, PostHtmlRequest param) {
		Uri request = param.Url!;
		Uri referer = param.Referer ?? ArchiWebHandler.SteamCommunityURL;
		Dictionary<string, string>? bodyData = param.BodyData;
		Dictionary<string, string> headers = MergeHeaders(param.Headers);
		HtmlDocumentResponse? response;

		if (param.WithSession) {
			response = await bot.ArchiWebHandler.UrlPostToHtmlDocumentWithSession(request, referer: referer, data: bodyData, headers: headers).ConfigureAwait(false);
		} else {
			response = await bot.ArchiWebHandler.WebBrowser.UrlPostToHtmlDocument(request, referer: referer, data: bodyData, headers: headers).ConfigureAwait(false);
		}

		IDocument? document = response?.Content;

		if (document == null) {
			return new GenericResponse<string?>("");
		}

		if (string.IsNullOrEmpty(param.Xpath)) {
			return new GenericResponse<string?>(document.ToHtml());
		}

		INode? selectNode = document.SelectSingleNode(param.Xpath);

		return new GenericResponse<string?>(selectNode?.ToHtml());
	}

	internal static async Task<GenericResponse<JsonObject?>?> PostJson(Bot bot, PostRequest param) {
		Uri request = param.Url!;
		Uri referer = param.Referer ?? ArchiWebHandler.SteamCommunityURL;
		Dictionary<string, string>? bodyData = param.BodyData;
		Dictionary<string, string> headers = MergeHeaders(param.Headers);

		ObjectResponse<JsonObject>? response;

		if (param.WithSession) {
			response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<JsonObject>(request, referer: referer, data: bodyData, headers: headers).ConfigureAwait(false);
		} else {
			response = await bot.ArchiWebHandler.WebBrowser.UrlPostToJsonObject<JsonObject, Dictionary<string, string>>(request, referer: referer, data: bodyData, headers: headers).ConfigureAwait(false);
		}

		return new GenericResponse<JsonObject?>(response?.Content);
	}

	internal static async Task<GenericResponse<string?>?> SteamApi(Bot bot, SteamApiRequest param) {
		string? token = bot.AccessToken;

		if (string.IsNullOrEmpty(token)) {
			return new GenericResponse<string?>(false, "Missing AccessToken", "");
		}

		Uri referer = param.Referer ?? ArchiWebHandler.SteamCommunityURL;
		Dictionary<string, string> headers = MergeHeaders(param.Headers);

		JsonElement? jsonElement = param.JsonData;
		string? json = null;

		if (jsonElement != null) {
			json = JsonSerializer.Serialize(jsonElement, JsonOptions);
		}

		StreamResponse? response;

		if (!string.IsNullOrEmpty(param.Method) && string.Equals(param.Method, "POST", StringComparison.OrdinalIgnoreCase)) {
			Uri request = new(SteamApiURL, $"/{param.Interface}/{param.Api}/{param.Version}/?access_token={token}");
			Dictionary<string, string>? data = null;

			if (!string.IsNullOrEmpty(json)) {
				data = new Dictionary<string, string> {
					{ "input_json", json },
				};
			}

			response = await bot.ArchiWebHandler.WebBrowser.UrlPostToStream(request, referer: referer, data: data, headers: headers).ConfigureAwait(false);
		} else {
			string origin = UrlEncode(referer.AbsoluteUri);
			Uri request = new(SteamApiURL, $"/{param.Interface}/{param.Api}/{param.Version}/?access_token={token}&origin={origin}");

			if (!string.IsNullOrEmpty(json)) {
				string encJson = UrlEncode(json);
				request = new Uri(SteamApiURL, $"/{param.Interface}/{param.Api}/{param.Version}/?access_token={token}&origin={origin}&input_json={encJson}");
			}

			response = await bot.ArchiWebHandler.WebBrowser.UrlGetToStream(request, referer: referer, headers: headers).ConfigureAwait(false);
		}

		if (response == null) {
			return new GenericResponse<string?>(false, "Http Response Empty", "");
		}

		if (!response.StatusCode.Equals(HttpStatusCode.OK)) {
			return new GenericResponse<string?>(false, $"Http StatusCode：{response.StatusCode}", "");
		}

		if (response.Content == null) {
			return new GenericResponse<string?>(response.StatusCode.ToString());
		}

		using StreamReader reader = new(response.Content);

		return new GenericResponse<string?>(await reader.ReadToEndAsync().ConfigureAwait(false));
	}

	private static Dictionary<string, string> MergeHeaders(Dictionary<string, string>? headers) {
		Dictionary<string, string> mergedHeaders = new(UserAgentHeader);

		if (headers is { Count: > 0 }) {
			foreach (KeyValuePair<string, string> kvp in headers) {
				mergedHeaders[kvp.Key] = kvp.Value;
			}
		}

		return mergedHeaders;
	}

	private static readonly ReadOnlyDictionary<char, string> CharacterEncodings = new Dictionary<char, string> {
		{ ' ', "%20" }, { '!', "%21" }, { '\"', "%22" }, { '#', "%23" }, { '$', "%24" },
		{ '%', "%25" }, { '&', "%26" }, { '\'', "%27" }, { '(', "%28" }, { ')', "%29" },
		{ '*', "%2A" }, { '+', "%2B" }, { ',', "%2C" }, { '-', "%2D" }, { '.', "%2E" },
		{ '/', "%2F" }, { ':', "%3A" }, { ';', "%3B" }, { '<', "%3C" }, { '=', "%3D" },
		{ '>', "%3E" }, { '@', "%40" }, { '[', "%5B" }, { '\\', "%5C" }, { ']', "%5D" },
		{ '^', "%5E" }, { '_', "%5F" }, { '`', "%60" }, { '{', "%7B" }, { '|', "%7C" },
		{ '}', "%7D" }, { '~', "%7E" },
	}.AsReadOnly();

	private static string UrlEncode(string url) {
		StringBuilder encodedUrl = new();

		foreach (char c in url) {
			if (CharacterEncodings.TryGetValue(c, out string? str)) {
				encodedUrl.Append(str);
			} else {
				encodedUrl.Append(c);
			}
		}

		return encodedUrl.ToString();
	}
}
