using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASFRequest.Request;

/// <summary>
/// SteamWebApiRequest请求参数
/// </summary>
public sealed record SteamApiRequest {
	/// <summary>
	/// 请求方法，默认Get
	/// </summary>
	[JsonInclude]
	[JsonRequired]
	public string? Method { get; private init; }

	/// <summary>
	/// 请求interface
	/// </summary>
	[JsonInclude]
	[JsonRequired]
	[Required]
	public string? Interface { get; private init; }

	/// <summary>
	/// 请求api
	/// </summary>
	[JsonInclude]
	[JsonRequired]
	[Required]
	public string? Api { get; private init; }

	/// <summary>
	/// 版本，默认v1
	/// </summary>
	[JsonInclude]
	[JsonRequired]
	public string Version { get; private init; } = "v1";

	/// <summary>
	/// Referer
	/// </summary>
	[JsonInclude]
	public Uri? Referer { get; set; }

	/// <summary>
	/// 自定义header
	/// </summary>
	[JsonInclude]
	public Dictionary<string, string>? Headers { get; init; }

	/// <summary>
	/// 参数
	/// </summary>
	[JsonInclude]
	public JsonElement? JsonData { get; init; }
}
