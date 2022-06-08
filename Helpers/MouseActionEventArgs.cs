using System.Drawing;

namespace BlazorApp1.Helpers
{
    public class MouseActionEventArgs:EventArgs
    {

        public MouseActionEventArgs(long id, TouchActionType type, Point location, bool isInContact)
        {
            Id = id;
            Type = type;
            Location = location;
            IsInContact = isInContact;
        }

        public long Id { private set; get; }

        public TouchActionType Type { private set; get; }

        public Point Location { private set; get; }

        public bool IsInContact { private set; get; }
    }

    public enum TouchActionType
    {
        Entered,
        Pressed,
        Moved,
        Released,
        Exited,
        Cancelled
    }
}
