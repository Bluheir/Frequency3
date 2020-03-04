using System;
using System.Collections.Generic;
using Discord;

namespace Frequency3.Core
{
	public class ParameterType
	{
		private readonly string helpText;
		private readonly string parameterName;
		private readonly Type type;
		private static Dictionary<string, ParameterType> _instances;
		private static Dictionary<Type, ParameterType> _types;

		public string HelpText => helpText;
		public string ParameterName => parameterName;
		public Type Type => type;

		public static IReadOnlyDictionary<string, ParameterType> TypeParameters => _instances;
		public static IReadOnlyDictionary<Type, ParameterType> TypeToParameter => _types;
		public static ParameterType StringType { get; } = new ParameterType(typeof(string), "text", "A bunch of text");
		public static ParameterType IntType { get; } = new ParameterType(typeof(int), "int", $"A number that can have a minimum value of {int.MinValue} and a max value of {int.MaxValue}");
		public static ParameterType BoolType { get; } = new ParameterType(typeof(bool), "bool", "A true or false value");
		public static ParameterType CharType { get; } = new ParameterType(typeof(char), "char", "A single unicode character");
		public static ParameterType UIntType { get; } = new ParameterType(typeof(uint), "uint", $"A number that can have a minimum value of 0 and a max value of {uint.MaxValue}");
		public static ParameterType SByteType { get; } = new ParameterType(typeof(sbyte), "sbyte", "A number that can have a minimum value of -128 and a max value of 127");
		public static ParameterType ByteType { get; } = new ParameterType(typeof(byte), "byte", "A number that can have a minimum value of 0 and a max value of 255");
		public static ParameterType ShortType { get; } = new ParameterType(typeof(short), "short", "A number that can have a minimum value of -32768 and a max value of 32767");
		public static ParameterType UShortType { get; } = new ParameterType(typeof(ushort), "ushort", "A number that can have a minimum value of 0 and a max value of 65535");
		public static ParameterType LongType { get; } = new ParameterType(typeof(long), "long", $"A number that can have a minimum value of {long.MinValue} and a max value of {long.MaxValue}");
		public static ParameterType UlongType { get; } = new ParameterType(typeof(ulong), "ulong", $"A number that can have a minimum value of 0 and a max value of {ulong.MaxValue}");
		public static ParameterType DoubleType { get; } = new ParameterType(typeof(double), "double", $"A decimal number with 64-bit precision");
		public static ParameterType DecimalType { get; } = new ParameterType(typeof(decimal), "decimal", $"A decimal number with arbritrary precison with a max value of {decimal.MaxValue}");
		public static ParameterType UserType { get; } = new ParameterType(typeof(IUser), "user", "A discord user");
		public static ParameterType TextChannelType { get; } = new ParameterType(typeof(ITextChannel), "channel", "A discord text channel");
		public static ParameterType RoleType { get; } = new ParameterType(typeof(IRole), "role", "A discord role");


		private ParameterType(Type type, string parametername, string helptext)
		{
			this.type = type ?? throw new ArgumentNullException($"Parameter {nameof(type)} cannot be null");
			parameterName = parametername ?? throw new ArgumentNullException($"Parameter {nameof(parametername)} cannot be null");
			helpText = helptext ?? throw new ArgumentNullException($"Parameter {nameof(helptext)} cannot be null");

			if(_instances == null)
			{
				_instances = new Dictionary<string, ParameterType>();
				_types = new Dictionary<Type, ParameterType>();
			}

			_instances.Add(parametername, this);
			_types.Add(type, this);
		}

		public override string ToString()
		{
			return parameterName;
		}

	}
}
