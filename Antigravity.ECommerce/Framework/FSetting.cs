using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FSetting
    {
        public static List<Setting> GetAll()
        {
            return BaseConnectionSql.ExecuteStoredProcedure<Setting>("SP_Settings_GetAll", null);
        }

        public static int UpdateValue(string key, string value, string updatedBy)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@SettingKey", key),
                new SqlParameter("@SettingValue", (object)value ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", updatedBy)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_Settings_UpdateValue", prm);
        }
    }
}
