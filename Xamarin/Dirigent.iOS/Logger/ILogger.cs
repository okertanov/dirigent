using System;

namespace Dirigent.iOS.Logging {
	public enum LogLevel {
		Fatal = 0,
		Error = 1,
		Warning = 2,
		Debug = 3,
		Trace = 4
	}

	public interface ILogger {
		void Trace(string format, params object[] args);
		void Debug(string format, params object[] args);
		void Warn(string format, params object[] args);
		void Error(string format, params object[] args);
		void Fatal(string format, params object[] args);
	}
}