namespace Xtate.Core;

public interface IServiceDefinition
{
	Uri            Type       { get; }
	Uri?           Source     { get; }
	string?        RawContent { get; }
	DataModelValue Content    { get; }
	DataModelValue Parameters { get; }
}