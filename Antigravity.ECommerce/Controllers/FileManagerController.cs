using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    public class FileManagerController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public FileManagerController(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string BasePath => Path.Combine(_env.WebRootPath, "uploads");

        [HttpGet]
        public IActionResult Index(string path = "", string fieldId = "", string CKEditorFuncNum = "")
        {
            EnsureUploadFolderExists();

            var currentPath = string.IsNullOrEmpty(path) ? BasePath : Path.Combine(BasePath, path.TrimStart('/'));
            if (!Directory.Exists(currentPath)) currentPath = BasePath;

            var currentRelativePath = currentPath.Substring(BasePath.Length).Replace("\\", "/");
            if (!currentRelativePath.StartsWith("/")) currentRelativePath = "/" + currentRelativePath;

            ViewBag.CurrentPath = currentRelativePath;
            ViewBag.FieldId = fieldId;
            ViewBag.CKEditorFuncNum = CKEditorFuncNum;
            ViewBag.FolderTree = GetFolderTree(BasePath, "");

            var items = new List<FileManagerItem>();

            // Get directories (ẩn thư mục .thumbs)
            var dirs = Directory.GetDirectories(currentPath);
            foreach (var d in dirs)
            {
                var dirInfo = new DirectoryInfo(d);
                if (dirInfo.Name == ".thumbs") continue; // Ẩn thư mục thumbnail
                items.Add(new FileManagerItem
                {
                    Name = dirInfo.Name,
                    IsDirectory = true,
                    Path = (currentRelativePath == "/" ? "" : currentRelativePath) + "/" + dirInfo.Name,
                    Size = 0,
                    DateModified = dirInfo.LastWriteTime
                });
            }

            // Get files
            var files = Directory.GetFiles(currentPath);
            foreach (var f in files)
            {
                var fileInfo = new FileInfo(f);
                var item = new FileManagerItem
                {
                    Name = fileInfo.Name,
                    IsDirectory = false,
                    Path = "/uploads" + (currentRelativePath == "/" ? "" : currentRelativePath) + "/" + fileInfo.Name,
                    Size = fileInfo.Length,
                    DateModified = fileInfo.LastWriteTime
                };

                // Thêm thông tin ảnh (kích thước, loại, thumbnail)
                if (Antigravity.ECommerce.Services.ImageOptimizerService.IsImageFile(f))
                {
                    var (w, h) = Antigravity.ECommerce.Services.ImageOptimizerService.GetImageDimensions(f);
                    item.ImageWidth = w;
                    item.ImageHeight = h;
                    item.ImageType = Antigravity.ECommerce.Services.ImageOptimizerService.GetImageType(w, h);
                    item.ImageTypeLabel = Antigravity.ECommerce.Services.ImageOptimizerService.GetImageTypeLabel(item.ImageType);
                    item.ImageTypeColor = Antigravity.ECommerce.Services.ImageOptimizerService.GetImageTypeColor(item.ImageType);
                    item.HasThumbnail = Antigravity.ECommerce.Services.ImageOptimizerService.HasThumbnail(f);
                    if (item.HasThumbnail)
                    {
                        var thumbPath = Antigravity.ECommerce.Services.ImageOptimizerService.GetThumbnailPath(f);
                        item.ThumbnailSize = new FileInfo(thumbPath).Length;
                    }
                }

                items.Add(item);
            }

            return View(items);
        }

        private List<FolderTreeItem> GetFolderTree(string physicalPath, string relativePath)
        {
            var result = new List<FolderTreeItem>();
            try
            {
                foreach (var d in Directory.GetDirectories(physicalPath))
                {
                    var info = new DirectoryInfo(d);
                    if (info.Name.Equals(".thumbs", StringComparison.OrdinalIgnoreCase)) continue;

                    var relPath = relativePath + "/" + info.Name;
                    result.Add(new FolderTreeItem
                    {
                        Name = info.Name,
                        RelativePath = relPath,
                        Children = GetFolderTree(d, relPath),
                        FileCount = Directory.GetFiles(d).Length
                    });
                }
            }
            catch { }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> files, string currentPath, string fieldId, string CKEditorFuncNum)
        {
            EnsureUploadFolderExists();
            if (files != null && files.Count > 0)
            {
                var folder = string.IsNullOrEmpty(currentPath) || currentPath == "/" ? BasePath : Path.Combine(BasePath, currentPath.TrimStart('/'));
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = file.FileName.Replace(" ", "-");
                        var filePath = Path.Combine(folder, fileName);
                        
                        int counter = 1;
                        while (System.IO.File.Exists(filePath))
                        {
                            var nameOnly = Path.GetFileNameWithoutExtension(fileName);
                            var ext = Path.GetExtension(fileName);
                            filePath = Path.Combine(folder, $"{nameOnly}_{counter++}{ext}");
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Tự động tạo thumbnail (ẢNH GỐC KHÔNG ĐỤNG)
                        if (Antigravity.ECommerce.Services.ImageOptimizerService.IsImageFile(filePath))
                        {
                            Antigravity.ECommerce.Services.ImageOptimizerService.GenerateThumbnail(filePath);
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { path = currentPath, fieldId = fieldId, CKEditorFuncNum = CKEditorFuncNum });
        }

        [HttpPost]
        public async Task<IActionResult> UploadCKEditor(IFormFile upload)
        {
            EnsureUploadFolderExists();
            if (upload != null && upload.Length > 0)
            {
                var fileName = upload.FileName;
                var filePath = Path.Combine(BasePath, fileName);
                
                int counter = 1;
                while (System.IO.File.Exists(filePath))
                {
                    var nameOnly = Path.GetFileNameWithoutExtension(fileName);
                    var ext = Path.GetExtension(fileName);
                    fileName = $"{nameOnly}_{counter++}{ext}";
                    filePath = Path.Combine(BasePath, fileName);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }
                
                return Json(new { uploaded = true, url = "/uploads/" + fileName });
            }
            return Json(new { uploaded = false, error = new { message = "Lỗi tải lên" } });
        }

        [HttpPost]
        public IActionResult CreateFolder(string folderName, string currentPath, string fieldId, string CKEditorFuncNum)
        {
            EnsureUploadFolderExists();
            if (!string.IsNullOrEmpty(folderName))
            {
                var folder = string.IsNullOrEmpty(currentPath) || currentPath == "/" ? BasePath : Path.Combine(BasePath, currentPath.TrimStart('/'));
                var newFolder = Path.Combine(folder, folderName);
                if (!Directory.Exists(newFolder)) Directory.CreateDirectory(newFolder);
            }
            return RedirectToAction("Index", new { path = currentPath, fieldId = fieldId, CKEditorFuncNum = CKEditorFuncNum });
        }

        [HttpPost]
        public IActionResult Delete(List<string> itemPaths, string currentPath, string fieldId, string CKEditorFuncNum)
        {
            if (itemPaths == null || itemPaths.Count == 0) return RedirectToAction("Index", new { path = currentPath, fieldId = fieldId, CKEditorFuncNum = CKEditorFuncNum });

            foreach (var itemPath in itemPaths)
            {
                if (string.IsNullOrEmpty(itemPath)) continue;

                var isFile = itemPath.StartsWith("/uploads/");
                if (isFile)
                {
                    // Xóa file
                    var physicalPath = Path.Combine(_env.WebRootPath, itemPath.TrimStart('/'));
                    // Xóa thumbnail tương ứng trước (ẢNH GỐC SẼ BỊ XÓA THEO LỆNH ADMIN)
                    if (Antigravity.ECommerce.Services.ImageOptimizerService.IsImageFile(physicalPath))
                    {
                        Antigravity.ECommerce.Services.ImageOptimizerService.DeleteThumbnail(physicalPath);
                    }
                    if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
                }
                else
                {
                    // Xóa thư mục
                    var physicalFolder = Path.Combine(BasePath, itemPath.TrimStart('/'));
                    if (Directory.Exists(physicalFolder)) Directory.Delete(physicalFolder, true);
                }
            }

            return RedirectToAction("Index", new { path = currentPath, fieldId = fieldId, CKEditorFuncNum = CKEditorFuncNum });
        }

        // ══════ API: Tạo lại Thumbnail cho 1 ảnh ══════
        [HttpPost]
        public IActionResult RegenerateThumbnail(string filePath)
        {
            var physicalPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (!System.IO.File.Exists(physicalPath))
                return Json(new { success = false, message = "File không tồn tại" });

            var result = Antigravity.ECommerce.Services.ImageOptimizerService.GenerateThumbnail(physicalPath);
            if (result)
            {
                var thumbPath = Antigravity.ECommerce.Services.ImageOptimizerService.GetThumbnailPath(physicalPath);
                var thumbSize = new FileInfo(thumbPath).Length;
                return Json(new { success = true, thumbSize = thumbSize });
            }
            return Json(new { success = false, message = "Không thể tạo thumbnail" });
        }

        // ══════ API: Crop ảnh (CHỈ tạo thumb crop, GỐC KHÔNG ĐỤNG) ══════
        [HttpPost]
        public IActionResult CropImage(string filePath, int x, int y, int width, int height)
        {
            var physicalPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (!System.IO.File.Exists(physicalPath))
                return Json(new { success = false, message = "File không tồn tại" });

            var result = Antigravity.ECommerce.Services.ImageOptimizerService.GenerateCroppedThumbnail(physicalPath, x, y, width, height);
            if (result)
            {
                var thumbPath = Antigravity.ECommerce.Services.ImageOptimizerService.GetThumbnailPath(physicalPath);
                var thumbSize = new FileInfo(thumbPath).Length;
                return Json(new { success = true, thumbSize = thumbSize });
            }
            return Json(new { success = false, message = "Không thể crop" });
        }

        // ══════ API: Xoay ảnh (CHỈ tạo thumb xoay, GỐC KHÔNG ĐỤNG) ══════
        [HttpPost]
        public IActionResult RotateImage(string filePath, int degrees)
        {
            var physicalPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (!System.IO.File.Exists(physicalPath))
                return Json(new { success = false, message = "File không tồn tại" });

            var result = Antigravity.ECommerce.Services.ImageOptimizerService.GenerateRotatedThumbnail(physicalPath, degrees);
            return Json(new { success = result });
        }

        // ══════ API: Tối ưu hàng loạt ══════
        [HttpPost]
        public IActionResult BulkOptimize()
        {
            var (total, success, savedBytes) = Antigravity.ECommerce.Services.ImageOptimizerService.RegenerateAll(BasePath);
            return Json(new { success = true, total, optimized = success, savedBytes });
        }

        // ══════ API: Thống kê ══════
        [HttpGet]
        public IActionResult OptimizationStats()
        {
            var stats = Antigravity.ECommerce.Services.ImageOptimizerService.GetStats(BasePath);
            return Json(stats);
        }

        private void EnsureUploadFolderExists()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
        }
    }

    public class FileManagerItem
    {
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateModified { get; set; }
        // Image optimization fields
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string ImageType { get; set; } = "";
        public string ImageTypeLabel { get; set; } = "";
        public string ImageTypeColor { get; set; } = "";
        public bool HasThumbnail { get; set; }
        public long ThumbnailSize { get; set; }
    }

    public class FolderTreeItem
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public int FileCount { get; set; }
        public List<FolderTreeItem> Children { get; set; } = new();
    }
}
