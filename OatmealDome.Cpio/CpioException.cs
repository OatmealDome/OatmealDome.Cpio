namespace OatmealDome.Cpio;

public sealed class CpioException : Exception
{
    public CpioException(string message) : base(message)
    {
    }

    public CpioException(string message, Exception inner) : base(message, inner)
    {
    }
}