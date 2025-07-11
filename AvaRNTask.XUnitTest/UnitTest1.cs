using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;

using AvaRNTask.Backend;
using AvaRNTask.ViewModels;
using AvaRNTask.Views;

namespace AvaRNTask.XUnitTest;

public class UnitTest1
{
	/// <summary>
	/// Тестовые данные
	/// </summary>
	#region TestData
	public static IEnumerable<object[]> IsLineIntersectingRectData()
	{
		yield return [new Line { StartPoint = new Point(1, 4), EndPoint = new Point(4, 4) }, new Rect(2, 2, 8, 8)];
	}

	public static IEnumerable<object[]> IsLineNotIntersectingRectData()
	{
		yield return [new Line { StartPoint = new Point(1, 1), EndPoint = new Point(3, 1) }, new Rect(2, 2, 8, 8)];
	}
	#endregion TestData

	/// <summary>
	/// Тест на проверку нахождения или пересечении линии с зоной области выделения
	/// </summary>
	/// <param name="line">Линия</param>
	/// <param name="rect">Прямоугольная область для выделения линии</param>
	[Theory]
	[MemberData(nameof(IsLineIntersectingRectData))]
	public void IsLineIntersectingRect(Line line, Rect rect)
	{
		bool isIntersecting = Intersections.Intersects(line, rect);
		
		Assert.True(isIntersecting);
	}

	/// <summary>
	/// UI Тест на проверку нахождения линий в зоне области выделения
	/// </summary>
	[AvaloniaFact]
	public void IsLineIntersectingRectUI()
	{
		var viewmodel = new MainViewModel();
		var main = new MainView { DataContext = viewmodel };

		var window = new Window { Content = main };
		window.Show();

		// рисуем треугольник с основанием к верху
		window.MouseDown(new Point(100, 400), MouseButton.Left);
		window.MouseUp(new Point(100, 400), MouseButton.Left);
		window.MouseDown(new Point(400, 400), MouseButton.Left);
		window.MouseUp(new Point(400, 400), MouseButton.Left);
		window.MouseDown(new Point(250, 600), MouseButton.Left);
		window.MouseUp(new Point(250, 600), MouseButton.Left);
		window.MouseDown(new Point(250, 600), MouseButton.Right);
		window.MouseUp(new Point(250, 600), MouseButton.Right);

		// выбираем режим выделения области
		viewmodel.SelectedMode = MainViewModel.DrawingMode.Selecting;

		// выделяем с X=150, Y=100 по X=700, Y=700
		window.MouseDown(new Point(150, 100), MouseButton.Left);
		window.MouseMove(new Point(700, 700));

		int triangleSides = 3;
		Assert.Equal(triangleSides, viewmodel.SelectedLines.Count);
	}

	/// <summary>
	/// Тест на проверку, что линия не лежит и не пересекается с зоной выделения области
	/// </summary>
	/// <param name="line">Линия</param>
	/// <param name="rect">Прямоугольная область для выделения линии</param>
	[Theory]
	[MemberData(nameof(IsLineNotIntersectingRectData))]
	public void IsLineNotIntersectingRect(Line line, Rect rect)
	{
		bool isNotIntersecting = Intersections.Intersects(line, rect);

		Assert.False(isNotIntersecting);
	}

	/// <summary>
	/// UI Тест на проверку, что линия не лежит и не пересекается с зоной выделения области
	/// </summary>
	[AvaloniaFact]
	public void IsLineNotIntersectingRectUI()
	{
		var viewmodel = new MainViewModel();
		var main = new MainView { DataContext = viewmodel };

		var window = new Window { Content = main };
		window.Show();

		// рисуем треугольник с основанием к верху
		window.MouseDown(new Point(100, 400), MouseButton.Left);
		window.MouseUp(new Point(100, 400), MouseButton.Left);
		window.MouseDown(new Point(400, 400), MouseButton.Left);
		window.MouseUp(new Point(400, 400), MouseButton.Left);
		window.MouseDown(new Point(250, 600), MouseButton.Left);
		window.MouseUp(new Point(250, 600), MouseButton.Left);
		window.MouseDown(new Point(250, 600), MouseButton.Right);
		window.MouseUp(new Point(250, 600), MouseButton.Right);

		// выбираем режим выделения области
		viewmodel.SelectedMode = MainViewModel.DrawingMode.Selecting;

		// выделяем с X=150, Y=100 по X=700, Y=700
		window.MouseDown(new Point(150, 100), MouseButton.Left);
		window.MouseMove(new Point(700, 700));

		int triangleSides = 3;
		Assert.Equal(triangleSides, viewmodel.SelectedLines.Count);
	}
}
