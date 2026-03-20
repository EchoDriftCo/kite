using System.Threading.Tasks;

namespace RecipeVault.Integrations.VideoDownload {
    /// <summary>
    /// Service for downloading video audio via yt-dlp
    /// </summary>
    public interface IVideoDownloadService {
        /// <summary>
        /// Download audio from a video URL
        /// </summary>
        /// <param name="videoUrl">URL of the video (YouTube, TikTok, Instagram, etc.)</param>
        /// <param name="includeSubtitles">Whether to extract subtitles/captions if available</param>
        /// <returns>Video download result with audio data and metadata</returns>
        Task<VideoDownloadResult> DownloadAudioAsync(string videoUrl, bool includeSubtitles = true);
    }
}
