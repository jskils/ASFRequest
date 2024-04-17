using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASFRequest.Request;

/// <summary>
/// 请求参数
/// </summary>
public record BaseRequest {
	/// <summary>
	/// 访问URL
	/// </summary>
	[JsonInclude]
	[JsonRequired]
	[Required]
	public Uri? Url { get; set; }

	/// <summary>
	/// 是否携带身份
	/// </summary>
	[JsonInclude]
	public bool WithSession { get; set; } = true;

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
}
