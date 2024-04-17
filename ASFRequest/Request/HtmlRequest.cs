using System.Text.Json.Serialization;

namespace ASFRequest.Request;

/// <inheritdoc />
/// <summary>
/// 请求参数
/// </summary>
public sealed record HtmlRequest : BaseRequest {
	/// <summary>
	/// xpath,初步筛选内容避免返回内容过多
	/// </summary>
	[JsonInclude]
	public string? Xpath { get; set; }
}
