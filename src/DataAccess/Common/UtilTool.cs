using System;
using System.IO;
using System.Xml.Serialization;
namespace DataAccess.Common
{
    public class UtilTool
    {
        /// <summary>
        /// xml反序列化， 由于servicestack.xmlserializer 有问题，就用msdn的了
        /// </summary>
        public static T XmlDeserialize<T>(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return default(T);

            XmlSerializer mySerializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(str))
            {
                return (T)mySerializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// json 反序列化
        /// </summary>
        public static T JsonDeserialize<T>(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return default(T);

            return 
                ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(str);
        }

        /// <summary>
        /// 序列化成josn
        /// </summary>
        public static string JsonSerialize(object model)
        {
            if (model == null) return string.Empty;

            return
                ServiceStack.Text.JsonSerializer.SerializeToString(model);
        }

        /// <summary>
        /// if relative path return full Path
        /// </summary>
        public static string GetFilePath(string path, string pathFolder = null)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            string root = Path.GetPathRoot(path);
            if (string.IsNullOrWhiteSpace(root))
            {
                if (string.IsNullOrWhiteSpace(pathFolder))
                    return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                else
                    return Path.Combine(pathFolder, path);
            }
            return path;
        }
    }
}
