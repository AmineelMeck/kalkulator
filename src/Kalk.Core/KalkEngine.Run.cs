﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Consolus;
using Scriban;
using Scriban.Functions;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Kalk.Core
{
    public partial class KalkEngine
    {
        public bool Run(params string[] args)
        {
//            Debugger.Launch();

            if (!Console.IsInputRedirected && !Console.IsOutputRedirected && ConsoleHelper.HasInteractiveConsole)
            {
                Repl = new ConsoleRepl();
                HasInteractiveConsole = true;
                
                InitializeRepl();

                try
                {
                    if (ConsoleRepl.IsSelf())
                    {
                        Console.Title = $"kalk {Version}";
                    }
                }
                catch
                {
                    // ignore
                }
            }

            Directory.CreateDirectory(KalkUserFolder);

            ShowVersion();
            WriteHighlightLine("# Type `help` for more information and at https://github.com/xoofx/kalk");

            if (Repl != null)
            {
                return RunInteractive();
            }
            else
            {
                return RunNonInteractive();
            }
        }

        private bool RunInteractive()
        {
            try
            {
                _clockReplInput.Restart();
                Repl.Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected exception {ex}");
                return false;
            }

            return true;
        }

        private bool RunNonInteractive()
        {
            bool success = true;
            string line;
            while ((line = InputReader.ReadLine()) != null)
            {
                if (EchoEnabled) OutputWriter.Write($">>> {line}");

                try
                {
                    var script = Parse(line);

                    if (script.HasErrors)
                    {
                        //throw new ScriptParserRuntimeException();
                        var errorBuilder = new StringBuilder();
                        foreach (var message in script.Messages)
                        {
                            if (errorBuilder.Length > 0) errorBuilder.AppendLine();
                            errorBuilder.Append(message.Message);
                        }

                        var error = errorBuilder.ToString();
                        throw new InvalidOperationException(error);
                    }
                    else
                    {
                        var result = EvaluatePage(script.Page);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ScriptRuntimeException runtimeEx)
                    {
                        WriteErrorLine(runtimeEx.OriginalMessage);
                    }
                    else
                    {
                        WriteErrorLine(ex.Message);
                    }
                    success = false;
                    break;
                }

                if (HasExit) break;
            }
            return success;
        }
    }
}