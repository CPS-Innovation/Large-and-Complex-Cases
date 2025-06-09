
namespace CPS.ComplexCases.FileTransfer.API.Models.Domain.Exceptions;

[Serializable]
public class FileExistsException : Exception
{
    public FileExistsException(string message)
        : base(message)
    {
    }
}