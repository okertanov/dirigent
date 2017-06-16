using System;
using System.Linq;

namespace Dirigent.Net.Sys.Helpers {
	public static class ExceptionExtensions {
		public static Exception Unwrap(this Exception e) {
			var aggregateException = e as AggregateException;
			if (aggregateException != null) {
				var firstException = aggregateException.Flatten().InnerExceptions.FirstOrDefault();
				return firstException;
			}
			return e;
		}
	}
}