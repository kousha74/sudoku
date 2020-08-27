using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class ClassicSudoku: AbstractSudoku 
    {
        //tbd replace with factory
        private static ClassicSudoku instance = null;

        private ClassicSudoku()
            :base(9)
        {

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

        public override List<Clique> createCliques()
        {
            List<Clique> cliques = new List<Clique>();

            //every cell has a clique
            int row;
            int col;
            int number;

            for (row = 0; row < size; row++)
            {
                for (col = 0; col < size; col++)
                {
                    Clique clique = new Clique(size);
                    for (number = 0; number < size; number++)
                    {
                        clique.addCondition(conditions[row, col, number]);
                    }
                    cliques.Add(clique);
                }
            }

            //every row has 9 cliques
            for (row = 0; row < size; row++)
            {
                for (number = 0; number < size; number++)
                {
                    Clique clique = new Clique(size);
                    for (col = 0; col < size; col++)
                    {
                        clique.addCondition(conditions[row, col, number]);
                    }
                    cliques.Add(clique);
                }
            }

            //every column has 9 cliques
            for (col = 0; col < size; col++)
            {
                for (number = 0; number < size; number++)
                {
                    Clique clique = new Clique(size);
                    for (row = 0; row < size; row++)
                    {
                        clique.addCondition(conditions[row, col, number]);
                    }
                    cliques.Add(clique);
                }
            }

            //every box has 9 cliques
            for (int boxRow = 0; boxRow < 3; boxRow++)
            {
                for (int boxCol = 0; boxCol < 3; boxCol++)
                {
                    for (number = 0; number < size; number++)
                    {
                        Clique clique = new Clique(size);

                        foreach(Condition condition in getBoxConditions(boxRow, boxCol, number))
                        {
                            clique.addCondition(condition);
                        }
                        cliques.Add(clique);
                    }
                }
            }

            return cliques;
        }

        public override bool areNeighbors(Condition condition1, Condition condition2)
        {
            if (condition1.number != condition2.number)
            {
                if ((condition1.row == condition2.row) && (condition1.col == condition2.col))
                {
                    return true;
                }
            }
            else
            {
                if (condition1.row == condition2.row)
                {
                    return true;
                }

                if (condition1.col == condition2.col)
                {
                    return true;
                }

                if ((getBoxRow(condition1) == getBoxRow(condition2)) && (getBoxCol(condition1) == getBoxCol(condition2)))
                {
                    return true;
                }
            }

            return false;
        }

        public int getBoxRow(Condition condition)
        {
            return condition.row / 3;
        }

        public int getBoxCol(Condition condition)
        {
            return condition.col / 3;
        }

        public List<Condition> getBoxConditions(int boxRow, int boxCol, int number)
        {
            List<Condition> boxConditions = new List<Condition>();
            for (int row = boxRow*3; row < boxRow * 3 + 3; row++)
            {
                for (int col = boxCol * 3; col < boxCol * 3 + 3; col++)
                {
                    boxConditions.Add(conditions[row, col, number]);
                }
            }

            return boxConditions;
        }

        public bool solvePuzzle(List<int> numbers)
        {
            setInitialNumbers(numbers);            
            return false;
        }
    }
}
