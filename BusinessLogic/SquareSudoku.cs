﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    abstract class SquareSudoku: AbstractSudoku
    {
        protected readonly ConditionSquareSudoku[,,] boardConditions;
        protected int[,] initialNumbers;
        protected int[,] allNumbers;

        //for UI only
        public List<Point> highlightedCells = new List<Point>();

        public SquareSudoku(int size, AbstractSudoku.SudokuType sudokuType)
            :base(size, sudokuType)
        {
            boardConditions = new ConditionSquareSudoku[size, size, size];

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    for (int number = 0; number < size; number++)
                    {
                        boardConditions[row, col, number] = new ConditionSquareSudoku(row, col, number, (row+1).ToString() + (col+1).ToString() + (number+1).ToString());
                    }
                }
            }
            initialNumbers = new int[size, size];
            allNumbers = new int[size, size];

            Reset();
        }

        public override void Reset()
        {
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    initialNumbers[row, col] = 0;
                    allNumbers[row, col] = 0;
                }
            }
            base.Reset();
        }

        public int getInitialNumber(int row, int col)
        {
            return initialNumbers[row, col];
        }

        public int getNumber(int row, int col)
        {
            return allNumbers[row, col];
        }

        public void setNumber(int row, int col, int number)
        {
            allNumbers[row, col] = number;
        }

        public string getCellHints(int row, int col)
        {
            string hints = "";

            //no hints of the number is available
            if (getInitialNumber(row, col) == 0)
            {
                for (int i = 0; i < size; i++)
                {
                    if (getStatus(boardConditions[row, col, i]) == Status.UNDECIDED)
                    {
                        hints += (i + 1).ToString();
                    }
                }
            }

            return hints;
        }

        public override Outcome addInitialNumber(int row, int col, int number)
        {
            initialNumbers[row, col] = number;
            allNumbers[row, col] = number;
            return setStatus(boardConditions[row, col, number - 1],Status.SATISFIED);
        }

        public override string getNumbersString()
        {
            string numbersStr = "";
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (allNumbers[row, col] == 0)
                    {
                        numbersStr += "X";
                    }
                    else
                    {
                        numbersStr += allNumbers[row, col].ToString();
                    }
                }
            }
            return numbersStr;
        }

        public override Outcome setInitialNumbers(List<int> numbers)
        {
            Reset();

            int i;
            for (i = 0; i < numbers.Count; i++)
            {
                int row = i / 9;
                int col = i % 9;
                //initialNumbers[row, col] = numbers[i];
                //allNumbers[row, col] = numbers[i];

                if (numbers[i] != 0)
                {
                    if (addInitialNumber(row, col, numbers[i]) == Outcome.FAILED)
                    {
                        return Outcome.FAILED;
                    }
                }
            }

            return Outcome.UPDATED;
        }

        public override void onConditionChanged(Condition condition, Status status)
        {
            ConditionSquareSudoku c = (ConditionSquareSudoku)condition;

            switch (status)
            {
                case Status.SATISFIED:
                    allNumbers[c.row, c.col] = c.number + 1;
                    sendString("Cell(" + c.row.ToString() + "," + c.col.ToString() + "): is set to " + (c.number + 1).ToString());
                    highlightedCells.Clear();
                    highlightedCells.Add(new Point(c.col, c.row));
                    break;

                case Status.NOT_SATISFIED:
                    sendString("Cell(" + c.row.ToString() + "," + c.col.ToString() + "): cannot be " + (c.number + 1).ToString());
                    break;
            }
        }

        public override void onPairFound(List<Condition> conditions)
        {
            string str = "";
            highlightedCells.Clear();
            foreach (Condition condition in conditions)
            {
                ConditionSquareSudoku c = (ConditionSquareSudoku)condition;
                str +=  "Cell(" + c.row.ToString() + "," + c.col.ToString() + "),";
                highlightedCells.Add(new Point(c.col, c.row));
            }
            sendString(str);
        }



    }
}
