using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public interface IResourceLoader
	{
		bool                 CanHandle(Uri uri);
		ValueTask<Resource>  Request(Uri uri, CancellationToken token);
		ValueTask<XmlReader> RequestXmlReader(Uri uri, XmlReaderSettings? readerSettings = default, XmlParserContext? parserContext = default, CancellationToken token = default);
	}
}