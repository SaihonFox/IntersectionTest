using Avalonia;
using Avalonia.Headless;

using AvaRNTask.XUnitTest;

[assembly: AvaloniaTestApplication(typeof(XTestProgram))]

namespace AvaRNTask.XUnitTest;

public class XTestProgram
{
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>()
			.UseHeadless(new AvaloniaHeadlessPlatformOptions());
}