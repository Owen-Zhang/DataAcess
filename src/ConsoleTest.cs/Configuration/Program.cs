using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleTest.cs.Model;
using DataAccess;

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
            
            
            var command2 = DbManager.GetDataCommand("GetSellerInfo2");
            command2.SetParameterValue("@SellerId", "A002");

            using (var gridRead = command2.QueryMultiple())
            {
                var first = gridRead.Read<SellerInfo>();
                var second = gridRead.Read<SellerInfo>();
                var i = 1;
            }

            var coommand3 = DbManager.GetDataCommand("UpateSellerInfo");
            coommand3.SetParameterValue("@SellerId", "A002");
            coommand3.SetParameterValue("@SellerName", "A002 Test agin");
            coommand3.ExecuteNonQuery();
               
            Console.ReadLine();
        }
    }
}
