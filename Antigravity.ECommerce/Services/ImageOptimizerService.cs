using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Antigravity.ECommerce.Services
{
    public static class ImageOptimizerService
    {
        // ══════ SETTINGS (Mặc định, có thể thay đổi qua Admin Setting) ══════
        public static bool EnableOptimization { get; set; } = true;
        
        private static int _maxLongestSide = 1200;
        private static int _quality = 80;

        public static int MaxLongestSide
        {
            get => _maxLongestSide;
            set => _maxLongestSide = Math.Clamp(value, 200, 4000);
        }

        public static int Quality
        {
            get => _quality;
            set => _quality = Math.Clamp(value, 30, 100);
        }

        // Watermark settings
        public static string? WatermarkUrl { get; set; }
        public static string WatermarkPosition { get; set; } = "BottomRight";
        public static int WatermarkOpacity { get; set; } = 50;
        public static int WatermarkSize { get; set; } = 15;
        public static string WatermarkExcludePaths { get; set; } = "";

        private const string ThumbFolderName = ".thumbs";

        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif" };

        // ══════ KIỂM TRA FILE CÓ PHẢI ẢNH KHÔNG ══════
        public static bool IsImageFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            // Bỏ qua thư mục .thumbs để tránh tạo thumbnail lồng nhau
            if (filePath.Contains($"\\{ThumbFolderName}\\") || filePath.Contains($"/{ThumbFolderName}/") || filePath.EndsWith(ThumbFolderName, StringComparison.OrdinalIgnoreCase)) return false;
            
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ImageExtensions.Contains(ext);
        }

        // ══════ LẤY ĐƯỜNG DẪN THUMBNAIL ══════
        public static string GetThumbnailPath(string originalPhysicalPath)
        {
            var dir = Path.GetDirectoryName(originalPhysicalPath)!;
            var nameWithoutExt = Path.GetFileNameWithoutExtension(originalPhysicalPath);
            var thumbDir = Path.Combine(dir, ThumbFolderName);
            return Path.Combine(thumbDir, nameWithoutExt + ".webp");
        }

        // ══════ LẤY ĐƯỜNG DẪN THUMBNAIL TỪ URL ══════
        public static string GetThumbnailUrlPath(string originalUrlPath)
        {
            // /uploads/SanPham/product.png → /uploads/SanPham/.thumbs/product.webp
            var dir = Path.GetDirectoryName(originalUrlPath)!.Replace("\\", "/");
            var nameWithoutExt = Path.GetFileNameWithoutExtension(originalUrlPath);
            return $"{dir}/{ThumbFolderName}/{nameWithoutExt}.webp";
        }

        // ══════ KIỂM TRA THUMBNAIL ĐÃ TỒN TẠI CHƯA ══════
        public static bool HasThumbnail(string originalPhysicalPath)
        {
            return File.Exists(GetThumbnailPath(originalPhysicalPath));
        }

        // ══════ TẠO THUMBNAIL (Hàm cốt lõi) ══════
        /// <summary>
        /// Tạo thumbnail WebP từ ảnh gốc. ẢNH GỐC KHÔNG BAO GIỜ BỊ THAY ĐỔI.
        /// </summary>
        public static bool GenerateThumbnail(string originalPhysicalPath)
        {
            try
            {
                if (!EnableOptimization) return false;

                if (!File.Exists(originalPhysicalPath) || !IsImageFile(originalPhysicalPath))
                    return false;

                var thumbPath = GetThumbnailPath(originalPhysicalPath);
                var thumbDir = Path.GetDirectoryName(thumbPath)!;
                if (!Directory.Exists(thumbDir))
                    Directory.CreateDirectory(thumbDir);

                using var originalStream = File.OpenRead(originalPhysicalPath);
                using var original = SKBitmap.Decode(originalStream);
                if (original == null) return false;

                // Tính kích thước mới giữ nguyên tỷ lệ
                var (newWidth, newHeight) = CalculateResizeDimensions(original.Width, original.Height);

                using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
                if (resized == null) return false;

                // Đóng dấu Watermark
                ApplyWatermark(resized, originalPhysicalPath);

                using var image = SKImage.FromBitmap(resized);
                using var data = image.Encode(SKEncodedImageFormat.Webp, _quality);
                using var output = File.Create(thumbPath);
                data.SaveTo(output);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsWatermarkExcluded(string physicalPath)
        {
            if (string.IsNullOrWhiteSpace(WatermarkExcludePaths) || string.IsNullOrEmpty(physicalPath)) return false;
            
            var excludes = WatermarkExcludePaths.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(x => x.Trim().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar))
                                                .Where(x => !string.IsNullOrEmpty(x));
            
            foreach (var ex in excludes)
            {
                if (physicalPath.Contains(ex, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // ══════ ĐÓNG DẤU WATERMARK ══════
        private static void ApplyWatermark(SKBitmap bitmap, string originalPhysicalPath)
        {
            if (string.IsNullOrEmpty(WatermarkUrl)) return;
            if (IsWatermarkExcluded(originalPhysicalPath)) return;
            
            try
            {
                // Đường dẫn thực tế của watermark (Ví dụ: /uploads/logo.png -> D:\...\uploads\logo.png)
                // Lấy web root path thông qua Environment (tạm dùng Path.Combine từ thư mục hiện tại nếu không có)
                // Tuy nhiên, WatermarkUrl thường là đường dẫn tương đối từ gốc web.
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var wmPhysicalPath = Path.Combine(webRootPath, WatermarkUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                
                if (!File.Exists(wmPhysicalPath)) return;

                using var wmStream = File.OpenRead(wmPhysicalPath);
                using var wmBitmap = SKBitmap.Decode(wmStream);
                if (wmBitmap == null) return;

                // Tính kích thước watermark dựa trên tỷ lệ quy định (ví dụ 15% cạnh của ảnh chính)
                int wmTargetWidth = (int)(bitmap.Width * (WatermarkSize / 100.0f));
                int wmTargetHeight = (int)(wmTargetWidth * ((float)wmBitmap.Height / wmBitmap.Width));

                // Nếu ảnh dọc, dựa vào chiều cao thay vì chiều rộng
                if (bitmap.Height > bitmap.Width)
                {
                    wmTargetHeight = (int)(bitmap.Height * (WatermarkSize / 100.0f));
                    wmTargetWidth = (int)(wmTargetHeight * ((float)wmBitmap.Width / wmBitmap.Height));
                }

                using var wmResized = wmBitmap.Resize(new SKImageInfo(wmTargetWidth, wmTargetHeight), SKFilterQuality.High);
                if (wmResized == null) return;

                using var canvas = new SKCanvas(bitmap);
                using var paint = new SKPaint
                {
                    Color = new SKColor(255, 255, 255, (byte)(255 * (WatermarkOpacity / 100.0f))),
                    FilterQuality = SKFilterQuality.High,
                    IsAntialias = true,
                    BlendMode = SKBlendMode.SrcOver
                };

                int x = 0, y = 0;
                int padding = Math.Max(10, bitmap.Width / 50); // Padding động

                switch (WatermarkPosition)
                {
                    case "TopLeft":
                        x = padding;
                        y = padding;
                        break;
                    case "TopRight":
                        x = bitmap.Width - wmTargetWidth - padding;
                        y = padding;
                        break;
                    case "BottomLeft":
                        x = padding;
                        y = bitmap.Height - wmTargetHeight - padding;
                        break;
                    case "Center":
                        x = (bitmap.Width - wmTargetWidth) / 2;
                        y = (bitmap.Height - wmTargetHeight) / 2;
                        break;
                    case "Tile":
                        // Phủ kín
                        for (int i = 0; i < bitmap.Width; i += wmTargetWidth + padding * 2)
                        {
                            for (int j = 0; j < bitmap.Height; j += wmTargetHeight + padding * 2)
                            {
                                canvas.DrawBitmap(wmResized, i, j, paint);
                            }
                        }
                        return;
                    case "BottomRight":
                    default:
                        x = bitmap.Width - wmTargetWidth - padding;
                        y = bitmap.Height - wmTargetHeight - padding;
                        break;
                }

                canvas.DrawBitmap(wmResized, x, y, paint);
            }
            catch { /* Bỏ qua lỗi vẽ watermark */ }
        }

        // ══════ TẠO THUMBNAIL VỚI CROP ══════
        /// <summary>
        /// Crop một vùng từ ảnh gốc rồi tạo thumbnail. ẢNH GỐC KHÔNG BỊ THAY ĐỔI.
        /// </summary>
        public static bool GenerateCroppedThumbnail(string originalPhysicalPath, int x, int y, int cropWidth, int cropHeight)
        {
            try
            {
                if (!File.Exists(originalPhysicalPath)) return false;

                var thumbPath = GetThumbnailPath(originalPhysicalPath);
                var thumbDir = Path.GetDirectoryName(thumbPath)!;
                if (!Directory.Exists(thumbDir))
                    Directory.CreateDirectory(thumbDir);

                using var originalStream = File.OpenRead(originalPhysicalPath);
                using var original = SKBitmap.Decode(originalStream);
                if (original == null) return false;

                // Giới hạn vùng crop trong phạm vi ảnh
                x = Math.Max(0, Math.Min(x, original.Width - 1));
                y = Math.Max(0, Math.Min(y, original.Height - 1));
                cropWidth = Math.Min(cropWidth, original.Width - x);
                cropHeight = Math.Min(cropHeight, original.Height - y);

                var cropRect = new SKRectI(x, y, x + cropWidth, y + cropHeight);
                using var cropped = new SKBitmap(cropWidth, cropHeight);
                using var canvas = new SKCanvas(cropped);
                canvas.DrawBitmap(original, cropRect, new SKRect(0, 0, cropWidth, cropHeight));

                // Resize nếu cần
                var (newW, newH) = CalculateResizeDimensions(cropWidth, cropHeight);
                using var resized = cropped.Resize(new SKImageInfo(newW, newH), SKFilterQuality.High);
                if (resized == null) return false;

                // Đóng dấu Watermark
                ApplyWatermark(resized, originalPhysicalPath);

                using var image = SKImage.FromBitmap(resized);
                using var data = image.Encode(SKEncodedImageFormat.Webp, _quality);
                using var output = File.Create(thumbPath);
                data.SaveTo(output);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ══════ TẠO THUMBNAIL VỚI XOAY ══════
        /// <summary>
        /// Xoay ảnh rồi tạo thumbnail. ẢNH GỐC KHÔNG BỊ THAY ĐỔI.
        /// </summary>
        public static bool GenerateRotatedThumbnail(string originalPhysicalPath, int degrees)
        {
            try
            {
                if (!File.Exists(originalPhysicalPath)) return false;

                var thumbPath = GetThumbnailPath(originalPhysicalPath);
                var thumbDir = Path.GetDirectoryName(thumbPath)!;
                if (!Directory.Exists(thumbDir))
                    Directory.CreateDirectory(thumbDir);

                using var originalStream = File.OpenRead(originalPhysicalPath);
                using var original = SKBitmap.Decode(originalStream);
                if (original == null) return false;

                // Xoay
                bool swap = degrees == 90 || degrees == 270;
                int rotW = swap ? original.Height : original.Width;
                int rotH = swap ? original.Width : original.Height;

                using var rotated = new SKBitmap(rotW, rotH);
                using var canvas = new SKCanvas(rotated);
                canvas.Translate(rotW / 2f, rotH / 2f);
                canvas.RotateDegrees(degrees);
                canvas.Translate(-original.Width / 2f, -original.Height / 2f);
                canvas.DrawBitmap(original, 0, 0);

                var (newW, newH) = CalculateResizeDimensions(rotW, rotH);
                using var resized = rotated.Resize(new SKImageInfo(newW, newH), SKFilterQuality.High);
                if (resized == null) return false;

                // Đóng dấu Watermark
                ApplyWatermark(resized, originalPhysicalPath);

                using var image = SKImage.FromBitmap(resized);
                using var data = image.Encode(SKEncodedImageFormat.Webp, _quality);
                using var output = File.Create(thumbPath);
                data.SaveTo(output);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ══════ XÓA THUMBNAIL ══════
        public static void DeleteThumbnail(string originalPhysicalPath)
        {
            try
            {
                var thumbPath = GetThumbnailPath(originalPhysicalPath);
                if (File.Exists(thumbPath))
                    File.Delete(thumbPath);
            }
            catch { }
        }

        // ══════ TẠO LẠI TOÀN BỘ THUMBNAIL (Khi đổi Setting) ══════
        public static (int total, int success, long savedBytes) RegenerateAll(string uploadsBasePath)
        {
            int total = 0, success = 0;
            long originalTotal = 0, thumbTotal = 0;

            // 1. Xóa tất cả thư mục .thumbs cũ
            foreach (var thumbDir in Directory.GetDirectories(uploadsBasePath, ThumbFolderName, SearchOption.AllDirectories))
            {
                try { Directory.Delete(thumbDir, true); } catch { }
            }

            // 2. Tạo lại thumbnail cho tất cả ảnh
            foreach (var file in Directory.GetFiles(uploadsBasePath, "*.*", SearchOption.AllDirectories))
            {
                if (!IsImageFile(file)) continue;
                // Bỏ qua file trong thư mục .thumbs (không nên có vì đã xóa ở trên)
                if (file.Contains($"{Path.DirectorySeparatorChar}{ThumbFolderName}{Path.DirectorySeparatorChar}")) continue;

                total++;
                var fileInfo = new FileInfo(file);
                originalTotal += fileInfo.Length;

                if (GenerateThumbnail(file))
                {
                    success++;
                    var thumbPath = GetThumbnailPath(file);
                    if (File.Exists(thumbPath))
                        thumbTotal += new FileInfo(thumbPath).Length;
                }
            }

            return (total, success, originalTotal - thumbTotal);
        }

        // ══════ THỐNG KÊ ══════
        public static ImageOptimizationStats GetStats(string uploadsBasePath)
        {
            var stats = new ImageOptimizationStats();

            foreach (var file in Directory.GetFiles(uploadsBasePath, "*.*", SearchOption.AllDirectories))
            {
                if (!IsImageFile(file)) continue;
                if (file.Contains($"{Path.DirectorySeparatorChar}{ThumbFolderName}{Path.DirectorySeparatorChar}")) continue;

                stats.TotalImages++;
                var fi = new FileInfo(file);
                stats.TotalOriginalSize += fi.Length;

                var thumbPath = GetThumbnailPath(file);
                if (File.Exists(thumbPath))
                {
                    stats.OptimizedCount++;
                    stats.TotalThumbSize += new FileInfo(thumbPath).Length;
                }
                else
                {
                    stats.NotOptimizedCount++;
                    stats.LargestUnoptimized.Add(new ImageFileInfo
                    {
                        Path = file.Substring(uploadsBasePath.Length).Replace("\\", "/"),
                        Size = fi.Length,
                        Name = fi.Name
                    });
                }
            }

            stats.LargestUnoptimized = stats.LargestUnoptimized.OrderByDescending(x => x.Size).Take(10).ToList();
            stats.SavedBytes = stats.TotalOriginalSize - stats.TotalThumbSize;

            return stats;
        }

        // ══════ LẤY KÍCH THƯỚC ẢNH ══════
        public static (int width, int height) GetImageDimensions(string physicalPath)
        {
            try
            {
                using var stream = File.OpenRead(physicalPath);
                using var codec = SKCodec.Create(stream);
                if (codec != null)
                    return (codec.Info.Width, codec.Info.Height);
            }
            catch { }
            return (0, 0);
        }

        // ══════ PHÂN LOẠI ẢNH ══════
        public static string GetImageType(int width, int height)
        {
            if (width == 0 || height == 0) return "unknown";
            double ratio = (double)width / height;
            if (ratio > 2.5) return "banner";
            if (ratio >= 1.4) return "landscape";
            if (ratio >= 0.8) return "square";
            return "portrait";
        }

        public static string GetImageTypeLabel(string type)
        {
            return type switch
            {
                "banner" => "Banner",
                "landscape" => "Ngang",
                "square" => "Vuông",
                "portrait" => "Dọc",
                _ => ""
            };
        }

        public static string GetImageTypeColor(string type)
        {
            return type switch
            {
                "banner" => "#50a5f1",
                "landscape" => "#f1b44c",
                "square" => "#0ab39c",
                "portrait" => "#7c5cbf",
                _ => "#aaa"
            };
        }

        // ══════ TÍNH KÍCH THƯỚC RESIZE ══════
        private static (int width, int height) CalculateResizeDimensions(int originalWidth, int originalHeight)
        {
            int longestSide = Math.Max(originalWidth, originalHeight);
            if (longestSide <= _maxLongestSide)
                return (originalWidth, originalHeight); // Không cần resize

            double scale = (double)_maxLongestSide / longestSide;
            int newWidth = (int)(originalWidth * scale);
            int newHeight = (int)(originalHeight * scale);
            return (Math.Max(1, newWidth), Math.Max(1, newHeight));
        }
    }

    // ══════ MODELS ══════
    public class ImageOptimizationStats
    {
        public int TotalImages { get; set; }
        public int OptimizedCount { get; set; }
        public int NotOptimizedCount { get; set; }
        public long TotalOriginalSize { get; set; }
        public long TotalThumbSize { get; set; }
        public long SavedBytes { get; set; }
        public List<ImageFileInfo> LargestUnoptimized { get; set; } = new();
    }

    public class ImageFileInfo
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public long Size { get; set; }
    }
}
