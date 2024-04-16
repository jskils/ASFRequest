using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASFRequest;

/// <summary>
/// 请求参数
/// </summary>
public record HtmlRequest : BaseRequest {
	/// <summary>
	/// xpath,初步筛选内容避免返回内容过多
	/// </summary>
	[JsonInclude]
	public string? Xpath { get; set; }
}
