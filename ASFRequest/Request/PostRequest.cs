using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ASFRequest.Request;

/// <inheritdoc />
/// <summary>
/// 请求参数
/// </summary>
public sealed record PostRequest : BaseRequest {
	/// <summary>
	/// post json参数
	/// </summary>
	[JsonInclude]
	public Dictionary<string, string>? BodyData { get; init; }
}
