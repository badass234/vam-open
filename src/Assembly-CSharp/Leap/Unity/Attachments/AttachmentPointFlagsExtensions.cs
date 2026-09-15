namespace Leap.Unity.Attachments
{
	public static class AttachmentPointFlagsExtensions
	{
		public static bool IsSinglePoint(this AttachmentPointFlags points)
		{
			return points != 0 && points == (AttachmentPointFlags)((uint)points & (uint)(0 - points));
		}

		public static bool ContainsPoint(this AttachmentPointFlags points, AttachmentPointFlags singlePoint)
		{
			return points.Contains(singlePoint);
		}

		public static bool Contains(this AttachmentPointFlags points, AttachmentPointFlags otherPoints)
		{
			if (points == AttachmentPointFlags.None || otherPoints == AttachmentPointFlags.None)
			{
				return false;
			}
			return (points & otherPoints) == otherPoints;
		}
	}
}
