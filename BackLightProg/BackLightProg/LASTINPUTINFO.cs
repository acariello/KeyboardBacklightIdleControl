using System;
using System.Runtime.InteropServices;

namespace BackLightProg
{
      [StructLayout(LayoutKind.Sequential)]
      public struct LASTINPUTINFO 
   {
         public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

         [MarshalAs(UnmanagedType.U4)]
         public UInt32 cbSize;
         [MarshalAs(UnmanagedType.U4)]
         public UInt32 dwTime;


   }
   
}