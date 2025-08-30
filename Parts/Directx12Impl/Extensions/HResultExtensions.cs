using Silk.NET.Core.Native;

using System.Runtime.InteropServices;

namespace Directx12Impl.Extensions;
public static class HResultExtensions
{
  public static Exception GetException(this HResult _hr)
  {
    return Marshal.GetExceptionForHR(_hr);
  }
}
