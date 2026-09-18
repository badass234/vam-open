using System.Collections.Generic;

namespace IKVM.Reflection
{
	public interface ICustomAttributeProvider
	{
		bool IsDefined(Type attributeType, bool inherit);

		IList<CustomAttributeData> __GetCustomAttributes(Type attributeType, bool inherit);
	}
}
