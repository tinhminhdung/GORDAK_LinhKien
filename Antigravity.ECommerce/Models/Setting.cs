using System;

namespace Antigravity.ECommerce.Models
{
    public class Setting : BaseModel
    {
        public int SettingId { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string? SettingValue { get; set; }
        public string? GroupName { get; set; }
        public string? Description { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class GlobalSettingsViewModel
    {
        // General
        public string SiteTitle { get; set; } = string.Empty;
        public string SiteDescription { get; set; } = string.Empty;
        public string? MetaDescription { get; set; } // SEO Meta
        public string? SeoKeywords { get; set; } // SEO Keywords
        public string Logo { get; set; } = string.Empty;
        public string Favicon { get; set; } = string.Empty;
        
        // Contact
        public string Hotline { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? MapCode { get; set; } // Mã nhúng Google Map
        public string? WorkingHours { get; set; } // Giờ làm việc
        public string? ContactPhone2 { get; set; } // Hotline phụ
        public string? ContactEmail2 { get; set; } // Email phụ
        public string? TaxCode { get; set; } // Mã số thuế
        public string Copyright { get; set; } = string.Empty;
        public string FooterInfo { get; set; } = string.Empty;
        
        // Invoice
        public string InvoiceHeader { get; set; } = string.Empty;
        public string InvoiceFooter { get; set; } = string.Empty;
        
        // Social
        public string Facebook { get; set; } = string.Empty;
        public string YouTube { get; set; } = string.Empty;
        public string Zalo { get; set; } = string.Empty;
        public string? Instagram { get; set; }
        
        // Maintenance & Errors
        public bool MaintenanceMode { get; set; }
        public string? MaintenanceMessage { get; set; }
        public string? Error404Message { get; set; }
        public string? Error500Message { get; set; }
        public string? Error403Message { get; set; }

        // Caching
        public bool EnableCache { get; set; }
        public int CacheTimeout { get; set; } // Minutes

        // Member Ranks Configuration
        public decimal RankSilverThreshold { get; set; }
        public decimal RankGoldThreshold { get; set; }
        public decimal RankDiamondThreshold { get; set; }

        // SEO & Analytics
        public string? RobotsContent { get; set; }
        public string? GoogleAnalyticsId { get; set; }
        public string? GeminiApiKey { get; set; } // Google Gemini AI API Key (Free)
        public string? HeaderScripts { get; set; } // Custom script in <head>
        public string? FooterScripts { get; set; } // Custom script before </body>

        // Theme Customizer
        public string? ThemeLayout { get; set; } = "vertical";
        public string? ThemeColorScheme { get; set; } = "light";
        public string? ThemeSidebar { get; set; } = "dark";
        public string? ThemeSidebarSize { get; set; } = "lg";
        public string? ThemeTopbar { get; set; } = "light";

        // SMTP Settings
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpEmail { get; set; }
        public string? SmtpPassword { get; set; }

        // Email Templates (HTML with {Variable} placeholders)
        public string? EmailTemplateOrderConfirm { get; set; }
        public string? EmailTemplateOrderStatus { get; set; }
        public string? EmailTemplatePasswordReset { get; set; }

        // Image Optimization Settings
        public bool Image_EnableOptimization { get; set; } = true;
        public int Image_MaxLongestSide { get; set; } = 1200;
        public int Image_Quality { get; set; } = 80;
        
        // Watermark Settings
        public string? Image_WatermarkUrl { get; set; }
        public string Image_WatermarkPosition { get; set; } = "BottomRight"; // TopLeft, TopRight, BottomLeft, BottomRight, Center, Tile
        public int Image_WatermarkOpacity { get; set; } = 50; // 10-100%
        public int Image_WatermarkSize { get; set; } = 15; // Kích thước watermark chiếm bao nhiêu % so với ảnh gốc (5-50%)
        public string? Image_WatermarkExcludePaths { get; set; } // Các thư mục không bị đóng dấu (cách nhau dấu phẩy)

        // Đơn hàng / Vận chuyển
        public decimal DefaultShippingFee { get; set; } = 0; // Phí vận chuyển mặc định (0 = Miễn phí)
    }
}
