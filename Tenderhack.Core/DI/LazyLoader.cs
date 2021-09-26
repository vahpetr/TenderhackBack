using System;
using Microsoft.Extensions.DependencyInjection;

namespace Tenderhack.Core.DI
{
  public class LazyLoader<T> : Lazy<T>
  {
    public LazyLoader(IServiceProvider sp) : base(() => sp.GetRequiredService<T>())
    {
    }
  }

}
