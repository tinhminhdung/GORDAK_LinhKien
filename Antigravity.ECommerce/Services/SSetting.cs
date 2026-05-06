using System.Collections.Generic;
using System.Linq;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    public class SSetting
    {
        public static List<Setting> GetAll()
        {
            return FSetting.GetAll();
        }

        public static Dictionary<string, string> GetDictionary()
        {
            var list = GetAll();
            return list.ToDictionary(x => x.SettingKey, x => x.SettingValue ?? string.Empty);
        }

        public static string GetValue(string key)
        {
            var list = GetAll();
            var setting = list.FirstOrDefault(x => x.SettingKey == key);
            return setting?.SettingValue ?? string.Empty;
        }

        public static GlobalSettingsViewModel GetViewModel()
        {
            var dict = GetDictionary();
            return new GlobalSettingsViewModel
            {
                SiteTitle = dict.GetValueOrDefault("SiteTitle", ""),
                SiteDescription = dict.GetValueOrDefault("SiteDescription", ""),
                MetaDescription = dict.GetValueOrDefault("MetaDescription", ""),
                SeoKeywords = dict.GetValueOrDefault("SeoKeywords", ""),
                Logo = dict.GetValueOrDefault("Logo", ""),
                Favicon = dict.GetValueOrDefault("Favicon", ""),
                Hotline = dict.GetValueOrDefault("Hotline", ""),
                Email = dict.GetValueOrDefault("Email", ""),
                Address = dict.GetValueOrDefault("Address", ""),
                MapCode = dict.GetValueOrDefault("MapCode", ""),
                WorkingHours = dict.GetValueOrDefault("WorkingHours", ""),
                ContactPhone2 = dict.GetValueOrDefault("ContactPhone2", ""),
                ContactEmail2 = dict.GetValueOrDefault("ContactEmail2", ""),
                TaxCode = dict.GetValueOrDefault("TaxCode", ""),
                Copyright = dict.GetValueOrDefault("Copyright", ""),
                FooterInfo = dict.GetValueOrDefault("FooterInfo", ""),
                Facebook = dict.GetValueOrDefault("Facebook", ""),
                Zalo = dict.GetValueOrDefault("Zalo", ""),
                YouTube = dict.GetValueOrDefault("YouTube", ""),
                Instagram = dict.GetValueOrDefault("Instagram", ""),
                MaintenanceMode = dict.GetValueOrDefault("MaintenanceMode", "false").ToLower() == "true",
                MaintenanceMessage = dict.GetValueOrDefault("MaintenanceMessage", ""),
                Error404Message = dict.GetValueOrDefault("Error404Message", ""),
                Error500Message = dict.GetValueOrDefault("Error500Message", ""),
                Error403Message = dict.GetValueOrDefault("Error403Message", ""),
                InvoiceHeader = dict.GetValueOrDefault("InvoiceHeader", ""),
                InvoiceFooter = dict.GetValueOrDefault("InvoiceFooter", ""),
                EnableCache = dict.GetValueOrDefault("EnableCache", "false").ToLower() == "true",
                CacheTimeout = int.TryParse(dict.GetValueOrDefault("CacheTimeout", "60"), out int ct) ? ct : 60,
                RankSilverThreshold = decimal.TryParse(dict.GetValueOrDefault("RankSilverThreshold", "2000000"), out decimal rs) ? rs : 2000000,
                RankGoldThreshold = decimal.TryParse(dict.GetValueOrDefault("RankGoldThreshold", "10000000"), out decimal rg) ? rg : 10000000,
                RankDiamondThreshold = decimal.TryParse(dict.GetValueOrDefault("RankDiamondThreshold", "50000000"), out decimal rd) ? rd : 50000000,
                RobotsContent = dict.GetValueOrDefault("RobotsContent", "User-agent: *\nDisallow: /Admin/"),
                GoogleAnalyticsId = dict.GetValueOrDefault("GoogleAnalyticsId", ""),
                GeminiApiKey = dict.GetValueOrDefault("GeminiApiKey", ""),
                HeaderScripts = dict.GetValueOrDefault("HeaderScripts", ""),
                FooterScripts = dict.GetValueOrDefault("FooterScripts", ""),
                ThemeLayout = dict.GetValueOrDefault("ThemeLayout", "vertical"),
                ThemeColorScheme = dict.GetValueOrDefault("ThemeColorScheme", "light"),
                ThemeSidebar = dict.GetValueOrDefault("ThemeSidebar", "dark"),
                ThemeSidebarSize = dict.GetValueOrDefault("ThemeSidebarSize", "lg"),
                ThemeTopbar = dict.GetValueOrDefault("ThemeTopbar", "light"),
                
                SmtpServer = dict.GetValueOrDefault("SmtpServer", ""),
                SmtpPort = int.TryParse(dict.GetValueOrDefault("SmtpPort", "587"), out int sp) ? sp : 587,
                SmtpEmail = dict.GetValueOrDefault("SmtpEmail", ""),
                SmtpPassword = dict.GetValueOrDefault("SmtpPassword", ""),
                
                EmailTemplateOrderConfirm = dict.GetValueOrDefault("EmailTemplateOrderConfirm", ""),
                EmailTemplateOrderStatus = dict.GetValueOrDefault("EmailTemplateOrderStatus", ""),
                EmailTemplatePasswordReset = dict.GetValueOrDefault("EmailTemplatePasswordReset", ""),

                // Image Optimization
                Image_EnableOptimization = dict.GetValueOrDefault("Image_EnableOptimization", "true").ToLower() == "true",
                Image_MaxLongestSide = int.TryParse(dict.GetValueOrDefault("Image_MaxLongestSide", "1200"), out int ims) ? ims : 1200,
                Image_Quality = int.TryParse(dict.GetValueOrDefault("Image_Quality", "80"), out int imq) ? imq : 80,
                
                // Watermark
                Image_WatermarkUrl = dict.GetValueOrDefault("Image_WatermarkUrl", ""),
                Image_WatermarkPosition = dict.GetValueOrDefault("Image_WatermarkPosition", "BottomRight"),
                Image_WatermarkOpacity = int.TryParse(dict.GetValueOrDefault("Image_WatermarkOpacity", "50"), out int op) ? op : 50,
                Image_WatermarkSize = int.TryParse(dict.GetValueOrDefault("Image_WatermarkSize", "15"), out int ws) ? ws : 15,
                Image_WatermarkExcludePaths = dict.GetValueOrDefault("Image_WatermarkExcludePaths", ""),
                
                DefaultShippingFee = decimal.TryParse(dict.GetValueOrDefault("DefaultShippingFee", "0"), out decimal dsf) ? dsf : 0,

                NewsPageSize = int.TryParse(dict.GetValueOrDefault("NewsPageSize", "12"), out int nps) ? nps : 12,
                NewsBigCount = int.TryParse(dict.GetValueOrDefault("NewsBigCount", "2"), out int nbc) ? nbc : 2,

                Home_HotProductCount = int.TryParse(dict.GetValueOrDefault("Home_HotProductCount", "8"), out int hpc) ? hpc : 8,
                Product_PageSize = int.TryParse(dict.GetValueOrDefault("Product_PageSize", "12"), out int pps) ? pps : 12
            };
        }

        public static void SaveSettings(GlobalSettingsViewModel model, string updatedBy)
        {
            FSetting.UpdateValue("SiteTitle", model.SiteTitle ?? "", updatedBy);
            FSetting.UpdateValue("SiteDescription", model.SiteDescription ?? "", updatedBy);
            FSetting.UpdateValue("MetaDescription", model.MetaDescription ?? "", updatedBy);
            FSetting.UpdateValue("SeoKeywords", model.SeoKeywords ?? "", updatedBy);
            FSetting.UpdateValue("Logo", model.Logo ?? "", updatedBy);
            FSetting.UpdateValue("Favicon", model.Favicon ?? "", updatedBy);
            FSetting.UpdateValue("Hotline", model.Hotline ?? "", updatedBy);
            FSetting.UpdateValue("Email", model.Email ?? "", updatedBy);
            FSetting.UpdateValue("Address", model.Address ?? "", updatedBy);
            FSetting.UpdateValue("MapCode", model.MapCode ?? "", updatedBy);
            FSetting.UpdateValue("WorkingHours", model.WorkingHours ?? "", updatedBy);
            FSetting.UpdateValue("ContactPhone2", model.ContactPhone2 ?? "", updatedBy);
            FSetting.UpdateValue("ContactEmail2", model.ContactEmail2 ?? "", updatedBy);
            FSetting.UpdateValue("TaxCode", model.TaxCode ?? "", updatedBy);
            FSetting.UpdateValue("Copyright", model.Copyright ?? "", updatedBy);
            FSetting.UpdateValue("FooterInfo", model.FooterInfo ?? "", updatedBy);
            FSetting.UpdateValue("InvoiceHeader", model.InvoiceHeader ?? "", updatedBy);
            FSetting.UpdateValue("InvoiceFooter", model.InvoiceFooter ?? "", updatedBy);
            FSetting.UpdateValue("Facebook", model.Facebook ?? "", updatedBy);
            FSetting.UpdateValue("YouTube", model.YouTube ?? "", updatedBy);
            FSetting.UpdateValue("Zalo", model.Zalo ?? "", updatedBy);
            FSetting.UpdateValue("Instagram", model.Instagram ?? "", updatedBy);
            FSetting.UpdateValue("MaintenanceMode", model.MaintenanceMode.ToString().ToLower(), updatedBy);
            FSetting.UpdateValue("MaintenanceMessage", model.MaintenanceMessage ?? "", updatedBy);
            FSetting.UpdateValue("Error404Message", model.Error404Message ?? "", updatedBy);
            FSetting.UpdateValue("Error500Message", model.Error500Message ?? "", updatedBy);
            FSetting.UpdateValue("Error403Message", model.Error403Message ?? "", updatedBy);
            FSetting.UpdateValue("EnableCache", model.EnableCache.ToString().ToLower(), updatedBy);
            FSetting.UpdateValue("CacheTimeout", model.CacheTimeout.ToString(), updatedBy);
            FSetting.UpdateValue("RankSilverThreshold", model.RankSilverThreshold.ToString(), updatedBy);
            FSetting.UpdateValue("RankGoldThreshold", model.RankGoldThreshold.ToString(), updatedBy);
            FSetting.UpdateValue("RankDiamondThreshold", model.RankDiamondThreshold.ToString(), updatedBy);
            FSetting.UpdateValue("RobotsContent", model.RobotsContent ?? "", updatedBy);
            FSetting.UpdateValue("GoogleAnalyticsId", model.GoogleAnalyticsId ?? "", updatedBy);
            FSetting.UpdateValue("GeminiApiKey", model.GeminiApiKey ?? "", updatedBy);
            FSetting.UpdateValue("HeaderScripts", model.HeaderScripts ?? "", updatedBy);
            FSetting.UpdateValue("FooterScripts", model.FooterScripts ?? "", updatedBy);
            
            FSetting.UpdateValue("ThemeLayout", model.ThemeLayout ?? "", updatedBy);
            FSetting.UpdateValue("ThemeColorScheme", model.ThemeColorScheme ?? "", updatedBy);
            FSetting.UpdateValue("ThemeSidebar", model.ThemeSidebar ?? "", updatedBy);
            FSetting.UpdateValue("ThemeSidebarSize", model.ThemeSidebarSize ?? "", updatedBy);
            FSetting.UpdateValue("ThemeTopbar", model.ThemeTopbar ?? "", updatedBy);
            
            FSetting.UpdateValue("SmtpServer", model.SmtpServer ?? "", updatedBy);
            FSetting.UpdateValue("SmtpPort", model.SmtpPort.ToString(), updatedBy);
            FSetting.UpdateValue("SmtpEmail", model.SmtpEmail ?? "", updatedBy);
            FSetting.UpdateValue("SmtpPassword", model.SmtpPassword ?? "", updatedBy);
            
            FSetting.UpdateValue("EmailTemplateOrderConfirm", model.EmailTemplateOrderConfirm ?? "", updatedBy);
            FSetting.UpdateValue("EmailTemplateOrderStatus", model.EmailTemplateOrderStatus ?? "", updatedBy);
            FSetting.UpdateValue("EmailTemplatePasswordReset", model.EmailTemplatePasswordReset ?? "", updatedBy);

            // Image Optimization
            FSetting.UpdateValue("Image_EnableOptimization", model.Image_EnableOptimization.ToString().ToLower(), updatedBy);
            FSetting.UpdateValue("Image_MaxLongestSide", model.Image_MaxLongestSide.ToString(), updatedBy);
            FSetting.UpdateValue("Image_Quality", model.Image_Quality.ToString(), updatedBy);
            
            // Watermark
            FSetting.UpdateValue("Image_WatermarkUrl", model.Image_WatermarkUrl ?? "", updatedBy);
            FSetting.UpdateValue("Image_WatermarkPosition", model.Image_WatermarkPosition ?? "BottomRight", updatedBy);
            FSetting.UpdateValue("Image_WatermarkOpacity", model.Image_WatermarkOpacity.ToString(), updatedBy);
            FSetting.UpdateValue("Image_WatermarkSize", model.Image_WatermarkSize.ToString(), updatedBy);
            FSetting.UpdateValue("Image_WatermarkExcludePaths", model.Image_WatermarkExcludePaths ?? "", updatedBy);
            
            FSetting.UpdateValue("DefaultShippingFee", model.DefaultShippingFee.ToString(), updatedBy);

            FSetting.UpdateValue("NewsPageSize", model.NewsPageSize.ToString(), updatedBy);
            FSetting.UpdateValue("NewsBigCount", model.NewsBigCount.ToString(), updatedBy);
            
            FSetting.UpdateValue("Home_HotProductCount", model.Home_HotProductCount.ToString(), updatedBy);
            FSetting.UpdateValue("Product_PageSize", model.Product_PageSize.ToString(), updatedBy);

            // Áp dụng ngay vào Service runtime
            ImageOptimizerService.EnableOptimization = model.Image_EnableOptimization;
            ImageOptimizerService.MaxLongestSide = model.Image_MaxLongestSide;
            ImageOptimizerService.Quality = model.Image_Quality;
            ImageOptimizerService.WatermarkUrl = model.Image_WatermarkUrl;
            ImageOptimizerService.WatermarkPosition = model.Image_WatermarkPosition ?? "BottomRight";
            ImageOptimizerService.WatermarkOpacity = model.Image_WatermarkOpacity;
            ImageOptimizerService.WatermarkSize = model.Image_WatermarkSize;
            ImageOptimizerService.WatermarkExcludePaths = model.Image_WatermarkExcludePaths ?? "";
        }
    }

    public static class DictionaryExtensions
    {
        public static string GetValueOrDefault(this Dictionary<string, string> dict, string key, string defaultValue)
        {
            return dict.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
