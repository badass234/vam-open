using System;
using System.Collections;

namespace Battlehub.RTCommon
{
	public class UndoStack<T> : IEnumerable
	{
		private int m_tosIndex;

		private T[] m_buffer;

		private int m_count;

		private int m_totalCount;

		public int Count => m_count;

		public bool CanPop => m_count > 0;

		public bool CanRestore => m_count < m_totalCount;

		public UndoStack(int size)
		{
			if (size == 0)
			{
				throw new ArgumentException("size should be greater than 0", "size");
			}
			m_buffer = new T[size];
		}

		public T Push(T item)
		{
			T result = m_buffer[m_tosIndex];
			m_buffer[m_tosIndex] = item;
			m_tosIndex++;
			m_tosIndex %= m_buffer.Length;
			if (m_count < m_buffer.Length)
			{
				m_count++;
				result = default(T);
			}
			m_totalCount = m_count;
			return result;
		}

		public T Restore()
		{
			if (!CanRestore)
			{
				throw new InvalidOperationException("nothing to restore");
			}
			if (m_count < m_totalCount)
			{
				m_tosIndex++;
				m_tosIndex %= m_buffer.Length;
				m_count++;
			}
			return Peek();
		}

		public T Peek()
		{
			if (m_count == 0)
			{
				throw new InvalidOperationException("Stack is empty");
			}
			int num = m_tosIndex - 1;
			if (num < 0)
			{
				num = m_buffer.Length - 1;
			}
			return m_buffer[num];
		}

		public T Pop()
		{
			if (m_count == 0)
			{
				throw new InvalidOperationException("Stack is empty");
			}
			m_count--;
			m_tosIndex--;
			if (m_tosIndex < 0)
			{
				m_tosIndex = m_buffer.Length - 1;
			}
			return m_buffer[m_tosIndex];
		}

		public void Clear()
		{
			m_tosIndex = 0;
			m_count = 0;
			m_totalCount = 0;
			for (int i = 0; i < m_buffer.Length; i++)
			{
				m_buffer[i] = default(T);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_buffer.GetEnumerator();
		}
	}
}
