using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;

namespace ASFRequest.Helper;

internal static class FileHelper {
	private static readonly SemaphoreSlim GlobalFileSemaphore = new(1, 1);

	internal static async Task<bool> Write(string filePath, string json) {
		ArgumentException.ThrowIfNullOrEmpty(filePath);
		ArgumentException.ThrowIfNullOrEmpty(json);

		string newFilePath = $"{filePath}.new";
		await GlobalFileSemaphore.WaitAsync().ConfigureAwait(false);

		try {
			if (File.Exists(filePath)) {
				string currentJson = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

				if (json == currentJson) {
					return true;
				}

				await File.WriteAllTextAsync(newFilePath, json).ConfigureAwait(false);
				File.Replace(newFilePath, filePath, null);
			} else {
				await File.WriteAllTextAsync(newFilePath, json).ConfigureAwait(false);
				File.Move(newFilePath, filePath);
			}

			return true;
		} catch (Exception e) {
			ASF.ArchiLogger.LogGenericException(e);

			return false;
		} finally {
			GlobalFileSemaphore.Release();
		}
	}
}
