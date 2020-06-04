using System.Drawing;
using System.Windows.Forms;

namespace XShot
{
    public class MouseTracker
    {
        public Point Last { get; set; }
        public Point Active { get; set; }

        public Selection Selection { get; set; }

        public MouseTracker() { Selection = new Selection(); }

        public void UpdateMouseMovement()
        {
            Last = Active;
            Active = Cursor.Position;
        }

        public bool IsMouseMoved()
        {
            return Active != Last;
        }

        public void StartSelecting()
        {
            Selection.Active = true;
            Selection.Start = Cursor.Position;
        }

        public void StopSelecting()
        {
            Selection.End = Cursor.Position;
            Selection.Active = false;
        }
    }

    public class Selection
    {
        public bool Active { get; set; }
        public Point Start { get; set; }
        public Point End { get; set; }
    }
}
