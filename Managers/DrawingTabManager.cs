using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlanIT2.TabTypes
{
    public class DrawingTabManager
    {
        private Action MarkUnsaved;
        private bool isDarkMode;

        private bool isDrawing = false;
        private Point lastPoint = Point.Empty;
        private Color penColor = Color.Black;
        private Pen currentPen = new Pen(Color.Black, 2);
        private bool isEraserMode = false;
        private int penSize = 2;

        // Shape drawing
        private DrawMode currentMode = DrawMode.Pen;
        private ShapeType currentShape = ShapeType.Rectangle;
        private LineType currentLineType = LineType.Line;
        private Point shapeStartPoint = Point.Empty;
        private Bitmap tempBitmap;

        // Text tool
        private List<DraggableText> draggableTexts = new List<DraggableText>();
        private DraggableText selectedText = null;
        private Point textDragStart;
        private int textSize = 12;

        private Bitmap drawingBitmap;
        private PictureBox drawPanel;
        private Stack<DrawingState> undoStack = new Stack<DrawingState>();
        private Stack<DrawingState> redoStack = new Stack<DrawingState>();
        private List<DrawableImage> drawableImages = new List<DrawableImage>();
        private DrawableImage selectedImage = null;
        private Point dragImageStartPoint;
        private TrackBar sizeTrackBar;
        private Label sizeLabel;
        private ToolStripButton eraserBtn;

        // Color palette
        private Color[] colorPalette = new Color[]
        {
            Color.Black, Color.White, Color.Red, Color.Green, Color.Blue,
            Color.Yellow, Color.Orange, Color.Purple, Color.Pink, Color.Brown
        };

        public enum DrawMode
        {
            Pen,
            Shape,
            Line,
            Text
        }

        public enum ShapeType
        {
            Rectangle,
            Ellipse
        }

        public enum LineType
        {
            Line,
            Arrow
        }

        private class DrawingState
        {
            public Bitmap Bitmap;
            public List<DrawableImage> Images;
            public List<DraggableText> Texts;

            public DrawingState Clone()
            {
                return new DrawingState
                {
                    Bitmap = (Bitmap)Bitmap.Clone(),
                    Images = new List<DrawableImage>(Images.Select(img => new DrawableImage
                    {
                        Image = img.Image,
                        Bounds = img.Bounds
                    })),
                    Texts = new List<DraggableText>(Texts.Select(txt => new DraggableText
                    {
                        Text = txt.Text,
                        Location = txt.Location,
                        Font = (Font)txt.Font.Clone(),
                        Color = txt.Color
                    }))
                };
            }
        }

        private class DraggableText
        {
            public string Text;
            public Point Location;
            public Font Font;
            public Color Color;
            public Rectangle Bounds
            {
                get
                {
                    using (var g = Graphics.FromImage(new Bitmap(1, 1)))
                    {
                        var size = g.MeasureString(Text, Font);
                        return new Rectangle(Location, new Size((int)size.Width, (int)size.Height));
                    }
                }
            }
        }

        public DrawingTabManager(Action markUnsaved, bool darkMode)
        {
            MarkUnsaved = markUnsaved;
            isDarkMode = darkMode;
        }

        public TabPage CreateDrawingTab(string name, string content = null)
        {
            TabPage tab = new TabPage(name);
            tab.SetRoundedCorners(15);
            tab.BackColor = Color.FromArgb(255, 255, 230);

            var panel = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(5) };
            panel.SetRoundedCorners(15);

            drawPanel = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Cross
            };
            drawPanel.MouseDown += DrawPanel_MouseDown;
            drawPanel.MouseMove += DrawPanel_MouseMove;
            drawPanel.MouseUp += DrawPanel_MouseUp;
            drawPanel.Paint += DrawPanel_Paint;
            drawPanel.SizeChanged += (s, e) =>
            {
                if (drawPanel.Width > 0 && drawPanel.Height > 0)
                {
                    if (drawingBitmap == null || drawingBitmap.Width != drawPanel.Width || drawingBitmap.Height != drawPanel.Height)
                    {
                        var newBitmap = new Bitmap(drawPanel.Width, drawPanel.Height);
                        if (drawingBitmap != null)
                        {
                            using (var g = Graphics.FromImage(newBitmap))
                            {
                                g.DrawImage(drawingBitmap, 0, 0, drawPanel.Width, drawPanel.Height);
                            }
                            drawingBitmap.Dispose();
                        }
                        drawingBitmap = newBitmap;
                        drawPanel.Image = drawingBitmap;
                    }
                }
            };
            panel.Controls.Add(drawPanel);

            var toolStrip = CreateDrawToolStrip();
            var panelTable = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            panelTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            panelTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panelTable.Controls.Add(toolStrip, 0, 0);
            panelTable.Controls.Add(panel, 0, 1);
            tab.Controls.Add(panelTable);

            if (content != null)
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(content);
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        drawingBitmap = new Bitmap(ms);
                        drawPanel.Image = drawingBitmap;
                    }
                }
                catch
                {
                    drawingBitmap = new Bitmap(drawPanel.Width > 0 ? drawPanel.Width : 800,
                                               drawPanel.Height > 0 ? drawPanel.Height : 600);
                    drawPanel.Image = drawingBitmap;
                }
            }
            else
            {
                drawingBitmap = new Bitmap(drawPanel.Width > 0 ? drawPanel.Width : 800,
                                          drawPanel.Height > 0 ? drawPanel.Height : 600);
                drawPanel.Image = drawingBitmap;
            }

            SaveState();
            return tab;
        }

        private void SaveState()
        {
            var state = new DrawingState
            {
                Bitmap = (Bitmap)drawingBitmap.Clone(),
                Images = new List<DrawableImage>(drawableImages.Select(img => new DrawableImage
                {
                    Image = img.Image,
                    Bounds = img.Bounds
                })),
                Texts = new List<DraggableText>(draggableTexts.Select(txt => new DraggableText
                {
                    Text = txt.Text,
                    Location = txt.Location,
                    Font = (Font)txt.Font.Clone(),
                    Color = txt.Color
                }))
            };
            undoStack.Push(state);
            redoStack.Clear();
        }

        private void DrawPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (currentMode == DrawMode.Text)
                {
                    selectedText = draggableTexts.FirstOrDefault(txt => txt.Bounds.Contains(e.Location));
                    if (selectedText != null)
                    {
                        textDragStart = e.Location;
                    }
                    else
                    {
                        AddTextAtPoint(e.Location);
                    }
                    return;
                }

                selectedImage = drawableImages.FirstOrDefault(img => img.Bounds.Contains(e.Location));
                if (selectedImage != null)
                {
                    dragImageStartPoint = e.Location;
                }
                else
                {
                    if (currentMode == DrawMode.Pen || isEraserMode)
                    {
                        isDrawing = true;
                        lastPoint = e.Location;
                    }
                    else
                    {
                        // Shape drawing modes
                        isDrawing = true;
                        shapeStartPoint = e.Location;
                        tempBitmap = (Bitmap)drawingBitmap.Clone();
                    }
                }
            }
        }

        private void DrawPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedText != null)
            {
                int dx = e.X - textDragStart.X;
                int dy = e.Y - textDragStart.Y;
                selectedText.Location = new Point(selectedText.Location.X + dx, selectedText.Location.Y + dy);
                textDragStart = e.Location;
                drawPanel.Invalidate();
            }
            else if (selectedImage != null)
            {
                int dx = e.X - dragImageStartPoint.X;
                int dy = e.Y - dragImageStartPoint.Y;
                selectedImage.Bounds = new Rectangle(
                    selectedImage.Bounds.X + dx,
                    selectedImage.Bounds.Y + dy,
                    selectedImage.Bounds.Width,
                    selectedImage.Bounds.Height);
                dragImageStartPoint = e.Location;
                drawPanel.Invalidate();
            }
            else if (isDrawing)
            {
                if (currentMode == DrawMode.Pen || isEraserMode)
                {
                    if (lastPoint != Point.Empty)
                    {
                        using (Graphics g = Graphics.FromImage(drawingBitmap))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            currentPen.Color = isEraserMode ? drawPanel.BackColor : penColor;
                            currentPen.Width = penSize;
                            g.DrawLine(currentPen, lastPoint, e.Location);
                        }
                        drawPanel.Invalidate();
                        lastPoint = e.Location;
                    }
                }
                else
                {
                    // Preview shape drawing
                    drawingBitmap = (Bitmap)tempBitmap.Clone();
                    using (Graphics g = Graphics.FromImage(drawingBitmap))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        currentPen.Width = penSize;
                        currentPen.Color = penColor;

                        if (currentMode == DrawMode.Line)
                        {
                            if (currentLineType == LineType.Line)
                                g.DrawLine(currentPen, shapeStartPoint, e.Location);
                            else
                                DrawArrow(g, shapeStartPoint, e.Location);
                        }
                        else if (currentMode == DrawMode.Shape)
                        {
                            if (currentShape == ShapeType.Rectangle)
                                DrawRectangle(g, shapeStartPoint, e.Location);
                            else
                                DrawEllipse(g, shapeStartPoint, e.Location);
                        }
                    }
                    drawPanel.Invalidate();
                }
            }
        }

        private void DrawPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;
                lastPoint = Point.Empty;
                SaveState();
                MarkUnsaved();

                if (tempBitmap != null)
                {
                    tempBitmap.Dispose();
                    tempBitmap = null;
                }
            }
            if (selectedImage != null)
            {
                SaveState();
                MarkUnsaved();
            }
            if (selectedText != null)
            {
                SaveState();
                MarkUnsaved();
            }
            selectedImage = null;
            selectedText = null;
        }

        private void DrawPanel_Paint(object sender, PaintEventArgs e)
        {
            if (drawingBitmap != null)
            {
                e.Graphics.DrawImage(drawingBitmap, 0, 0);
            }
            foreach (var dImage in drawableImages)
            {
                e.Graphics.DrawImage(dImage.Image, dImage.Bounds);
            }
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            foreach (var txt in draggableTexts)
            {
                using (Brush brush = new SolidBrush(txt.Color))
                {
                    e.Graphics.DrawString(txt.Text, txt.Font, brush, txt.Location);
                }
            }
        }

        private void DrawRectangle(Graphics g, Point start, Point end)
        {
            int x = Math.Min(start.X, end.X);
            int y = Math.Min(start.Y, end.Y);
            int width = Math.Abs(end.X - start.X);
            int height = Math.Abs(end.Y - start.Y);
            g.DrawRectangle(currentPen, x, y, width, height);
        }

        private void DrawEllipse(Graphics g, Point start, Point end)
        {
            int x = Math.Min(start.X, end.X);
            int y = Math.Min(start.Y, end.Y);
            int width = Math.Abs(end.X - start.X);
            int height = Math.Abs(end.Y - start.Y);
            g.DrawEllipse(currentPen, x, y, width, height);
        }

        private void DrawArrow(Graphics g, Point start, Point end)
        {
            g.DrawLine(currentPen, start, end);

            double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
            int arrowSize = penSize * 3 + 5;

            Point arrow1 = new Point(
                (int)(end.X - arrowSize * Math.Cos(angle - Math.PI / 6)),
                (int)(end.Y - arrowSize * Math.Sin(angle - Math.PI / 6))
            );
            Point arrow2 = new Point(
                (int)(end.X - arrowSize * Math.Cos(angle + Math.PI / 6)),
                (int)(end.Y - arrowSize * Math.Sin(angle + Math.PI / 6))
            );

            g.DrawLine(currentPen, end, arrow1);
            g.DrawLine(currentPen, end, arrow2);
        }

        private void AddTextAtPoint(Point location)
        {
            using (var inputForm = new Form())
            {
                inputForm.Text = "Enter Text";
                inputForm.Width = 350;
                inputForm.Height = 150;
                inputForm.StartPosition = FormStartPosition.CenterParent;

                var textBox = new TextBox
                {
                    Location = new Point(10, 10),
                    Width = 310,
                    Height = 40,
                    Multiline = true
                };

                var okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(160, 60),
                    Width = 75
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(245, 60),
                    Width = 75
                };

                inputForm.Controls.AddRange(new Control[] { textBox, okButton, cancelButton });
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;

                if (inputForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    var newText = new DraggableText
                    {
                        Text = textBox.Text,
                        Location = location,
                        Font = new Font("Arial", textSize, FontStyle.Regular),
                        Color = penColor
                    };
                    draggableTexts.Add(newText);
                    drawPanel.Invalidate();
                    SaveState();
                    MarkUnsaved();
                }
            }
        }

        private void AddDrawImage(string filePath)
        {
            try
            {
                var image = Image.FromFile(filePath);
                var newImage = new DrawableImage
                {
                    Image = image,
                    Bounds = new Rectangle(0, 0, image.Width / 2, image.Height / 2)
                };
                drawableImages.Add(newImage);
                drawPanel.Invalidate();
                SaveState();
                MarkUnsaved();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UndoDraw()
        {
            if (undoStack.Count > 1)
            {
                var currentState = undoStack.Pop();
                redoStack.Push(currentState);

                var previousState = undoStack.Peek();
                RestoreState(previousState);
            }
        }

        private void RedoDraw()
        {
            if (redoStack.Count > 0)
            {
                var state = redoStack.Pop();
                undoStack.Push(state);
                RestoreState(state);
            }
        }

        private void RestoreState(DrawingState state)
        {
            drawingBitmap = (Bitmap)state.Bitmap.Clone();
            drawPanel.Image = drawingBitmap;

            drawableImages.Clear();
            foreach (var img in state.Images)
            {
                drawableImages.Add(new DrawableImage
                {
                    Image = img.Image,
                    Bounds = img.Bounds
                });
            }

            draggableTexts.Clear();
            foreach (var txt in state.Texts)
            {
                draggableTexts.Add(new DraggableText
                {
                    Text = txt.Text,
                    Location = txt.Location,
                    Font = (Font)txt.Font.Clone(),
                    Color = txt.Color
                });
            }

            drawPanel.Invalidate();
        }

        private ToolStrip CreateDrawToolStrip()
        {
            var toolStrip = new ToolStrip();
            toolStrip.BackColor = Color.LightGray;

            // Drawing mode buttons
            ToolStripButton penBtn = new ToolStripButton("✏️ Pen");
            ToolStripDropDownButton shapeBtn = new ToolStripDropDownButton("⬜ Shape");
            ToolStripDropDownButton lineBtn = new ToolStripDropDownButton("📏 Line");
            ToolStripButton textBtn = new ToolStripButton("📝 Text");

            void UpdateModeButtons()
            {
                penBtn.Checked = currentMode == DrawMode.Pen && !isEraserMode;
                shapeBtn.BackColor = currentMode == DrawMode.Shape ? Color.LightBlue : Color.Transparent;
                lineBtn.BackColor = currentMode == DrawMode.Line ? Color.LightBlue : Color.Transparent;
                textBtn.Checked = currentMode == DrawMode.Text;
                if (eraserBtn != null)
                    eraserBtn.Checked = isEraserMode;
            }

            penBtn.CheckOnClick = true;
            penBtn.Checked = true;
            penBtn.Click += (s, e) => { currentMode = DrawMode.Pen; isEraserMode = false; UpdateModeButtons(); };

            // Shape submenu
            var rectItem = new ToolStripMenuItem("Rectangle");
            rectItem.Click += (s, e) => { currentMode = DrawMode.Shape; currentShape = ShapeType.Rectangle; isEraserMode = false; shapeBtn.Text = "⬜ Rect"; UpdateModeButtons(); };
            var ellipseItem = new ToolStripMenuItem("Ellipse");
            ellipseItem.Click += (s, e) => { currentMode = DrawMode.Shape; currentShape = ShapeType.Ellipse; isEraserMode = false; shapeBtn.Text = "⭕ Ellipse"; UpdateModeButtons(); };
            shapeBtn.DropDownItems.Add(rectItem);
            shapeBtn.DropDownItems.Add(ellipseItem);

            // Line submenu
            var straightLineItem = new ToolStripMenuItem("Straight Line");
            straightLineItem.Click += (s, e) => { currentMode = DrawMode.Line; currentLineType = LineType.Line; isEraserMode = false; lineBtn.Text = "📏 Line"; UpdateModeButtons(); };
            var arrowItem = new ToolStripMenuItem("Arrow");
            arrowItem.Click += (s, e) => { currentMode = DrawMode.Line; currentLineType = LineType.Arrow; isEraserMode = false; lineBtn.Text = "➡️ Arrow"; UpdateModeButtons(); };
            lineBtn.DropDownItems.Add(straightLineItem);
            lineBtn.DropDownItems.Add(arrowItem);

            textBtn.CheckOnClick = true;
            textBtn.Click += (s, e) => { currentMode = DrawMode.Text; isEraserMode = false; UpdateModeButtons(); };

            toolStrip.Items.Add(penBtn);
            toolStrip.Items.Add(shapeBtn);
            toolStrip.Items.Add(lineBtn);
            toolStrip.Items.Add(textBtn);
            toolStrip.Items.Add(new ToolStripSeparator());

            // Eraser
            eraserBtn = new ToolStripButton("🧹 Eraser");
            eraserBtn.CheckOnClick = true;
            eraserBtn.Click += (s, e) =>
            {
                isEraserMode = eraserBtn.Checked;
                if (isEraserMode)
                {
                    currentMode = DrawMode.Pen;
                }
                UpdateModeButtons();
            };
            toolStrip.Items.Add(eraserBtn);
            toolStrip.Items.Add(new ToolStripSeparator());

            // Size control
            sizeLabel = new Label
            {
                Text = $"Size: {penSize}",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(2)
            };

            sizeTrackBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 20,
                Value = penSize,
                Width = 100,
                Height = 20
            };
            sizeTrackBar.ValueChanged += (s, e) =>
            {
                penSize = sizeTrackBar.Value;
                textSize = penSize + 8;
                currentPen.Width = penSize;
                sizeLabel.Text = $"Size: {penSize}";
            };

            toolStrip.Items.Add(new ToolStripControlHost(sizeLabel));
            toolStrip.Items.Add(new ToolStripControlHost(sizeTrackBar));
            toolStrip.Items.Add(new ToolStripSeparator());

            // Color selection
            var penColorBtn = new ToolStripButton("🎨 Color", null, (s, e) =>
            {
                using (ColorDialog cd = new ColorDialog())
                {
                    cd.Color = penColor;
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        penColor = cd.Color;
                        currentPen.Color = penColor;
                    }
                }
            });

            toolStrip.Items.Add(penColorBtn);

            // Quick color palette
            foreach (var color in colorPalette)
            {
                var colorBtn = new ToolStripButton();
                colorBtn.BackColor = color;
                colorBtn.Width = 20;
                colorBtn.Click += (s, e) =>
                {
                    penColor = color;
                    currentPen.Color = penColor;
                };
                toolStrip.Items.Add(colorBtn);
            }

            toolStrip.Items.Add(new ToolStripSeparator());

            // Undo/Redo
            var undoDrawBtn = new ToolStripButton("↶ Undo", null, (s, e) => UndoDraw());
            var redoDrawBtn = new ToolStripButton("↷ Redo", null, (s, e) => RedoDraw());
            toolStrip.Items.Add(undoDrawBtn);
            toolStrip.Items.Add(redoDrawBtn);

            toolStrip.Items.Add(new ToolStripSeparator());

            // Insert picture
            var insertPicBtn = new ToolStripButton("🖼️ Insert", null, (s, e) =>
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    AddDrawImage(openFile.FileName);
                }
            });
            toolStrip.Items.Add(insertPicBtn);

            // Clear canvas
            var clearBtn = new ToolStripButton("🗑️ Clear", null, (s, e) =>
            {
                var result = MessageBox.Show("Are you sure you want to clear the canvas?",
                    "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    using (Graphics g = Graphics.FromImage(drawingBitmap))
                    {
                        g.Clear(drawPanel.BackColor);
                    }
                    drawableImages.Clear();
                    draggableTexts.Clear();
                    drawPanel.Invalidate();
                    SaveState();
                    MarkUnsaved();
                }
            });
            toolStrip.Items.Add(clearBtn);

            return toolStrip;
        }

        public string GetContent(TabPage tab)
        {
            // Create a composite image with all elements
            var compositeBitmap = new Bitmap(drawingBitmap.Width, drawingBitmap.Height);
            using (Graphics g = Graphics.FromImage(compositeBitmap))
            {
                g.DrawImage(drawingBitmap, 0, 0);

                foreach (var dImage in drawableImages)
                {
                    g.DrawImage(dImage.Image, dImage.Bounds);
                }

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                foreach (var txt in draggableTexts)
                {
                    using (Brush brush = new SolidBrush(txt.Color))
                    {
                        g.DrawString(txt.Text, txt.Font, brush, txt.Location);
                    }
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                compositeBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}