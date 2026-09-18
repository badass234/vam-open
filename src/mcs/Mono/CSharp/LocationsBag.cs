using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mono.CSharp
{
	public class LocationsBag
	{
		public class MemberLocations
		{
			public readonly IList<Tuple<Modifiers, Location>> Modifiers;

			private List<Location> locations;

			public Location this[int index] => locations[index];

			public int Count => locations.Count;

			public MemberLocations(IList<Tuple<Modifiers, Location>> mods)
			{
				Modifiers = mods;
			}

			public MemberLocations(IList<Tuple<Modifiers, Location>> mods, Location loc)
				: this(mods)
			{
				AddLocations(loc);
			}

			public MemberLocations(IList<Tuple<Modifiers, Location>> mods, Location[] locs)
				: this(mods)
			{
				AddLocations(locs);
			}

			public MemberLocations(IList<Tuple<Modifiers, Location>> mods, List<Location> locs)
				: this(mods)
			{
				locations = locs;
			}

			public void AddLocations(Location loc)
			{
				if (locations == null)
				{
					locations = new List<Location>();
				}
				locations.Add(loc);
			}

			public void AddLocations(params Location[] additional)
			{
				if (locations == null)
				{
					locations = new List<Location>(additional);
				}
				else
				{
					locations.AddRange(additional);
				}
			}
		}

		private Dictionary<object, List<Location>> simple_locs = new Dictionary<object, List<Location>>(ReferenceEquality<object>.Default);

		private Dictionary<MemberCore, MemberLocations> member_locs = new Dictionary<MemberCore, MemberLocations>(ReferenceEquality<MemberCore>.Default);

		[Conditional("FULL_AST")]
		public void AddLocation(object element, params Location[] locations)
		{
			simple_locs.Add(element, new List<Location>(locations));
		}

		[Conditional("FULL_AST")]
		public void InsertLocation(object element, int index, Location location)
		{
			List<Location> value;
			if (!simple_locs.TryGetValue(element, out value))
			{
				value = new List<Location>();
				simple_locs.Add(element, value);
			}
			value.Insert(index, location);
		}

		[Conditional("FULL_AST")]
		public void AddStatement(object element, params Location[] locations)
		{
			if (locations.Length == 0)
			{
				throw new ArgumentException("Statement is missing semicolon location");
			}
		}

		[Conditional("FULL_AST")]
		public void AddMember(MemberCore member, IList<Tuple<Modifiers, Location>> modLocations)
		{
			member_locs.Add(member, new MemberLocations(modLocations));
		}

		[Conditional("FULL_AST")]
		public void AddMember(MemberCore member, IList<Tuple<Modifiers, Location>> modLocations, Location location)
		{
			member_locs.Add(member, new MemberLocations(modLocations, location));
		}

		[Conditional("FULL_AST")]
		public void AddMember(MemberCore member, IList<Tuple<Modifiers, Location>> modLocations, params Location[] locations)
		{
			member_locs.Add(member, new MemberLocations(modLocations, locations));
		}

		[Conditional("FULL_AST")]
		public void AddMember(MemberCore member, IList<Tuple<Modifiers, Location>> modLocations, List<Location> locations)
		{
			member_locs.Add(member, new MemberLocations(modLocations, locations));
		}

		[Conditional("FULL_AST")]
		public void AppendTo(object element, Location location)
		{
			List<Location> value;
			if (!simple_locs.TryGetValue(element, out value))
			{
				value = new List<Location>();
				simple_locs.Add(element, value);
			}
			value.Add(location);
		}

		[Conditional("FULL_AST")]
		public void AppendToMember(MemberCore existing, params Location[] locations)
		{
			MemberLocations value;
			if (member_locs.TryGetValue(existing, out value))
			{
				value.AddLocations(locations);
			}
		}

		public List<Location> GetLocations(object element)
		{
			List<Location> value;
			simple_locs.TryGetValue(element, out value);
			return value;
		}

		public MemberLocations GetMemberLocation(MemberCore element)
		{
			MemberLocations value;
			member_locs.TryGetValue(element, out value);
			return value;
		}
	}
}
