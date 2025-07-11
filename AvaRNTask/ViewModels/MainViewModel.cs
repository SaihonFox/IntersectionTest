using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

using AvaRNTask.Backend;

using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AvaRNTask.ViewModels;

public partial class MainViewModel : ViewModelBase
{
	// Режим рисования
	public enum DrawingMode
	{
		PlacingPoints = 0,	// Режим размещения точек
		Selecting			// Режим выделения
	}

	// Точки по которым будут строиться линии
	[ObservableProperty]
	private ObservableCollection<Point> _points = [];

	// Все линии на канвасе
	[ObservableProperty]
	private ObservableCollection<Line> _lines = [];

	// Выделенные линии
	[ObservableProperty]
	public ObservableCollection<Line> _selectedLines = [];

	// Область выделения
	[ObservableProperty]
	private Rect? _selectionRect = null;

	// Массив из всех режимов рисования. Нужен для списка в ComboBox
	public DrawingMode[] DrawingModes => Enum.GetValues<DrawingMode>();

	// Текущий режим рисования
	[ObservableProperty]
	private DrawingMode _selectedMode = DrawingMode.PlacingPoints;

	public void UpdateSelection(Rect rect)
	{
		// Обновляем прямоугольную область выделения
		SelectionRect = rect;
		foreach (var line in Lines)
		{
			// Проверяем, пересекается ли линия с прямоугольной областью выделения
			bool isIntersecting = Intersections.Intersects(line, rect);
			// Если линия пересекается то меняем цвет на зеленый, иначе возвращаем черный
			line.Stroke = isIntersecting ? Brushes.LimeGreen : Brushes.Black;
			// Обновляем список выделенных линий
			if (!isIntersecting && SelectedLines.Contains(line))
				SelectedLines.Remove(line); // убираем из списка
			else if(isIntersecting && !SelectedLines.Contains(line))
				SelectedLines.Add(line); // добавляем в список
		}
	}

	public void ClearSelection()
	{
		// Очищаем прямоугольную область выделения
		SelectionRect = null;
		foreach (var line in SelectedLines.ToList())
		{
			line.Stroke = Brushes.Black; // Возвращаем исходный цвет выделенным линиям
			SelectedLines.Remove(line); // Убираем линию из списка выделенных линий
		}
	}
}