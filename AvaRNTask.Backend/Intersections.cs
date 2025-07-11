using Avalonia;
using Avalonia.Controls.Shapes;

namespace AvaRNTask.Backend;

public static class Intersections
{
	/// <summary>
	/// Определяет, пересекается ли линия с прямоугольником.
	/// </summary>
	/// <param name="line">Линия пересекаемая прямугольной областью</param>
	/// <param name="rect">Прямоугольник пересекающий линию</param>
	/// <remarks>
	/// Упрощенный вариант алгоритма Коэна — Сазерленда <see href="https://ru.wikibooks.org/wiki/%D0%A0%D0%B5%D0%B0%D0%BB%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D0%B8_%D0%B0%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC%D0%BE%D0%B2/%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC_%D0%9A%D0%BE%D1%8D%D0%BD%D0%B0_%E2%80%94_%D0%A1%D0%B0%D0%B7%D0%B5%D1%80%D0%BB%D0%B5%D0%BD%D0%B4%D0%B0">Wiki</see>
	/// </remarks>
	/// <returns>
	/// true - пересекается
	/// false - не пересекается
	/// </returns>
	public static bool Intersects(Line line, Rect rect)
	{
		// Определение границ линии
		var minX = Math.Min(line.StartPoint.X, line.EndPoint.X);
		var maxX = Math.Max(line.StartPoint.X, line.EndPoint.X);
		var minY = Math.Min(line.StartPoint.Y, line.EndPoint.Y);
		var maxY = Math.Max(line.StartPoint.Y, line.EndPoint.Y);

		// Если прямоугольнмк никак не пересекается с линией
		if (rect.Left > maxX || rect.Right < minX)
			return false;
		if (rect.Top > maxY || rect.Bottom < minY)
			return false;

		// В случае полного нахождения линии внутри прямоугольника, то возвращаем true
		if (rect.Contains(new Rect(minX, minY, maxX - minX, maxY - minY)))
			return true;

		double yForX(double x) => line.StartPoint.Y - (x - line.StartPoint.X) * ((line.StartPoint.Y - line.EndPoint.Y) / (line.EndPoint.X - line.StartPoint.X));

		var yAtRectLeft = yForX(rect.Left);
		var yAtRectRight = yForX(rect.Right);

		// Если нижняя граница прямоугольника выше двух точек линии, то они не пересекаются
		if (rect.Bottom < yAtRectLeft && rect.Bottom < yAtRectRight)
			return false;
		// Если верхняя граница прямоугольника ниже двух точек линии, то они не пересекаются
		if (rect.Top > yAtRectLeft && rect.Top > yAtRectRight)
			return false;

		return true;
	}
}