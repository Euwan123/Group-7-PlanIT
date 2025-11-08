using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PlanIT2
{
    public static class ControlExtensions
    {
      
        /// <param name="control">The control to apply rounded corners to</param>
        /// <param name="radius">The radius of the rounded corners</param>
        public static void SetRoundedCorners(this Control control, int radius)
        {
            if (control == null || radius <= 0)
                return;

            // Create the rounded rectangle path
            GraphicsPath path = GetRoundedRectPath(control.ClientRectangle, radius);
            control.Region = new Region(path);

            // Handle resize to update the region
            control.Resize += (s, e) =>
            {
                if (control.ClientRectangle.Width > 0 && control.ClientRectangle.Height > 0)
                {
                    GraphicsPath newPath = GetRoundedRectPath(control.ClientRectangle, radius);
                    control.Region = new Region(newPath);
                }
            };
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            // Ensure the diameter doesn't exceed the rectangle dimensions
            diameter = Math.Min(diameter, Math.Min(rect.Width, rect.Height));

            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top left corner
            path.AddArc(arc, 180, 90);

            // Top right corner
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right corner
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left corner
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}