using System;
using System.Data.SqlClient;
using Login_Agent_578.Config;

namespace Login_Agent_578
{
    internal class SqlClient
    {
        private gConfig g_Config;

        internal SqlClient()
        {
            g_Config = gConfig.getInstance();
        }

        internal AccountInfo getAccountInfo(string id)
        {
            AccountInfo info = new AccountInfo();
            string query = string.Format(g_Config.QueryString[0], id);

            using (SqlConnection conn = new SqlConnection(g_Config.SqlConn))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                try
                {
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            info.id = rdr[0].ToString().Trim();
                            info.pwd = rdr[1].ToString().Trim();
                            info.status = rdr[2].ToString().Trim();
                            info.expDate = Convert.ToDateTime(rdr[3].ToString().Trim());
                        }
                    }
                }
                catch { }
            }
            return info;
        }

        internal DateTime getExpireDate(string id)
        {
            DateTime date = DateTime.Now.AddMinutes(3);
            string query = string.Format(g_Config.QueryString[1], id);

            using (SqlConnection conn = new SqlConnection(g_Config.SqlConn))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                try
                {
                    conn.Open();
                    //날짜가 null일경우 현재 시각 리턴
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                            date = Convert.ToDateTime(rdr[0].ToString().Trim());
                    }
                }
                catch { }
            }
            return date;
        }

        internal bool isConnectable()
        {
            bool isConn = false;
            string query = "select getdate()";

            using (SqlConnection conn = new SqlConnection(g_Config.SqlConn))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                try
                {
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                            isConn = true;
                    }
                }
                catch { }
            }
            return isConn;
        }

        internal bool isExistsTable(string table)
        {
            bool isExists = false;
            string query = string.Format("select count(*) from information_schema.tables where table_name = '{0}'", table);

            using (SqlConnection conn = new SqlConnection(g_Config.SqlConn))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                try
                {
                    conn.Open();
                    object countValue = cmd.ExecuteScalar();
                    if (0 < (int)countValue)
                        isExists = true;
                }
                catch { }
            }
            return isExists;
        }

        internal bool isExistsColumn(string column, string table)
        {
            bool isExists = false;
            string query = string.Format("select count(*) from information_schema.columns where column_name = '{0}' and table_name = '{1}'", column, table);

            using (SqlConnection conn = new SqlConnection(g_Config.SqlConn))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                try
                {
                    conn.Open();
                    object countValue = cmd.ExecuteScalar();
                    if (0 < (int)countValue)
                        isExists = true;
                }
                catch { }
            }
            return isExists;
        }
    }
}