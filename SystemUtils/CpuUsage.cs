using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SystemUtils.CPU
{
    public class CpuUsage
    {
        /// <summary>
        /// Initializes a new CpuUsageNt instance.
        /// </summary>
        /// <exception cref="NotSupportedException">One of the system calls fails.</exception>
        public CpuUsage()
        {
            byte[] timeInfo = new byte[32];		// SYSTEM_TIME_INFORMATION structure
            byte[] perfInfo = new byte[312];	// SYSTEM_PERFORMANCE_INFORMATION structure
            byte[] baseInfo = new byte[44];		// SYSTEM_BASIC_INFORMATION structure
            int ret;
            // get new system time
            ret = NtQuerySystemInformation(SYSTEM_TIMEINFORMATION, timeInfo, timeInfo.Length, IntPtr.Zero);
            if (ret != NO_ERROR)
                throw new NotSupportedException();
            // get new CPU's idle time
            ret = NtQuerySystemInformation(SYSTEM_PERFORMANCEINFORMATION, perfInfo, perfInfo.Length, IntPtr.Zero);
            if (ret != NO_ERROR)
                throw new NotSupportedException();
            // get number of processors in the system
            ret = NtQuerySystemInformation(SYSTEM_BASICINFORMATION, baseInfo, baseInfo.Length, IntPtr.Zero);
            if (ret != NO_ERROR)
                throw new NotSupportedException();
            // store new CPU's idle and system time and number of processors
            oldIdleTime = BitConverter.ToInt64(perfInfo, 0); // SYSTEM_PERFORMANCE_INFORMATION.liIdleTime
            oldSystemTime = BitConverter.ToInt64(timeInfo, 8); // SYSTEM_TIME_INFORMATION.liKeSystemTime
            processorCount = baseInfo[40];
        }

        /// <summary>
        /// Determines the current average CPU load.
        /// </summary>
        /// <returns>An integer that holds the CPU load percentage.</returns>
        /// <exception cref="NotSupportedException">One of the system calls fails. The CPU time can not be obtained.</exception>
        public int Query()
        {
            byte[] timeInfo = new byte[32];		// SYSTEM_TIME_INFORMATION structure
            byte[] perfInfo = new byte[312];	// SYSTEM_PERFORMANCE_INFORMATION structure
            double dbIdleTime, dbSystemTime;
            int ret;
            // get new system time
            ret = NtQuerySystemInformation(SYSTEM_TIMEINFORMATION, timeInfo, timeInfo.Length, IntPtr.Zero);
            if (ret != NO_ERROR)
                throw new NotSupportedException();
            // get new CPU's idle time
            ret = NtQuerySystemInformation(SYSTEM_PERFORMANCEINFORMATION, perfInfo, perfInfo.Length, IntPtr.Zero);
            if (ret != NO_ERROR)
                throw new NotSupportedException();
            // CurrentValue = NewValue - OldValue
            dbIdleTime = BitConverter.ToInt64(perfInfo, 0) - oldIdleTime;
            dbSystemTime = BitConverter.ToInt64(timeInfo, 8) - oldSystemTime;
            // CurrentCpuIdle = IdleTime / SystemTime
            if (dbSystemTime != 0)
                dbIdleTime = dbIdleTime / dbSystemTime;
            // CurrentCpuUsage% = 100 - (CurrentCpuIdle * 100) / NumberOfProcessors
            dbIdleTime = 100.0 - dbIdleTime * 100.0 / processorCount + 0.5;
            // store new CPU's idle and system time
            oldIdleTime = BitConverter.ToInt64(perfInfo, 0); // SYSTEM_PERFORMANCE_INFORMATION.liIdleTime
            oldSystemTime = BitConverter.ToInt64(timeInfo, 8); // SYSTEM_TIME_INFORMATION.liKeSystemTime
            return (int)dbIdleTime;
        }

        /// <summary>
        /// NtQuerySystemInformation is an internal Windows function that retrieves various kinds of system information.
        /// </summary>
        /// <param name="dwInfoType">One of the values enumerated in SYSTEM_INFORMATION_CLASS, indicating the kind of system information to be retrieved.</param>
        /// <param name="lpStructure">Points to a buffer where the requested information is to be returned. The size and structure of this information varies depending on the value of the SystemInformationClass parameter.</param>
        /// <param name="dwSize">Length of the buffer pointed to by the SystemInformation parameter.</param>
        /// <param name="returnLength">Optional pointer to a location where the function writes the actual size of the information requested.</param>
        /// <returns>Returns a success NTSTATUS if successful, and an NTSTATUS error code otherwise.</returns>
        [DllImport("ntdll", EntryPoint = "NtQuerySystemInformation")]
        private static extern int NtQuerySystemInformation(int dwInfoType, byte[] lpStructure, int dwSize, IntPtr returnLength);
        /// <summary>Returns the number of processors in the system in a SYSTEM_BASIC_INFORMATION structure.</summary>
        private const int SYSTEM_BASICINFORMATION = 0;
        /// <summary>Returns an opaque SYSTEM_PERFORMANCE_INFORMATION structure.</summary>
        private const int SYSTEM_PERFORMANCEINFORMATION = 2;
        /// <summary>Returns an opaque SYSTEM_TIMEOFDAY_INFORMATION structure.</summary>
        private const int SYSTEM_TIMEINFORMATION = 3;
        /// <summary>The value returned by NtQuerySystemInformation is no error occurred.</summary>
        private const int NO_ERROR = 0;
        /// <summary>Holds the old idle time.</summary>
        private long oldIdleTime;
        /// <summary>Holds the old system time.</summary>
        private long oldSystemTime;
        /// <summary>Holds the number of processors in the system.</summary>
        private double processorCount;
    }
}
