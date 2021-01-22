using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace EasyWSServices.Code
{
    public static class DB
    {
        public static DataTable GetDT(string SP)
        {
            DataTable GetDT = null/* TODO Change to default(_) if this is not a reference type */;
            string strErr = "";

            string sqlConn = Code.Methods.GetConfiguration().GetSection("wf_easynet").Value;
            var MyConn = new SqlConnection(sqlConn);
            SqlCommand sql;

            // SP = cancellaparolepericolose(SP)

            sql = new SqlCommand(SP, MyConn);
            sql.CommandTimeout = 500;
            System.Data.SqlClient.SqlDataAdapter dataAdapter = new System.Data.SqlClient.SqlDataAdapter(sql);
            System.Data.DataSet dataSet = new System.Data.DataSet();
            try
            {
                dataAdapter.Fill(dataSet);
                GetDT = dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                strErr = ex.Message;
            }
            return GetDT;
        }

        public static DataTable GetDT2(string StoreProcedureName, List<SqlParameter> Paramether, ref string errore)
        {
            DataTable GetDT2 = null/* TODO Change to default(_) if this is not a reference type */;
            try
            {
                string sqlConn = Code.Methods.GetConfiguration().GetSection("ConnectionStrings").GetSection("wf_easynet").Value;
                using (SqlConnection conn = new SqlConnection(sqlConn))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(StoreProcedureName))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;
                        cmd.CommandTimeout = 500;
                        foreach (SqlParameter s in Paramether)
                            cmd.Parameters.Add(s);
                        using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                        {
                            System.Data.DataSet dataSet = new System.Data.DataSet();
                            adp.Fill(dataSet);
                            GetDT2 = dataSet.Tables[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errore = ex.Message;
            }
            return GetDT2;
        }
    }
}
