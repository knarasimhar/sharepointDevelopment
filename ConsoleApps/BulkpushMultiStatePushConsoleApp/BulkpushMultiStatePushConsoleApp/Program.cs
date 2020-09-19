using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using System.Management;

namespace BulkpushMultiStatePushConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string strStates = getConfigvalue("stateids");
            try
            {
                // Get the current process.
                Process currentProcess = Process.GetCurrentProcess();

                // Get all processes running on the local computer.
                Process[] localAll = Process.GetProcesses();

                // Get all instances of Notepad running on the local computer.
                // This will return an empty array if notepad isn't running.
                Process[] localByName = Process.GetProcessesByName("BulkPushConsoleApp");
                string strCurRunnStateids = "";
                foreach(Process Proc in localByName)
                { 
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + Proc.Id))
                using (ManagementObjectCollection objects = searcher.Get())
                {
                        // return 
                        if(objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString()!=null)
                        strCurRunnStateids +=  objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString().Split(' ')[1] + ",";
                }
                }
               // return;
                foreach (string stateid in strStates.Split(','))
                {

                    Console.WriteLine("Processing stateid is " + stateid);
                    if (stateid != "" && !strCurRunnStateids.Contains("," + stateid + ","))
                    { 
                        ProcessStartInfo info = new ProcessStartInfo(getConfigvalue("exepath"));
                        //info.UseShellExecute = false;
                        info.Arguments = stateid;
                        Process.Start(info);
                      }
                    // System.Diagnostics.Process.Start(getConfigvalue("exepath") + " " + stateid);
                   // System.Diagnostics.Process.Start(@"D:\ConsoleTasks\SPBulkPush_FMRTASKS_MultiArges\BulkPushConsoleApp.exe" + " " + stateid);
                }
                
                //Console.ReadLine();
                //return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //Console.ReadLine();

        }

        public static String getConfigvalue(String key)
        {
            if (ConfigurationSettings.AppSettings[key] != null)
                return ConfigurationSettings.AppSettings[key];
            else
                return "";
        }
    }
}
