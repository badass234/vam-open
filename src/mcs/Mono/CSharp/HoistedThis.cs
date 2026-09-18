namespace Mono.CSharp
{
	public class HoistedThis : HoistedVariable
	{
		public HoistedThis(AnonymousMethodStorey storey, Field field)
			: base(storey, field)
		{
		}
	}
}
