using Nucleus.Diagnostics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Nucleus {
    public static class ConsoleU {
        public static OutputLevel currentLevel = OutputLevel.High;
        private static readonly object locker = new object();

        public static bool WaitingForInput { get; private set; }

        public static int LeftOffset { get; set; }
        public static string Prefix { get; set; }

        public static Func<string, string> Filter;
        public static Action BeforeWriteLine;

        public static Point WritingPosition { get; private set; }

        public static bool WritingToFile { get; private set; }
        public static string WritingToFilePath { get; private set; }
        public static string CurrentLine { get; private set; }
        public static List<string> WritingToLines { get; private set; }

        private static string ProcessLine(string line) {
            int maxSize = Console.WindowWidth;

            if (line.Length >= maxSize) {
                line = line.Remove(maxSize, line.Length - maxSize);//cut start
                //line = line.Remove(0, line.Length - maxSize);// cut end
            }

            if (Filter != null) {
                line = Filter(line);
            }

            return line;
        }

        private static void SetOffset() {
            if (Console.CursorLeft == 0) {
                Console.SetCursorPosition(LeftOffset, Console.CursorTop);

                if (WritingToFile) {
                    string spaces = StringUtil.RepeatCharacter(' ', LeftOffset);
                    //WritingToFileWriter.Write(spaces);
                }
            }
        }

        private static void InvokeBeforeWriteLine() {
            if (BeforeWriteLine != null) {
                BeforeWriteLine();
            }
        }

        private static void WriteToLine(string line) {
            if (WritingToFile) {
                if (WritingPosition == null) {
                    CurrentLine = CurrentLine + line;
                } else {
                    int finalSize = WritingPosition.X + line.Length;
                    int dif = finalSize - CurrentLine.Length;
                    if (dif > 0) {
                        string spaces = StringUtil.RepeatCharacter(' ', dif);
                        CurrentLine += spaces;
                    }

                    char[] chars = CurrentLine.ToCharArray();
                    for (int i = 0; i < line.Length; i++) {
                        chars[i + WritingPosition.X] = line[i];
                    }
                    CurrentLine = new string(chars);
                }
            }

            WritingPosition = new Point(WritingPosition.X + line.Length, WritingPosition.Y);
        }

        public static void Write(string line) {
            Console.Write(line);
            WriteToLine(line);
        }

        private static void ConsoleWriteLine(string line) {
            Write(line);
            ConsoleWriteLine();
        }

        private static void ConsoleWriteLine() {
            InvokeBeforeWriteLine();
            Console.WriteLine();

            if (WritingToFile) {
                WritingToLines.Add(CurrentLine);
                CurrentLine = "";
                WritingPosition = new Point(0, WritingPosition.Y + 1);
            }
        }

        public static void SetCursorPosition(int x, int y) {
            Console.SetCursorPosition(x, y);

            if (WritingToFile) {
                WritingPosition = new Point(x, y);
            }
        }

        public static void BeginWriteToFile(string path, bool delete = true) {
            if (delete && File.Exists(path)) {
                File.Delete(path);
            }

            CurrentLine = "";
            WritingToFile = true;
            WritingToFilePath = path;
            WritingToLines = new List<string>();
        }

        public static void EndWriteToFile() {
            // write all lines
            File.WriteAllLines(WritingToFilePath, WritingToLines.ToArray());
            WritingToFilePath = null;
            WritingToFile = false;
        }

        public static string ReadLine() {
            WaitingForInput = true;
            Console.Write("> ");
            string line = Console.ReadLine();
            WaitingForInput = false;
            return line;
        }

        public static void WriteLine() {
            ConsoleWriteLine();
        }

        public static void WriteLine(string line) {
            lock (locker) {
                line = ProcessLine(line);
                SetOffset();
                ConsoleWriteLine(line);
            }
        }

        public static void WriteLine(string line, ConsoleColor foreGroundColor) {
            lock (locker) {
                line = ProcessLine(line);
                ConsoleColor current = Console.ForegroundColor;
                Console.ForegroundColor = foreGroundColor;

                SetOffset();
                Write(line);

                Console.ForegroundColor = current;

                ConsoleWriteLine();
            }
        }

        public static void WriteLine(string line, ConsoleColor foreGroundColor, int leftOffset) {
            lock (locker) {
                line = ProcessLine(line);
                ConsoleColor current = Console.ForegroundColor;
                Console.ForegroundColor = foreGroundColor;
                Console.SetCursorPosition(leftOffset, Console.CursorTop);
                Write(line);

                Console.ForegroundColor = current;

                ConsoleWriteLine();
            }
        }

        public static void Write(string line, ConsoleColor foreGroundColor) {
            lock (locker) {
                line = ProcessLine(line);
                ConsoleColor current = Console.ForegroundColor;
                Console.ForegroundColor = foreGroundColor;
                SetOffset();
                Write(line);
                Console.ForegroundColor = current;
            }
        }

        public static void Write(string line, ConsoleColor foreGroundColor, int leftOffset) {
            lock (locker) {
                line = ProcessLine(line);
                ConsoleColor current = Console.ForegroundColor;
                Console.ForegroundColor = foreGroundColor;
                Console.SetCursorPosition(leftOffset, Console.CursorTop);
                Write(line);
                Console.ForegroundColor = current;
            }
        }

        public static void WriteLine(string line, ConsoleColor foreGroundColor, OutputLevel logLevel) {
            if (logLevel < currentLevel) {
                return;
            }

            lock (locker) {
                line = ProcessLine(line);
                ConsoleColor current = Console.ForegroundColor;
                Console.ForegroundColor = foreGroundColor;
                SetOffset();
                ConsoleWriteLine(line);
                Console.ForegroundColor = current;
            }
        }


        public static void WriteLine(string line, OutputLevel logLevel) {
            if (logLevel < currentLevel) {
                return;
            }

            lock (locker) {
                line = ProcessLine(line);
                SetOffset();
                ConsoleWriteLine(line);
            }
        }

        public static ConsoleKeyInfo ReadKey() {
            return Console.ReadKey();
        }

    }
}
