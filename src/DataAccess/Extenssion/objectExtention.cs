namespace DataAccess.Extenssion
{
    public static class objectExtention
    {
        /// <summary>
        /// 将对象转换成json字符串
        /// </summary>
        public static string ToJson(this object content)
        {
            if (content == null)
                return string.Empty;

            if (content is string)
                return (string)content;

            return ServiceStack.Text.JsonSerializer.SerializeToString(content);
        }
    }
}
