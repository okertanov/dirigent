using System;

namespace Dirigent.Net.Logging {
	public sealed class ConsoleLogger : Logger, ILogger {
		private string _category;
		private LogLevel _currentLogLevel = LogLevel.Fatal;

		public ConsoleLogger(string category) {
			_category = category;
			_currentLogLevel = GetLogLevel();
		}

		public override void Trace(string format, params object[] args) {
			LogImpl(LogLevel.Trace, _category, format, args);
		}

		public override void Debug(string format, params object[] args) {
			LogImpl(LogLevel.Debug, _category, format, args);
		}

		public override void Warn(string format, params object[] args) {
			LogImpl(LogLevel.Warning, _category, format, args);
		}

		public override void Error(string format, params object[] args) {
			LogErrImpl(LogLevel.Error, _category, format, args);
		}

		public override void Fatal(string format, params object[] args) {
			LogErrImpl(LogLevel.Fatal, _category, format, args);
		}

		private void LogImpl(LogLevel level, string category, string format, params object[] args) {
			var toWrite = (int)_currentLogLevel >= (int)level;
			var msg = String.Empty;

			if (toWrite) {
				var errLevelMsg = level.ToString();
				msg = String.Format("{0}: {1}: {2}",
					errLevelMsg, category,
					String.Format(format, args));
				System.Console.WriteLine(msg);
			}
		}

		private void LogErrImpl(LogLevel level, string category, string format, params object[] args) {
			var errLevelMsg = (int)level > (int)LogLevel.Error ? "Fatal" : "Error";
			var msg = String.Format("{0}: {1}: {2}",
				errLevelMsg, category,
				String.Format(format, args));

			System.Console.Error.WriteLine(msg);
		}

		private static LogLevel GetLogLevel() {
			#if LOG_FATAL
			return LogLevel.Fatal;
			#elif LOG_ERROR
			return LogLevel.Error;
			#elif LOG_WARN
			return LogLevel.Warning;
			#elif LOG_DEBUG
			return LogLevel.Debug;
			#elif LOG_TRACE
			return LogLevel.Trace;
			#else
			return LogLevel.Fatal;
			#endif
		}
	}
}