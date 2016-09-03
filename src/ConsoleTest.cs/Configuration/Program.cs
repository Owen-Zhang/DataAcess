using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleTest.cs.Model;
using DataAccess.Main;

namespace ConsoleTest.cs
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start");
            DbManager.LoadDataAccessConfig();

            
            var command = DbManager.GetDataCommand("GetSellerInfo");
            command.SetParameterValue("@SellerId", "A002");
            //command.ExecuteNonQuery();
            var result = command.QueryList<string>();
            
            /*
            var command = DbManager.GetDataCommand("GetSellerInfo2");
            command.SetParameterValue("@SellerId", "xxx");

            try
            {
                var gridRead = command.QueryMultiple();
                var first = gridRead.Read<SellerInfo>();
                var second = gridRead.Read<SellerInfo>();
            }
            catch (Exception e)
            {
            }
            finally {
                command.CloseConnection();
            }*/
               
            Console.ReadLine();
        }
    }
}
