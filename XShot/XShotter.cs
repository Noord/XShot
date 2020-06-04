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
            var img = CropImage(activeImage, GetRectangle(t.selection.p1, t.selection.p2));
            img.Save(String.Format("{0}.jpg", ++imageCounter), ImageFormat.Jpeg);
            panel.Invalidate();
        }

        public void On_Mouse_Move(object sender, MouseEventArgs e)
        {
            t.updateMouseMovement();
            if (t.IsMouseMoved())
                panel.Invalidate();
        }

        private void On_Paint(object sender, PaintEventArgs e)
        {
            zoom.Image = CropImage(activeImage, new Rectangle(t.active.X, t.active.Y, 30, 30));
            zoom.Location = GetZoomLoc();
            zoom.Refresh(); // picturebox i yenilemek icin

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;

            e.Graphics.DrawLine(redPen, new Point(t.active.X, res.Top), new Point(t.active.X, res.Bottom));
            e.Graphics.DrawLine(redPen, new Point(res.Left, t.active.Y), new Point(res.Right, t.active.Y));
            e.Graphics.DrawString(t.active.X + " - " + t.active.Y, Font, new SolidBrush(Color.Orange), t.active.X + 10, t.active.Y);

            if (t.selection.active)
                e.Graphics.DrawRectangle(yellowPen, GetRectangle(t.selection.p1, t.active));
        }

        private void On_Key_Down(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) // esc ye basinca cik
                Application.Exit();
        }

        private static Rectangle GetRectangle(Point start, Point end)
        {
            return new Rectangle(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
        }

        private static Image CropImage(Image image, Rectangle r) // resmin icinden bir kisim dikdortgeni kopyala
        {
            Bitmap old = new Bitmap(image);
            PixelFormat format = old.PixelFormat;
            return old.Clone(r, format);
        }

        private Point GetZoomLoc()
        {
            if (t.active.X <= res.Width / 2 && t.active.Y <= res.Height / 2)
                return new Point(t.active.X + 10, t.active.Y + 10);

            else if (t.active.X <= res.Width / 2 && t.active.Y > res.Height / 2)
                return new Point(t.active.X + 10, t.active.Y - 310);

            else if (t.active.X > res.Width / 2 && t.active.Y <= res.Height / 2)
                return new Point(t.active.X - 310, t.active.Y + 10);

            return new Point(t.active.X - 310, t.active.Y - 310);
        }
    }
}
