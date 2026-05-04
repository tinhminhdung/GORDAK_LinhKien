using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FAdminUser
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return "";
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = md5.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public static AdminUser? Login(string username, string password)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Username", username) };
            var list = BaseConnectionSql.Query<AdminUser>("SELECT u.*, r.RoleName FROM AdminUsers u LEFT JOIN AdminRoles r ON u.RoleId = r.RoleId WHERE u.Username = @Username AND u.Status = 1", prm);
            
            if (list == null || list.Count == 0) return null;

            var user = list[0];
            string hashedInput = HashPassword(password);

            // Kiểm tra mật khẩu mã hóa MD5 HOẶC mật khẩu text thuần (hỗ trợ tài khoản cũ)
            if (user.Password == hashedInput || user.Password == password)
            {
                return user;
            }

            return null;
        }

        public static List<AdminPermission> GetPermissions(int roleId)
        {
            var prm = new SqlParameter[] { new SqlParameter("@RoleId", roleId) };
            return BaseConnectionSql.ExecuteStoredProcedure<AdminPermission>("SP_AdminPermissions_GetByRole", prm) ?? new List<AdminPermission>();
        }

        public static AdminUser? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@UserId", id) };
            var list = BaseConnectionSql.Query<AdminUser>("SELECT u.*, r.RoleName FROM AdminUsers u LEFT JOIN AdminRoles r ON u.RoleId = r.RoleId WHERE u.UserId = @UserId", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static List<AdminUser> GetAll()
        {
            return BaseConnectionSql.Query<AdminUser>("SELECT u.*, r.RoleName FROM AdminUsers u LEFT JOIN AdminRoles r ON u.RoleId = r.RoleId ORDER BY u.UserId DESC", null) ?? new List<AdminUser>();
        }

        public static int Insert(AdminUser model)
        {
            var sql = @"INSERT INTO AdminUsers (Username, Password, FullName, Email, Phone, Avatar, RoleId, Status, CreatedAt, CreatedBy) 
                        VALUES (@Username, @Password, @FullName, @Email, @Phone, @Avatar, @RoleId, @Status, GETDATE(), @CreatedBy); 
                        SELECT SCOPE_IDENTITY();";
            var prm = new SqlParameter[]
            {
                new SqlParameter("@Username", model.Username),
                new SqlParameter("@Password", HashPassword(model.Password ?? "")),
                new SqlParameter("@FullName", model.FullName ?? (object)DBNull.Value),
                new SqlParameter("@Email", model.Email ?? (object)DBNull.Value),
                new SqlParameter("@Phone", model.Phone ?? (object)DBNull.Value),
                new SqlParameter("@Avatar", model.Avatar ?? (object)DBNull.Value),
                new SqlParameter("@RoleId", model.RoleId ?? (object)DBNull.Value),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@CreatedBy", model.CreatedBy ?? "Admin")
            };
            var result = BaseConnectionSql.ExecuteScalar(sql, prm);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static int Update(AdminUser model)
        {
            var sql = @"UPDATE AdminUsers SET 
                        FullName = @FullName, Email = @Email, Phone = @Phone, Avatar = @Avatar, 
                        RoleId = @RoleId, Status = @Status, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy";
            
            var prmList = new List<SqlParameter> {
                new SqlParameter("@FullName", model.FullName ?? (object)DBNull.Value),
                new SqlParameter("@Email", model.Email ?? (object)DBNull.Value),
                new SqlParameter("@Phone", model.Phone ?? (object)DBNull.Value),
                new SqlParameter("@Avatar", model.Avatar ?? (object)DBNull.Value),
                new SqlParameter("@RoleId", model.RoleId ?? (object)DBNull.Value),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@UpdatedBy", model.UpdatedBy ?? "Admin"),
                new SqlParameter("@UserId", model.UserId)
            };

            if (!string.IsNullOrEmpty(model.Password))
            {
                sql += ", Password = @Password";
                prmList.Add(new SqlParameter("@Password", HashPassword(model.Password)));
            }

            sql += " WHERE UserId = @UserId";
            return BaseConnectionSql.ExecuteNonQuery(sql, prmList.ToArray());
        }

        public static int Delete(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@UserId", id) };
            return BaseConnectionSql.ExecuteNonQuery("DELETE FROM AdminUsers WHERE UserId = @UserId AND RoleId <> 1", prm); // Tránh xóa Admin chính
        }
    }
}
