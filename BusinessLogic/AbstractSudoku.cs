using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    abstract class AbstractSudoku
    {
        private Dictionary<Condition, ConditionInfo> conditions = new Dictionary<Condition, ConditionInfo>();
        protected List<Clique> cliques = new List<Clique>();

        public readonly int size;
        public readonly int[] bitCounter;
        private IAbstractSudoku listener = null;
        public enum SudokuType { CLASSIC };

        public readonly SudokuType sudokuType;
        private ChainSolver chainSolver = null;

        public AbstractSudoku(int size, SudokuType sudokuType)
        {
            int allCombinations = 2 ^ size;
            bitCounter = new int[allCombinations];
            for (ushort i = 0; i <allCombinations; i++)
            {
                bitCounter[i] = Bits.countSetBits(i);
            }

            this.size = size;
            this.sudokuType = sudokuType;
        }

        public abstract void onConditionChanged(Condition condition, Status status);
        public abstract void onPairFound(List<Condition> conditions);
        public abstract Outcome addInitialNumber(int row, int col, int number);
        public abstract Outcome setInitialNumbers(List<int> numbers);
        public abstract string getNumbersString();

        public void onPairFound(List<ConditionInfo> infos)
        {
            List<Condition> conditionList = new List<Condition>();

            foreach(ConditionInfo info in infos)
            {
                conditionList.Add(info.condition);
            }

            onPairFound(conditionList);
        }

        public void setListener(IAbstractSudoku listener)
        {
            this.listener = listener;
        }

        public bool init(List<RawData> data)
        {
            if (cliques.Count != 0)
            {
                return false;
            }

            //logging all the cliques
            foreach (RawData rawData in data)
            {
                Clique clique = new Clique(size, rawData.name, this);
                cliques.Add(clique);
                
                List<ConditionInfo> conditionInfos = new List<ConditionInfo>();

                foreach (Condition condition in rawData.conditions)
                {
                    ConditionInfo conditionInfo = null;
                    if (conditions.ContainsKey(condition))
                    {
                        conditionInfo = conditions[condition];
                    }
                    else
                    {
                        conditionInfo = new ConditionInfo(condition, this);
                        conditions.Add(condition, conditionInfo);
                    }

                    conditionInfo.addClique(clique);
                    clique.addCondition(conditionInfo);

                    foreach (ConditionInfo info in conditionInfos)
                    {
                        info.addNeighbor(conditionInfo);
                        conditionInfo.addNeighbor(info);
                    }

                    conditionInfos.Add(conditionInfo);
                }
            }
            chainSolver = new ChainSolver(this, cliques);
            return true;
        }

        public virtual void Reset()
        {
            foreach (KeyValuePair<Condition, ConditionInfo> entry in conditions)
            {
                entry.Value.reset();
            }

            if (cliques != null)
            {
                foreach (Clique clique in cliques)
                {
                    clique.reset();
                }
            }
        }

        //returns false if nothing happened
        public Outcome solveStep()
        {
            Outcome outcome = Outcome.NO_CHANGE;

            List<Clique.State> states = new List<Clique.State>()
            {
                Clique.State.CHECK_SINGLE,
                Clique.State.CHECK_SUBSET,
                Clique.State.CHECK_PAIR
            };

            foreach (Clique.State state in states)
            {
                foreach (Clique clique in cliques)
                {
                    if (clique.getState() == state)
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
            }

            chainSolver.solveChains();

            return outcome;
        }

        public Outcome setStatus(Condition condition, Status status)
        {
            if (conditions.ContainsKey(condition))
            {
                return conditions[condition].setStatus(status);
            }

            return Outcome.FAILED;
        }

        public Status getStatus(Condition condition)
        {
            if (conditions.ContainsKey(condition))
            {
                return conditions[condition].getStatus();
            }

            return Status.UNDECIDED;
        }

        public List<Clique> getCommonCliques(ConditionInfo info1, ConditionInfo info2)
        {
            return getCommonCliques(new List<ConditionInfo>() { info1, info2 });
        }

        public static List<Clique> getCommonCliques(List<ConditionInfo> conditionInfos)
        {
            List<Clique> cliques = new List<Clique>();

            foreach(ConditionInfo info in conditionInfos)
            {
                if (cliques.Count == 0)
                {
                    //copy the cliques
                    foreach(Clique clique in info.cliques)
                    {
                        cliques.Add(clique);
                    }
                }
                else
                {
                    cliques = cliques.Intersect(info.cliques).ToList();
                }
            }

            return cliques;
        }

        public List<Clique> getCliques(int undecidedConditions)
        {
            List<Clique> desiredCliques = new List<Clique>();

            foreach(Clique clique in cliques)
            {
                if (clique.getUndecidedConditions().Count == undecidedConditions)
                {
                    desiredCliques.Add(clique);
                }
            }

            return desiredCliques;
        }

        public void sendString(string str)
        {
            if (listener != null)
            {
                listener.onNewString(str);
            }
        }

        public interface IAbstractSudoku
        {
            void onNewString(string str);
        }
    }
}
