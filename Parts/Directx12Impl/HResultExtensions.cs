using Silk.NET.Core.Native;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Directx12Impl;
public static class HResultExtensions
{
  public static Exception GetException(this HResult _hr)
  {
    return Marshal.GetExceptionForHR(_hr);
  }
}
