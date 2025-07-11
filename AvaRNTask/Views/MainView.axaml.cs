using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

using AvaRNTask.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static AvaRNTask.ViewModels.MainViewModel;

namespace AvaRNTask.Views;

public partial class MainView : UserControl
{
	// Вьюмодел окна
	private MainViewModel ViewModel => (DataContext as MainViewModel)!;

	// Начальная позиция области выделения
	private Point? firstSelectionPoint = null;

	// Область выделения
	private Rect? selectionArea = null;

	// Генератор псевдослучайных чисел
	private readonly Random random = new();

	// Выделяем ли мы в данный момент
	private bool isSelecting = false;

	public MainView()
	{
		InitializeComponent();

		// Присоединение событий канваса
		canvas.PointerPressed += Canvas_PointerPressed;
		canvas.PointerMoved += Canvas_PointerMoved;
		canvas.PointerReleased += Canvas_PointerReleased;

		// Присоединение событий кнопок
		generate.Click += Gen_Click;
		clear_canvas.Click += Clear_canvas_Click;

		Loaded += (_, _) =>
		{
			ViewModel.Lines.CollectionChanged += (_, e) =>
			{
				if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
					desktop.MainWindow!.Title = $"Кол-во линий: {ViewModel.Lines.Count}";
			};
		};
	}

	/// <summary>
	/// Асинхронно рисуем линии по заданным точкам на канвасе
	/// </summary>
	/// <param name="points">
	/// Список точек по которым должны будут опираться линии
	/// </param>
	async Task DrawLinesAsync(List<Point> points, CancellationToken token = default)
	{
		if (points.Count <= 1) return;
		List<Line> lines = [];
		for (int i = 0; i < points.Count - 1; i++)
			lines.Add(CreateLine(points[i], points[i + 1]));

		// С 50% шансом добавит замыкающую линию между первой и последней точкой
		if(random.Next(2) == 1)
			lines.Add(CreateLine(points[^1], points[0]));

		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			foreach (var line in lines)
			{
				ViewModel.Lines.Add(line);
				canvas.Children.Add(line);
			}
			if(selectionArea != null)
				ViewModel.UpdateSelection(((Rect)selectionArea));
		}, DispatcherPriority.Background, token);
	}

	/// <summary>
	/// Создание линий по заданным точка
	/// </summary>
	/// <param name="startPoint">Начальная точка</param>
	/// <param name="endPoint">Конечная точка</param>
	/// <returns>Линия, с заданными точками</returns>
	Line CreateLine(Point startPoint, Point endPoint) => Dispatcher.UIThread.Invoke(() =>
		new Line
		{
			StartPoint = startPoint,
			EndPoint = endPoint,
			Stroke = Brushes.Black,
			StrokeThickness = 1
		}
	);

	/// <summary>
	/// Генерирует список точек по которым будут строиться линии
	/// </summary>
	/// <returns>Список точек</returns>
	List<Point> GenerateRandomPoints()
	{
		List<Point> points = [];
		int segments = random.Next(3, 10);

		for (int j = 0; j < segments; j++)
		{
			points.Add(new Point(
				random.Next(0, (int)canvas.Bounds.Width),
				random.Next(0, (int)canvas.Bounds.Height)
			));
		}

		return points;
	}

	/// <summary>
	/// Обработчик нажатия на кнопку генерации линий
	/// </summary>
	async void Gen_Click(object? sender, RoutedEventArgs e)
	{
		int repeatCount = int.Parse(await Dispatcher.UIThread.InvokeAsync(() => shapes_count.Text ?? "0"));

		await Parallel.ForAsync(0, repeatCount, async (i, token) =>
		{
			List<Point> points = GenerateRandomPoints();
			await DrawLinesAsync(points).ConfigureAwait(false);
		});
	}

	void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		var point = e.GetPosition(canvas); // позиция курсора на канвасе

		// Режим размещения точек
		if (ViewModel.SelectedMode == DrawingMode.PlacingPoints)
		{
			// ЛКМ - добавление новой точки и рисование линии
			if (e.Properties.IsLeftButtonPressed)
			{
				if (ViewModel.Points.Count == 0)
					ViewModel.Points.Add(point);
				else
				{
					// проверка на дубликата точки с предыдущим разом
					if (ViewModel.Points[^1].Equals(point))
						return;

					var line = new Line
					{
						StartPoint = ViewModel.Points[^1],
						EndPoint = point,
						Stroke = Brushes.Black,
						StrokeThickness = 1
					};
					ViewModel.Points.Add(point);
					ViewModel.Lines.Add(line);
					canvas.Children.Add(line);
				}
			}
			// ПКМ - замыкание многоугольника
			if(e.Properties.IsRightButtonPressed)
			{
				if(ViewModel.Points.Count > 2)
				{
					double x = ViewModel.Points[0].X, y = ViewModel.Points[0].Y;
					// Проверка, чтобы точки не лежали по одной оси подряд
					if (!(ViewModel.Points.All(p => p.X == x) || ViewModel.Points.All(p => p.Y == y)))
					{
						var line = new Line
						{
							StartPoint = ViewModel.Points[^1],
							EndPoint = ViewModel.Points[0],
							Stroke = Brushes.Black,
							StrokeThickness = 1
						};
						ViewModel.Lines.Add(line);
						canvas.Children.Add(line);
					}
				}
				ViewModel.Points.Clear();
			}
		}
		// Режим выделения 
		else if (ViewModel.SelectedMode == DrawingMode.Selecting)
		{
			//	ViewModel.ClearSelection();
			firstSelectionPoint = point; // задаем начальную точку выделения
			isSelecting = true;
		}
	}

	/// <summary>
	/// Обработка перемещения мыши по канвасу
	/// </summary>
	void Canvas_PointerMoved(object? sender, PointerEventArgs e)
	{
		var point = e.GetPosition(canvas); // позиция курсора на канвасе
		mouse_pos.Text = $"X: {point.X,0}\nY: {point.Y,0}";
		
		// пропускаем если в данный момент мы просто проводимся мышкой по канвасу, а не выделяем
		if (!(ViewModel.SelectedMode == DrawingMode.Selecting && isSelecting)) return;

		// Обновление зоны выделения области
		selectionArea = new Rect(
			Math.Min(firstSelectionPoint!.Value.X, point.X),
			Math.Min(firstSelectionPoint.Value.Y, point.Y),
			Math.Abs(point.X - firstSelectionPoint.Value.X),
			Math.Abs(point.Y - firstSelectionPoint.Value.Y)
		);
		ViewModel.UpdateSelection((Rect)selectionArea); // Обновление области выделения на стороне вьюмодела
	}

	/// <summary>
	/// Обработка отпускания мыши и очистка визуального выделения линий
	/// </summary>
	void Canvas_PointerReleased(object? sender, PointerEventArgs e)
	{
		if (ViewModel.SelectedMode == DrawingMode.Selecting && isSelecting)
		{
			isSelecting = false;
			firstSelectionPoint = null;
			ViewModel.UpdateSelection(new Rect());
		}
		selectionArea = new Rect();
	}

	/// <summary>
	/// Очистка канваса от линий
	/// </summary>
	void Clear_canvas_Click(object? sender, RoutedEventArgs e)
	{
		ViewModel.Points.Clear();
		ViewModel.SelectedLines.Clear();
		ViewModel.Lines.Clear();
		canvas.Children.RemoveAll(canvas.Children.Where(x => x.GetType() != typeof(Rectangle)));
	}
}