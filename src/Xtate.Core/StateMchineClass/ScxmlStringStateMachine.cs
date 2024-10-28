using System.IO;

namespace Xtate.Core;

public class ScxmlStringStateMachine(string scxml) : ScxmlStateMachine
{
	protected override TextReader CreateTextReader() => new StringReader(scxml);
}