using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.IPC.Controllers.Api;
using ArchiSteamFarm.IPC.Responses;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Security;
using ArchiSteamFarm.Steam.Storage;
using ASFRequest.Helper;
using ASFRequest.Request;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebRequest = ASFRequest.Web.WebRequest;

namespace ASFRequest.Controller;

/// <summary>
/// 请求控制器
/// </summary>
[Route("/Api/ASFRequest/[action]", Name = nameof(ASFRequest))]
[SwaggerTag(nameof(ASFRequest))]
public sealed class RequestController : ArchiController {
	/// <summary>
	/// 批量新增机器人
	/// </summary>
	/// <param name="requests"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPost]
	[SwaggerOperation(Summary = "批量新增机器人", Description = "批量新增机器人")]
	[ProducesResponseType(typeof(GenericResponse<IReadOnlyDictionary<string, GenericResponse<HttpStatusCode>>>), (int) HttpStatusCode.OK)]
	[ProducesResponseType(typeof(GenericResponse), (int) HttpStatusCode.BadRequest)]
	public async Task<ActionResult<GenericResponse>> AddBot([FromBody] IReadOnlyCollection<AddBotRequest>? requests) {
		if ((requests == null) || (requests.Count == 0)) {
			return BadRequest(new GenericResponse(false, "配置不能为空"));
		}

		Dictionary<string, bool> result = new(requests.Count);

		foreach (AddBotRequest request in requests) {
			string botName = request.BotName;
			JsonObject? botConfig = request.BotConfig;
			JsonObject? maFile = request.MaFile;
			bool success = false;

			if (string.IsNullOrEmpty(botName) || (botConfig == null)) {
				result[botName] = success;

				continue;
			}

			string configFilePath = Bot.GetFilePath(botName, Bot.EFileType.Config);
			string configJson = botConfig.ToJsonText(true);
			success = await FileHelper.Write(configFilePath, configJson).ConfigureAwait(false);

			if (!success) {
				result[botName] = success;

				continue;
			}

			if (maFile != null) {
				string maFilePath = Bot.GetFilePath(botName, Bot.EFileType.MobileAuthenticator);
				string maJson = maFile.ToJsonText(true);
				bool maFileSuccess = await FileHelper.Write(maFilePath, maJson).ConfigureAwait(false);
				success = maFileSuccess;
			}

			result[botName] = success;
		}

		return Ok(new GenericResponse<IReadOnlyDictionary<string, bool>>(result));
	}

	/// <summary>
	/// GET请求返回状态
	/// </summary>
	/// <param name="botNames"></param>
	/// <param name="requestParam"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPost("{botNames:required}")]
	[SwaggerOperation(Summary = "GET请求返回状态", Description = "GET请求返回状态")]
	[ProducesResponseType(typeof(GenericResponse<IReadOnlyDictionary<string, GenericResponse<HttpStatusCode>>>), (int) HttpStatusCode.OK)]
	[ProducesResponseType(typeof(GenericResponse), (int) HttpStatusCode.BadRequest)]
	public async Task<ActionResult<GenericResponse>> GetState(string botNames, [FromBody] BaseRequest requestParam) {
		if (string.IsNullOrEmpty(botNames)) {
			throw new ArgumentNullException(nameof(botNames));
		}

		HashSet<Bot>? bots = Bot.GetBots(botNames);

		if ((bots == null) || (bots.Count == 0)) {
			return BadRequest(new GenericResponse(false, string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)));
		}

		IList<(string BotName, GenericResponse<HttpStatusCode>? Data)> results = await Utilities.InParallel(
			bots.Select(
				async bot => {
					if (!bot.IsConnectedAndLoggedOn) {
						return (bot.BotName, new GenericResponse<HttpStatusCode>(false, string.Format(CultureInfo.CurrentCulture, Strings.BotDisconnected, bot.BotName), HttpStatusCode.InternalServerError));
					}

					GenericResponse<HttpStatusCode>? result = await WebRequest.GetState(bot, requestParam).ConfigureAwait(false);

					return (bot.BotName, result);
				}
			)
		).ConfigureAwait(false);

		Dictionary<string, GenericResponse<HttpStatusCode>?> response = results.ToDictionary(static x => x.BotName, static x => x.Data);

		return Ok(new GenericResponse<IReadOnlyDictionary<string, GenericResponse<HttpStatusCode>?>>(response));
	}

	/// <summary>
	/// GET请求返回Html
	/// </summary>
	/// <param name="botNames"></param>
	/// <param name="requestParam"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPost("{botNames:required}")]
	[SwaggerOperation(Summary = "GET请求返回Html", Description = "GET请求返回Html")]
	[ProducesResponseType(typeof(GenericResponse<IReadOnlyDictionary<string, GenericResponse<string?>>>), (int) HttpStatusCode.OK)]
	[ProducesResponseType(typeof(GenericResponse), (int) HttpStatusCode.BadRequest)]
	public async Task<ActionResult<GenericResponse>> GetHtml(string botNames, [FromBody] HtmlRequest requestParam) {
		if (string.IsNullOrEmpty(botNames)) {
			throw new ArgumentNullException(nameof(botNames));
		}

		HashSet<Bot>? bots = Bot.GetBots(botNames);

		if ((bots == null) || (bots.Count == 0)) {
			return BadRequest(new GenericResponse(false, string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)));
		}

		IList<(string BotName, GenericResponse<string?>? Data)> results = await Utilities.InParallel(
			bots.Select(
				async bot => {
					if (!bot.IsConnectedAndLoggedOn) {
						return (bot.BotName, new GenericResponse<string?>(false, string.Format(CultureInfo.CurrentCulture, Strings.BotDisconnected, bot.BotName), ""));
					}

					GenericResponse<string?>? result = await WebRequest.GetHtml(bot, requestParam).ConfigureAwait(false);

					return (bot.BotName, result);
				}
			)
		).ConfigureAwait(false);

		Dictionary<string, GenericResponse<string?>?> response = results.ToDictionary(static x => x.BotName, static x => x.Data);

		return Ok(new GenericResponse<IReadOnlyDictionary<string, GenericResponse<string?>?>>(response));
	}

	/// <summary>
	/// GET请求返回Json
	/// </summary>
	/// <param name="botNames"></param>
	/// <param name="requestParam"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPost("{botNames:required}")]
	[SwaggerOperation(Summary = "GET请求返回Json", Description = "GET请求返回Json")]
	[ProducesResponseType(typeof(GenericResponse<IReadOnlyDictionary<string, GenericResponse<JsonObject?>>>), (int) HttpStatusCode.OK)]
	[ProducesResponseType(typeof(GenericResponse), (int) HttpStatusCode.BadRequest)]
	public async Task<ActionResult<GenericResponse>> GetJson(string botNames, [FromBody] BaseRequest requestParam) {
		if (string.IsNullOrEmpty(botNames)) {
			throw new ArgumentNullException(nameof(botNames));
		}

		HashSet<Bot>? bots = Bot.GetBots(botNames);

		if ((bots == null) || (bots.Count == 0)) {
			return BadRequest(new GenericResponse(false, string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)));
		}

		IList<(string BotName, GenericResponse<JsonObject?>? Data)> results = await Utilities.InParallel(
			bots.Select(
				async bot => {
					if (!bot.IsConnectedAndLoggedOn) {
						return (bot.BotName, new GenericResponse<JsonObject?>(false, string.Format(CultureInfo.CurrentCulture, Strings.BotDisconnected, bot.BotName), null));
					}

					GenericResponse<JsonObject?>? result = await WebRequest.GetJson(bot, requestParam).ConfigureAwait(false);

					return (bot.BotName, result);
				}
			)
		).ConfigureAwait(false);

		Dictionary<string, GenericResponse<JsonObject?>?> response = results.ToDictionary(static x => x.BotName, static x => x.Data);

		return Ok(new GenericResponse<IReadOnlyDictionary<string, GenericResponse<JsonObject?>?>>(response));
	}

	/// <summary>
	/// POST请求返回Html
	/// </summary>
	/// <param name="botNames"></param>
	/// <param name="requestParam"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPost("{botNames:required}")]
	[SwaggerOperation(Summary = "POST请求返回Html", Description = "POST请求返回Html")]
	[ProducesResponseType(typeof(GenericResponse<IReadOnlyDictionary<string, GenericResponse<string?>>>), (int) HttpStatusCode.OK)]
	[ProducesResponseType(typeof(GenericResponse), (int) HttpStatusCode.BadRequest)]
	public async Task<ActionResult<GenericResponse>> PostHtml(string botNames, [FromBody] PostHtmlRequest requestParam) {
		if (string.IsNullOrEmpty(botNames)) {
			throw new ArgumentNullException(nameof(botNames));
		}

		HashSet<Bot>? bots = Bot.GetBots(botNames);

		if ((bots == null) || (bots.Count == 0)) {
			return BadRequest(new GenericResponse(false, string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)));
		}

		IList<(string BotName, GenericResponse<string?>? Data)> results = await Utilities.InParallel(
			bots.Select(
				async bot => {
					if (!bot.IsConnectedAndLoggedOn) {
						return (bot.BotName, new GenericResponse<string?>(false, string.Format(CultureInfo.CurrentCulture, Strings.BotDisconnected, bot.BotName), ""));
					}

					GenericResponse<string?>? result = await WebRequest.PostHtml(bot, requestParam).ConfigureAwait(false);

					return (bot.BotName, result);
				}
			)
		).ConfigureAwait(false);

		Dictionary<string, GenericResponse<string?>?> response = results.ToDictionary(static x => x.BotName, static x => x.Data);

		return Ok(new GenericResponse<IReadOnlyDictionary<string, GenericResponse<string?>?>>(response));
	}

	/// <summary>
	/// POST请求返回Json
	/// </summary>
	/// <param name="botNames"></param>
	/// <param name="requestParam"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPost("{botNames:required}")]
	[SwaggerOperation(Summary = "POST请求返回Json", Description = "POST请求返回Json")]
	[ProducesResponseType(typeof(GenericResponse<IReadOnlyDictionary<string, GenericResponse<JsonObject?>>>), (int) HttpStatusCode.OK)]
	[ProducesResponseType(typeof(GenericResponse), (int) HttpStatusCode.BadRequest)]
	public async Task<ActionResult<GenericResponse>> PostJson(string botNames, [FromBody] PostRequest requestParam) {
		if (string.IsNullOrEmpty(botNames)) {
			throw new ArgumentNullException(nameof(botNames));
		}

		HashSet<Bot>? bots = Bot.GetBots(botNames);

		if ((bots == null) || (bots.Count == 0)) {
			return BadRequest(new GenericResponse(false, string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)));
		}

		IList<(string BotName, GenericResponse<JsonObject?>? Data)> results = await Utilities.InParallel(
			bots.Select(
				async bot => {
					if (!bot.IsConnectedAndLoggedOn) {
						return (bot.BotName, new GenericResponse<JsonObject?>(false, string.Format(CultureInfo.CurrentCulture, Strings.BotDisconnected, bot.BotName), null));
					}

					GenericResponse<JsonObject?>? result = await WebRequest.PostJson(bot, requestParam).ConfigureAwait(false);

					return (bot.BotName, result);
				}
			)
		).ConfigureAwait(false);

		Dictionary<string, GenericResponse<JsonObject?>?> response = results.ToDictionary(static x => x.BotName, static x => x.Data);

		return Ok(new GenericResponse<IReadOnlyDictionary<string, GenericResponse<JsonObject?>?>>(response));
	}
}
