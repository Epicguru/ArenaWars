using UnityEngine;

public static class StringFormatting
{
    public static string Form(this string str, params object[] args)
    {
        return string.Format(str, args);
    }
}