using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Media;

namespace Sudoku
{
    class Sudoku
    {
        #region Class Decelerations

        BusinessLogic.AbstractSudoku abstractSudoku = null;

        private Random R;

        private float xOffset;

        private float yOffset;

        private float boardWidth;

        private Point hitPos = new Point(-1,-1);

        public int sw = 0;

        private DateTime starttime;

        private string duration;

        private string msg_text1;

        private string msg_text2;

        private bool DisplayMessage = false;

        private bool PriorityMessage = false;

        public enum SudokuSound
        {
            Stop = 0,
            Square = 1,
            No = 2,
            Fine = 3,
            Delete = 4,
            Solved = 5,
            NewPuzzle = 6,
        };

        public delegate void SudokuEvent(SudokuSound S);

        public delegate void SudokuEvent2();

        public event SudokuEvent PlaySound;

        public event SudokuEvent2 RequestRepaint;

        public bool ShowErrors = false;

        #endregion

        public Sudoku(BusinessLogic.AbstractSudoku abstractSudoku)
        {
            this.abstractSudoku = abstractSudoku;

            R = new Random();

            return;
        }

        public void SetLocation(Point Location)
        {
            if (DisplayMessage & !PriorityMessage)
            {
                DisplayMessage = false;
                return;
            }

            int new_hitx = (int)Math.Floor((Location.X - xOffset) / boardWidth * 9.0);

            int new_hity = (int)Math.Floor((Location.Y - yOffset) / boardWidth * 9.0);

            if (new_hitx < 0 || new_hitx > 8 || new_hity < 0 || new_hity > 8)
            {
                hitPos.X = -1;

                return;
            }

            if (new_hitx == hitPos.X && new_hity == hitPos.Y)
            {
                hitPos.X = -1;
            }
            else
            {
                //if (f[new_hity, new_hitx] == 0)//tbd
                {
                    hitPos.X = new_hitx;

                    hitPos.Y = new_hity;
                }
            }
        }

        public void Deselect()
        {
            hitPos.X = -1;
        }

        public void DeleteCurrentSquare()
        {
            if (hitPos.X != -1)
            {
            }

            hitPos.X = -1;
        }

        public void KeyPress(char KeyCode)
        {
            if (DisplayMessage & !PriorityMessage)
            {
                DisplayMessage = false;

                return;
            }

            if (KeyCode < '1' || KeyCode > '9') return;

            if (hitPos.X == -1) return;

            if (abstractSudoku.addInitialNumber(hitPos.Y, hitPos.X, KeyCode - '0') == BusinessLogic.Outcome.FAILED)
            {
                MessageBox.Show("ERROR!!!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            hitPos.X = -1;

            // ComputeErrors();

            if (PlaySound != null) PlaySound(SudokuSound.Square);

            //tbd
            /*if (isSolved())
            {
                while (hitx != -1) Application.DoEvents();

                Timer T = new Timer();

                T.Tick += new EventHandler(DisplayPuzzleSolved);

                T.Interval = 1500;

                T.Start();

                TimeSpan d = (DateTime.Now - starttime);

                duration = "";

                if (d.Minutes > 0) duration = d.Minutes.ToString() + " minute";

                if (d.Minutes > 1) duration += "s";

                if (d.Minutes > 0 && d.Seconds > 0) duration += " and ";

                if (d.Seconds > 0) duration += d.Seconds.ToString() + " second";

                if (d.Seconds > 1) duration += "s";

            }*/
        }

        public void GenerateGame()
        {
            if (PlaySound != null) PlaySound(SudokuSound.Stop);

            starttime = DateTime.Now;

            DisplayMessage = false;

            if (PlaySound != null) PlaySound(SudokuSound.NewPuzzle);

            if (RequestRepaint != null) RequestRepaint();

        }

        public void RenderMessage(string line1, string line2, bool priority)
        {
            if (!priority && DisplayMessage) return;

            msg_text1 = line1;
            msg_text2 = line2;
            PriorityMessage = priority;
            DisplayMessage = true;

            if (RequestRepaint  != null) RequestRepaint();
        }


        public string GetGameString()
        {
            string result = "";

            return result;
        }

        public bool SetGameString2(string game)
        {
            char[] spearator = { '\n', ' ', '\r' };
            string[] strs = game.Split(spearator, 81, StringSplitOptions.RemoveEmptyEntries);

            if (strs.Length != 81)
            {
                return false;
            }
            List<int> initialNumbers = new List<int>();

            for (int i = 0; i < 81;i++)
            {
                initialNumbers.Add(Convert.ToInt32(strs[i]));
            }
            if (abstractSudoku.setInitialNumbers(initialNumbers) == BusinessLogic.Outcome.FAILED)
            {
                MessageBox.Show("ERROR!!!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            DisplayMessage = false;

            return true;

        }

        public bool SetGameString(string game)
        {
            if (game.Length != 81) return SetGameString2(game);

            for (int n = 0; n < 81; n++)
            {
                char c = game[n];

                if ((c < '1' || c > '9') && c != 'X') return false;
            }

            List<int> initialNumbers = new List<int>();

            for (int n = 0; n < 81; n++)
            {
                string c = game.Substring(n, 1);

                int i = n / 9;

                int j = n % 9;

                if (c == "X")
                {
                    initialNumbers.Add(0);
                }
                else
                {
                    initialNumbers.Add(Convert.ToInt32(c));
                }
            }

            if (abstractSudoku.setInitialNumbers(initialNumbers) == BusinessLogic.Outcome.FAILED)
            {
                MessageBox.Show("ERROR!!!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            DisplayMessage = false;

            return true;

        }

        public void Draw(Graphics G, float angle)//tbd remove angle
        {
            if (abstractSudoku.sudokuType == BusinessLogic.AbstractSudoku.SudokuType.CLASSIC)
            {
                DrawClassicSudoku(G, angle, (BusinessLogic.ClassicSudoku)abstractSudoku );
            }
        }

        public void DrawClassicSudoku(Graphics G, float angle, BusinessLogic.ClassicSudoku classicSudoku)
        {
            Color Background = Color.DarkKhaki;

            Brush BG1 = new Pen(Color.Khaki).Brush;

            Brush BG2 = new Pen(Color.LightGoldenrodYellow).Brush;

            Brush BG;

            Brush Selected = new Pen(Color.BurlyWood).Brush;
            Brush highlightedBrush = new Pen(Color.Pink).Brush;

            Selected = new SolidBrush(Color.FromArgb(64, Color.RoyalBlue));

            Brush FontColor1 = Brushes.Black;

            Brush FontColor2 = Brushes.RoyalBlue;

            Brush FontColor3 = Brushes.Crimson;

            Brush FontColor = FontColor1;

            Pen Error = new Pen(Color.Red, 3);

            SolidBrush SmallFontColor = new SolidBrush(Color.FromArgb(200, Color.Black));

            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            G.Clear(Background);

            Pen Border1 = new Pen(Color.Black, 3);

            Pen Border2 = new Pen(Color.Black, 1);

            float w = G.VisibleClipBounds.Width;

            float h = G.VisibleClipBounds.Height;

            float min = Math.Min(w*(float)0.75, h);

            float centre_x = w / 2;

            float centre_y = h / 2;

            float startx = centre_x - min / 2;

            float starty = centre_y - min / 2;

            float m = 50;

            xOffset = startx + m;
            xOffset = w * (float)0.25 + m;

            yOffset = starty + m;

            boardWidth = min - 2 * m;
            float cellWidth = boardWidth / 9;

            if (boardWidth <= 0) return;

            float error_circle_m = boardWidth / 9 * 0.95f;

            Font F = new Font("Arial", boardWidth / 20);

            Font Fsmall = new Font("Arial", boardWidth / 72);

            G.TranslateTransform(centre_x, centre_y);

            G.RotateTransform(angle);

            G.TranslateTransform(-centre_x, -centre_y);

            G.FillRectangle(Brushes.White, xOffset, yOffset, boardWidth, boardWidth);

            G.DrawRectangle(Border1, xOffset, yOffset, boardWidth, boardWidth);

            if (sw == 5)
            {
                FontColor2 = FontColor1;
                FontColor3 = FontColor1;
            }

            string num = "";

            //drawing boxes

            for (float i = 0; i < 3; i++)
            {
                for (float j = 0; j < 3; j++)
                {
                    G.DrawRectangle(Border1, xOffset + boardWidth * i / 3, yOffset + boardWidth * j / 3, boardWidth / 3, boardWidth / 3);

                    if ((i + j) % 2 == 0) BG = BG1; else BG = BG2;

                    G.FillRectangle(BG, xOffset + boardWidth * i / 3, yOffset + boardWidth * j / 3, boardWidth / 3, boardWidth / 3);
                }

            }

            //highlighted Cells
            foreach (Point point in classicSudoku.highlightedCells)
            {
                G.FillRectangle(highlightedBrush, xOffset + cellWidth * point.X, yOffset + cellWidth * point.Y, cellWidth, cellWidth);
            }


            for (float i = 0; i < 9; i++)
            {
                for (float j = 0; j < 9; j++)
                {
                    if (i == hitPos.X && j == hitPos.Y) G.FillRectangle(Selected, xOffset + cellWidth * i, yOffset + cellWidth * j, cellWidth, cellWidth);

                    G.DrawRectangle(Border2, xOffset + cellWidth * i, yOffset + cellWidth*j, cellWidth, cellWidth);

                    int index_i = Convert.ToInt32(i);

                    int index_j = Convert.ToInt32(j);

                    num = "";

                    int number = classicSudoku.getInitialNumber(index_j, index_i);

                    if (number != 0)
                    {
                        num = number.ToString();
                        FontColor = FontColor1;
                    }
                    else
                    {
                        number = classicSudoku.getNumber(index_j, index_i);
                        if (number != 0)
                        {
                            num = number.ToString();
                            FontColor = FontColor2;
                        }
                    }

                    //num = classicSudoku.getInitialNumber(index_j, index_i).ToString();

                    //if (num == "0") num = "";

                    //if (f[index_j, index_i] == 1) FontColor = FontColor1;

                    //if (f[index_j, index_i] == 0) FontColor = FontColor2;

                    SizeF size_num = G.MeasureString(num, F);

                    float num_x = xOffset + cellWidth * i + (boardWidth / 9 - size_num.Width) / 2;

                    float num_y = yOffset + cellWidth * j + (boardWidth / 9 - size_num.Height) / 2;

                    // Rectangles: x for number, y for hints and z for error circle

                    RectangleF x = new RectangleF(num_x, num_y, size_num.Width, size_num.Height);

                    RectangleF y = new RectangleF(xOffset + cellWidth * i, yOffset + cellWidth * j , cellWidth, cellWidth);

                    RectangleF z = new RectangleF(xOffset + cellWidth * i + error_circle_m, yOffset + cellWidth * j + error_circle_m, boardWidth / 9 - 2 * error_circle_m, boardWidth / 9 - 2 * error_circle_m);

                    float num_centre_x = num_x + size_num.Width / 2;

                    float num_centre_y = num_y + size_num.Height / 2;

                    G.TranslateTransform(num_centre_x, num_centre_y);

                    G.RotateTransform(-angle);

                    G.TranslateTransform(-num_centre_x, -num_centre_y);

                    G.DrawString(num, F, FontColor, x);

                    G.TranslateTransform(num_centre_x, num_centre_y);

                    G.RotateTransform(+angle);

                    G.TranslateTransform(-num_centre_x, -num_centre_y);

                    //if (e[index_j, index_i] == 1 && ShowErrors) G.DrawEllipse(Error, z);

                    string hints = classicSudoku.getCellHints(index_j, index_i);

                    G.DrawString(hints, Fsmall, SmallFontColor, y);

                }
            }


            //if (solved == 1) ;

            if (DisplayMessage)
            {
                RenderMessageBox(G, msg_text1, msg_text2, boardWidth / 22, boardWidth / 36, w, h, xOffset, yOffset, boardWidth);
            }

            //RenderMessageBox(G, "14 Possible Deductions", "Press any key to continue", realw / 22, realw / 36, w, h, realx, realy, realw);

            //RenderMessageBox(G, "Everything's cool dude!", "Press any key to continue", realw / 22, realw / 36, w, h, realx, realy, realw);

        }

        private void RenderMessageBox(Graphics G, string txt1, string txt2, float emSize1, float emSize2, float w, float h, float realx, float realy, float realw)
        {
            Brush Overlay = new SolidBrush(Color.FromArgb(150, Color.White));

            Brush MsgBoxBG = new SolidBrush(Color.LightSlateGray);

            RectangleF msgbox = new RectangleF(realx + realw / 12, realy + realw / 4, realw * 10 / 12, realw / 3);

            Font F1 = new Font("Arial", emSize1);

            Font F2 = new Font("Arial", emSize2);

            SizeF txt1size = G.MeasureString(txt1, F1);

            SizeF txt2size = G.MeasureString(txt2, F2);

            G.FillRectangle(Overlay, new RectangleF(0, 0, w, h));

            G.FillRectangle(MsgBoxBG, msgbox);

            G.DrawRectangle(Pens.Black, realx + realw / 12, realy + realw / 4, realw * 10 / 12, realw / 3);

            PointF txt1pos = new PointF(msgbox.Left + (msgbox.Width - txt1size.Width) / 2, msgbox.Top + (msgbox.Height - txt1size.Height) / 2 - msgbox.Height / 5);

            PointF txt2pos = new PointF(msgbox.Left + (msgbox.Width - txt2size.Width) / 2, msgbox.Top + (msgbox.Height - txt2size.Height) / 2 + msgbox.Height / 5);

            G.DrawString(txt1, F1, Brushes.White, txt1pos);

            G.DrawString(txt2, F2, Brushes.White, txt2pos);

        }



        public bool SolveStep()
        {
            if (abstractSudoku.solveStep() == BusinessLogic.Outcome.FAILED)
            {
                MessageBox.Show("Solve Step Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return true;
        }

        public bool SolvePuzzle()
        {
            while (SolveStep()) ;

            return false;

        }

        public bool LoadFile(string file)
        {
            StreamReader R = new StreamReader(file);

            string content = R.ReadLine();

            R.Close();

            if (content.Length != 81) return false;

            return SetGameString(content);
        }

        public bool SaveFile(string file)
        {

            StreamWriter W = new StreamWriter(file);

            W.WriteLine(GetGameString());

            W.Close();

            return true;

        }


        public bool isSolved()
        {
            return false;

        }

        #region Private Functions

        #endregion

    }
}
