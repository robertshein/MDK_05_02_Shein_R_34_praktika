using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DragAndDrop_Shein
{
    public partial class PuzzleWindow : Window
    {
        const int COLS = 3, ROWS = 3;
        const double PIECE_SIZE = 120;
        const double PUZZLE_LEFT = 460;
        const double PUZZLE_TOP = 80;
        const double SNAP_DIST = 40;

        readonly Random rnd = new Random();

        int[] targetCol, targetRow;
        bool[] placed;
        int placedCount;

        FrameworkElement dragging;
        Point dragOffset;
        DispatcherTimer timer;

        public PuzzleWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => BuildPuzzle();
        }

        void BuildPuzzle()
        {
            for (int i = mainCanvas.Children.Count - 1; i >= 0; i--)
            {
                FrameworkElement fe = mainCanvas.Children[i] as FrameworkElement;
                if (fe != null && fe.Tag is int)
                    mainCanvas.Children.RemoveAt(i);
            }

            placedCount = 0;
            statusText.Text = "";
            targetCol = new int[ROWS * COLS];
            targetRow = new int[ROWS * COLS];
            placed = new bool[ROWS * COLS];

            BitmapSource src = CreatePuzzleImage();

            for (int r = 0; r < ROWS; r++)
                for (int c = 0; c < COLS; c++)
                {
                    Rectangle slot = new Rectangle
                    {
                        Width = PIECE_SIZE,
                        Height = PIECE_SIZE,
                        Stroke = new SolidColorBrush(Color.FromRgb(0x90, 0xA4, 0xAE)),
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Color.FromArgb(60, 200, 210, 220)),
                        Tag = r * COLS + c
                    };
                    Canvas.SetLeft(slot, PUZZLE_LEFT + c * PIECE_SIZE);
                    Canvas.SetTop(slot, PUZZLE_TOP  + r * PIECE_SIZE);
                    Canvas.SetZIndex(slot, 0);
                    mainCanvas.Children.Add(slot);
                }

            int[] order = ShuffledIndices(ROWS * COLS);
            double cellW = 370.0 / COLS;
            double cellH = 430.0 / ROWS;

            for (int i = 0; i < ROWS * COLS; i++)
            {
                int idx = order[i];
                int pr = idx / COLS;
                int pc = idx % COLS;
                targetRow[idx] = pr;
                targetCol[idx] = pc;

                int srcW = src.PixelWidth  / COLS;
                int srcH = src.PixelHeight / ROWS;
                CroppedBitmap crop = new CroppedBitmap(src,
                    new Int32Rect(pc * srcW, pr * srcH, srcW, srcH));

                Border piece = new Border
                {
                    Width = PIECE_SIZE,
                    Height = PIECE_SIZE,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(2),
                    Child = new Image { Source = crop, Stretch = Stretch.Fill },
                    Cursor = Cursors.Hand,
                    Tag = idx,
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        BlurRadius = 6,
                        ShadowDepth = 3,
                        Opacity = 0.4
                    }
                };
                piece.MouseDown += Piece_MouseDown;

                int gr = i / COLS;
                int gc = i % COLS;
                double lx = 10 + gc * cellW + rnd.Next(-12, 12);
                double ly = 45 + gr * cellH + rnd.Next(-10, 10);
                Canvas.SetLeft(piece, lx);
                Canvas.SetTop(piece,  ly);
                Canvas.SetZIndex(piece, 1);
                mainCanvas.Children.Add(piece);
            }

            if (timer == null)
            {
                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(1000.0 / 60)
                };
                timer.Tick += Timer_Tick;
            }
        }

        BitmapSource CreatePuzzleImage()
        {
            int size = 360;
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                size, size, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawRectangle(
                    new LinearGradientBrush(
                        Color.FromRgb(0x1A, 0x23, 0x7E),
                        Color.FromRgb(0x00, 0x96, 0x88), 45),
                    null, new Rect(0, 0, size, size));

                Brush circleBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                dc.DrawEllipse(circleBrush, null, new Point(180, 180), 150, 150);
                dc.DrawEllipse(circleBrush, null, new Point(80,  80),   70,  70);
                dc.DrawEllipse(circleBrush, null, new Point(290, 280),  60,  60);

                Pen starPen = new Pen(
                    new SolidColorBrush(Color.FromArgb(80, 255, 215, 0)), 3);
                double cx = 180, cy = 180, R = 130, r2 = 55;
                for (int k = 0; k < 5; k++)
                {
                    double a1 = Math.PI / 2 + k * 4 * Math.PI / 5;
                    double a2 = Math.PI / 2 + k * 4 * Math.PI / 5 + 2 * Math.PI / 5;
                    dc.DrawLine(starPen,
                        new Point(cx + R  * Math.Cos(a1), cy - R  * Math.Sin(a1)),
                        new Point(cx + r2 * Math.Cos(a2), cy - r2 * Math.Sin(a2)));
                }

                Typeface tf = new Typeface(new FontFamily("Arial"),
                    FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                FormattedText ft = new FormattedText(
                    "PUZZLE", CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, tf, 40, Brushes.White,
                    VisualTreeHelper.GetDpi(dv).PixelsPerDip);
                dc.DrawText(ft, new Point(cx - ft.Width / 2, cy - ft.Height / 2));

                Pen gridPen = new Pen(
                    new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)), 2);
                int cell = size / 3;
                for (int i = 1; i < 3; i++)
                {
                    dc.DrawLine(gridPen, new Point(i * cell, 0), new Point(i * cell, size));
                    dc.DrawLine(gridPen, new Point(0, i * cell), new Point(size, i * cell));
                }
            }
            rtb.Render(dv);
            return rtb;
        }

        int[] ShuffledIndices(int n)
        {
            int[] arr = new int[n];
            for (int i = 0; i < n; i++) arr[i] = i;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                int t = arr[i]; arr[i] = arr[j]; arr[j] = t;
            }
            return arr;
        }

        void Piece_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (dragging != null) return;
            FrameworkElement piece = (FrameworkElement)sender;
            int idx = (int)piece.Tag;
            if (placed[idx]) return;

            dragging = piece;
            dragOffset = e.GetPosition(piece);
            Canvas.SetZIndex(piece, 10);
            timer.Start();
            e.Handled = true;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            if (dragging == null) return;
            Point pos = Mouse.GetPosition(mainCanvas);
            Canvas.SetLeft(dragging, pos.X - dragOffset.X);
            Canvas.SetTop(dragging, pos.Y - dragOffset.Y);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (dragging == null) return;

            timer.Stop();
            FrameworkElement piece = dragging;
            dragging = null;

            int idx = (int)piece.Tag;
            if (placed[idx]) { Canvas.SetZIndex(piece, 2); return; }

            double tx = PUZZLE_LEFT + targetCol[idx] * PIECE_SIZE;
            double ty = PUZZLE_TOP  + targetRow[idx] * PIECE_SIZE;
            double cx = Canvas.GetLeft(piece) + PIECE_SIZE / 2;
            double cy = Canvas.GetTop(piece)  + PIECE_SIZE / 2;
            double dist = Math.Sqrt(
                Math.Pow(cx - (tx + PIECE_SIZE / 2), 2) +
                Math.Pow(cy - (ty + PIECE_SIZE / 2), 2));

            if (dist < SNAP_DIST)
            {
                Canvas.SetLeft(piece, tx);
                Canvas.SetTop(piece,  ty);
                Canvas.SetZIndex(piece, 2);
                placed[idx] = true;
                piece.Cursor = Cursors.Arrow;
                piece.Effect = null;
                Border border = piece as Border;
                if (border != null)
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47));
                placedCount++;
                if (placedCount == ROWS * COLS)
                    statusText.Text = "Пазл собран! Молодец!";
            }
            else
            {
                Canvas.SetZIndex(piece, 1);
            }
        }

        void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            BuildPuzzle();
        }
    }
}
