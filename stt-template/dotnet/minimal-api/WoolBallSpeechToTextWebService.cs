using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WoolBall.SpeechToText
{
    public class TranscriptionOptions
    {
        public string Model { get; set; } = "onnx-community/whisper-large-v3-turbo_timestamped";
        public string Language { get; set; } = "pt";
        public bool ReturnTimestamps { get; set; } = false;
        public bool Webvtt { get; set; } = false;
    }

    public class WoolBallSpeechToTextWebService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.woolball.xyz/v1";
        private const string API_KEY_PLACEHOLDER = "{{API_KEY}}";

        public WoolBallSpeechToTextWebService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY_PLACEHOLDER}");
        }

        /// <summary>
        /// Transcribes audio from a URL
        /// </summary>
        /// <param name="audioUrl">URL of the audio file to transcribe</param>
        /// <param name="options">Optional: TranscriptionOptions object containing model, language, and other settings</param>
        /// <returns>TranscriptionResult containing text and optional timestamps or WebVTT</returns>
        public async Task<TranscriptionResult> TranscribeFromUrlAsync(
            string audioUrl,
            TranscriptionOptions options = null)
        {
            options ??= new TranscriptionOptions();

            var requestUrl = $"{BaseUrl}/speech-to-text?model={Uri.EscapeDataString(options.Model)}&language={options.Language}&returnTimestamps={options.ReturnTimestamps}&webvtt={options.Webvtt}";
            
            var content = new StringContent(
                JsonSerializer.Serialize(new { url = audioUrl }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();
            
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TranscriptionResult>(jsonResponse);
        }

        /// <summary>
        /// Transcribe audio from URL with timestamps
        /// </summary>
        public async Task<TranscriptionResult> TranscribeFromUrlWithTimestampsAsync(
            string audioUrl,
            string language = "pt")
        {
            return await TranscribeFromUrlAsync(audioUrl, new TranscriptionOptions
            {
                Language = language,
                ReturnTimestamps = true
            });
        }

        /// <summary>
        /// Transcribe audio from URL with WebVTT subtitles
        /// </summary>
        public async Task<TranscriptionResult> TranscribeFromUrlWithWebVttAsync(
            string audioUrl,
            string language = "pt")
        {
            return await TranscribeFromUrlAsync(audioUrl, new TranscriptionOptions
            {
                Language = language,
                ReturnTimestamps = true,
                Webvtt = true
            });
        }

        /// <summary>
        /// Transcribes audio from a file
        /// </summary>
        /// <param name="audioData">Byte array containing the audio or video file data</param>
        /// <param name="options">Optional: TranscriptionOptions object containing model, language, and other settings</param>
        /// <returns>TranscriptionResult containing text and optional timestamps or WebVTT</returns>
        public async Task<TranscriptionResult> TranscribeFromFileAsync(
            byte[] audioData,
            TranscriptionOptions options = null)
        {
            options ??= new TranscriptionOptions();

            var requestUrl = $"{BaseUrl}/speech-to-text?model={Uri.EscapeDataString(options.Model)}&language={options.Language}&returnTimestamps={options.ReturnTimestamps}&webvtt={options.Webvtt}";
            
            var content = new ByteArrayContent(audioData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");

            var response = await _httpClient.PostAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();
            
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TranscriptionResult>(jsonResponse);
        }

        /// <summary>
        /// Transcribe audio file with timestamps
        /// </summary>
        public async Task<TranscriptionResult> TranscribeFromFileWithTimestampsAsync(
            byte[] audioData,
            string language = "pt")
        {
            return await TranscribeFromFileAsync(audioData, new TranscriptionOptions
            {
                Language = language,
                ReturnTimestamps = true
            });
        }

        /// <summary>
        /// Transcribe audio file with WebVTT subtitles
        /// </summary>
        public async Task<TranscriptionResult> TranscribeFromFileWithWebVttAsync(
            byte[] audioData,
            string language = "pt")
        {
            return await TranscribeFromFileAsync(audioData, new TranscriptionOptions
            {
                Language = language,
                ReturnTimestamps = true,
                Webvtt = true
            });
        }

        /// <summary>
        /// Gets available speech-to-text models
        /// </summary>
        /// <returns>Array of available model names</returns>
        public async Task<string[]> GetAvailableModelsAsync()
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/speech-to-text-models");
            response.EnsureSuccessStatusCode();
            
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<string[]>(jsonResponse);
        }
    }

    public class TranscriptionResult
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("chunks")]
        public TranscriptionChunk[] Chunks { get; set; }

        [JsonPropertyName("webvtt")]
        public string WebVtt { get; set; }
    }

    public class TranscriptionChunk
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("start")]
        public double Start { get; set; }

        [JsonPropertyName("end")]
        public double End { get; set; }
    }
}
