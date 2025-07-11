using Avalonia;

using System;

namespace AvaRNTask.Models;

public class Selection
{
	public Point TopLeft;

	public Point BottomRight;

	public double Width => Math.Max(TopLeft.X, BottomRight.X) - Math.Min(TopLeft.X, BottomRight.X);

	public double Height => Math.Max(TopLeft.Y, BottomRight.Y) - Math.Min(TopLeft.Y, BottomRight.Y);
}