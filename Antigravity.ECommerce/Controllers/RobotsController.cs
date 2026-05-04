using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using System.Text;

namespace Antigravity.ECommerce.Controllers
{
    public class RobotsController : Controller
    {
        [Route("robots.txt")]
        public IActionResult Index()
        {
            var robotsContent = SSetting.GetValue("RobotsContent");
            
            if (string.IsNullOrEmpty(robotsContent))
            {
                // Default content if not set
                var sb = new StringBuilder();
                sb.AppendLine("User-agent: *");
                sb.AppendLine("Disallow: /Admin/");
                sb.AppendLine("Disallow: /Error/");
                sb.AppendLine("Allow: /");
                sb.AppendLine("");
                sb.AppendLine($"Sitemap: {Request.Scheme}://{Request.Host}/sitemap.xml");
                robotsContent = sb.ToString();
            }
            else
            {
                // Ensure sitemap is present if not already in content
                if (!robotsContent.Contains("Sitemap:", System.StringComparison.OrdinalIgnoreCase))
                {
                    robotsContent += $"\n\nSitemap: {Request.Scheme}://{Request.Host}/sitemap.xml";
                }
            }

            return Content(robotsContent, "text/plain", Encoding.UTF8);
        }
    }
}
