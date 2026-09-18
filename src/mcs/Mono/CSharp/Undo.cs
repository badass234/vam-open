using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.CSharp
{
	public class Undo
	{
		private List<Action> undo_actions;

		public void AddTypeContainer(TypeContainer current_container, TypeDefinition tc)
		{
			if (current_container == tc)
			{
				Console.Error.WriteLine("Internal error: inserting container into itself");
				return;
			}
			if (undo_actions == null)
			{
				undo_actions = new List<Action>();
			}
			if (current_container.Containers != null)
			{
				TypeContainer existing = current_container.Containers.FirstOrDefault((TypeContainer l) => l.MemberName.Basename == tc.MemberName.Basename);
				if (existing != null)
				{
					current_container.RemoveContainer(existing);
					undo_actions.Add(() =>
					{
						current_container.AddTypeContainer(existing);
					});
				}
			}
			undo_actions.Add(() =>
			{
				current_container.RemoveContainer(tc);
			});
		}

		public void ExecuteUndo()
		{
			if (undo_actions == null)
			{
				return;
			}
			foreach (Action undo_action in undo_actions)
			{
				undo_action();
			}
			undo_actions = null;
		}
	}
}
