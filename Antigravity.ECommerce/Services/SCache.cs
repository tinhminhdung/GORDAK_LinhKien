using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;

namespace Antigravity.ECommerce.Services
{
    /// <summary> Dịch vụ quản lý bộ nhớ đệm (RAM Cache) của hệ thống </summary>
    public class SCache
    {
        private static IMemoryCache? _cache;
        private static CancellationTokenSource _resetToken = new();
        public static readonly AsyncLocal<bool> BypassCache = new();

        // Cache cấu hình Settings để không phải query DB mỗi lần cache access
        private static int _cachedTimeout = 60;
        private static bool _cachedEnabled = true;
        private static DateTime _settingsLastCheck = DateTime.MinValue;
        private static readonly object _settingsLock = new();

        /// <summary> Khởi tạo cache từ ứng dụng (gọi 1 lần trong Program.cs) </summary>
        public static void Initialize(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary> Lấy dữ liệu từ Cache, nếu không có thì gọi hàm và lưu lại </summary>
        public static T? GetOrSet<T>(string key, Func<T> getData, int timeoutMinutes = 60)
        {
            if (_cache == null || BypassCache.Value) return getData();

            // Chỉ check settings tối đa 1 phút/lần (tránh query DB mỗi cache access)
            if ((DateTime.UtcNow - _settingsLastCheck).TotalMinutes >= 1)
            {
                lock (_settingsLock)
                {
                    if ((DateTime.UtcNow - _settingsLastCheck).TotalMinutes >= 1)
                    {
                        try
                        {
                            var settings = SSetting.GetViewModel();
                            _cachedEnabled = settings.EnableCache;
                            _cachedTimeout = settings.CacheTimeout;
                        }
                        catch { /* Nếu chưa có bảng Settings thì dùng mặc định */ }
                        _settingsLastCheck = DateTime.UtcNow;
                    }
                }
            }

            if (!_cachedEnabled || _cachedTimeout <= 0) return getData();
            timeoutMinutes = _cachedTimeout;

            if (!_cache.TryGetValue(key, out T? result))
            {
                result = getData();
                var opts = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(timeoutMinutes))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(timeoutMinutes * 2))
                    .AddExpirationToken(new CancellationChangeToken(_resetToken.Token));

                _cache.Set(key, result, opts);
            }

            return result;
        }

        /// <summary> Xóa 1 phần tử khỏi Cache theo key </summary>
        public static void Remove(string key)
        {
            _cache?.Remove(key);
        }

        /// <summary> Xóa toàn bộ Cache hệ thống (Làm mới hoàn toàn) </summary>
        public static void ClearAll()
        {
            // Hủy token cũ → toàn bộ entry đang dùng token này sẽ bị expire
            if (_resetToken != null && !_resetToken.IsCancellationRequested)
            {
                var oldToken = _resetToken;
                _resetToken = new CancellationTokenSource();
                oldToken.Cancel();
                oldToken.Dispose();
            }
            // Reset settings check để lần tiếp theo sẽ load lại settings mới
            _settingsLastCheck = DateTime.MinValue;
        }
    }
}
