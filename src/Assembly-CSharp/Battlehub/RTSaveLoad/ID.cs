namespace Battlehub.RTSaveLoad
{
	public struct ID
	{
		private long m_id;

		public ID(long id)
		{
			m_id = id;
		}

		public override bool Equals(object obj)
		{
			if (obj is ID)
			{
				ID iD = (ID)obj;
				return m_id.Equals(iD.m_id);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return m_id.GetHashCode();
		}

		public override string ToString()
		{
			return m_id.ToString();
		}
	}
}
