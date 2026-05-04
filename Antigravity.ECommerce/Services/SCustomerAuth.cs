using System;
using System.Security.Cryptography;
using System.Text;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    public class SCustomerAuth
    {
        public static Customer? GetByEmailOrPhone(string username)
        {
            return FCustomer.GetByEmailOrPhone(username);
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string input, string hash)
        {
            string hashOfInput = HashPassword(input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInput, hash) == 0;
        }

        public static Customer? Login(string username, string password)
        {
            var customer = GetByEmailOrPhone(username);
            if (customer != null && !string.IsNullOrEmpty(customer.Password))
            {
                if (VerifyPassword(password, customer.Password))
                {
                    return customer;
                }
            }
            return null;
        }

        public static int Register(Customer model)
        {
            int check = FCustomer.CheckExists(model.Phone, model.Email);
            if (check == 1) throw new Exception("Số điện thoại này đã được đăng ký.");
            if (check == 2) throw new Exception("Email này đã được đăng ký.");
            
            model.Password = HashPassword(model.Password ?? string.Empty);
            return FCustomer.Insert(model);
        }
    }
}
