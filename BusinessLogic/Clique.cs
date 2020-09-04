using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class Clique
    {
        public enum State { IDLE, CHECK_SINGLE, CHECK_SUBSET, CHECK_PAIR, SOLVED }
        public readonly HashSet<ConditionInfo> conditions = new HashSet<ConditionInfo>();

        private readonly int size;
        private static int globalId = 0;
        public readonly int id;

        private State state = State.IDLE;

        //once one condition of a clique is satisfied, we are done with it
        public readonly string name;
        private readonly AbstractSudoku abstractSudoku;
        List<ConditionInfo> undecidedConditions = new List<ConditionInfo>();

        public Clique(int size, string name, AbstractSudoku abstractSudoku)
        {
            this.size = size;
            this.name = name;
            this.abstractSudoku = abstractSudoku;
            id = globalId++;
            Logger.Instance.WriteLine(Logger.LogLevel.ERROR, "Id = " + id.ToString() + ", Clique Created: " + name);
            reset();
        }

        public bool active { get => state != State.SOLVED; }

        public void reset()
        {
            state = State.IDLE;
            buildUndecidedConditions();
        }

        public void addCondition(ConditionInfo condition)
        {
            conditions.Add(condition);
        }

        public int getCommonConditionCount(Clique other)
        {
            int count = 0;

            foreach (ConditionInfo conditionInfo in conditions)
            {
                if (other.hasCondition(conditionInfo))
                {
                    count++;
                }
            }

            return count;
        }

        public bool hasCondition(ConditionInfo otherCondition)
        {
            foreach(ConditionInfo conditionInfo in conditions)
            {
                if (conditionInfo.condition.getId() == otherCondition.condition.getId())
                {
                    return true;
                }
            }

            return false;
        }

        public void setUpdateNeeded()
        {
            if (active)
            {
                state = State.CHECK_SINGLE;
            }
        }

        public void deactivate()
        {
            state = State.SOLVED;
            undecidedConditions.Clear();
        }

        public State getState()
        {
            return state;
        }

        public Outcome updateClique()
        {
            Outcome outcome = Outcome.NO_CHANGE;

            switch(state)
            {
                case State.IDLE:
                case State.SOLVED:
                    break;

                case State.CHECK_SINGLE:
                    buildUndecidedConditions();
                    outcome = checkSingles();
                    state = State.CHECK_SUBSET;
                    break;

                case State.CHECK_SUBSET:
                    outcome = checkSubsets();
                    state = State.CHECK_PAIR;
                    break;

                case State.CHECK_PAIR:
                    outcome = checkPairs();
                    state = State.IDLE;
                    break;
            }
       
            return outcome;
        }

        private Outcome checkSingles()
        {
            if (undecidedConditions.Count == 0)
            {
                return Outcome.FAILED;
            }
            else if (undecidedConditions.Count == 1)
            {
                
                abstractSudoku.sendString("Clique: " + name + " Single");
                Outcome outcome = undecidedConditions[0].setStatus(Status.SATISFIED);
                deactivate();
                return outcome;
            }

            return Outcome.NO_CHANGE;
        }

        private Outcome checkSubsets()
        {
            Outcome outcome = Outcome.NO_CHANGE;

            foreach (Clique clique in AbstractSudoku.getCommonCliques(undecidedConditions))
            {
                //if it's not this clique
                if (clique.id != id)
                {
                    switch (clique.removeAllBut(undecidedConditions))
                    {
                        case Outcome.FAILED:
                            return Outcome.FAILED;

                        case Outcome.UPDATED:
                            abstractSudoku.onPairFound(undecidedConditions);
                            outcome = Outcome.UPDATED;
                            break;

                        case Outcome.NO_CHANGE:
                            break;
                    }

                }
            }

            return outcome;
        }

        private Outcome checkPairs()
        {
            Outcome outcome = Outcome.NO_CHANGE;

            if (active && undecidedConditions.Count == 2 )
            {
                ConditionInfo A = undecidedConditions[0];
                ConditionInfo B = undecidedConditions[1];

                foreach(ConditionInfo C in B.getBlueNeighbors())
                {
                    if (C.id != A.id)
                    {
                        List<Clique> cliquesBC = abstractSudoku.getCommonCliques(B, C);

                        foreach (ConditionInfo D in C.getRedNeighbors())
                        {
                            List<Clique> cliquesAD = abstractSudoku.getCommonCliques(A, D);

                            if (cliquesAD.Count > 0)
                            {
                                foreach(Clique clique in cliquesBC)
                                {
                                    switch (clique.removeAllBut(B, C))
                                    {
                                        case Outcome.FAILED:
                                            return Outcome.FAILED;

                                        case Outcome.UPDATED:
                                            outcome = Outcome.UPDATED;
                                            break;

                                        case Outcome.NO_CHANGE:
                                            break;
                                    }

                                }
                                foreach (Clique clique in cliquesAD)
                                {
                                    switch (clique.removeAllBut(A, D))
                                    {
                                        case Outcome.FAILED:
                                            return Outcome.FAILED;

                                        case Outcome.UPDATED:
                                            outcome = Outcome.UPDATED;
                                            break;

                                        case Outcome.NO_CHANGE:
                                            break;
                                    }
                                }

                                //for logging only
                                if (outcome == Outcome.UPDATED)
                                {
                                    abstractSudoku.onPairFound(new List<ConditionInfo>() { A, B, C, D });
                                }
                            }
                        }
                    }
                }
            }

            return outcome;
        }

        public Outcome removeAllBut(ConditionInfo info1, ConditionInfo info2)
        {
            return removeAllBut(new List<ConditionInfo>() { info1, info2 });
        }
        public Outcome removeAllBut(List<ConditionInfo> excludedconditions)
        {
            Outcome outcome = Outcome.NO_CHANGE;

            foreach(ConditionInfo info in conditions)
            {
                bool isExcluded = false;
                foreach(ConditionInfo excludedcondition in excludedconditions)
                {
                    if(info.condition.id == excludedcondition.condition.id)
                    {
                        isExcluded = true;
                        break;
                    }
                }

                if (!isExcluded)
                {
                    switch (info.setStatus(Status.NOT_SATISFIED))
                    {
                        case Outcome.FAILED:
                            return Outcome.FAILED;

                        case Outcome.UPDATED:
                            outcome = Outcome.UPDATED;
                            break;

                        case Outcome.NO_CHANGE:
                            break;
                    }
                }
            }

            return outcome;
        }

        private void buildUndecidedConditions()
        {
            undecidedConditions.Clear();

            foreach(ConditionInfo condition in conditions)
            {
                if (condition.getStatus() == Status.UNDECIDED)
                {
                    undecidedConditions.Add(condition);
                }
            }
        }

        public List<ConditionInfo> getUndecidedConditions()
        {
            return undecidedConditions;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return id == ((Clique)obj).id;
        }


    }
}
