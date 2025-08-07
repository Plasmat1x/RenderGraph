using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAPI.Enums;
public enum ResourceDimension
{
  Unknown,
  Buffer,
  Texture1D,
  Texture1DArray,
  Texture2D,
  Texture2DArray,
  Texture2DMS,
  Texture2DMSArray,
  Texture3D,
  TextureCube,
  TextureCubeArray,
  ExtendedBuffer,
  StructuredBuffer,
  ByteAddressBuffer,
  Stream,
  AccelerationStructure,
  BufferEx
}