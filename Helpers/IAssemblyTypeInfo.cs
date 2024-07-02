namespace Xtate.Core;

public interface IAssemblyTypeInfo
{
	public string FullTypeName    { get; }
	public string AssemblyName    { get; }
	public string AssemblyVersion { get; }
}