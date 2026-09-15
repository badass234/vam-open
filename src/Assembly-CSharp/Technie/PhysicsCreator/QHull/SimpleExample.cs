namespace Technie.PhysicsCreator.QHull
{
	public class SimpleExample
	{
		public static void main(string[] args)
		{
			Point3d[] points = new Point3d[7]
			{
				new Point3d(0.0, 0.0, 0.0),
				new Point3d(1.0, 0.5, 0.0),
				new Point3d(2.0, 0.0, 0.0),
				new Point3d(0.5, 0.5, 0.5),
				new Point3d(0.0, 0.0, 2.0),
				new Point3d(0.1, 0.2, 0.3),
				new Point3d(0.0, 2.0, 0.0)
			};
			QuickHull3D quickHull3D = new QuickHull3D();
			quickHull3D.build(points);
		}
	}
}
