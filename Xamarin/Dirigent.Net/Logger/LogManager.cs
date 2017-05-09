using System;

namespace Dirigent.Net.Logging {
	public static class LogManager {
		public static Logger GetLogger(string category) {
			return new ConsoleLogger(category);
		}

		public static Logger GetLogger(Type type) {
			return GetLogger(type.Name);
		}

		public static Logger GetLogger<T>() where T : class {
			return GetLogger(typeof(T));
		}
	}
}