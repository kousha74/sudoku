﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Media;


namespace Sudoku
{
    public partial class Form1 : Form, BusinessLogic.AbstractSudoku.IAbstractSudoku
    {
        Sudoku S;

        private SoundPlayer SquarePlace;

        private SoundPlayer PuzzleSolved;

        private SoundPlayer PuzzleFine;

        private SoundPlayer MoveNo;

        private Communicator C;

        private BusinessLogic.AbstractSudoku abstractSudoku = null;

        public Form1()
        {
            InitializeComponent();

            abstractSudoku = new BusinessLogic.ClassicSudoku();
            S = new Sudoku(abstractSudoku);

            Application.DoEvents();

            //S.GenerateGame();

            LoadSounds();

            S.PlaySound += new Sudoku.SudokuEvent(S_PlaySound);

            S.RequestRepaint += new Sudoku.SudokuEvent2(S_RequestRepaint);

            C = new Communicator();

            C.StatusChange += new Communicator.MethodDelegate(C_StatusChange);

            abstractSudoku.setListener(this);
        }

        void C_StatusChange()
        {
            this.Text = C.Status;
        }

        void S_RequestRepaint()
        {
            pictureBox1.Invalidate();
        }

        private void LoadSounds()
        {

            SquarePlace = new SoundPlayer("square.wav");

            PuzzleSolved = new SoundPlayer("warm-interlude.wav");

            PuzzleFine = new SoundPlayer("fine.wav");

            MoveNo = new SoundPlayer("no.wav");

            Application.DoEvents();
            /*
            SquarePlace.Load();

            PuzzleSolved.Load();

            PuzzleFine.Load();

            MoveNo.Load();
             * */
        }

        void S_PlaySound(Sudoku.SudokuSound S)
        {
            /*
            switch (S)
            {
                case Sudoku.SudokuSound.Square:
                   // SquarePlace.Play();
                    break;
                case Sudoku.SudokuSound.Solved:
                    PuzzleSolved.Play();
                    break;
                case Sudoku.SudokuSound.No:
                    MoveNo.Play();
                    break;
                case Sudoku.SudokuSound.Fine:
                    PuzzleFine.Play();
                    break;
                case Sudoku.SudokuSound.Stop :
                    PuzzleSolved.Stop();
                    break;
            }
             */


        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            S.Draw(e.Graphics, 0);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                S.SetLocation(e.Location);

                pictureBox1.Invalidate();
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            S.KeyPress((char)e.KeyValue);

            if ((int)e.KeyValue == 27) S.Deselect();

            if ((int)e.KeyValue == 46) S.DeleteCurrentSquare();

            pictureBox1.Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void newToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            abstractSudoku.Reset();
            S.GenerateGame();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            S.RenderMessage("Sudoku 1.0", "Ghaith Tarawneh (gtarawneh@hotmail.com)", false);

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(abstractSudoku.getNumbersString());
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool result = S.SetGameString(Clipboard.GetText());

            if (!result)
            {
                Application.DoEvents();

                MessageBox.Show("The clipboard does not contain a valid puzzle.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void solveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BusinessLogic.Outcome outcome = abstractSudoku.solveStep();
            while (outcome == BusinessLogic.Outcome.UPDATED)
            {
                outcome = abstractSudoku.solveStep();

                if (outcome == BusinessLogic.Outcome.FAILED)
                {
                    MessageBox.Show("Solve Step Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            pictureBox1.Invalidate();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Insert(0,"Solving one step");
            S.SolveStep();
            
            pictureBox1.Invalidate();
        }

        private void checkSolvabilityToolStripMenuItem_Click(object sender, EventArgs e)
        {
                pictureBox1.Invalidate();
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";

            DialogResult R = openFileDialog1.ShowDialog();

            if (R != DialogResult.OK) return;

            bool result = S.LoadFile(openFileDialog1.FileName);

            if (!result)
            {
                Application.DoEvents();

                MessageBox.Show("Error loading puzzle file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {


        }

        private void checkSolutionsCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //S.RenderMessage(L.Count().ToString() + " possible deductions!", "Press any key to continue", false);
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult R = saveFileDialog1.ShowDialog();

            if (R != DialogResult.OK) return;

            bool result = S.SaveFile(saveFileDialog1.FileName);

            if (!result)
            {
                Application.DoEvents();

                MessageBox.Show("Error saving puzzle file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void Form1_Leave(object sender, EventArgs e)
        {

        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            pictureBox1.Invalidate();
        }

        private void menuStrip1_MouseHover(object sender, EventArgs e)
        {

        }

        private void menuStrip1_MouseEnter(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void menuStrip1_MouseLeave(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            pictureBox1.Invalidate();
        }

        public void onNewString(string str)
        {
            listBox1.Items.Insert(0, str);
        }

        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            int test = 1;
        }
    }
}
