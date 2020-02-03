using Godot;
using System.Linq;
using System.Collections.Generic;

using Queue = System.Collections.Queue;

public class Console : RichTextLabel {
    public class Logger {
        private Console console;
        private string name;

        public LogLevel LogLevelFilter {
            get => console.LogLevelFilter;
            set => console.LogLevelFilter = value;
        }

        public Logger(Console console, string name) {
            this.console = console;
            this.name = name;
        }

        public void Debug(params object[] args) {
            console._Debug(name, args);
        }

        public void Info(params object[] args) {
            console._Info(name, args);
        }

        public void Error(params object[] args) {
            console._Error(name, args);
        }

        public void Warn(params object[] args) {
            console._Warn(name, args);
        }
    }

    public const int MAX_LINES = 20;

    public enum LogLevel {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3
    };

    private Queue logLines = new Queue();
    private Dictionary<string, Logger> loggers = new Dictionary<string, Logger>();
    private LogLevel logFilter = LogLevel.Debug;
    public LogLevel LogLevelFilter {
        get => logFilter;
        set => logFilter = value;
    }

    public Logger GetLogger(string name) {
        if (loggers.ContainsKey(name)) {
            return loggers[name];
        } else {
            var logger = new Logger(this, name);
            loggers[name] = logger;
            return logger;
        }
    }

    public override void _Ready() {
        BbcodeText = "";
    }

    private void _Debug(string logger, params object[] args) {
        _Log(LogLevel.Debug, logger, args);
    }

    private void _Info(string logger, params object[] args) {
        _Log(LogLevel.Info, logger, args);
    }

    private void _Error(string logger, params object[] args) {
        _Log(LogLevel.Error, logger, args);
    }

    private void _Warn(string logger, params object[] args) {
        _Log(LogLevel.Warn, logger, args);
    }

    private void _Log(LogLevel level, string logger, params object[] args) {
        if (level < logFilter) {
            return;
        }

        if (logLines.Count == MAX_LINES) {
            logLines.Dequeue();
        }

        logLines.Enqueue(_Format(level, logger, args, true));
        GD.Print(_Format(level, logger, args, false));

        _ShowLines();
    }

    private void _ShowLines() {
        BbcodeText = (string)logLines.ToArray().Aggregate((all, line) => all + "\n" + (string)line);
    }

    private string _Format(LogLevel level, string logger, object[] args, bool withBbCode) {
        string output = "";
        string loggerStr = "|" + logger + "|";
        if (withBbCode) {
            loggerStr = "[color=grey]" + loggerStr + "[/color]";
        }

        output += _FormatLogLevel(level, withBbCode) + " ";
        output += loggerStr + " ";
        output += args.Select(x => x.ToString()).Aggregate((x, e) => x += " " + e + " ");

        if (withBbCode) {
            return "[code]" + output + "[/code]";
        } else {
            return output;
        }
    }

    private string _FormatLogLevel(LogLevel level, bool withBbCode) {
        var logStr = "[" + level.ToString().ToUpper() + "]";
        if (withBbCode) {
            return "[color=" + _FormatLogLevelColor(level) + "]" + logStr + "[/color]";
        } else {
            return logStr;
        }
    }

    private string _FormatLogLevelColor(LogLevel level) {
        if (level == LogLevel.Error) {
            return "red";
        } else if (level == LogLevel.Warn) {
            return "yellow";
        } else if (level == LogLevel.Info) {
            return "blue";
        } else if (level == LogLevel.Debug) {
            return "lime";
        }

        return "white";
    }
}
