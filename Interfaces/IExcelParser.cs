using System;

namespace AISIots.Interfaces;

public interface IExcelParser<out T> : IDisposable
{
    T Parse();
}
