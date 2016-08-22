using Dapper;
using System.Linq;
using DataAccess.Config;
using System.Collections.Generic;

namespace DataAccess.Main
{
    /// <summary>
    /// 如果找不到那个配制的sql 调用方请抛出错误
    /// </summary>
    public class DataCommand
    {
        /// <summary>
        /// sql文件中所配制的一个节点数据
        /// </summary>
        private CommandContent commandConfig;

        /// <summary>
        /// sql文件中生成的参数集合
        /// </summary>
        private List<Parameter> configParemeterList;

        private DynamicParameters dapperParameters;

        internal DataCommand(CommandContent config)
        {
            commandConfig = config;
            configParemeterList = GetParametersFromConfig();
            dapperParameters = new DynamicParameters();
            AddReturnOrOutPutParameter();
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        public void SetParameterValue(string parameterName, object value)
        {
            parameterName = CheckAndAddPre(parameterName);
            var configTemp = configParemeterList.FirstOrDefault(item => 
                string.Equals(item.Name, parameterName, System.StringComparison.OrdinalIgnoreCase));

            if (configTemp != null)
                dapperParameters.Add(configTemp.Name, value, configTemp.DbType, configTemp.Direction, configTemp.Size);
        }

        public 

        private List<Parameter> GetParametersFromConfig()
        {
            if (commandConfig.Parameters == null || commandConfig.Parameters.Count == 0)
                return new List<Parameter>();

            return commandConfig.Parameters.Select(item => 
                    new Parameter {
                        Name = item.Name,
                        DbType = item.DbType,
                        Direction = item.Direction,
                        Size = item.Size
                    }).ToList();
        }

        private string CheckAndAddPre(string paramName)
        {
            if (!paramName.StartsWith("@"))
                paramName = string.Format("@{0}", paramName);
            return paramName;
        }

        /// <summary>
        /// 将输出参数和返回参数加到参数列表中
        /// </summary>
        private void AddReturnOrOutPutParameter()
        {
            configParemeterList.FindAll(item =>
                item.Direction == System.Data.ParameterDirection.Output ||
                item.Direction == System.Data.ParameterDirection.ReturnValue)
            .ForEach(temp => dapperParameters.Add(temp.Name, null, temp.DbType, temp.Direction, temp.Size));
        }
    }
}
