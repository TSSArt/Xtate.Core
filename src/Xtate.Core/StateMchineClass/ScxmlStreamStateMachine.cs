using System.IO;

namespace Xtate.Core;

public class ScxmlStreamStateMachine(Stream stream) : ScxmlStateMachine
{
	protected override TextReader CreateTextReader() => new StreamReader(stream);
}