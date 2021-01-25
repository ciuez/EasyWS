using SAPB1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.Json;

namespace EasyWSServices.Code
{

    public class apiResponse
    {
        public bool success { get; set; }
        public dynamic response { get; set; }
        public apiResponse(bool _success, dynamic _response)
        {
            this.success = _success;
            this.response = _response;
        }
    }
    public class cliente_piva_servizio
    {
        public string famiglia { get; set; }
    }
    public class cliente_piva
    {
        public string piva { get; set; }
        public string ragsoc { get; set; }
        public string commerciale { get; set; }
        public List<cliente_piva_servizio> servizi { get; set; }
    }

    public class Api
    {
        public static apiResponse Login(string username, string password)
        {
            apiResponse _ret = new apiResponse(false, null);

            if (!String.IsNullOrEmpty(username))
            {
                if (!String.IsNullOrEmpty(password))
                {
                    string user = Code.Methods.GetConfiguration().GetSection("appSettings").GetSection("username").Value;
                    string pass = Code.Methods.GetConfiguration().GetSection("appSettings").GetSection("password").Value;

                    if (username == user && pass == password)
                    {
                        _ret = new apiResponse(true, JWTokenUtility.GenerateToken(1, username));
                    }
                }
            }
            return _ret;
        }
        public static apiResponse LoginAuth(Int64 user_ID)
        {
            apiResponse _ret = new apiResponse(true, null);
            return _ret;
        }

        public static apiResponse CliPIVA(string ipAddress, string piva)
        {
            apiResponse _ret = new apiResponse(false, null);
            
            if (!String.IsNullOrEmpty(piva))
            {
                string msg = "";

                string sap_uri = Code.Methods.GetConfiguration().GetSection("appSettings").GetSection("sap_uri").Value;
                string sap_user = Code.Methods.GetConfiguration().GetSection("appSettings").GetSection("sap_user").Value;
                string sap_psw = Code.Methods.GetConfiguration().GetSection("appSettings").GetSection("sap_psw").Value;
                string sap_db = Code.Methods.GetConfiguration().GetSection("appSettings").GetSection("sap_db").Value;

                if (Easy_SAP_lib.connessione.Connetti(sap_uri, sap_user, sap_psw, sap_db, ref msg))
                {
                    //IT00716060967
                    //CZ04312210
                    //IT09548810960
                    BusinessPartner bp = null;
                    SalesPerson comm = null;
                    Easy_SAP_lib.clienti.cliente_sel_from_piva(piva, ref msg, ref bp, ref comm);

                    cliente_piva cliente = new cliente_piva();

                    if (bp != null)
                    {
                        cliente.piva = bp.FederalTaxID;
                        cliente.ragsoc = bp.CardName;
                        cliente.commerciale = comm.SalesEmployeeName;

                        string spName = "EASYGEST..sp_prodotti_elenco3";
                        List<SqlParameter> param = new List<SqlParameter>();
                        //param.Add(new SqlParameter("@codCli", bp.CardCode.Substring(bp.CardCode.Length - 3)));
                        //param.Add(new SqlParameter("@clidesc", ""));
                        param.Add(new SqlParameter("@codCli", ""));
                        param.Add(new SqlParameter("@clidesc", bp.CardName));
                        DataTable dt = DB.GetDT2(spName, param, ref msg);

                        List<cliente_piva_servizio> servizi = new List<cliente_piva_servizio>();
                        if (dt != null)
                        {
                            foreach (DataRow r in dt.Rows)
                            {
                                cliente_piva_servizio servizio = new cliente_piva_servizio();
                                servizio.famiglia = r["famiglia"].ToString();
                                servizi.Add(servizio);
                            }
                        }
                        cliente.servizi = servizi;
                    }
                    
                    _ret = new apiResponse(true, cliente);
                    
                    Easy_SAP_lib.connessione.Disconnetti(ref msg);
                }                
            }
            CliPIVA_Log(ipAddress, piva, _ret);
            return _ret;
        }

        private static void CliPIVA_Log(string ipAddress, string piva, apiResponse ret)
        {
            string fileName = "Log.txt";
            if (File.Exists("Logs/" + fileName))
            {
                long size = new FileInfo("Logs/" + fileName).Length;
                if (size >= 10 * 1024 * 1024)
                {
                    File.Move("Logs/" + fileName, "Logs/" + DateTime.Now.Year.ToString() + 
                        DateTime.Now.Month.ToString().PadLeft(2,'0') +
                        DateTime.Now.Day.ToString().PadLeft(2, '0') + 
                        "_" + fileName);
                }
            }
            using (StreamWriter sw = File.AppendText("Logs/" + fileName))
            {
                sw.WriteLine(DateTime.Now.ToString() + " - WebService: CliPIVA");
                sw.WriteLine(DateTime.Now.ToString() + " - IP address: " + ipAddress);
                sw.WriteLine(DateTime.Now.ToString() + " - P.IVA input: " + piva);
                if (ret.response != null)
                {
                    sw.WriteLine(DateTime.Now.ToString() + " - Response: " + JsonSerializer.Serialize(ret.response));
                }
                else
                {
                    sw.WriteLine(DateTime.Now.ToString() + " - Response: null");
                }
            }
        }
    }
}