using System;
using System.Collections.Generic;
using System.Linq;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    public class SCategory
    {
        /// <summary> Giới hạn đệ quy tối đa để tránh StackOverflow khi data lỗi vòng tròn </summary>
        private const int MaxRecursionDepth = 20;

        public static Category? GetById(int id)
        {
            return FCategory.GetById(id);
        }

        public static List<Category> GetAll()
        {
            return FCategory.GetAll();
        }

        /// <summary> 
        /// Lấy danh sách phẳng theo thứ tự cây (có prefix "--" cho indent).
        /// Kết quả được CACHE để tránh build lại mỗi lần gọi.
        /// </summary>
        public static List<Category> GetHierarchical()
        {
            return SCache.GetOrSet("Category_Hierarchical", () => {
                var all = FCategory.GetAll();
                var lookup = all.ToLookup(x => x.ParentId);
                var result = new List<Category>();
                BuildHierarchy(lookup, result, 0, "", 0);
                return result;
            }, 60) ?? new List<Category>();
        }

        private static void BuildHierarchy(ILookup<int, Category> lookup, List<Category> result, int parentId, string prefix, int depth)
        {
            if (depth >= MaxRecursionDepth) return;

            foreach (var item in lookup[parentId])
            {
                var copy = new Category 
                { 
                    CategoryId = item.CategoryId, 
                    Name = prefix + item.Name,
                    ParentId = item.ParentId,
                    Slug = item.Slug,
                    CategoryType = item.CategoryType,
                    LinkType = item.LinkType,
                    Url = item.Url,
                    Target = item.Target,
                    MenuPosition = item.MenuPosition
                };
                result.Add(copy);
                BuildHierarchy(lookup, result, item.CategoryId, prefix + "-- ", depth + 1);
            }
        }

        public static List<Category> GetMenuTree(string position = "Header", int? categoryType = null)
        {
            var all = FCategory.GetAll().Where(x => x.Status == 1).ToList();
            if (categoryType.HasValue)
            {
                all = all.Where(x => x.CategoryType == categoryType.Value || x.ParentId != 0).ToList();
            }
            
            var roots = all.Where(x => x.ParentId == 0).ToList();
            if (!string.IsNullOrEmpty(position))
            {
                roots = roots.Where(x => x.MenuPosition != null && x.MenuPosition.Contains(position)).ToList();
            }

            if (categoryType.HasValue)
            {
                roots = roots.Where(x => x.CategoryType == categoryType.Value).ToList();
            }

            var lookup = all.ToLookup(x => x.ParentId);
            foreach (var root in roots)
            {
                root.Children = BuildMenuTree(lookup, root.CategoryId, 0);
            }
            return roots;
        }

        private static List<Category> BuildMenuTree(ILookup<int, Category> lookup, int parentId, int depth)
        {
            if (depth >= MaxRecursionDepth) return new List<Category>();

            var children = lookup[parentId].OrderBy(x => x.SortOrder).ToList();
            foreach (var child in children)
            {
                child.Children = BuildMenuTree(lookup, child.CategoryId, depth + 1);
            }
            return children;
        }

        /// <summary>
        /// Lấy tất cả ID con cháu của 1 category.
        /// Dùng Dictionary lookup O(N) thay vì LINQ scan O(N²).
        /// Có chống vòng tròn bằng HashSet visited.
        /// </summary>
        public static List<int> GetDescendantIds(int parentId)
        {
            var all = GetAll();
            var lookup = all.ToLookup(x => x.ParentId);
            var result = new List<int> { parentId };
            var visited = new HashSet<int> { parentId };
            AddChildIdsOptimized(lookup, parentId, result, visited, 0);
            return result;
        }

        /// <summary>
        /// Overload nhận sẵn list để tránh gọi GetAll() nhiều lần khi dùng trong vòng lặp.
        /// </summary>
        public static List<int> GetDescendantIds(int parentId, ILookup<int, Category> lookup)
        {
            var result = new List<int> { parentId };
            var visited = new HashSet<int> { parentId };
            AddChildIdsOptimized(lookup, parentId, result, visited, 0);
            return result;
        }

        private static void AddChildIdsOptimized(ILookup<int, Category> lookup, int parentId, List<int> result, HashSet<int> visited, int depth)
        {
            if (depth >= MaxRecursionDepth) return;

            foreach (var child in lookup[parentId])
            {
                if (visited.Add(child.CategoryId)) // HashSet.Add trả false nếu đã tồn tại → chống vòng tròn
                {
                    result.Add(child.CategoryId);
                    AddChildIdsOptimized(lookup, child.CategoryId, result, visited, depth + 1);
                }
            }
        }

        /// <summary>
        /// Build cây con từ danh sách phẳng - CLONE objects (không mutate cache).
        /// Dùng chung cho tất cả Admin Category controllers.
        /// </summary>
        public static List<Category> BuildTree(List<Category> source, int parentId, Func<IEnumerable<Category>, IEnumerable<Category>>? sortFunc = null)
        {
            var lookup = source.ToLookup(x => x.ParentId);
            return BuildTreeInternal(lookup, parentId, 0, sortFunc);
        }

        private static List<Category> BuildTreeInternal(ILookup<int, Category> lookup, int parentId, int depth, Func<IEnumerable<Category>, IEnumerable<Category>>? sortFunc)
        {
            if (depth >= MaxRecursionDepth) return new List<Category>();

            IEnumerable<Category> children = lookup[parentId];
            if (sortFunc != null)
                children = sortFunc(children);
            else
                children = children.OrderBy(x => x.SortOrder);

            return children.Select(x => new Category
            {
                CategoryId = x.CategoryId,
                ParentId = x.ParentId,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                Content = x.Content,
                Image = x.Image,
                ImageAlt = x.ImageAlt,
                Banner = x.Banner,
                Icon = x.Icon,
                SortOrder = x.SortOrder,
                Status = x.Status,
                SeoTitle = x.SeoTitle,
                SeoDescription = x.SeoDescription,
                SeoKeywords = x.SeoKeywords,
                CategoryType = x.CategoryType,
                CategoryTypeName = x.CategoryTypeName,
                LinkType = x.LinkType,
                Url = x.Url,
                Target = x.Target,
                MenuPosition = x.MenuPosition,
                ParentName = x.ParentName,
                ItemCount = x.ItemCount,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedBy = x.UpdatedBy,
                Children = BuildTreeInternal(lookup, x.CategoryId, depth + 1, sortFunc)
            }).ToList();
        }

        /// <summary>
        /// Tính tổng ItemCount (aggregate) cho TẤT CẢ categories trong 1 lần duyệt O(N).
        /// Thuật toán bottom-up: tổng = ItemCount bản thân + tổng tất cả con cháu.
        /// </summary>
        public static Dictionary<int, int> ComputeAggregateCounts(List<Category> categories)
        {
            var lookup = categories.ToLookup(x => x.ParentId);
            var selfCounts = categories.ToDictionary(x => x.CategoryId, x => x.ItemCount);
            var aggregateCounts = new Dictionary<int, int>();

            int Compute(int categoryId)
            {
                if (aggregateCounts.TryGetValue(categoryId, out int cached)) return cached;

                int self = selfCounts.GetValueOrDefault(categoryId, 0);
                int childSum = 0;
                foreach (var child in lookup[categoryId])
                {
                    childSum += Compute(child.CategoryId);
                }
                aggregateCounts[categoryId] = self + childSum;
                return self + childSum;
            }

            foreach (var cat in categories)
            {
                Compute(cat.CategoryId);
            }

            return aggregateCounts;
        }

        public static List<Category> Search(string? keyword, int? parentId, int? status, string sortColumn, string sortOrder, int pageIndex, int pageSize, int? categoryType = null)
        {
            return FCategory.Search(keyword, parentId, status, categoryType, sortColumn, sortOrder, pageIndex, pageSize);
        }

        public static int Insert(Category obj)
        {
            int result = FCategory.Insert(obj);
            if (result > 0) ClearCacheAndRefresh();
            return result;
        }

        public static int Update(Category obj)
        {
            int result = FCategory.Update(obj);
            if (result > 0) ClearCacheAndRefresh();
            return result;
        }

        public static int UpdateQuick(int id, int? status, int? sortOrder)
        {
            var result = BaseConnectionSql.ExecuteNonQuery(
                "SP_Categories_UpdateQuick",
                new Microsoft.Data.SqlClient.SqlParameter("@CategoryId", id),
                new Microsoft.Data.SqlClient.SqlParameter("@SortOrder", (object?)sortOrder ?? DBNull.Value),
                new Microsoft.Data.SqlClient.SqlParameter("@Status", (object?)status ?? DBNull.Value)
            );
            if (result > 0) ClearCacheAndRefresh();
            return result;
        }

        public static int Delete(int id)
        {
            int result = FCategory.Delete(id);
            if (result > 0) ClearCacheAndRefresh();
            return result;
        }

        /// <summary>
        /// Xóa nhiều danh mục cùng lúc - chỉ clear cache và refresh sitemap 1 LẦN DUY NHẤT.
        /// Tránh N+1 query pattern khi gọi Delete() trong vòng lặp.
        /// </summary>
        public static int BulkDelete(List<int> ids)
        {
            int deleted = 0;
            foreach (var id in ids)
            {
                int result = FCategory.Delete(id);
                if (result > 0) deleted++;
            }
            if (deleted > 0) ClearCacheAndRefresh();
            return deleted;
        }

        /// <summary>
        /// Cập nhật nhanh nhiều danh mục cùng lúc - chỉ clear cache 1 LẦN.
        /// </summary>
        public static int BulkUpdateQuick(List<int> ids, int? status, int? sortOrder)
        {
            int updated = 0;
            foreach (var id in ids)
            {
                var result = BaseConnectionSql.ExecuteNonQuery(
                    "SP_Categories_UpdateQuick",
                    new Microsoft.Data.SqlClient.SqlParameter("@CategoryId", id),
                    new Microsoft.Data.SqlClient.SqlParameter("@SortOrder", (object?)sortOrder ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@Status", (object?)status ?? DBNull.Value)
                );
                if (result > 0) updated++;
            }
            if (updated > 0) ClearCacheAndRefresh();
            return updated;
        }

        /// <summary>
        /// Xóa toàn bộ cache liên quan đến Category VÀ refresh sitemap.
        /// Đây là điểm duy nhất gọi RefreshSitemap - controller KHÔNG CẦN gọi lại.
        /// </summary>
        private static void ClearCacheAndRefresh()
        {
            SCache.Remove("Category_All");
            SCache.Remove("Category_Hierarchical");
            SCache.Remove("Menu_Tree_Header");
            SCache.Remove("Menu_Tree_Footer");
            
            // Xóa cache cấu hình trang chủ
            SCache.Remove("HomeCategorySettings_All");
            SCache.Remove("Home_HomeCategories");
            
            // Xóa luôn cache sản phẩm của các danh mục trang chủ (do đổi tên, đổi trạng thái...)
            try {
                var homeCats = BaseConnectionSql.ExecuteStoredProcedure<HomeCategorySetting>("SP_HomeCategorySettings_GetAll", null);
                if (homeCats != null) {
                    foreach (var hc in homeCats) {
                        SCache.Remove($"Home_CategoryProducts_{hc.CategoryId}");
                    }
                }
            } catch { }
            
            SSeo.RefreshSitemapAndCache();
        }
    }
}
