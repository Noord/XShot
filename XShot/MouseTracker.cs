using System.Drawing;
using System.Windows.Forms;

namespace XShot
{
    public class MouseTracker
    {
        public Point last;
        public Point active;

        public Selection selection { get; set; }

        public MouseTracker() { selection = new Selection(); }

        public void updateMouseMovement()
        {
            last = active;
            active = Cursor.Position;
        }

        public bool IsMouseMoved()
        {
            return active != last;
        }

        public void StartSelecting()
        {
            selection.active = true;
            selection.p1 = Cursor.Position;
        }

        public void StopSelecting()
        {
            selection.p2 = Cursor.Position;
            selection.active = false;
        }
    }

    public class Selection
    {
        public bool active { get; set; }
        public Point p1 { get; set; }
        public Point p2 { get; set; }
    }
}
