using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	internal interface IPersistenceContext
	{
		int       GetState(int key);
		int       GetState(int key, int subKey);
		void      SetState(int key, int value);
		void      SetState(int key, int subKey, int value);
		void      ClearState(int key);
		ValueTask CheckPoint(int level, CancellationToken token);
		ValueTask Shrink(CancellationToken token);
	}
}