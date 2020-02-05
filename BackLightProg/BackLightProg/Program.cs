using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections;

namespace BackLightProg
{
   static class Constants
   {
      public const UInt64 TIMEOUT_MS            = (15 * 1000);
      public const UInt64 TIMER_INTERVAL_MS     = 1000;

      public const string CMD_EXE_NAME          = "cmd.exe";
      public const string CMD_EXECUTE_CMD       = "/C ";
      public const string TASK_KILL_CMD         = "TASKKILL /IM tposd.exe";

      public enum E_IDLE_STATE
      {
         STATE_NONE,
         STATE_IDLE,
         STATE_ACTIVE,

         STATE_MAX
      }
   }

   static class Program
   {
      [DllImport("user32.dll")]
      static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

      static Constants.E_IDLE_STATE m_eCurrIdleState = Constants.E_IDLE_STATE.STATE_NONE;

      static MethodInfo m_miSetKeyboardBackLightStatusFunc;

      static object m_objObject;


      static bool InitializeLenovoKeyboardHandler()
      {
         bool bReturnStatus = false;

         TraceLog("InitializeLenovoKeyboardHandler " + DateTime.Now);

         Assembly myAssembly;

         myAssembly = Assembly.LoadFile("C:\\ProgramData\\Lenovo\\ImController\\Plugins\\ThinkKeyboardPlugin\\x86\\Keyboard_Core.dll");

         TraceLog("LoadFile " + DateTime.Now);

         Type myType = myAssembly.GetType("Keyboard_Core.KeyboardControl");

         TraceLog("GetType " + DateTime.Now);

         m_objObject = Activator.CreateInstance(myType);

         TraceLog("CreateInstance " + DateTime.Now);

         IEnumerable list = myType.GetMethods();

         TraceLog("GetMethods " + DateTime.Now);

         m_miSetKeyboardBackLightStatusFunc = MethodClass.GetRuntimeMethodsExt(myType, "SetKeyboardBackLightStatus", new Type[] { });

         TraceLog("GetRuntimeMethodsExt " + DateTime.Now);

         bReturnStatus = true;

         return bReturnStatus;
      }
      static void Main(string[] args)
      {
         InitializeLenovoKeyboardHandler();

         // Run forever
         while (true)
         {
            Thread.Sleep((int)Constants.TIMER_INTERVAL_MS);

            ProcessIdleCheck();
         }
      }

      // Every 1000ms 
      static void ProcessIdleCheck()
      {
         UInt64 nIdleTime = 0;
         UInt64 nEnvTicks = 0;
         UInt64 nLastInputTick = 0;
         LASTINPUTINFO lastInputInfo;

         lastInputInfo = new LASTINPUTINFO();
         lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

         lastInputInfo.dwTime = 0;

         nEnvTicks = (UInt64)Environment.TickCount;

         if (GetLastInputInfo(ref lastInputInfo))
         {
            nLastInputTick = lastInputInfo.dwTime;

            nIdleTime = nEnvTicks - nLastInputTick;
         }

         // Process state change
         if (nIdleTime > Constants.TIMEOUT_MS)
         {
            // On change
            if (m_eCurrIdleState != Constants.E_IDLE_STATE.STATE_IDLE)
            {
               // We are idle now
               m_eCurrIdleState = Constants.E_IDLE_STATE.STATE_IDLE;

               EnableBacklight(false);
            }
         }
         else if (nIdleTime < Constants.TIMEOUT_MS)
         {
            // On change
            if (m_eCurrIdleState != Constants.E_IDLE_STATE.STATE_ACTIVE)
            {
               // Not idle anymore
               m_eCurrIdleState = Constants.E_IDLE_STATE.STATE_ACTIVE;

               EnableBacklight(true);
            }
         }

         GC.Collect();
      }

      static int EnableBacklight(bool bIsOn)
      {
         object[] objArguments;

         if (bIsOn)
         {
            // Full backlight power 2
            objArguments = new object[] { 2 };
         }
         else
         {
            // Disable backlight completely 0
            objArguments = new object[] { 0 };
         }

         // Prevent the on screen display image of backlight control changing
         RunSystemCommand(Constants.TASK_KILL_CMD);

         UInt32 nReturnCode = (UInt32)m_miSetKeyboardBackLightStatusFunc.Invoke(m_objObject, objArguments);

         // Prevent the on screen display image of backlight control changing
         RunSystemCommand(Constants.TASK_KILL_CMD);

         return (int)nReturnCode;
      }

      static void RunSystemCommand(string strCommand)
      {
         System.Diagnostics.Process process = new System.Diagnostics.Process();
         System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

         startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
         startInfo.FileName = Constants.CMD_EXE_NAME;
         startInfo.Arguments = Constants.CMD_EXECUTE_CMD + strCommand;

         process.StartInfo = startInfo;

         process.Start();
      }

      static void TraceLog(string Message)
      {
         string strPath = "";
         string strFilePath = "";

         strPath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";

         if (!Directory.Exists(strPath))
         {
            Directory.CreateDirectory(strPath);
         }

         strFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

         if (!File.Exists(strFilePath))
         {
            // Create file 
            using (StreamWriter sw = File.CreateText(strFilePath))
            {
               sw.WriteLine(Message);
            }
         }
         else
         {
            // Append to file
            using (StreamWriter sw = File.AppendText(strFilePath))
            {
               sw.WriteLine(Message);
            }
         }
      }
   }
}




