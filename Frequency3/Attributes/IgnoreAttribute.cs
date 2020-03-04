using System;

namespace Frequency3.Attributes
{
	[System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class IgnoreAttribute : Attribute
	{
		public IgnoreAttribute() { }
	}
}
