using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Memory;

namespace EZRAGEMP
{
    public class Program
    {
        public Mem MemLib = new Mem();

        static void Main(string[] args)
        {
            var program = new Program();
            program.openGame();
            Console.ReadKey();
        }
        private void openGame()
        {
            Console.WriteLine("RageMP teleporter by Oniel special for EZcheats.RU!");
            Process TargetProcess = Process.GetProcessesByName("GTA5")[0];
            SigScanSharp Sigscan = new SigScanSharp(TargetProcess.Handle);
            Sigscan.SelectModule(TargetProcess.MainModule);
           

            if (MemLib.OpenProcess("GTA5"))
            {

                var dwBlip = Sigscan.FindPattern("4C 8D 05 ? ? ? ? 0F B7 C1");
                var dwWorld = Sigscan.FindPattern("48 8B 05 ? ? ? ? 48 8B 58 08 48 85 DB 74 32");

                dwWorld = dwWorld + (ulong)(MemLib.ReadInt(sHEX(dwWorld + (ulong)3)) + 7);
                dwBlip = dwBlip + (ulong)(MemLib.ReadInt(sHEX(dwBlip + (ulong)3)) + 7);

                float fPlayerX = MemLib.ReadFloat(sHEX(dwWorld) + ",0x8,0x30,0x50");
                float fPlayerY = MemLib.ReadFloat(sHEX(dwWorld) + ",0x8,0x30,0x54");
                float fPlayerZ = MemLib.ReadFloat(sHEX(dwWorld) + ",0x8,0x30,0x58");

                float fVehicleX = MemLib.ReadFloat(sHEX(dwWorld) + ",0x8,0xD28,0x30,0x50");
                float fVehicleY = MemLib.ReadFloat(sHEX(dwWorld) + ",0x8,0xD28,0x30,0x54");
                float fVehicleZ = MemLib.ReadFloat(sHEX(dwWorld) + ",0x8,0xD28,0x30,0x58");

                //Console.WriteLine(fVehicleZ);

                float fBlipX = fPlayerX; 
                float fBlipY = fPlayerY;

                while (true)
                {
                    byte[] f3 = BitConverter.GetBytes(Proc.GetAsyncKeyState(0x72));
                    byte[] f4 = BitConverter.GetBytes(Proc.GetAsyncKeyState(0x73));
                    if (f3[0] == 1)
                    {
                        for (int i = 2000; i != 1; i--)
                        {
                            var n = MemLib.ReadLong(string.Format("0x{0:X}", dwBlip + (ulong)(i * 8)));
                            if (n > 0 && MemLib.ReadInt(string.Format("0x{0:X}", n + 0x40)) == 8 && MemLib.ReadInt(string.Format("0x{0:X}", n + 0x48)) == 84)
                            {
                                fBlipX = MemLib.ReadFloat(string.Format("0x{0:X}", n + 0x10));
                                fBlipY = MemLib.ReadFloat(string.Format("0x{0:X}", n + 0x14));
                                Console.WriteLine("Teleported to " + fBlipX + ";" + fBlipY);
                                
                                var t1 = MemLib.ReadLong(string.Format("0x{0:X}", dwWorld)) + 8;
                                var pointer = MemLib.ReadLong(string.Format("0x{0:X}", t1));

                                if(MemLib.ReadInt(string.Format("0x{0:X}", pointer + 0x1468)) == 2)
                                {
                                    pointer = MemLib.ReadLong(string.Format("0x{0:X}", pointer + 0xD28)); //readPointer(p + 0xD28)
                                }

                                MemLib.WriteMemory(sHEX(dwWorld) + ",0x8,0x30,0x50", "float", fBlipX.ToString());
                                MemLib.WriteMemory(sHEX(dwWorld) + ",0x8,0x30,0x54", "float", fBlipY.ToString());
                                MemLib.WriteMemory(sHEX(dwWorld) + ",0x8,0x30,0x58", "float", "-250");

                                MemLib.WriteMemory(string.Format("0x{0:X}", pointer) + "+0x90", "float", fBlipX.ToString());
                                MemLib.WriteMemory(string.Format("0x{0:X}", pointer) + "+0x94", "float", fBlipY.ToString());
                                MemLib.WriteMemory(string.Format("0x{0:X}", pointer) + "+0x98", "float", "-250");
                            }
                        }
                        Console.Beep();
                    }
                    if (f4[0] == 1)
                    {
                        for (int i = 2000; i != 1; i--)
                        {
                            var n = MemLib.ReadLong(string.Format("0x{0:X}", dwBlip + (ulong)(i * 8)));
                            if (n > 0 && MemLib.ReadInt(string.Format("0x{0:X}", n + 0x40)) == 8 && MemLib.ReadInt(string.Format("0x{0:X}", n + 0x48)) == 84)
                            {
                                // get blip position
                                fBlipX = MemLib.ReadFloat(string.Format("0x{0:X}", n + 0x10));
                                fBlipY = MemLib.ReadFloat(string.Format("0x{0:X}", n + 0x14));
                                Console.WriteLine("Teleported to " + fBlipX + ";" + fBlipY);

                                // set vehicle position
                                MemLib.WriteMemory(sHEX(dwWorld) + ",0x8,0xD28,0x30,0x50", "float", fBlipX.ToString());
                                MemLib.WriteMemory(sHEX(dwWorld) + ",0x8,0xD28,0x30,0x54", "float", fBlipY.ToString());
                                MemLib.WriteMemory(sHEX(dwWorld) + ",0x8,0xD28,0x30,0x58", "float", "-250");
                            }
                        }
                        Console.Beep();
                    }
                    Thread.Sleep(1);
                }
            }
            else
            {
                Console.WriteLine("GTA5.exe not found!");
            }

        }
        public string sHEX(ulong dwAddr)
        {
            return string.Format("0x{0:X}", dwAddr);
        }
    }

}

public class SigScanSharp
{
    private IntPtr g_hProcess { get; set; }
    private byte[] g_arrModuleBuffer { get; set; }
    private ulong g_lpModuleBase { get; set; }

    private Dictionary<string, string> g_dictStringPatterns { get; }

    public SigScanSharp(IntPtr hProc)
    {
        g_hProcess = hProc;
        g_dictStringPatterns = new Dictionary<string, string>();
    }

    public bool SelectModule(ProcessModule targetModule)
    {
        g_lpModuleBase = (ulong)targetModule.BaseAddress;
        g_arrModuleBuffer = new byte[targetModule.ModuleMemorySize];

        g_dictStringPatterns.Clear();

        return Win32.ReadProcessMemory(g_hProcess, g_lpModuleBase, g_arrModuleBuffer, targetModule.ModuleMemorySize);
    }

    public void AddPattern(string szPatternName, string szPattern)
    {
        g_dictStringPatterns.Add(szPatternName, szPattern);
    }

    private bool PatternCheck(int nOffset, byte[] arrPattern)
    {
        for (int i = 0; i < arrPattern.Length; i++)
        {
            if (arrPattern[i] == 0x0)
                continue;

            if (arrPattern[i] != this.g_arrModuleBuffer[nOffset + i])
                return false;
        }

        return true;
    }

    public ulong FindPattern(string szPattern)
    {
        if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
            throw new Exception("Module is null");

        Stopwatch stopwatch = Stopwatch.StartNew();

        byte[] arrPattern = ParsePatternString(szPattern);

        for (int nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
        {
            if (this.g_arrModuleBuffer[nModuleIndex] != arrPattern[0])
                continue;

            if (PatternCheck(nModuleIndex, arrPattern))
            {
                return g_lpModuleBase + (ulong)nModuleIndex;
            }
        }


        return 0;
    }
    public Dictionary<string, ulong> FindPatterns()
    {
        if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
            throw new Exception("<odule is null");

        Stopwatch stopwatch = Stopwatch.StartNew();

        byte[][] arrBytePatterns = new byte[g_dictStringPatterns.Count][];
        ulong[] arrResult = new ulong[g_dictStringPatterns.Count];

        // PARSE PATTERNS
        for (int nIndex = 0; nIndex < g_dictStringPatterns.Count; nIndex++)
            arrBytePatterns[nIndex] = ParsePatternString(g_dictStringPatterns.ElementAt(nIndex).Value);

        // SCAN FOR PATTERNS
        for (int nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
        {
            for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
            {
                if (arrResult[nPatternIndex] != 0)
                    continue;

                if (this.PatternCheck(nModuleIndex, arrBytePatterns[nPatternIndex]))
                    arrResult[nPatternIndex] = g_lpModuleBase + (ulong)nModuleIndex;
            }
        }

        Dictionary<string, ulong> dictResultFormatted = new Dictionary<string, ulong>();

        // FORMAT PATTERNS
        for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
            dictResultFormatted[g_dictStringPatterns.ElementAt(nPatternIndex).Key] = arrResult[nPatternIndex];

        return dictResultFormatted;
    }

    private byte[] ParsePatternString(string szPattern)
    {
        List<byte> patternbytes = new List<byte>();

        foreach (var szByte in szPattern.Split(' '))
            patternbytes.Add(szByte == "?" ? (byte)0x0 : Convert.ToByte(szByte, 16));

        return patternbytes.ToArray();
    }

    private static class Win32
    {

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, int dwSize, int lpNumberOfBytesRead = 0);
    }
}
internal static class Proc
{
    internal delegate int ThreadProc(IntPtr param);

    [DllImport("Kernel32.dll")]
    internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UInt32 nSize, ref UInt32 lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, ref UInt32 lpNumberOfBytesWritten);

    [DllImport("User32.dll")]
    public static extern short GetAsyncKeyState(int ArrowKeys);
}