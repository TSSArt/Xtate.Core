namespace Xtate.Builder
{
	public interface ICustomActionBuilder
	{
		ICustomAction Build();

		void SetXml(string xml);
	}
}