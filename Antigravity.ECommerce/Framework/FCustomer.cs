using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FCustomer
    {
        public static int Insert(Customer model)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@FullName", model.FullName),
                new SqlParameter("@Phone", model.Phone),
                new SqlParameter("@Email", (object?)model.Email ?? DBNull.Value),
                new SqlParameter("@Password", (object?)model.Password ?? DBNull.Value),
                new SqlParameter("@Avatar", (object?)model.Avatar ?? DBNull.Value),
                new SqlParameter("@DateOfBirth", (object?)model.DateOfBirth ?? DBNull.Value),
                new SqlParameter("@Gender", model.Gender),
                new SqlParameter("@Address", (object?)model.Address ?? DBNull.Value),
                new SqlParameter("@WardId", (object?)model.WardId ?? DBNull.Value),
                new SqlParameter("@ProvinceId", (object?)model.ProvinceId ?? DBNull.Value),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@Note", (object?)model.Note ?? DBNull.Value)
            };
            return Convert.ToInt32(BaseConnectionSql.ExecuteScalar("SP_Customers_Insert", prms));
        }

        public static int Update(Customer model)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@CustomerId", model.CustomerId),
                new SqlParameter("@FullName", model.FullName),
                new SqlParameter("@Phone", model.Phone),
                new SqlParameter("@Email", (object?)model.Email ?? DBNull.Value),
                new SqlParameter("@Avatar", (object?)model.Avatar ?? DBNull.Value),
                new SqlParameter("@DateOfBirth", (object?)model.DateOfBirth ?? DBNull.Value),
                new SqlParameter("@Gender", model.Gender),
                new SqlParameter("@Address", (object?)model.Address ?? DBNull.Value),
                new SqlParameter("@WardId", (object?)model.WardId ?? DBNull.Value),
                new SqlParameter("@ProvinceId", (object?)model.ProvinceId ?? DBNull.Value),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@Note", (object?)model.Note ?? DBNull.Value)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_Customers_Update", prms);
        }

        public static List<Customer> Search(string? kw, int? rank, int? status, string sort, string order, int page, int size)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@Keyword", (object?)kw ?? DBNull.Value),
                new SqlParameter("@MemberRank", (object?)rank ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@ProvinceId", DBNull.Value), // Reserved for future use
                new SqlParameter("@WardId", DBNull.Value),    // Reserved for future use
                new SqlParameter("@SortColumn", sort ?? "CreatedAt"),
                new SqlParameter("@SortOrder", order ?? "DESC"),
                new SqlParameter("@PageIndex", page),
                new SqlParameter("@PageSize", size)
            };
            return BaseConnectionSql.ExecuteStoredProcedure<Customer>("SP_Customers_Search", prms);
        }

        public static Customer? GetById(int id)
        {
            var prms = new SqlParameter[] { new SqlParameter("@CustomerId", id) };
            var data = BaseConnectionSql.ExecuteStoredProcedure<Customer>("SP_Customers_GetById", prms);
            if (data != null && data.Count > 0) return data[0];
            return null;
        }

        public static int RefreshRank(int customerId)
        {
            var prms = new SqlParameter[] { new SqlParameter("@CustomerId", customerId) };
            return BaseConnectionSql.ExecuteNonQuery("SP_Customers_RefreshRank", prms);
        }

        public static List<Order> GetOrderHistory(int customerId, string? kw = null, int? status = null, int page = 1, int size = 20)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@Keyword", (object?)kw ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@PageIndex", page),
                new SqlParameter("@PageSize", size)
            };
            return BaseConnectionSql.ExecuteStoredProcedure<Order>("SP_Customers_OrderHistory", prms);
        }

        public static int Delete(int customerId)
        {
            var prms = new SqlParameter[] { new SqlParameter("@CustomerId", customerId) };
            return BaseConnectionSql.ExecuteNonQuery("SP_Customers_Delete", prms);
        }

        public static Customer? GetByPhone(string phone)
        {
            var prms = new SqlParameter[] { new SqlParameter("@Keyword", phone), new SqlParameter("@PageIndex", 1), new SqlParameter("@PageSize", 1) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Customer>("SP_Customers_Search", prms);
            if (list != null && list.Count > 0 && list[0].Phone == phone) return list[0];
            return null;
        }

        public static Customer? GetByEmailOrPhone(string username)
        {
            var prms = new SqlParameter[] { new SqlParameter("@Username", username) };
            var data = BaseConnectionSql.ExecuteStoredProcedure<Customer>("SP_Customers_GetByEmailOrPhone", prms);
            if (data != null && data.Count > 0) return data[0];
            return null;
        }

        public static int CheckExists(string phone, string? email)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@Phone", phone),
                new SqlParameter("@Email", (object?)email ?? DBNull.Value)
            };
            return Convert.ToInt32(BaseConnectionSql.ExecuteScalar("SP_Customers_CheckExists", prms));
        }

        public static int UpdatePassword(int customerId, string password)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@Password", password)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_Customers_UpdatePassword", prms);
        }

        public static int UpdateProfile(Customer model)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@CustomerId", model.CustomerId),
                new SqlParameter("@FullName", model.FullName),
                new SqlParameter("@Phone", model.Phone),
                new SqlParameter("@DateOfBirth", (object?)model.DateOfBirth ?? DBNull.Value),
                new SqlParameter("@Gender", model.Gender),
                new SqlParameter("@Address", (object?)model.Address ?? DBNull.Value),
                new SqlParameter("@WardId", (object?)model.WardId ?? DBNull.Value),
                new SqlParameter("@ProvinceId", (object?)model.ProvinceId ?? DBNull.Value),
                new SqlParameter("@Avatar", (object?)model.Avatar ?? DBNull.Value)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_Customers_UpdateProfile", prms);
        }
    }
}
