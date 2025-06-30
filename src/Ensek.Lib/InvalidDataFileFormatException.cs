namespace Ensek.Lib;

using System;

public class InvalidDataFileFormatException : Exception
{
    public InvalidDataFileFormatException(string msg)
        : base(msg)
    {
    }
}