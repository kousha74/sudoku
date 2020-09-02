using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class ConditionInfo
    {
        public readonly HashSet<Clique> cliques = new HashSet<Clique>();
        public readonly HashSet<ConditionInfo> neighbors = new HashSet<ConditionInfo>();
        public readonly Condition condition;

        //very important: it can only change once
        private Status status = Status.UNDECIDED;
        private readonly AbstractSudoku abstractSudoku;

        public ConditionInfo(Condition condition, AbstractSudoku abstractSudoku)
        {
            this.condition = condition;
            this.abstractSudoku = abstractSudoku;
        }

        public bool addClique(Clique clique)
        {
            if (cliques.Contains(clique))
            {
                return false;
            }

            cliques.Add(clique);
            return true;
        }

        public bool addNeighbor(ConditionInfo info)
        {
            if (neighbors.Contains(info))
            {
                return false;
            }

            neighbors.Add(info);
            return true;
        }

        public Outcome setStatus(Status status)
        {
            if (this.status != status)
            {
                if (this.status != Status.UNDECIDED)
                {
                    return Outcome.FAILED;
                }
                else
                {
                    this.status = status;
                    return processStatusChange();
                }
            }

            return Outcome.NO_CHANGE;
        }

        private Outcome processStatusChange()
        {
            Outcome outcome = Outcome.UPDATED;

            if (status == Status.SATISFIED)
            {

                abstractSudoku.onConditionSatidfied(condition);

                foreach (ConditionInfo conditionInfo in neighbors)
                {
                    if (conditionInfo.setStatus(Status.NOT_SATISFIED) == Outcome.FAILED)
                    {
                        return Outcome.FAILED;
                    }
                }

                foreach(Clique clique in cliques)
                {
                    clique.deactivate();
                }
            }
            else
            {
                foreach (Clique clique in cliques)
                {
                    clique.setUpdateNeeded();
                }
            }

            return outcome;
        }

        public void reset()
        {
            status = Status.UNDECIDED;
        }

        public Status getStatus()
        {
            return status;
        }

        public override int GetHashCode()
        {
            return condition.id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return condition.id == ((ConditionInfo)obj).condition.id;
        }

    }
}
