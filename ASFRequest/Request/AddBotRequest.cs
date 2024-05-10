using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ASFRequest.Request;

/// <summary>
/// 添加机器人请求参数
/// </summary>
public sealed record AddBotRequest {
	/// <summary>
	/// 机器人名称
	/// </summary>
	[JsonInclude]
	[JsonRequired]
	[Required]
	public string BotName { get; private init; } = "";

	/// <summary>
	/// 配置
	/// </summary>
	[JsonInclude]
	[JsonRequired]
	[Required]
	public JsonObject? BotConfig { get; private init; }

	/// <summary>
	/// 令牌
	/// </summary>
	[JsonInclude]
	public JsonObject? MaFile { get; private init; }
}
