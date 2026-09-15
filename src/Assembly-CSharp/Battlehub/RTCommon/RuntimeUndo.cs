using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTCommon
{
	public static class RuntimeUndo
	{
		private class BoolState
		{
			public bool value;

			public BoolState(bool v)
			{
				value = v;
			}
		}

		private class TransformRecord
		{
			public Vector3 position;

			public Quaternion rotation;

			public Vector3 scale;

			public Transform parent;

			public int siblingIndex = -1;
		}

		private class SelectionRecord
		{
			public UnityEngine.Object[] objects;

			public UnityEngine.Object activeObject;
		}

		private class RTSelectionInternalsAccessor : RuntimeSelection
		{
			public static UnityEngine.Object activeObjectProperty
			{
				get
				{
					return RuntimeSelection.m_activeObject;
				}
				set
				{
					RuntimeSelection.m_activeObject = value;
				}
			}

			public static UnityEngine.Object[] objectsProperty
			{
				get
				{
					return RuntimeSelection.m_objects;
				}
				set
				{
					RuntimeSelection.SetObjects(value);
				}
			}
		}

		private static List<Record> m_group;

		private static UndoStack<Record[]> m_stack;

		private static Stack<UndoStack<Record[]>> m_stacks;

		public const int Limit = 8192;

		public static bool Enabled { get; set; }

		public static bool CanUndo => m_stack.CanPop;

		public static bool CanRedo => m_stack.CanRestore;

		public static bool IsRecording => m_group != null;

		public static event RuntimeUndoEventHandler BeforeUndo;

		public static event RuntimeUndoEventHandler UndoCompleted;

		public static event RuntimeUndoEventHandler BeforeRedo;

		public static event RuntimeUndoEventHandler RedoCompleted;

		public static event RuntimeUndoEventHandler StateChanged;

		static RuntimeUndo()
		{
			Reset();
		}

		public static void Reset()
		{
			Enabled = true;
			m_group = null;
			m_stack = new UndoStack<Record[]>(8192);
			m_stacks = new Stack<UndoStack<Record[]>>();
		}

		public static void BeginRecord()
		{
			if (Enabled)
			{
				m_group = new List<Record>();
			}
		}

		public static void EndRecord()
		{
			if (!Enabled)
			{
				return;
			}
			if (m_group != null)
			{
				Record[] array = m_stack.Push(m_group.ToArray());
				if (array != null)
				{
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Purge();
					}
				}
				if (RuntimeUndo.StateChanged != null)
				{
					RuntimeUndo.StateChanged();
				}
			}
			m_group = null;
		}

		private static void RecordActivateDeactivate(GameObject g, BoolState value)
		{
			RecordObject(g, value, delegate(Record record)
			{
				GameObject gameObject = (GameObject)record.Target;
				BoolState boolState = (BoolState)record.State;
				if ((bool)gameObject && gameObject.activeSelf != boolState.value)
				{
					ExposeToEditor component = gameObject.GetComponent<ExposeToEditor>();
					if ((bool)component)
					{
						component.MarkAsDestroyed = !boolState.value;
					}
					else
					{
						gameObject.SetActive(boolState.value);
					}
					return true;
				}
				return false;
			}, delegate(Record record)
			{
				BoolState boolState2 = (BoolState)record.State;
				if (!boolState2.value)
				{
					GameObject gameObject2 = (GameObject)record.Target;
					if ((bool)gameObject2)
					{
						ExposeToEditor component2 = gameObject2.GetComponent<ExposeToEditor>();
						if ((bool)component2)
						{
							if (component2.MarkAsDestroyed)
							{
								UnityEngine.Object.DestroyImmediate(gameObject2);
							}
						}
						else if (!gameObject2.activeSelf)
						{
							UnityEngine.Object.DestroyImmediate(gameObject2);
						}
					}
				}
			});
		}

		public static void BeginRegisterCreateObject(GameObject g)
		{
			if (Enabled)
			{
				RecordActivateDeactivate(g, new BoolState(false));
			}
		}

		public static void RegisterCreatedObject(GameObject g)
		{
			if (Enabled)
			{
				ExposeToEditor component = g.GetComponent<ExposeToEditor>();
				if ((bool)component)
				{
					component.MarkAsDestroyed = false;
				}
				else
				{
					g.SetActive(true);
				}
				RecordActivateDeactivate(g, new BoolState(true));
			}
		}

		public static void BeginDestroyObject(GameObject g)
		{
			if (Enabled)
			{
				RecordActivateDeactivate(g, new BoolState(true));
			}
		}

		public static void DestroyObject(GameObject g)
		{
			if (Enabled)
			{
				ExposeToEditor component = g.GetComponent<ExposeToEditor>();
				if ((bool)component)
				{
					component.MarkAsDestroyed = true;
				}
				else
				{
					g.SetActive(false);
				}
				RecordActivateDeactivate(g, new BoolState(false));
			}
		}

		private static object GetValue(object target, MemberInfo m)
		{
			PropertyInfo propertyInfo = m as PropertyInfo;
			if (propertyInfo != null)
			{
				return propertyInfo.GetValue(target, null);
			}
			FieldInfo fieldInfo = m as FieldInfo;
			if (fieldInfo != null)
			{
				return fieldInfo.GetValue(target);
			}
			throw new ArgumentException("member is not FieldInfo and is not PropertyInfo", "m");
		}

		private static void SetValue(object target, MemberInfo m, object value)
		{
			PropertyInfo propertyInfo = m as PropertyInfo;
			if (propertyInfo != null)
			{
				propertyInfo.SetValue(target, value, null);
				return;
			}
			FieldInfo fieldInfo = m as FieldInfo;
			if (fieldInfo != null)
			{
				fieldInfo.SetValue(target, value);
				return;
			}
			throw new ArgumentException("member is not FieldInfo and is not PropertyInfo", "m");
		}

		private static Array DuplicateArray(Array array)
		{
			Array array2 = (Array)Activator.CreateInstance(array.GetType(), array.Length);
			if (array != null)
			{
				for (int i = 0; i < array2.Length; i++)
				{
					array2.SetValue(array.GetValue(i), i);
				}
			}
			return array;
		}

		public static void RecordValue(object target, MemberInfo memberInfo)
		{
			if (!Enabled)
			{
				return;
			}
			if (!(memberInfo is PropertyInfo) && !(memberInfo is FieldInfo))
			{
				Debug.LogWarning("Unable to record value");
				return;
			}
			object value = GetValue(target, memberInfo);
			if (value != null && value is Array)
			{
				object value2 = DuplicateArray((Array)value);
				SetValue(target, memberInfo, value2);
			}
			RecordObject(target, value, delegate(Record record)
			{
				object target2 = record.Target;
				if (target2 == null)
				{
					return false;
				}
				if (target2 is UnityEngine.Object && (UnityEngine.Object)target2 == null)
				{
					return false;
				}
				object state = record.State;
				object value3 = GetValue(target2, memberInfo);
				bool flag = true;
				if (state == null && value3 == null)
				{
					flag = false;
				}
				else if (state != null && value3 != null)
				{
					if (state is IEnumerable<object>)
					{
						IEnumerable<object> first = (IEnumerable<object>)state;
						IEnumerable<object> second = (IEnumerable<object>)value3;
						flag = !first.SequenceEqual(second);
					}
					else
					{
						flag = !state.Equals(value3);
					}
				}
				if (flag)
				{
					SetValue(target2, memberInfo, state);
				}
				return flag;
			}, delegate
			{
			});
		}

		public static void RecordTransform(Transform target, Transform parent = null, int siblingIndex = -1)
		{
			if (!Enabled)
			{
				return;
			}
			TransformRecord transformRecord = new TransformRecord();
			transformRecord.position = target.position;
			transformRecord.rotation = target.rotation;
			transformRecord.scale = target.localScale;
			TransformRecord transformRecord2 = transformRecord;
			transformRecord2.parent = parent;
			transformRecord2.siblingIndex = siblingIndex;
			RecordObject(target, transformRecord2, delegate(Record record)
			{
				Transform transform = (Transform)record.Target;
				if (!transform)
				{
					return false;
				}
				TransformRecord transformRecord3 = (TransformRecord)record.State;
				bool flag = transform.position != transformRecord3.position || transform.rotation != transformRecord3.rotation || transform.localScale != transformRecord3.scale;
				bool flag2 = transformRecord3.siblingIndex == -1;
				if (!flag2)
				{
					flag = flag || transform.parent != transformRecord3.parent || transform.GetSiblingIndex() != transformRecord3.siblingIndex;
				}
				if (flag)
				{
					Transform parent2 = transform.parent;
					if (!flag2)
					{
						transform.SetParent(transformRecord3.parent, true);
						transform.SetSiblingIndex(transformRecord3.siblingIndex);
					}
					transform.position = transformRecord3.position;
					transform.rotation = transformRecord3.rotation;
					transform.localScale = transformRecord3.scale;
				}
				return flag;
			}, delegate
			{
			});
		}

		public static void RecordSelection()
		{
			if (!Enabled)
			{
				return;
			}
			RecordObject(null, new SelectionRecord
			{
				objects = RuntimeSelection.objects,
				activeObject = RuntimeSelection.activeObject
			}, delegate(Record record)
			{
				SelectionRecord selectionRecord = (SelectionRecord)record.State;
				bool flag = false;
				if (selectionRecord.objects != null && RuntimeSelection.objects != null)
				{
					if (selectionRecord.objects.Length != RuntimeSelection.objects.Length)
					{
						flag = true;
					}
					else
					{
						for (int i = 0; i < RuntimeSelection.objects.Length; i++)
						{
							if (selectionRecord.objects[i] != RuntimeSelection.objects[i])
							{
								flag = true;
								break;
							}
						}
					}
				}
				else if (selectionRecord.objects == null)
				{
					flag = RuntimeSelection.objects != null && RuntimeSelection.objects.Length != 0;
				}
				else if (RuntimeSelection.objects == null)
				{
					flag = selectionRecord.objects != null && selectionRecord.objects.Length != 0;
				}
				if (flag)
				{
					List<UnityEngine.Object> list = null;
					if (selectionRecord.objects != null)
					{
						list = selectionRecord.objects.ToList();
						if (selectionRecord.activeObject != null)
						{
							if (list.Contains(selectionRecord.activeObject))
							{
								list.Remove(selectionRecord.activeObject);
								list.Insert(0, selectionRecord.activeObject);
							}
							else
							{
								list.Insert(0, selectionRecord.activeObject);
							}
						}
						RTSelectionInternalsAccessor.activeObjectProperty = selectionRecord.activeObject;
						RTSelectionInternalsAccessor.objectsProperty = list.ToArray();
					}
					else
					{
						RTSelectionInternalsAccessor.activeObjectProperty = null;
						RTSelectionInternalsAccessor.objectsProperty = null;
					}
				}
				return flag;
			}, delegate
			{
			});
		}

		public static void RecordComponent(MonoBehaviour component)
		{
			Type type = component.GetType();
			if (type != typeof(MonoBehaviour))
			{
				List<FieldInfo> list = new List<FieldInfo>();
				while (type != typeof(MonoBehaviour))
				{
					list.AddRange(type.GetSerializableFields());
					type = type.BaseType();
				}
				bool flag = false;
				if (!IsRecording)
				{
					flag = true;
					BeginRecord();
				}
				for (int i = 0; i < list.Count; i++)
				{
					RecordValue(component, list[i]);
				}
				if (flag)
				{
					EndRecord();
				}
			}
		}

		public static void RecordObject(object target, object state, ApplyCallback applyCallback, PurgeCallback purgeCallback = null)
		{
			if (!Enabled)
			{
				return;
			}
			if (purgeCallback == null)
			{
				purgeCallback = delegate
				{
				};
			}
			if (m_group != null)
			{
				m_group.Add(new Record(target, state, applyCallback, purgeCallback));
				return;
			}
			Record[] array = m_stack.Push(new Record[1]
			{
				new Record(target, state, applyCallback, purgeCallback)
			});
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Purge();
				}
			}
			if (RuntimeUndo.StateChanged != null)
			{
				RuntimeUndo.StateChanged();
			}
		}

		public static void Redo()
		{
			if (!Enabled || !m_stack.CanRestore)
			{
				return;
			}
			if (RuntimeUndo.BeforeRedo != null)
			{
				RuntimeUndo.BeforeRedo();
			}
			bool flag;
			do
			{
				flag = false;
				Record[] array = m_stack.Restore();
				foreach (Record record in array)
				{
					flag |= record.Apply();
				}
			}
			while (!flag && m_stack.CanRestore);
			if (RuntimeUndo.RedoCompleted != null)
			{
				RuntimeUndo.RedoCompleted();
			}
		}

		public static void Undo()
		{
			if (!Enabled || !m_stack.CanPop)
			{
				return;
			}
			if (RuntimeUndo.BeforeUndo != null)
			{
				RuntimeUndo.BeforeUndo();
			}
			bool flag;
			do
			{
				flag = false;
				Record[] array = m_stack.Pop();
				foreach (Record record in array)
				{
					flag |= record.Apply();
				}
			}
			while (!flag && m_stack.CanPop);
			if (RuntimeUndo.UndoCompleted != null)
			{
				RuntimeUndo.UndoCompleted();
			}
		}

		public static void Purge()
		{
			if (!Enabled)
			{
				return;
			}
			foreach (Record[] item in (IEnumerable)m_stack)
			{
				if (item != null)
				{
					foreach (Record record in item)
					{
						record.Purge();
					}
				}
			}
			m_stack.Clear();
			if (RuntimeUndo.StateChanged != null)
			{
				RuntimeUndo.StateChanged();
			}
		}

		public static void Store()
		{
			if (Enabled)
			{
				m_stacks.Push(m_stack);
				m_stack = new UndoStack<Record[]>(8192);
				if (RuntimeUndo.StateChanged != null)
				{
					RuntimeUndo.StateChanged();
				}
			}
		}

		public static void Restore()
		{
			if (!Enabled)
			{
				return;
			}
			if (m_stack != null)
			{
				m_stack.Clear();
			}
			if (m_stacks.Count > 0)
			{
				m_stack = m_stacks.Pop();
				if (RuntimeUndo.StateChanged != null)
				{
					RuntimeUndo.StateChanged();
				}
			}
		}
	}
}
