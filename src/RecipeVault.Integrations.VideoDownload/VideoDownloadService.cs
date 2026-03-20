using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RecipeVault.Integrations.VideoDownload {
    /// <summary>
    /// Service for downloading video audio using yt-dlp CLI
    /// </summary>
    public class VideoDownloadService : IVideoDownloadService {
        private readonly ILogger<VideoDownloadService> logger;
        private static readonly char[] NewLineSeparators = { '\r', '\n' };

        /// <summary>
        /// Initializes a new instance of VideoDownloadService
        /// </summary>
        public VideoDownloadService(ILogger<VideoDownloadService> logger) {
            this.logger = logger;
        }

        /// <summary>
        /// Download audio from a video URL using yt-dlp
        /// </summary>
        public async Task<VideoDownloadResult> DownloadAudioAsync(string videoUrl, bool includeSubtitles = true) {
            if (string.IsNullOrWhiteSpace(videoUrl)) {
                throw new ArgumentException("Video URL is required", nameof(videoUrl));
            }

            // Validate URL format
            if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out var uri)) {
                throw new ArgumentException("Invalid video URL format", nameof(videoUrl));
            }

            logger.LogInformation("Starting video download from: {VideoUrl}", videoUrl);

            // Create temp directory for download
            var tempDir = Path.Combine(Path.GetTempPath(), $"rv_video_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try {
                // Download audio to temp file
                var audioPath = Path.Combine(tempDir, "audio.%(ext)s");
                var metadataPath = Path.Combine(tempDir, "metadata.json");

                var args = $"--extract-audio --audio-format mp3 --audio-quality 192K " +
                          $"--write-info-json --output \"{audioPath}\" " +
                          $"--no-playlist --max-filesize 50M ";

                if (includeSubtitles) {
                    args += "--write-auto-subs --sub-langs en ";
                }

                args += $"\"{videoUrl}\"";

                logger.LogDebug("Executing yt-dlp with args: {Args}", args);

                var process = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "yt-dlp",
                        Arguments = args,
                        WorkingDirectory = tempDir,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                var output = string.Empty;
                var error = string.Empty;

                process.OutputDataReceived += (sender, e) => {
                    if (e.Data != null) {
                        output += e.Data + "\n";
                        logger.LogDebug("yt-dlp output: {Output}", e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) => {
                    if (e.Data != null) {
                        error += e.Data + "\n";
                        logger.LogDebug("yt-dlp error: {Error}", e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync().ConfigureAwait(false);

                if (process.ExitCode != 0) {
                    logger.LogError("yt-dlp failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                    throw new InvalidOperationException($"Failed to download video: {error}");
                }

                // Find downloaded audio file
                var audioFiles = Directory.GetFiles(tempDir, "audio.*");
                var audioFile = Array.Find(audioFiles, f => 
                    f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase));

                if (audioFile == null || !File.Exists(audioFile)) {
                    throw new InvalidOperationException("Audio file not found after download");
                }

                // Read audio data
                var audioData = await File.ReadAllBytesAsync(audioFile).ConfigureAwait(false);
                var audioFormat = Path.GetExtension(audioFile).TrimStart('.').ToLowerInvariant();

                logger.LogInformation("Audio downloaded: {Size} bytes, format: {Format}", audioData.Length, audioFormat);

                // Find and parse metadata JSON
                var infoJsonFiles = Directory.GetFiles(tempDir, "*.info.json");
                VideoMetadata metadata = null;
                if (infoJsonFiles.Length > 0) {
                    var metadataJson = await File.ReadAllTextAsync(infoJsonFiles[0]).ConfigureAwait(false);
                    metadata = JsonSerializer.Deserialize<VideoMetadata>(metadataJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                // Extract subtitles if available
                string subtitles = null;
                if (includeSubtitles) {
                    var subtitleFiles = Directory.GetFiles(tempDir, "*.vtt");
                    if (subtitleFiles.Length == 0) {
                        subtitleFiles = Directory.GetFiles(tempDir, "*.srt");
                    }
                    if (subtitleFiles.Length > 0) {
                        var subtitleText = await File.ReadAllTextAsync(subtitleFiles[0]).ConfigureAwait(false);
                        subtitles = CleanSubtitles(subtitleText);
                        logger.LogInformation("Extracted subtitles: {Length} characters", subtitles.Length);
                    }
                }

                return new VideoDownloadResult {
                    AudioData = audioData,
                    AudioFormat = audioFormat,
                    Platform = metadata?.ExtractorKey?.ToLowerInvariant() ?? DetectPlatform(videoUrl),
                    DurationSeconds = metadata?.Duration,
                    ThumbnailUrl = metadata?.Thumbnail,
                    Subtitles = subtitles,
                    VideoTitle = metadata?.Title
                };
            }
            finally {
                // Clean up temp directory
                try {
                    if (Directory.Exists(tempDir)) {
                        Directory.Delete(tempDir, recursive: true);
                    }
                }
                catch (Exception ex) {
                    logger.LogWarning(ex, "Failed to clean up temp directory: {TempDir}", tempDir);
                }
            }
        }

        private static string DetectPlatform(string url) {
            if (url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || 
                url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase)) {
                return "youtube";
            }
            if (url.Contains("tiktok.com", StringComparison.OrdinalIgnoreCase)) {
                return "tiktok";
            }
            if (url.Contains("instagram.com", StringComparison.OrdinalIgnoreCase)) {
                return "instagram";
            }
            return "unknown";
        }

        private static string CleanSubtitles(string rawSubtitles) {
            if (string.IsNullOrWhiteSpace(rawSubtitles)) {
                return string.Empty;
            }

            // Remove WebVTT headers
            var text = Regex.Replace(rawSubtitles, @"WEBVTT\s*[\r\n]+", string.Empty);
            
            // Remove timestamps and position markers (00:00:00.000 --> 00:00:02.000 align:start position:0%)
            text = Regex.Replace(text, @"\d{2}:\d{2}:\d{2}\.\d{3}\s*-->\s*\d{2}:\d{2}:\d{2}\.\d{3}[^\r\n]*[\r\n]+", string.Empty);
            
            // Remove SRT numbering (just numbers on their own line)
            text = Regex.Replace(text, @"^\d+\s*$", string.Empty, RegexOptions.Multiline);
            
            // Remove duplicate consecutive lines
            var lines = text.Split(NewLineSeparators, StringSplitOptions.RemoveEmptyEntries);
            var uniqueLines = new System.Collections.Generic.List<string>();
            string lastLine = null;
            foreach (var line in lines) {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed) && trimmed != lastLine) {
                    uniqueLines.Add(trimmed);
                    lastLine = trimmed;
                }
            }
            
            return string.Join(" ", uniqueLines);
        }

        private sealed class VideoMetadata {
            public string Title { get; set; }
            public int? Duration { get; set; }
            public string Thumbnail { get; set; }
            public string ExtractorKey { get; set; }
        }
    }
}
