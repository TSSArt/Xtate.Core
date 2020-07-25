namespace Xtate
{
	public interface IErrorProcessor
	{
		bool LineInfoRequired { get; }
		void AddError(ErrorItem errorItem);
		void ThrowIfErrors();
	}
}