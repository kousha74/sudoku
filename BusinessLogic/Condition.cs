using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class Condition
    {
        public enum Status { UNDECIDED, SATISFIED, NOT_SATISFIED}

        public readonly int row;
        public readonly int col;
        public readonly int number;

        //very important: it can only change once
        private Status status = Status.UNDECIDED;

        private List<Condition> neighbors = new List<Condition>();
        private List<Clique> cliques = new List<Clique>();

        public readonly int id;

        private static int globalId = 0;

        public Condition(int row, int col, int number)
        {
            this.row = row;
            this.col = col;
            this.number = number;
            id = globalId++;
        }

        public void addNeighbor(Condition condition)
        {
            neighbors.Add(condition);
        }

        public void addClique(Clique clique)
        {
            cliques.Add(clique);
        }

        public bool setStatus(Status status)
        {
            if (this.status != status)
            {
                if (this.status != Status.UNDECIDED)
                {
                    return false;
                }
                else
                {
                    this.status = status;
                    return handleNewStatus();
                }
            }
            return true;
        }

        public void reset()
        {
            status = Status.UNDECIDED;
        }

        private bool handleNewStatus()
        {
            if (status == Status.SATISFIED)
            {
                //mark all the neighbors as NOT_SATISFIED
                foreach (Condition condition in neighbors)
                {
                    if (!condition.setStatus(Status.NOT_SATISFIED))
                    {
                        return false;
                    }
                }

                ClassicSudoku.Instance.setNumber(row, col, number + 1);
            }

            foreach(Clique clique in cliques)
            {
                clique.setUpdateNeeded();
            }

            return true;
        }

        public Status GetStatus()
        {
            return status;
        }

        public int getId()
        {
            return id;
        }
    }
}
