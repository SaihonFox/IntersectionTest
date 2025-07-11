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
	// �������� ����
	private MainViewModel ViewModel => (DataContext as MainViewModel)!;

	// ��������� ������� ������� ���������
	private Point? firstSelectionPoint = null;

	// ������� ���������
	private Rect? selectionArea = null;

	// ��������� ��������������� �����
	private readonly Random random = new();

	// �������� �� �� � ������ ������
	private bool isSelecting = false;

	public MainView()
	{
		InitializeComponent();

		// ������������� ������� �������
		canvas.PointerPressed += Canvas_PointerPressed;
		canvas.PointerMoved += Canvas_PointerMoved;
		canvas.PointerReleased += Canvas_PointerReleased;

		// ������������� ������� ������
		generate.Click += Gen_Click;
		clear_canvas.Click += Clear_canvas_Click;

		Loaded += (_, _) =>
		{
			ViewModel.Lines.CollectionChanged += (_, e) =>
			{
				if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
					desktop.MainWindow!.Title = $"���-�� �����: {ViewModel.Lines.Count}";
			};
		};
	}

	/// <summary>
	/// ���������� ������ ����� �� �������� ������ �� �������
	/// </summary>
	/// <param name="points">
	/// ������ ����� �� ������� ������ ����� ��������� �����
	/// </param>
	async Task DrawLinesAsync(List<Point> points, CancellationToken token = default)
	{
		if (points.Count <= 1) return;
		List<Line> lines = [];
		for (int i = 0; i < points.Count - 1; i++)
			lines.Add(CreateLine(points[i], points[i + 1]));

		// � 50% ������ ������� ���������� ����� ����� ������ � ��������� ������
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
	/// �������� ����� �� �������� �����
	/// </summary>
	/// <param name="startPoint">��������� �����</param>
	/// <param name="endPoint">�������� �����</param>
	/// <returns>�����, � ��������� �������</returns>
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
	/// ���������� ������ ����� �� ������� ����� ��������� �����
	/// </summary>
	/// <returns>������ �����</returns>
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
	/// ���������� ������� �� ������ ��������� �����
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
		var point = e.GetPosition(canvas); // ������� ������� �� �������

		// ����� ���������� �����
		if (ViewModel.SelectedMode == DrawingMode.PlacingPoints)
		{
			// ��� - ���������� ����� ����� � ��������� �����
			if (e.Properties.IsLeftButtonPressed)
			{
				if (ViewModel.Points.Count == 0)
					ViewModel.Points.Add(point);
				else
				{
					// �������� �� ��������� ����� � ���������� �����
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
			// ��� - ��������� ��������������
			if(e.Properties.IsRightButtonPressed)
			{
				if(ViewModel.Points.Count > 2)
				{
					double x = ViewModel.Points[0].X, y = ViewModel.Points[0].Y;
					// ��������, ����� ����� �� ������ �� ����� ��� ������
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
		// ����� ��������� 
		else if (ViewModel.SelectedMode == DrawingMode.Selecting)
		{
			//	ViewModel.ClearSelection();
			firstSelectionPoint = point; // ������ ��������� ����� ���������
			isSelecting = true;
		}
	}

	/// <summary>
	/// ��������� ����������� ���� �� �������
	/// </summary>
	void Canvas_PointerMoved(object? sender, PointerEventArgs e)
	{
		var point = e.GetPosition(canvas); // ������� ������� �� �������
		mouse_pos.Text = $"X: {point.X,0}\nY: {point.Y,0}";
		
		// ���������� ���� � ������ ������ �� ������ ���������� ������ �� �������, � �� ��������
		if (!(ViewModel.SelectedMode == DrawingMode.Selecting && isSelecting)) return;

		// ���������� ���� ��������� �������
		selectionArea = new Rect(
			Math.Min(firstSelectionPoint!.Value.X, point.X),
			Math.Min(firstSelectionPoint.Value.Y, point.Y),
			Math.Abs(point.X - firstSelectionPoint.Value.X),
			Math.Abs(point.Y - firstSelectionPoint.Value.Y)
		);
		ViewModel.UpdateSelection((Rect)selectionArea); // ���������� ������� ��������� �� ������� ���������
	}

	/// <summary>
	/// ��������� ���������� ���� � ������� ����������� ��������� �����
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
	/// ������� ������� �� �����
	/// </summary>
	void Clear_canvas_Click(object? sender, RoutedEventArgs e)
	{
		ViewModel.Points.Clear();
		ViewModel.SelectedLines.Clear();
		ViewModel.Lines.Clear();
		canvas.Children.RemoveAll(canvas.Children.Where(x => x.GetType() != typeof(Rectangle)));
	}
}