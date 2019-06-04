using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace İleri_Programlama
{
    public static class dbConnection
    {
        public static string srConnectionString = "server=localhost\\SQLEXPRESS;database=school_management;Integrated Security=SSPI;"; 
        //delete \\SQLEXPRESS if error is happening        
        
        //"Data Source=localhost;" +"Initial Catalog=school_management;" +"Integrated Security=SSPI;";

        
       

        public class cmdParameterType
        {

            public cmdParameterType(string _parameterName, object _objParam)
            {
                parameterName = _parameterName;
                objParam = _objParam;
            }

            

            public string parameterName = "";
            public object objParam;
        }

        public static DataTable DB_Select(string srQuery,List<cmdParameterType> lstParameters)
        {
            try
            {
                var dtTable = new DataTable();

                using (var connection = new SqlConnection(srConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(srQuery, connection))
                    {
                        foreach (var vrPerParameter in lstParameters)
                        {
                            command.Parameters.AddWithValue(vrPerParameter.parameterName, vrPerParameter.objParam);
                        }
                        connection.Open();
                        dtTable.Load(command.ExecuteReader());
                    }
                }
                return dtTable;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
            
        }

    }
}

