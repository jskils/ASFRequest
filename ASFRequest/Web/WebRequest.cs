﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.IPC.Responses;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web.Responses;

namespace ASFRequest;

internal static class WebRequest {
	private static readonly Dictionary<string, string> UserAgentHeader = new(5) {
		{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36" },
		{ "Sec-Ch-Ua", "\"Not.A/Brand\";v=\"8\", \"Chromium\";v=\"114\", \"Google Chrome\";v=\"114\"" },
		{ "Sec-Ch-Ua-Mobile", "?0" },
		{ "Sec-Ch-Ua-Platform", "\"Windows\"" },
		{ "Accept-Language", "zh-CN,zh;q=0.9" }
	};

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

		return new GenericResponse<string?>(selectNode?.Text());
	}

	private static Dictionary<string, string> MergeHeaders(Dictionary<string, string>? headers) {
		if (headers is not { Count: > 0 }) {
			return UserAgentHeader;
		}

		Dictionary<string, string> mergeHeaders = UserAgentHeader;
		headers.ToList().ForEach(kvp => mergeHeaders[kvp.Key] = kvp.Value);

		return mergeHeaders;
	}
}
