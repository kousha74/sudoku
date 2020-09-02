using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    abstract class SquareSudoku: AbstractSudoku
    {
        protected readonly ConditionSquareSudoku[,,] boardConditions;
        protected int[,] initialNumbers;
        protected int[,] allNumbers;

        public SquareSudoku(int size)
            :base(size)
        {
            boardConditions = new ConditionSquareSudoku[size, size, size];

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    for (int number = 0; number < size; number++)
                    {
                        boardConditions[row, col, number] = new ConditionSquareSudoku(row, col, number);
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

        private void createConditions()
        {
            int row;
            int col;
            int number;

            for (row = 0; row < size; row++)
            {
                for (col = 0; col < size; col++)
                {
                    for (number = 0; number < size; number++)
                    {
                        boardConditions[row, col, number] = new ConditionSquareSudoku(row, col, number); 
                    }
                }
            }
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

        public Outcome addInitialNumber(int row, int col, int number)
        {
            initialNumbers[row, col] = number;
            allNumbers[row, col] = number;
            return setStatus(boardConditions[row, col, number - 1],Status.SATISFIED);
        }

        public string getNumbersString()
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

        public Outcome setInitialNumbers(List<int> numbers)
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

        public override void onConditionSatidfied(Condition condition)
        {
            ConditionSquareSudoku c = (ConditionSquareSudoku)condition;

            allNumbers[c.row, c.col] = c.number + 1;
        }

    }
}
