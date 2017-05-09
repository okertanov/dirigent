using System;
using System.Diagnostics;

namespace Dirigent.Net.Logging {
	public abstract class Logger {
		[Conditional("LOG_TRACE")]
		public abstract void Trace(string format, params object[] args);

		[Conditional("LOG_TRACE"), Conditional("LOG_DEBUG")]
		public abstract void Debug(string format, params object[] args);

		[Conditional("LOG_TRACE"), Conditional("LOG_DEBUG"), Conditional("LOG_WARN")]
		public abstract void Warn(string format, params object[] args);

		[Conditional("LOG_TRACE"), Conditional("LOG_DEBUG"), Conditional("LOG_WARN"), Conditional("LOG_ERROR")]
		public abstract void Error(string format, params object[] args);

		public abstract void Fatal(string format, params object[] args);
	}
}