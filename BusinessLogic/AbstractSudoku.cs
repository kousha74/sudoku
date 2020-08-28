using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    abstract class AbstractSudoku
    {
        protected readonly Condition[,,] conditions;
        protected readonly List<Condition> conditionList = new List<Condition>();
        protected readonly List<Clique> cliques;
        protected int[,] initialNumbers;
        protected int[,] allNumbers;

        public readonly int size;
        public readonly int[] bitCounter;

        public AbstractSudoku(int size)
        {
            int allCombinations = 2 ^ size;
            bitCounter = new int[allCombinations];
            for (ushort i = 0; i <allCombinations; i++)
            {
                bitCounter[i] = Bits.countSetBits(i);
            }

            this.size = size;
            conditions = new Condition[size, size, size];
            createConditions();
            cliques = createCliques();

            //logging all the cliques
            foreach (Clique clique in cliques)
            {
                Logger.Instance.WriteLine(Logger.LogLevel.ERROR, clique.ToString());
            }


            //assing cliques to conditions
            foreach(Clique clique in cliques)
            {
                foreach(Condition condition in clique.conditions)
                {
                    condition.addClique(clique);
                }
            }

            //cliques with overlap
            int count = cliques.Count;

            for (int first = 0; first < count; first++)
            {
                for (int second = first + 1; second < count; second++)
                {
                    if (cliques[first].getCommonConditionCount(cliques[second]) > 1)
                    {
                        cliques[first].addClique(cliques[second]);
                        cliques[second].addClique(cliques[first]);
                    }

                    if (cliques[first].isSibling(cliques[second]))
                    {
                        cliques[first].addSibling(cliques[second]);
                        cliques[second].addSibling(cliques[first]);
                    }
                }
            }

            initialNumbers = new int[size, size];
            allNumbers = new int[size, size];

            reset();
        }

        public abstract List<Clique> createCliques();

        public abstract bool areNeighbors(Condition condition1, Condition condition2);

        private void createConditions()
        {
            int row;
            int col;
            int number;

            for (row = 0; row < size; row++)
            {
                for (col = 0; col <size; col++)
                {
                    for (number = 0; number < size; number++)
                    {
                        Condition condition = new Condition(row, col, number);
                        conditions[row, col, number] = condition;
                        conditionList.Add(condition);
                    }
                } 
            }

            //neighboring conditions
            int count = conditionList.Count;
            Condition condition1;
            Condition condition2;

            for (int first = 0; first < count; first++)
            {
                condition1 = conditionList[first];

                for (int second = first+1; second < count; second++)
                {
                    condition2 = conditionList[second];

                    if (areNeighbors(condition1, condition2))
                    {
                        condition1.addNeighbor(condition2);
                        condition2.addNeighbor(condition1);
                    }
                }
            }
        }

        public void reset()
        {
            foreach (Condition condition in conditionList)
            {
                condition.reset();
            }

            foreach(Clique clique in cliques)
            {
                clique.reset();
            }

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    initialNumbers[row, col] = 0;
                    allNumbers[row, col] = 0;
                }
            }
        }

        public int getInitialNumber(int row, int col)
        {
            return initialNumbers[row,col];
        }

        public int getNumber(int row, int col)
        {
            return allNumbers[row,col];
        }

        public void setNumber(int row, int col, int number)
        {
            allNumbers[row,col] = number;
        }

        public string getCellHints(int row, int col)
        {
            string hints = "";

            //no hints of the number is available
            if (getInitialNumber(row,col) == 0)
            {
                for (int i = 0; i < size; i++)
                {
                    if (conditions[row,col,i].GetStatus() == Condition.Status.UNDECIDED)
                    {
                        hints += (i + 1).ToString();
                    }
                }
            }

            return hints;
        }

        //returns false if nothing happened
        public Outcome solveStep()
        {
            Outcome outcome = Outcome.NO_CHANGE;

            foreach(Clique clique in cliques)
            {
                if (clique.needsUpdate())
                {
                    switch (clique.updateClique())
                    {
                        case Outcome.FAILED:
                            return Outcome.FAILED;

                        case Outcome.UPDATED:
                            return Outcome.UPDATED;

                        case Outcome.NO_CHANGE:
                            break;
                    }
                }
            }

            return outcome;
        }

        //returns false if fails
        public Outcome setInitialNumbers(List<int> numbers)
        {
            reset();

            int i;
            for ( i = 0; i < numbers.Count; i++)
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

        //returns false if fails
        public Outcome addInitialNumber(int row, int col, int number)
        {
            initialNumbers[row, col] = number;
            allNumbers[row, col] = number;
            return conditions[row, col, number - 1].setStatus(Condition.Status.SATISFIED);
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
    }
}
