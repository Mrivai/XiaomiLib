

using System;
using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa.Util
{
  public class CommandFormat
  {
    public static byte[] StructToBytes(object structObj)
    {
      int length = 48;
      byte[] destination = new byte[length];
      IntPtr num = Marshal.AllocHGlobal(length);
      Marshal.StructureToPtr(structObj, num, false);
      Marshal.Copy(num, destination, 0, length);
      Marshal.FreeHGlobal(num);
      for (int index = 20; index < destination.Length; ++index)
        destination[index] = 0;
      return destination;
    }

    public static byte[] StructToBytes(object structObj, int length)
    {
      int length1 = length;
      byte[] destination = new byte[length1];
      IntPtr num = Marshal.AllocHGlobal(length1);
      Marshal.StructureToPtr(structObj, num, false);
      Marshal.Copy(num, destination, 0, length1);
      Marshal.FreeHGlobal(num);
      for (int index = 20; index < destination.Length; ++index)
        destination[index] = 0;
      return destination;
    }

    public static object BytesToStuct(byte[] bytes, Type type)
    {
      int num1 = Marshal.SizeOf(type);
      if (num1 > bytes.Length)
        return null;
      IntPtr num2 = Marshal.AllocHGlobal(num1);
      Marshal.Copy(bytes, 0, num2, num1);
      object structure = Marshal.PtrToStructure(num2, type);
      Marshal.FreeHGlobal(num2);
      return structure;
    }
  }
}
