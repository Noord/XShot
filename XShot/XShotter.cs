using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace XShot
{
    public partial class XShotter : Form
    {
        private ScreenCapture capturer;
        private MouseTracker t;

        private Rectangle res; // resolution
        private Image activeImage;

        private DoubleBufferedPanel panel;
        private PictureBox zoom;

        private Pen redPen = new Pen(Color.Red, 2);
        private Pen yellowPen = new Pen(Color.Yellow, 1);

        private int imageCounter = 0;

        public XShotter()
        {
            InitializeComponent();

            capturer = new ScreenCapture();

            Hide();
            activeImage = capturer.CaptureScreen();
            Show();

            res = Screen.PrimaryScreen.Bounds;
            t = new MouseTracker();

            zoom = new Zoom();
            zoom.Size = new Size(300, 300);
            zoom.SizeMode = PictureBoxSizeMode.Zoom;
            zoom.Image = activeImage;

            panel = new DoubleBufferedPanel();
            panel.Size = Size;
            panel.BackgroundImage = activeImage;

            panel.Controls.Add(zoom);
            Controls.Add(panel);

            KeyDown += new KeyEventHandler(On_Key_Down);
            panel.MouseMove += new MouseEventHandler(On_Mouse_Move);
            panel.MouseDown += new MouseEventHandler(On_Mouse_Down);
            panel.MouseUp += new MouseEventHandler(On_Mouse_Up);
            panel.Paint += new PaintEventHandler(On_Paint);
        }

        public void On_Mouse_Down(object sender, MouseEventArgs e)
        {
            t.StartSelecting();
            panel.Invalidate();
        }

        public void On_Mouse_Up(object sender, MouseEventArgs e)
        {
            t.StopSelecting();
            var img = CropImage(activeImage, GetRectangle(t.Selection.Start, t.Selection.End));
            img.Save(String.Format("{0}.jpg", ++imageCounter), ImageFormat.Jpeg);
            panel.Invalidate();
        }

        public void On_Mouse_Move(object sender, MouseEventArgs e)
        {
            t.UpdateMouseMovement();
            if (t.IsMouseMoved())
                panel.Invalidate();
        }

        private void On_Paint(object sender, PaintEventArgs e)
        {
            zoom.Image = CropImage(activeImage, new Rectangle(t.Active.X, t.Active.Y, 30, 30));
            zoom.Location = GetZoomLoc();
            zoom.Refresh();

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;

            e.Graphics.DrawLine(redPen, new Point(t.Active.X, res.Top), new Point(t.Active.X, res.Bottom));
            e.Graphics.DrawLine(redPen, new Point(res.Left, t.Active.Y), new Point(res.Right, t.Active.Y));
            e.Graphics.DrawString(t.Active.X + " - " + t.Active.Y, Font, new SolidBrush(Color.Orange), t.Active.X + 10, t.Active.Y);

            if (t.Selection.Active)
                e.Graphics.DrawRectangle(yellowPen, GetRectangle(t.Selection.Start, t.Active));
        }

        private void On_Key_Down(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Application.Exit();
        }

        private static Rectangle GetRectangle(Point start, Point end)
        {
            return new Rectangle(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
        }

        private static Image CropImage(Image image, Rectangle r)
        {
            Bitmap old = new Bitmap(image);
            PixelFormat format = old.PixelFormat;
            return old.Clone(r, format);
        }

        private Point GetZoomLoc()
        {
            if (t.Active.X <= res.Width / 2 && t.Active.Y <= res.Height / 2)
                return new Point(t.Active.X + 10, t.Active.Y + 10);

            else if (t.Active.X <= res.Width / 2 && t.Active.Y > res.Height / 2)
                return new Point(t.Active.X + 10, t.Active.Y - 310);

            else if (t.Active.X > res.Width / 2 && t.Active.Y <= res.Height / 2)
                return new Point(t.Active.X - 310, t.Active.Y + 10);

            return new Point(t.Active.X - 310, t.Active.Y - 310);
        }
    }
}
