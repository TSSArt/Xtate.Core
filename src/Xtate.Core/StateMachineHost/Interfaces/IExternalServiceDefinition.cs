namespace Xtate.Core;

public interface IExternalServiceDefinition
{
	Uri            Type       { get; }
	Uri?           Source     { get; }
	string?        RawContent { get; }
	DataModelValue Content    { get; }
	DataModelValue Parameters { get; }
}