using System.Data;
using System.Runtime.CompilerServices;
using Borm.Data;

namespace Borm;

public sealed class Transaction : IDisposable, IAsyncDisposable
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private readonly List<Table> _tables;
    private readonly bool _writeOnCommit;

    internal Transaction()
    {

    }
}
