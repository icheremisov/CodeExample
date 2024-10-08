using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace XLib.Core.Runtime.Discord {

	public class DiscordWebhook {
		/// <summary>
		/// Webhook url
		/// </summary>
		public string Url { get; set; }

		private void AddField(MemoryStream stream, string bound, string cDisposition, string cType, byte[] data) {
			var prefix = stream.Length > 0 ? "\r\n--" : "--";
			var fBegin = $"{prefix}{bound}\r\n";

			var fBeginBuffer = Utils.Encode(fBegin);
			var cDispositionBuffer = Utils.Encode(cDisposition);
			var cTypeBuffer = Utils.Encode(cType);

			stream.Write(fBeginBuffer, 0, fBeginBuffer.Length);
			stream.Write(cDispositionBuffer, 0, cDispositionBuffer.Length);
			stream.Write(cTypeBuffer, 0, cTypeBuffer.Length);
			stream.Write(data, 0, data.Length);
		}

		private void SetJsonPayload(MemoryStream stream, string bound, string json) {
			var cDisposition = "Content-Disposition: form-data; name=\"payload_json\"\r\n";
			var cType = "Content-Type: application/octet-stream\r\n\r\n";
			AddField(stream, bound, cDisposition, cType, Utils.Encode(json));
		}

		private void SetFile(MemoryStream stream, string bound, int index, FileInfo file) {
			var cDisposition = $"Content-Disposition: form-data; name=\"file_{index}\"; filename=\"{file.Name}\"\r\n";
			var cType = "Content-Type: application/octet-stream\r\n\r\n";
			AddField(stream, bound, cDisposition, cType, File.ReadAllBytes(file.FullName));
		}

		private void SetFile(MemoryStream stream, string bound, int index, FileData file) {
			var cDisposition = $"Content-Disposition: form-data; name=\"file_{index}\"; filename=\"{file.Name}\"\r\n";
			var cType = "Content-Type: application/octet-stream\r\n\r\n";
			AddField(stream, bound, cDisposition, cType, file.Data);
		}

		/// <summary>
		/// Send webhook message
		/// </summary>
		public Task Send(DiscordMessage message, params FileInfo[] files) {
			if (string.IsNullOrEmpty(Url)) throw new ArgumentNullException("Invalid Webhook URL.");

			var bound = "------------------------" + DateTime.Now.Ticks.ToString("x");
			var stream = new MemoryStream();
			for (var i = 0; i < files.Length; i++) SetFile(stream, bound, i, files[i]);

			return SendInternal(message, stream, bound);
		}

		/// <summary>
		/// Send webhook message
		/// </summary>
		public Task Send(DiscordMessage message, params FileData[] files) {
			if (string.IsNullOrEmpty(Url)) throw new ArgumentNullException("Invalid Webhook URL.");

			var bound = "------------------------" + DateTime.Now.Ticks.ToString("x");
			var stream = new MemoryStream();
			for (var i = 0; i < files.Length; i++) SetFile(stream, bound, i, files[i]);

			return SendInternal(message, stream, bound);
		}

		private async Task SendInternal(DiscordMessage message, MemoryStream stream, string bound) {
			using var client = new HttpClient();

			var json = message.ToString();
			SetJsonPayload(stream, bound, json);

			var bodyEnd = Utils.Encode($"\r\n--{bound}--");
			stream.Write(bodyEnd, 0, bodyEnd.Length);
			stream.Seek(0, SeekOrigin.Begin);

			var content = new StreamContent(stream);
			content.Headers.Add("Content-Type", $"multipart/form-data;boundary=\"{bound}\"");

			var request = new HttpRequestMessage();
			request.Method = HttpMethod.Post;
			request.Content = content;
			request.RequestUri = new Uri($"{Url}?wait=true");

			try {
				var response = await client.SendAsync(request);
			}
			catch (WebException ex) {
				throw new WebException(Utils.Decode(ex.Response.GetResponseStream()));
			}

			stream.Dispose();
		}
		
	}

}