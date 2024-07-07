using System.Diagnostics;

namespace Xtate.Core;

public class FileLogWriter : TraceLogWriter
{
	private class FileListener(string fileName) : TextWriterTraceListener(fileName)
	{
		public string FileName { get; } = fileName;
	}

	public FileLogWriter(string file, Type source) : base(source, SourceLevels.All)
	{
		var listenerCollection = Trace.Listeners;

		if (listenerCollection.OfType<FileListener>().All(listener => listener.FileName != file))
		{
			listenerCollection.Add(new FileListener(file));
		}
	}
}