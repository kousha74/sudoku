using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class ClassicSudoku: SquareSudoku 
    {
        //tbd replace with factory
        private static ClassicSudoku instance = null;

        private ClassicSudoku()
            :base(9)
        {
            init(createCliques());
        }

        public static ClassicSudoku Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ClassicSudoku();
                }
                return instance;
            }
        }

        public List<RawData> createCliques()
        {
            List<RawData> cliques = new List<RawData>();

            //every cell has a clique
            int row;
            int col;
            int number;

            for (row = 0; row < size; row++)
            {
                for (col = 0; col < size; col++)
                {
                    List<Condition> conditions = new List<Condition>();
                    for (number = 0; number < size; number++)
                    {
                        conditions.Add(boardConditions[row, col, number]);
                    }
                    cliques.Add(new RawData(conditions, "Cell(" + row.ToString() + "," + col.ToString() + ")" ));
                }
            }

            //every row has 9 cliques
            for (row = 0; row < size; row++)
            {
                for (number = 0; number < size; number++)
                {
                    List<Condition> conditions = new List<Condition>();
                    for (col = 0; col < size; col++)
                    {
                        conditions.Add(boardConditions[row, col, number]);
                    }
                    cliques.Add(new RawData(conditions, "Row: " + row.ToString() + ", Number: " + number.ToString()));
                }
            }

            //every column has 9 cliques
            for (col = 0; col < size; col++)
            {
                for (number = 0; number < size; number++)
                {
                    List<Condition> conditions = new List<Condition>();
                    for (row = 0; row < size; row++)
                    {
                        conditions.Add(boardConditions[row, col, number]);
                    }
                    cliques.Add(new RawData(conditions, "Col: " + col.ToString() + ", Number: " + number.ToString()));
                }
            }

            //every box has 9 cliques
            for (int boxRow = 0; boxRow < 3; boxRow++)
            {
                for (int boxCol = 0; boxCol < 3; boxCol++)
                {
                    for (number = 0; number < size; number++)
                    {
                        List<Condition> conditions = new List<Condition>();
                        foreach (Condition condition in getBoxConditions(boxRow, boxCol, number))
                        {
                            conditions.Add(condition);
                        }
                        cliques.Add(new RawData(conditions, "BOX(" + boxRow.ToString() + "," + boxCol.ToString() + "), Number: " + number.ToString()));
                    }
                }
            }

            return cliques;
        }

        public List<Condition> getBoxConditions(int boxRow, int boxCol, int number)
        {
            List<Condition> boxConditions = new List<Condition>();
            for (int row = boxRow*3; row < boxRow * 3 + 3; row++)
            {
                for (int col = boxCol * 3; col < boxCol * 3 + 3; col++)
                {
                    boxConditions.Add(boardConditions[row, col, number]);
                }
            }

            return boxConditions;
        }
    }
}
