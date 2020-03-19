using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCase_Finder
{
    class Program
    {
        private static readonly string GENERATOR_PY = @"--Path of Input Generator Which is Written in Python--";
        private static readonly string ANSWER_EXE = @"--Path of EXE Which is Right for the Problem--";
        private static readonly string TEST_EXE = @"--Path of EXE to Find a Test-Case Which Gives WA--";
        static readonly int WAIT = 5000;

        enum ProcessResult { RTE, TLE, PASS }

        static void Main(string[] args)
        {
            ProcessStartInfo answerStart = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                FileName = ANSWER_EXE
            }, testStart = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                FileName = TEST_EXE
            };

            try
            {
                for (int i = 1; ; ++i)
                {
                    Console.Write($"Case {i} : ");

                    ProcessResult genResult = GenerateInput(out string input);
                    if (genResult != ProcessResult.PASS)
                        throw new Exception($"테스트케이스를 생성하는데 실패하였습니다... ({genResult.ToString()})");

                    ProcessResult answerResult = GetOutput(answerStart, input, out string answerOutput);
                    ProcessResult testResult = GetOutput(testStart, input, out string testOutput);
                    bool AC = IsSame(answerOutput, testOutput);

                    Console.Write($"Right Source = {answerResult.ToString()}, Test Source = {testResult.ToString()} <{(AC ? "AC" : "WA")}>\n");

                    if (answerResult != ProcessResult.PASS || testResult != ProcessResult.PASS || !AC)
                    {
                        Console.WriteLine("[Input]");
                        Console.WriteLine(input);

                        Console.WriteLine("\n[Right Source]");
                        Console.WriteLine(answerOutput);

                        Console.WriteLine("\n[Test Source]");
                        Console.WriteLine(testOutput);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("예외가 발생하였습니다.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("아무키나 누르시면 종료합니다...");
                Console.ReadKey(true);
            }
        }

        static ProcessResult GenerateInput(out string result)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                FileName = "python.exe",
                Arguments = "\"" + GENERATOR_PY + "\""
            };

            return GetOutput(startInfo, null, out result);
        }

        static ProcessResult GetOutput(ProcessStartInfo exeInfo, string input, out string output)
        {
            try
            {
                using (Process exe = new Process())
                {
                    exe.StartInfo = exeInfo;
                    exe.Start();

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        using (StringReader sr = new StringReader(input))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                                exe.StandardInput.WriteLine(line);

                            exe.StandardInput.WriteLine();
                            exe.StandardInput.Flush();
                        }
                    }

                    if (!exe.WaitForExit(WAIT))
                    {
                        exe.Kill();
                        output = null;
                        return ProcessResult.TLE;
                    }

                    output = exe.StandardOutput.ReadToEnd();
                    return ProcessResult.PASS;
                }
            }
            catch (Exception)
            {
                output = null;
                return ProcessResult.RTE;
            }
        }

        static bool IsSame(string o1, string o2)
        {
            using (StringReader sr1 = new StringReader(o1))
            using (StringReader sr2 = new StringReader(o2))
            {
                string line1, line2;
                while (true)
                {
                    line1 = sr1.ReadLine().Trim();
                    line2 = sr2.ReadLine().Trim();

                    bool isEnd1 = string.IsNullOrWhiteSpace(line1), isEnd2 = string.IsNullOrWhiteSpace(line2);

                    if (isEnd1 && isEnd2)
                        return true;

                    else if (isEnd1 ^ isEnd2)
                        return false;

                    else
                    {
                        int commonSize = Math.Min(line1.Length, line2.Length);

                        for (int i = 0; i < commonSize; ++i)
                            if (line1[i] != line2[i])
                                return false;

                        if ((commonSize < line1.Length && !string.IsNullOrWhiteSpace(line1.Substring(commonSize))) ||
                            (commonSize < line2.Length && !string.IsNullOrWhiteSpace(line2.Substring(commonSize))))
                            return false;
                    }
                }
            }
        }
    }
}
