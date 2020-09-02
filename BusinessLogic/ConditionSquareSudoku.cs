using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class ConditionSquareSudoku: Condition
    {
        public readonly int row;
        public readonly int col;
        public readonly int number;

        public ConditionSquareSudoku(int row, int col, int number)
        {
            this.row = row;
            this.col = col;
            this.number = number;
        }
    }
}
