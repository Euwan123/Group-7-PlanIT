using System;
using System.Drawing;

namespace PlanIT2.TabTypes
{
    public class TabData
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public Color TabColor { get; set; }
        public string AiHistory { get; set; }
    }

    public class DrawableImage
    {
        public Image Image { get; set; }
        public Rectangle Bounds { get; set; }
    }
}