using System;
using System.Collections.Generic;
using System.Text;

namespace Frequency2.Types.Attributes
{
	[System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public sealed class IgnoreAttribute : Attribute
	{
		public IgnoreAttribute()
		{}
	}
}
