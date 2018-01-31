using System;

namespace HelseID.Clients.Common
{
    public static class StringExtensions
    {
        public static bool IsNotNullOrEmpty(this String text)
        {
            return !string.IsNullOrEmpty(text);
        }

        public static bool IsNullOrEmpty(this String text)
        {
            return string.IsNullOrEmpty(text);
        }
    }
}
