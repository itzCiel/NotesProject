namespace NotesProject.Extensions
{
    public static class StringExtensions
    {
        public static string ReduceLength(this string content, int length)
        {
            if (!string.IsNullOrEmpty(content))
            {
                if(content.Length > length)
                {
                    return content.Substring(0, length) + "...";
                }
            }
            return content;
        }
    }
}
