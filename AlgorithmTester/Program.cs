using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace AlgorithmTester
{
    class Program
    {
        static readonly string EXE_PATH = @"--Algorithm EXE Path--";
        static readonly string TXT_PATH = @"--Input Path--";
        static readonly int WAIT = 5000;

        static DateTime StartTime;

        static void Main(string[] args)
        {
            Process exeProc = new Process();

            try
            {
                Console.WriteLine("EXE를 실행합니다...");

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = false;

                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                startInfo.FileName = EXE_PATH;

                exeProc.StartInfo = startInfo;
                exeProc.Start();

                Console.WriteLine("EXE를 성공적으로 실행하였습니다!\n");

                Console.WriteLine("TXT 테스트 케이스를 표준 입력에 쓰는 중입니다...");
                using (StreamReader sr = File.OpenText(TXT_PATH))
                {
                    StartTime = DateTime.Now;

                    string lineToWrite;
                    while ((lineToWrite = sr.ReadLine()) != null)
                        exeProc.StandardInput.WriteLine(lineToWrite);

                    exeProc.StandardInput.WriteLine();
                    exeProc.StandardInput.Flush();
                }
                Console.WriteLine("TXT 테스트 케이스 쓰기를 완료하였습니다!\n");

                Console.WriteLine($"이제부터 프로세스 실행이 끝날 때까지 기다립니다... (최대 {WAIT}ms)\n");
                bool timeOut = !exeProc.WaitForExit(WAIT);

                if (timeOut)
                {
                    exeProc.Kill();
                    Console.WriteLine($"프로세스가 타임아웃 되었습니다...");
                }
                else
                {
                    Console.WriteLine($"프로세스가 {(DateTime.Now - StartTime).TotalSeconds}초 동안 실행되었습니다! (Exit Code : {exeProc.ExitCode})");
                }

                using (Stream cs = Console.OpenStandardOutput())
                {
                    Console.WriteLine("[ Output Start ]");
                    exeProc.StandardOutput.BaseStream.CopyTo(cs);
                    Console.WriteLine("[ Output End ]\n");

                    Console.WriteLine("[ Error Start ]");
                    exeProc.StandardError.BaseStream.CopyTo(cs);
                    Console.WriteLine("[ Error End ]");
                }

                Console.WriteLine("테스트가 종료되었습니다. 아무키나 누르시면 종료합니다...");
                Console.ReadKey(true);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("테스트 도중 예외가 발생하였습니다...");
                Console.Error.WriteLine($"예외 정보 : {e.Message}\n{e.StackTrace}");

                Console.WriteLine("아무키나 누르시면 종료합니다...");
                Console.ReadKey(true);
            }
            finally
            {
                exeProc?.Close();
            }
        }
    }
}
