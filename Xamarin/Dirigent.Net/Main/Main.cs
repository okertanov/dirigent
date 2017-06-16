using System;
using System.Threading;
using Dirigent.Net.UI.Components;
using UIKit;

namespace Dirigent.Net.Main {
	public class Application {
		static void Main(string[] args) {
			try {
				InitializeDomainExceptionsHandler();

				UIApplication.Main(args, null, "AppDelegate");
			}
			catch(Exception e) {
				HandleCriticalException(e);
			}
		}

		//
		// InitializeDomainExceptionsHandler() - Unhandled Exceptions event handler.
		//
		// See http://msdn.microsoft.com/en-us/library/system.appdomain.unhandledexception.aspx
		//
		private static void InitializeDomainExceptionsHandler() {
			// Pre-create one "unknown" exception.
			var exception = new Exception(@"Unhandled unknown exception");

			AppDomain.CurrentDomain.UnhandledException += delegate (object sender, System.UnhandledExceptionEventArgs eventArgs) {
				try {
					// Unwrap exception from the exception object.
					exception = (Exception)eventArgs.ExceptionObject;
				}
				catch (Exception e) {
					// Bad luck: an exception in exception handling code. So, just propagate it.
					exception = e;
				}
				finally {
					HandleCriticalException(exception);
				}
			};
		}

		//
		// HandleCriticalException() - Critical exception reporting.
		//
		private static void HandleCriticalException(Exception e) {
			const string errorTitle = "Unhandled exception occured.\nApplicatin will be terminated.";
			const string errorFormat = "\n\n{0}\nDetails:\n{1}\n\n";

			// Format string message for output.
			var exceptionString = String.Format(errorFormat, errorTitle, e.ToString());

			Console.WriteLine(exceptionString);

			var alert = new AlertView(AlertViewType.Close, errorTitle, e?.Message ?? "Unknown error.");
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
			alert.ShowModal(cts);

			Console.WriteLine("Aborting...");
			Thread.CurrentThread.Abort();
		}
	}
}