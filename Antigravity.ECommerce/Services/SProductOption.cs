using System;
using System.Collections.Generic;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    public class SProductOption
    {
        public static List<ProductOption> GetAll()
        {
            return SCache.GetOrSet("ProductOptions_All", () => FProductOption.GetAll(), 1440); // Cache 24h
        }

        public static ProductOption? GetById(int id)
        {
            return FProductOption.GetById(id);
        }

        public static int Insert(ProductOption obj)
        {
            int result = FProductOption.Insert(obj);
            if (result > 0) SCache.Remove("ProductOptions_All");
            return result;
        }

        public static int Update(ProductOption obj)
        {
            int result = FProductOption.Update(obj);
            if (result > 0) SCache.Remove("ProductOptions_All");
            return result;
        }

        public static int Delete(int id)
        {
            int result = FProductOption.Delete(id);
            if (result > 0) SCache.Remove("ProductOptions_All");
            return result;
        }
    }
}
