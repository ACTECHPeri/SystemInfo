using System;
using System.Diagnostics;
using System.Management;

namespace SystemInfo
{
    public static class ServerSystem
    {
        public static string ServerName()
        {
            //Connection credentials to the remote computer - not needed if the logged in account has access
            ConnectionOptions oConn = new ConnectionOptions();
            //oConn.Username = "username";
            //oConn.Password = "password";

            // Get the system name:
            ManagementObjectSearcher search = new ManagementObjectSearcher("select SystemName from Win32_Processor");
            string computername = @"localhost";
            foreach (ManagementObject service in search.Get())
            {
                foreach (PropertyData data in service.Properties)
                {
                    if (data.Name == "SystemName")
                        computername = data.Value.ToString();
                }
            }

            return computername;
        }

        public static string GetMachinePerformance()
        {
            string output = "";

            output = output + CPUUsage.Query() + Environment.NewLine + RAM.Query() + Environment.NewLine + HardDisk.Query();

            return output;
        }
    }

    public static class HardDisk
    {
        public static string Query()
        {
            long mb = 1048576; //megabyte in # of bytes 1024x1024
            long gb = 1073741824; //gigabyte in # of bytes 1024x1024x1024

            //Connection credentials to the remote computer - not needed if the logged in account has access
            ConnectionOptions oConn = new ConnectionOptions();
            //oConn.Username = "username";
            //oConn.Password = "password";

            string computername = @"\\" + ServerSystem.ServerName();
            ManagementScope oMs = new ManagementScope(computername, oConn);
            //ManagementScope oMs = new ManagementScope("\\\\localhost", oConn);

            //get Fixed disk stats
            ObjectQuery oQuery = new ObjectQuery("select FreeSpace,Size,Name from Win32_LogicalDisk where DriveType=3");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);

            ManagementObjectCollection oReturnCollection = oSearcher.Get();

            //variables for numerical conversions
            double fs = 0;
            double us = 0;
            double tot = 0;
            double up = 0;
            double fp = 0;

            //for string formating args
            object[] oArgs = new object[2];

            string output = "";

            //loop through found drives and write out info
            foreach (ManagementObject oReturn in oReturnCollection)
            {
                // Disk name
                output = output + "Hard disk (" + oReturn["Name"].ToString() + "); ";

                //Free space in GB
                fs = Convert.ToInt64(oReturn["FreeSpace"]) / gb;

                //Total space in GB
                tot = Convert.ToInt64(oReturn["Size"]) / gb;

                //Used space in GB
                us = (Convert.ToInt64(oReturn["Size"]) - Convert.ToInt64(oReturn["FreeSpace"])) / gb;

                //used percentage
                up = us / tot * 100;

                //free percentage
                fp = fs / tot * 100;

                //used space args
                oArgs[0] = (object)us;
                oArgs[1] = (object)up;

                //write out used space stats
                output = output + string.Format("Used: {0:#,###.##} Gb ({1:###.##})%; ", oArgs);

                //free space args
                oArgs[0] = fs;
                oArgs[1] = fp;

                //write out free space stats
                output = output + string.Format("Free: {0:#,###.##} Gb ({1:###.##})%; ", oArgs);
                output = output + string.Format("Size: {0:#,###.##} Gb", tot) + Environment.NewLine;
            }

            return output;
        }
    }

    public static class RAM
    {
        public static string Query()
        {
            long mb = 1048576;
            long gb = 1073741824;
            string output = "RAM: ";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select TotalVisibleMemorySize, FreePhysicalMemory from Win32_OperatingSystem");

            foreach (ManagementObject service in mos.Get())
            {
                foreach (PropertyData data in service.Properties)
                {
                    if (data.Name == "FreePhysicalMemory")
                    {
                        double bytes = Convert.ToDouble(data.Value.ToString()) / mb;
                        output = output + string.Format("{0:#,###.##} Gb", bytes); // bytes.ToString("R") + " Gb";
                    }
                }
            }

            return output;
        }
    }

    public static class CPUUsage
    {
        public static string Query()
        {
            string output = "CPU: ";

            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(1000); // wait a second to get a valid reading
            float usage = cpuCounter.NextValue();
            output = output + string.Format("{0:#,###.##} %", usage);

            return output;
        }
    }
}
