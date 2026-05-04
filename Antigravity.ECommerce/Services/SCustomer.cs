using System;
using System.Collections.Generic;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    public class SCustomer
    {
        public static int Insert(Customer model)
        {
            return FCustomer.Insert(model);
        }

        public static int Update(Customer model)
        {
            return FCustomer.Update(model);
        }

        public static List<Customer> Search(string? kw, int? rank, int? status, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            return FCustomer.Search(kw, rank, status, sort, order, page, size);
        }

        public static Customer? GetById(int id)
        {
            return FCustomer.GetById(id);
        }

        public static int RefreshRank(int customerId)
        {
            return FCustomer.RefreshRank(customerId);
        }

        public static List<Order> GetOrderHistory(int customerId, string? kw = null, int? status = null, int page = 1, int size = 20)
        {
            return FCustomer.GetOrderHistory(customerId, kw, status, page, size);
        }

        public static int Delete(int customerId)
        {
            return FCustomer.Delete(customerId);
        }

        public static Customer? GetByPhone(string phone)
        {
            return FCustomer.GetByPhone(phone);
        }
    }
}
