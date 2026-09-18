namespace Mono.CSharp
{
	internal interface IDynamicBinder
	{
		Expression CreateCallSiteBinder(ResolveContext ec, Arguments args);
	}
}
