
using System;

public class LoadingException : Exception
{
    public LoadingException()
        : base()
    {

    }

    public LoadingException(string message)
        :base(message)
    {

    }

    public LoadingException(Exception inner)
        :base("Unspecified exception when loading assets.", inner)
    {

    }

    public LoadingException(string message, Exception inner)
        :base(message, inner)
    {

    }
}