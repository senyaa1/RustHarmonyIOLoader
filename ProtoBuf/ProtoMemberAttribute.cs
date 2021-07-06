﻿using System;
using System.Reflection;

namespace ProtoBuf
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ProtoMemberAttribute : Attribute, IComparable, IComparable<ProtoMemberAttribute>
	{
		internal MemberInfo Member;
		internal bool TagIsPinned;

		private string name;
		private DataFormat dataFormat;
		private int tag;
		private MemberSerializationOptions options;

		public int CompareTo(object other)
		{
			return CompareTo(other as ProtoMemberAttribute);
		}
		public int CompareTo(ProtoMemberAttribute other)
		{
			if (other == null)
			{
				return -1;
			}
			if (this == other)
			{
				return 0;
			}
			int num = tag.CompareTo(other.tag);
			if (num == 0)
			{
				num = string.CompareOrdinal(name, other.name);
			}
			return num;
		}
		public ProtoMemberAttribute(int tag) : this(tag, false) { }
		internal ProtoMemberAttribute(int tag, bool forced)
		{
			if (tag <= 0 && !forced)
			{
				throw new ArgumentOutOfRangeException("tag");
			}
			this.tag = tag;
		}
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public DataFormat DataFormat
		{
			get
			{
				return dataFormat;
			}
			set
			{
				dataFormat = value;
			}
		}

		public int Tag
		{
			get
			{
				return this.tag;
			}
		}

		internal void Rebase(int tag)
		{
			this.tag = tag;
		}

		public bool IsRequired
		{
			get
			{
				return (options & MemberSerializationOptions.Required) == MemberSerializationOptions.Required;
			}
			set
			{
				if (value)
				{
					options |= MemberSerializationOptions.Required;
					return;
				}
				options &= ~MemberSerializationOptions.Required;
			}
		}

		public bool IsPacked
		{
			get
			{
				return (options & MemberSerializationOptions.Packed) == MemberSerializationOptions.Packed;
			}
			set
			{
				if (value)
				{
					options |= MemberSerializationOptions.Packed;
					return;
				}
				options &= ~MemberSerializationOptions.Packed;
			}
		}
		public bool OverwriteList
		{
			get
			{
				return (options & MemberSerializationOptions.OverwriteList) == MemberSerializationOptions.OverwriteList;
			}
			set
			{
				if (value)
				{
					options |= MemberSerializationOptions.OverwriteList;
					return;
				}
				options &= ~MemberSerializationOptions.OverwriteList;
			}
		}

		public bool AsReference
		{
			get
			{
				return (options & MemberSerializationOptions.AsReference) == MemberSerializationOptions.AsReference;
			}
			set
			{
				if (value)
				{
					options |= MemberSerializationOptions.AsReference;
				}
				else
				{
					options &= ~MemberSerializationOptions.AsReference;
				}
				options |= MemberSerializationOptions.AsReferenceHasValue;
			}
		}

		internal bool AsReferenceHasValue
		{
			get
			{
				return (options & MemberSerializationOptions.AsReferenceHasValue) == MemberSerializationOptions.AsReferenceHasValue;
			}
			set
			{
				if (value)
				{
					options |= MemberSerializationOptions.AsReferenceHasValue;
					return;
				}
				options &= ~MemberSerializationOptions.AsReferenceHasValue;
			}
		}

		public bool DynamicType
		{
			get
			{
				return (options & MemberSerializationOptions.DynamicType) == MemberSerializationOptions.DynamicType;
			}
			set
			{
				if (value)
				{
					options |= MemberSerializationOptions.DynamicType;
					return;
				}
				options &= ~MemberSerializationOptions.DynamicType;
			}
		}
		public MemberSerializationOptions Options
		{
			get
			{
				return options;
			}
			set
			{
				options = value;
			}
		}
	}
}
