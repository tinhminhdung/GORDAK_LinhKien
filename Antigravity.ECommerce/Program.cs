using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));

// ── Performance Optimization ──
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// ── Authentication (Cookie) ──
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.LogoutPath = "/Admin/Logout";
        options.AccessDeniedPath = "/Admin/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

// ── Session ──
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Cache ──
builder.Services.AddMemoryCache();

var app = builder.Build();

// Khởi tạo Dịch vụ Cache
var cache = app.Services.GetRequiredService<IMemoryCache>();
Antigravity.ECommerce.Services.SCache.Initialize(cache);

// Init BaseConnectionSql mapping
Antigravity.ECommerce.Services.BaseConnectionSql.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Xử lý trang lỗi tùy chỉnh
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}

app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
}

// ── Image Optimization: Tự động phục vụ ảnh WebP thumbnail ──
// Đặt TRƯỚC UseStaticFiles để chặn request ảnh trước
app.UseMiddleware<Antigravity.ECommerce.Framework.ImageOptimizeMiddleware>();

// Load Image Settings từ Database (nếu có)
try
{
    var settings = Antigravity.ECommerce.Services.SSetting.GetViewModel();
    Antigravity.ECommerce.Services.ImageOptimizerService.EnableOptimization = settings.Image_EnableOptimization;
    Antigravity.ECommerce.Services.ImageOptimizerService.MaxLongestSide = settings.Image_MaxLongestSide;
    Antigravity.ECommerce.Services.ImageOptimizerService.Quality = settings.Image_Quality;
    Antigravity.ECommerce.Services.ImageOptimizerService.WatermarkUrl = settings.Image_WatermarkUrl;
    Antigravity.ECommerce.Services.ImageOptimizerService.WatermarkPosition = settings.Image_WatermarkPosition ?? "BottomRight";
    Antigravity.ECommerce.Services.ImageOptimizerService.WatermarkOpacity = settings.Image_WatermarkOpacity;
    Antigravity.ECommerce.Services.ImageOptimizerService.WatermarkSize = settings.Image_WatermarkSize;
    Antigravity.ECommerce.Services.ImageOptimizerService.WatermarkExcludePaths = settings.Image_WatermarkExcludePaths ?? "";
}
catch { }

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 365 days
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});
app.UseRouting();
app.UseMiddleware<Antigravity.ECommerce.Framework.AdminNoCacheMiddleware>();
app.UseMiddleware<Antigravity.ECommerce.Framework.MaintenanceMiddleware>();
app.UseSession();
app.UseMiddleware<Antigravity.ECommerce.Framework.VisitorTrackingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "about",
    pattern: "gioi-thieu.html",
    defaults: new { controller = "Home", action = "About" });

app.MapControllerRoute(
    name: "news_root",
    pattern: "tin-tuc.html",
    defaults: new { controller = "News", action = "Index" });

app.MapControllerRoute(
    name: "products_root",
    pattern: "san-pham.html",
    defaults: new { controller = "Product", action = "Index" });

app.MapControllerRoute(
    name: "category",
    pattern: "san-pham/{slug}.html",
    defaults: new { controller = "Product", action = "Category" });

app.MapControllerRoute(
    name: "news_category",
    pattern: "tin-tuc/{slug}.html",
    defaults: new { controller = "News", action = "Category" });

app.MapControllerRoute(
    name: "video_root",
    pattern: "video.html",
    defaults: new { controller = "Video", action = "Index" });

app.MapControllerRoute(
    name: "video_category",
    pattern: "video/{urlCategory}.html",
    defaults: new { controller = "Video", action = "Index" });

app.MapControllerRoute(
    name: "video_detail",
    pattern: "video/detail/{slug}.html",
    defaults: new { controller = "Video", action = "Detail" });

app.MapControllerRoute(
    name: "gallery_root",
    pattern: "thu-vien-anh.html",
    defaults: new { controller = "Gallery", action = "Index" });

app.MapControllerRoute(
    name: "gallery_category",
    pattern: "thu-vien-anh/{urlCategory}.html",
    defaults: new { controller = "Gallery", action = "Index" });

app.MapControllerRoute(
    name: "gallery_detail",
    pattern: "album/{slug}.html",
    defaults: new { controller = "Gallery", action = "Detail" });

app.MapControllerRoute(
    name: "document_root",
    pattern: "tai-lieu.html",
    defaults: new { controller = "Document", action = "Index" });

app.MapControllerRoute(
    name: "document_category",
    pattern: "tai-lieu/{urlCategory}.html",
    defaults: new { controller = "Document", action = "Index" });

app.MapControllerRoute(
    name: "faq_root",
    pattern: "hoi-dap.html",
    defaults: new { controller = "FAQ", action = "Index" });

app.MapControllerRoute(
    name: "faq_category",
    pattern: "hoi-dap/{urlCategory}.html",
    defaults: new { controller = "FAQ", action = "Index" });

app.MapControllerRoute(
    name: "news_detail",
    pattern: "tin-tuc/detail/{slug}.html",
    defaults: new { controller = "News", action = "Detail" });

app.MapControllerRoute(
    name: "page_detail",
    pattern: "bai-viet/{slug}.html",
    defaults: new { controller = "Page", action = "Detail" });

app.MapControllerRoute(
    name: "product",
    pattern: "san-pham/detail/{slug}-{id:int}.html",
    defaults: new { controller = "Product", action = "Detail" });

app.MapControllerRoute(
    name: "member_login",
    pattern: "dang-nhap.html",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "member_register",
    pattern: "dang-ky.html",
    defaults: new { controller = "Account", action = "Register" });

app.MapControllerRoute(
    name: "member_dashboard",
    pattern: "thanh-vien.html",
    defaults: new { controller = "Member", action = "Dashboard" });

app.MapControllerRoute(
    name: "member_orders",
    pattern: "thanh-vien/don-hang.html",
    defaults: new { controller = "Member", action = "Orders" });

app.MapControllerRoute(
    name: "member_profile",
    pattern: "thanh-vien/ho-so.html",
    defaults: new { controller = "Member", action = "Profile" });

app.MapControllerRoute(
    name: "member_wishlist",
    pattern: "thanh-vien/yeu-thich.html",
    defaults: new { controller = "Member", action = "Wishlist" });

app.MapControllerRoute(
    name: "member_changepassword",
    pattern: "thanh-vien/doi-mat-khau.html",
    defaults: new { controller = "Member", action = "ChangePassword" });

app.MapControllerRoute(
    name: "member_reviews",
    pattern: "thanh-vien/danh-gia.html",
    defaults: new { controller = "Member", action = "MyReviews" });

app.MapControllerRoute(
    name: "cart_index",
    pattern: "gio-hang.html",
    defaults: new { controller = "Cart", action = "Index" });

app.MapControllerRoute(
    name: "cart_checkout",
    pattern: "thanh-toan.html",
    defaults: new { controller = "Cart", action = "Checkout" });

app.MapControllerRoute(
    name: "cart_process_checkout",
    pattern: "xu-ly-thanh-toan.html",
    defaults: new { controller = "Cart", action = "ProcessCheckout" });

app.MapControllerRoute(
    name: "cart_success",
    pattern: "dat-hang-thanh-cong.html",
    defaults: new { controller = "Cart", action = "Success" });

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
