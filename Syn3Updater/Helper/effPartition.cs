/*
 * Effective Partition Problem (optimization version) solution
 * by Fedor Naumenko (fedor.naumenko@gmail.com), 2018
 * 
 * Effectively distributes a set of input positive integers into k subsets,
 * such that the difference between the subset sums is minimized.
 * 
 * https://www.codeproject.com/Articles/1265125/Fast-and-Practically-perfect-Partition-Problem-Sol
 * 
 */

using System;
using System.Collections.Generic;

namespace Cyanlabs.Syn3Updater.Helper
{
    using numb_id = System.UInt32;
    using numb_val = System.UInt32;
    using ss_id = System.UInt16;
    using ss_sum = System.UInt64;

    sealed class effPartition
    {
        /// <summary>Represent the identified number to be partitioned</summary>
        public sealed class IdNumber
        {
            /// <summary>
            /// bin's inverse index: count_of_bins-1 for the first and 0 for the last one
            /// </summary>
            internal ss_id BinIInd;
            /// <summary>number's ID</summary>
            internal numb_id Id;
            /// <summary>number's value</summary>
            internal numb_val Val;

            /// <summary>Constructor by number ID and value.</summary>
            /// <param name="id">number's ID</param>
            /// <param name="val">number's value</param>
            public IdNumber(numb_id id, numb_val val) { BinIInd = 0; Id = id; Val = val; }

            /// <summary>Constructor by number.</summary>
            /// <param name="numb">number to be copied</param>
            internal IdNumber(IdNumber numb) { BinIInd = numb.BinIInd; Id = numb.Id; Val = numb.Val; }

            /// <summary>Gets number's value.</summary>
            public numb_val Value { get { return Val; } }

            /// <summary>Gets number's ID.</summary>
            public numb_id ID { get { return Id; } }

            /// <summary>
            /// Returns true if number is not allocated and its value is fitted to the upper limit
            /// </summary>
            /// <param name="val">upper limit</param>
            /// <returns>true if number is not allocated and its value is fitted to the upper limit</returns>
		    internal bool IsFitted(float val) { return BinIInd == 0 && Val <= val; }
        }

        class IdNumbComparer : IComparer<IdNumber>
        {
            /// <summary>Default comparer for sorting numbers by val descent</summary>
            /// <param name="x">first number</param>
            /// <param name="y">second number</param>
            /// <returns></returns>
            public int Compare(IdNumber x, IdNumber y)
            {
                if (x.Val < y.Val) return 1;
                if (x.Val > y.Val) return -1;
                return 0;
            }
        }

        /// <summary>Represents a specialized container of identified numbers</summary>
        public sealed class IdNumbers : List<IdNumber> //, ICloneable<IdNumbers>
        {
            /// <summary>
            /// minimum tolerance: part of minimal numb value defines the start accuracy
            /// with which the free space of the average sum is measured.
            /// </summary>
            /// <remarks>
            /// It is a heuristic value provides satisfactory inaccuracy in a single pass in most of cases
            ///  </remarks>
		    const int minTol = 20;

            public IdNumbers(int capacity) : base(capacity) { }

            internal IdNumbers(IdNumbers numbers)
            {
                Capacity = numbers.Capacity;
                foreach (IdNumber n in numbers)
                    Add(new IdNumber(n));
            }

            /// <summary>Copies bin's indexes</summary>
            internal void CopyIndexes(IdNumbers numbers)
            {
                for (int i = 0; i < Count; i++)
                    this[i].BinIInd = numbers[i].BinIInd;
            }

            /// <summary>
            /// Gets minimum accuracy with which the available space of the average sum should be measured
            /// </summary>
            internal float GetMinUp
            {
                get
                {
                    return Math.Max((float)this[Count - 1].Val / minTol, 1F);
                }
            }

            /// <summary>Calculates average values sum</summary>
            /// <param name="ssCnt">count of subsets</param>
            /// <returns>average values sum</returns>
		    internal float AvrSum(ss_id ssCnt)
            {
                float sum = 0;
                foreach (IdNumber n in this) sum += n.Val;
                return sum / ssCnt;
            }

            /// <summary>Marks all numbers as unallocated</summary>
            internal void Reset()
            {
                ForEach((IdNumber n) => { n.BinIInd = 0; });
            }

            /// <summary>Sorts numbers in descending order</summary>
            internal void SortByDescent() { Sort(new IdNumbComparer()); }
        }

        /// <summary>Represents partition</summary>
        public sealed class Subset
        {
            /// <summary>subset's ID</summary>
		    internal ss_id id;
            /// <summary>sum of subset numbers</summary>
		    internal ss_sum sumVal;
            /// <summary>number IDs container</summary>
		    internal List<numb_id> numbIDs;

            internal static int CompareInDescend(Subset s1, Subset s2)
            {
                {
                    if (s1.sumVal < s2.sumVal) return 1;
                    if (s1.sumVal > s2.sumVal) return -1;
                    return 0;
                }
            }

            internal static int CompareInAscend(Subset s1, Subset s2)
            {
                {
                    if (s1.sumVal > s2.sumVal) return 1;
                    if (s1.sumVal < s2.sumVal) return -1;
                    return 0;
                }
            }

            /// <summary>Creates an empty Subset with reserved capacity</summary>
            /// <param name="nCnt">expected count of numbers</param>
            internal Subset(int nCnt)
            {
                id = 0; sumVal = 0; numbIDs = new List<numb_id>(nCnt);
            }

            /// <summary>
            /// Clears instance: removes all number IDs, sets the sum of number values to zero
            /// </summary>
            internal void Clear() { sumVal = 0; numbIDs.Clear(); }

            /// <summary>Adds number</summary>
            /// <param name="n">number to add</param>
            internal void AddNumb(IdNumber n)
            {
                sumVal += n.Val;
                numbIDs.Add(n.Id);
            }

            /// <summary>Prints subset</summary>
            /// <param name="sumWidth">maximum count of digits in subsets sum value to align sums left (doesn't used)</param>
            /// <param name="prNumbCnt">maximum count of printed number IDs or 0 if all</param>
            internal void Print(byte sumWidth, int prNumbCnt = 0)
            {
                Console.Write("set {0:d}\tsum {1:d}  numbIDs:", ID, sumVal);
                if (prNumbCnt > 0 && prNumbCnt < numbIDs.Count)
                {
                    for (int i = 0; i < prNumbCnt; i++)
                        Console.Write(" {0:d}", numbIDs[i]);
                    Console.Write("... ({0:d})", numbIDs.Count);
                }
                else
                    foreach (numb_id id in numbIDs)
                        Console.Write(String.Format(" {0,2}", id));
                Console.WriteLine();
            }

            /// <summary>Gets subset's ID</summary>
            public ss_id ID { get { return id; } }

            /// <summary>Gets sum of subset numbers</summary>
            public ss_sum Sum { get { return sumVal; } }

            /// <summary>Gets number IDs container</summary>
            public List<numb_id> NumbIDs { get { return numbIDs; } }
        }

        /// <summary>Represents subsets and their inaccuracy</summary>
        internal class Result
        {
            /// <summary>maximum difference between the subset sums (inaccuracy)</summary>
            internal ss_sum SumDiff;
            /// <summary>subsets</summary>
            internal List<Subset> Bins;

            /// <summary>Constructs Result and reserves subsets capacity</summary>
            /// <param name="nCnt">count of numbers</param>
            /// <param name="ssCnt">count of subsets</param>
            internal Result(int nCnt, ss_id ssCnt)
            {
                SumDiff = largestSum;
                Bins = new List<Subset>(ssCnt);
                for (ss_id i = 0; i < ssCnt; i++)
                    Bins.Add(new Subset(nCnt / ssCnt + 2));
            }

            /// <summary>Gets the count of subsets</summary>
            internal ss_id SubsetCount { get { return (ss_id)Bins.Count; } }

            /// <summary>Sorts subsets by their sum</summary>
            /// <param name="ascend">if true then in ascending order, otherwise in descending order</param>
            internal void Sort(bool ascend)
            {
                if (ascend) Bins.Sort(Subset.CompareInAscend);
                else Bins.Sort(Subset.CompareInDescend);
            }

            /// <summary>Sorts subsets in descending order and sets subsets ID starting with 1</summary>
            internal void SetIDs()
            {
                // set subset's ID
                ss_id i = 1;		// first subset ID
                Bins.ForEach((Subset ss) => { ss.id = i++; });
            }

            /// <summary>Clears instance: removes all subsets number IDs and resets the sum of the number values
            /// </summary>
            internal void Clear()
            {
                SumDiff = largestSum;
                Bins.ForEach((Subset ss) => { ss.Clear(); });
            }

            /// <summary>Outfits this result</summary>
            /// <param name="numbs">numbers to be distributed</param>
            /// <param name="ind">index of the first subset: 1 for SGreedy() or 0 for DSTree()</param>
            /// <param name="diff">difference between the subset sums</param>
            internal void Fill(IdNumbers numbs, byte ind, ss_sum diff)
            {
                ss_id ssCnt = (ss_id)(SubsetCount - ind);   // count of subsets minus first index
                Clear();
                foreach (IdNumber n in numbs)
                    Bins[ssCnt - n.BinIInd].AddNumb(n);
                SumDiff = diff;
            }

            /// <summary>Sets the minimum/maximum subset sums and their difference for this instance
            /// </summary>
            /// <param name="minSum">minimum subset sum to be retrieved></param>
            /// <param name="maxSum">maximum subset sum to be retrieved</param>
            /// <returns>true if sum difference is updated</returns>
            internal bool SetSumDiff(ref ss_sum minSum, ref ss_sum maxSum)
            {
                minSum = maxSum = Bins[0].sumVal;
                //for(int i=1; i<Bins.Count; i++)
                //    if (Bins[i].sumVal > maxSum) maxSum = Bins[i].sumVal;
                //    else if (Bins[i].sumVal < minSum) minSum = Bins[i].sumVal;
                foreach (Subset ss in Bins)
                    if (ss.sumVal > maxSum) maxSum = ss.sumVal;
                    else if (ss.sumVal < minSum) minSum = ss.sumVal;
                ss_sum newSumDiff = maxSum - minSum;
                if (newSumDiff < SumDiff)
                {
                    SumDiff = newSumDiff;
                    return true;
                }
                return false;
            }

            /// <summary>Prints an instance</summary>
            /// <param name="prNumbCnt">maximum count of printed number IDs or 0 if all</param>
            internal void Print(int prNumbCnt = 0)
            {
                if (Bins.Count == 0) return;
                byte width = DigitsCnt(Bins[0].sumVal);	// first sum is the biggest
                foreach (Subset ss in Bins)
                    ss.Print(width, prNumbCnt);
            }

            /// <summary>Gets count of digist in a value</summary>
            /// <param name="val">value</param>
            /// <returns>count of digist</returns>
		    internal static byte DigitsCnt(ss_sum val)
            {
                byte res = 0;
                for (; val > 0; val /= 10, res++) ;
                return res;
            }

            //// <summary>Sorts subsets in descending order</summary>
            //internal void SortByDescent() { Bins.Sort(new SubsetComparer()); }
        }

        /// <summary>Encapsulates partition methods</summary>
        internal class Partition
        {
            /// <summary>the minimum DSTree() invokes limit</summary>
		    const uint minCallLimit = 1000000;
            /// <summary>flag of DSTree() completion by limit</summary>
            const byte limFlag = 0x1;
            /// <summary>flag of DSTree() completion by 'perfect' result</summary>
            const byte perfFlag = 0x2;

            /// <summary>current DSTree() invokes limit</summary>
		    UInt64 callLimit;
            /// <summary>counter of DSTree() invokes</summary>
		    uint callCnt;
            /// <summary>input numbers</summary>
		    IdNumbers numbs;
            /// <summary>numbers with the best unsaved maximum sum difference: used in DSTree()</summary>
            IdNumbers standbyNumbs;
            /// <summary>current result</summary>
            Result currResult;
            /// <summary>final result</summary>
            Result finalResult;
            /// <summary>current minimum sum among subsets</summary>
            ss_sum minSum;
            /// <summary>current maximum sum among subsets</summary>
            ss_sum maxSum;
            /// <summary>current maximum difference between the subset sums (inaccuracy)</summary>
            ss_sum sumDiff;
            /// <summary>last best inaccuracy: used in DSTree()</summary>
            ss_sum lastSumDiff;
            /// <summary>unsaved maximum difference between the subset sums: used in DSTree()</summary>
            ss_sum standbySumDiff;
            /// <summary>average sum value among subsets</summary>
            float avrSum;
            /// <summary>true if avrSum is not an integer (has a nonzero fractional part)</summary>
            bool isAvrFract;
            /// <summary>holder of DSTree() completion flags (signs)</summary>
            byte complete;
            /// <summary>Raises completion flag</summary>
            /// <param name="flag">completion flag to be raised</param>
            void RaiseComplFlag(byte flag) { complete |= flag; }

            /// <summary>Gets true if 'perfect' completion flag is raised</summary>
            bool IsCompleteByPerfect { get { return (complete & perfFlag) != 0; } }

            /// <summary>Gets true if current result is the best possible</summary>
		    bool IsResultPerfect { get { return sumDiff == 0 || (isAvrFract && sumDiff == 1); } }

            /// <summary>Sets the best minimum/maximum subset sum and their difference</summary>
            /// <param name="res">current result that delivers the sums as candidates for the bes</param>
            /// <returns>true if the more narrow range is achieved</returns>
		    bool SetRange(Result res)
            {
                ss_sum resMinSum = 0, resMaxSum = 0;

                if (res.SetSumDiff(ref resMinSum, ref resMaxSum) && res.SumDiff < sumDiff)
                {
                    sumDiff = res.SumDiff;
                    minSum = resMinSum;
                    maxSum = resMaxSum;
                    return true;
                }
                return false;
            }

            /// <summary>Performs 'Unconditional Greedy' partition</summary>
            /// <param name="ssCnt">count of subsets</param>
		    void UGreedy(ss_id ssCnt)
            {
                int i = 0, shift = 1;

                foreach (IdNumber n in numbs)
                {
                    finalResult.Bins[i].AddNumb(n);
                    i += shift;
                    if (i / ssCnt > 0) { i--; shift = -1; }	// last subset, flip to reverse round order
                    else if (i < 0) { i++; shift = 1; }		// first subset, flip to direct  round order
                }
                SetRange(finalResult);		// set minSum, maxSum, sumDiff
                finalResult.SetIDs();       // set subsets ID
            }

            /// <summary>Performs 'Greedy' partition</summary>
            /// <param name="ssCnt">count of subsets</param>
            void Greedy(ss_id ssCnt)
            {
                ss_id i, k;		// subsets cyclic index, index of subset with minimum sum

                foreach (IdNumber n in numbs)
                {
                    if (n.BinIInd != 0)
                        continue;		// needs for SGreedy, for Greedy itself is redundant
                    minSum = currResult.Bins[k = 0].sumVal;
                    for (i = 1; i < ssCnt; i++)				// loop through the bins from the second one
                        if (currResult.Bins[i].sumVal <= minSum)
                            minSum = currResult.Bins[k = i].sumVal;
                    currResult.Bins[k].sumVal += n.Val;
                    n.BinIInd = (ss_id)(ssCnt - k);
                }

                if (SetRange(currResult))  // is the result better than after previous UGreedy() call?
                {
                    finalResult.Fill(numbs, 0, currResult.SumDiff);		// outfit final result
                    finalResult.SetIDs();
                }
            }

            /// <summary>Wrapper to 'Greedy' partition</summary>
            /// <param name="ssCnt">count of subsets</param>
            void WrapGreedy(ss_id ssCnt)
            {
                numbs.Reset();
                currResult = new Result(numbs.Count, ssCnt);
                Greedy(ssCnt);
            }

            /// <summary>Performs 'Sequential stuffing Greedy' partition</summary>
            /// <param name="ssCnt">count of subsets</param>
            void SGreedy(ss_id ssCnt)
            {
                int freeCnt;        // number of unallocated numbs
                int i, k = 1;       // bin index, delta multiplicator
                float avrUp;		// raised average sum
                float up = numbs.GetMinUp;	// delta above average sum

                do  // loop through the numbs until unfitted numbs count becomes less then half of bins
                {
                    freeCnt = numbs.Count;
                    numbs.Reset();
                    currResult.Clear();
                    avrUp = avrSum + up * k++;

                    for (i = 0; i < currResult.Bins.Count; i++)
                        foreach (IdNumber n in numbs)
                            if (n.IsFitted(avrUp - currResult.Bins[i].sumVal))
                            {
                                currResult.Bins[i].sumVal += n.Val;
                                n.BinIInd = (ss_id)(ssCnt - i);
                                freeCnt--;
                            }
                }
                // this heuristic contition provided satisfactory inaccuracy in a single pass in most of cases
                while (freeCnt > ssCnt / 2);

                // distribute remaining unallocated numbs by Greed protocol
                // Checking for freeCnt==0 can be omitted, since as n happens very rarely
                Greedy(ssCnt);
            }

            /// <summary>Performs 'Dynamic Search Tree' ('perfect') partition</summary>
            /// <param name="currItInd">numb's index from which the cycle continues</param>
            /// <param name="invInd">current inverse index of subset</param>
		    void DSTree(int currItInd, ss_id invInd)
            {
                if (complete != 0) return;
                if (++callCnt == callLimit) RaiseComplFlag(limFlag);
                Subset ss = currResult.Bins[currResult.Bins.Count - 1 - invInd];    // curent bin

                if (invInd != 0)
                {		// not last bin
                    IdNumber n;
                    for (int i = currItInd; i < numbs.Count; i++)
                    {
                        n = numbs[i];
                        if (n.BinIInd == 0 && n.Val + ss.sumVal < maxSum)
                        {
                            ss.sumVal += n.Val;
                            n.BinIInd = invInd;
                            if (i + 1 < numbs.Count)            // checkup just to avoid blank recursive invoke
                                DSTree(i + 1, invInd);              // try to fit next numb to the same bin
                            if (ss.sumVal > minSum)             // bin is full
                                DSTree(0, (ss_id)(invInd - 1));	// try to fit unallocated numbs to the next bin
                            ss.sumVal -= n.Val;                // clear the traces of current numb processing
                            n.BinIInd = 0;
                        }
                    }
                }
                else
                {				// last bin
                    // accumulate sum for the last bin
                    foreach (IdNumber n in numbs)
                        if (n.BinIInd == 0)     // zero invIndex means that number belongs to the last bin
                            ss.sumVal += n.Val;
                    if (SetRange(currResult))   // is inaccuracy better than the previous one?
                    {
                        standbyNumbs.CopyIndexes(numbs);	// keep current numbers as the standby one
                        lastSumDiff = standbySumDiff = sumDiff;	// for the next standby result selection
                        if (IsResultPerfect) RaiseComplFlag(perfFlag);
                    }
                    else if (currResult.SumDiff < standbySumDiff)  // should we keep current result as standby?
                    {
                        standbyNumbs.CopyIndexes(numbs);	// keep current numbers as the standby one
                        standbySumDiff = currResult.SumDiff;
                    }
                    ss.sumVal = 0;              // clear last bin sum
                }
            }

            /// <summary>Performs iterative 'Dynamic Search Tree' partition.</summary>
            /// <param name="ssCnt">count of subsets</param>
            void ISTree(ss_id ssCnt)
            {
                // initial range expansion around average 
                numb_val up = avrSum < numbs[0].Val ?
                    (numbs[0].Val - System.Convert.ToUInt32(avrSum) + 2) : 1;
                if (up > avrSum) up = System.Convert.ToUInt32(avrSum) - 1;
                standbySumDiff = largestSum;	// undefined standby inaccuracy
                lastSumDiff = finalResult.SumDiff;
                standbyNumbs = new IdNumbers(numbs);
                do
                {
                    minSum = (ss_sum)(avrSum - up);
                    maxSum = (ss_sum)(avrSum + up);
                    sumDiff = maxSum - minSum;
                    callCnt = complete = 0;
                    numbs.Reset();
                    currResult.Clear();
                    DSTree(0, (ss_id)(ssCnt - 1));

                    if (IsCompleteByPerfect || (up *= 2) >= minSum) // increase and checkup range expansion
                        break;
                    if (currResult.SumDiff > standbySumDiff)	// is current result worse than standby one?
                    {
                        break;
                    }
                }
                while (lastSumDiff != currResult.SumDiff);  // until previous and current inaccuracy are different
                                                            // use last fitted result
                {
                    SetRange(finalResult);
                    finalResult.Fill(standbyNumbs, 1, standbySumDiff);
                }
            }
            /// <summary>Partition method delegate</summary>
            /// <param name="ssCnt">count od subsets</param>
            delegate void PartitionMethodDlg(ss_id ssCnt);

            /// <summary>Partition method delegates</summary>
            static PartitionMethodDlg[] methods = new PartitionMethodDlg[4];

            /// <summary>Performes partitioning</summary>
            /// <param name="i">index of partition method to call</param>
            /// <param name="ssCnt">count od subsets</param>
            /// <returns>true if result is 'perfect'</returns>
            bool DoPartition(int i, ss_id ssCnt)
            {

                methods[i](ssCnt);
                return IsResultPerfect;
            }

            /// <summary>Creates an instance and performs partition</summary>
            /// <param name="nmbs">identified values to be distributed</param>
            /// <param name="result">final result</param>
            /// <param name="avr">average value sum</param>
            /// <param name="limMult">DSTree method call's limit multiplier;
            /// if 0 then omit DSTree method invoking</param>
            public Partition(IdNumbers nmbs, Result result, float avr, byte limMult)
            {
                numbs = nmbs;
                finalResult = result;
                avrSum = avr;
                isAvrFract = (int)avrSum != avrSum;
                callLimit = minCallLimit * limMult;
                sumDiff = largestSum;
                ss_id ssCnt = finalResult.SubsetCount;
                int i = 0;
                methods[i++] = UGreedy; methods[i++] = WrapGreedy; methods[i++] = SGreedy; methods[i++] = ISTree;
                numbs.SortByDescent();
                int cnt = limMult > 0 ? 4 : 3;

                // for the degenerate case numbs.Count<=ssCnt method UGreedy() is comepletely enough
                if (numbs.Count <= ssCnt) cnt = 1;
                for (i = 0; i < cnt; i++)
                    if (DoPartition(i, ssCnt)) return;
            }
        };

        /// <summary>largest possible sum</summary>
        const ss_sum largestSum = UInt64.MaxValue;
        /// <summary>final partition</summary>
        Result result;
        /// <summary>average sum value among subsets</summary>
        float avr;

        /// <summary>Initializes partition by identified numbers.</summary>
        /// <param name="numbs">identified numbers to be distributed</param>
        /// <param name="ssCnt">count of subsets</param>
        /// <param name="limMult">DSTree method call's limit multiplier;
        /// if 0 then omit DSTree method invoking (fast, but not 'perfect)</param>
        void Init(IdNumbers numbs, ss_id ssCnt, byte limMult)
        {
            result = new Result(numbs.Count, ssCnt);
            if (ssCnt == 0) return;
            effPartition.Partition Partition = new Partition(numbs, result, avr = numbs.AvrSum(ssCnt), limMult);
        }

        /// <summary>Constructs numbers partition by identified numbers,
        ///  with sums sorted in descending order.</summary>
        /// <param name="numbs">identified numbers to be distributed</param>
        /// <param name="ssCnt">count of subsets;
        /// if 0 then creates an empty partition with undefined (maximum type's value) inaccuracy</param>
        /// <param name="limMult">DSTree method call's limit multiplier -
        /// it increases the limit of 1 million recursive invokes by limMult times;
        /// if 0 then omit DSTree method invoking (fast, but not 'perfect')</param>
        public effPartition(IdNumbers numbs, ss_id ssCnt, byte limMult = 1)
        {
            Init(numbs, ssCnt, limMult);
        }

        /// <summary>Constructs numbers partition by numbers,
        ///  with sums sorted in descending order.</summary>
        /// <param name="vals">numbers to be distributed;
        /// their ID are assigned according to their ordinal numbers in array</param>
        /// <param name="ssCnt">count of subsets;
        /// if 0 then creates an empty partition with undefined (maximum type's value) inaccuracy</param>
        /// <param name="limMult">DSTree method call's limit multiplier -
        /// it increases the limit of 1 million recursive invokes by limMult times;
        /// if 0 then omit DSTree method invoking (fast, but not 'perfect')</param>
        public effPartition(List<numb_val> vals, ss_id ssCnt, byte limMult = 1)
        {
            numb_id i = 1;
            IdNumbers numbs = new IdNumbers(vals.Count);

            foreach (numb_val val in vals)
                numbs.Add(new IdNumber(i++, val));
            Init(numbs, ssCnt, limMult);
        }

        /// <summary>Gets the number of subsets.</summary>
        public ss_id SubsetCount { get { return result.SubsetCount; } }

        /// <summary>Gets subset by index</summary>
        /// <param name="i">index of subset</param>
        /// <returns>subset</returns>
        public Subset this[ss_id i] { get { return result.Bins[i]; } }

        /// <summary>Gets a reference to the subsets container.</summary>
        public List<Subset> Subsets { get { return result.Bins; } }

        /// <summary>Gets average summary value among subsets.</summary>
        public float AvrSum { get { return avr; } }

        /// <summary>Gets inaccuracy: the difference between maximum and minimum
        /// summary value among subsets.
        /// </summary>
        public ss_sum Inacc { get { return result.SumDiff; } }

        /// <summary>Gets relative inaccuracy: the inaccuracy in percentage to average summary.
        /// </summary>
        public float RelInacc { get { return 100F * Inacc / avr; } }

        /// <summary>Sorts subsets by their sum.</summary>
        /// <param name="ascend">if true then in ascending order, otherwise in descending order</param>
        public void Sort(bool ascend = true) { if (Inacc > 0) result.Sort(ascend); }

        /// <summary>Outputs subsets to console.</summary>
        /// <param name="prSumDiff">if true then prints sum diference (in absolute and relative)</param>
        /// <param name="prNumbCnt">maximum number of printed number IDs or 0 if ptint all</param>
        public void Print(bool prSumDiff = true, int prNumbCnt = 0)
        {
            if (prSumDiff)
                Console.WriteLine("inaccuracy: {0:d} ({1:f}%)", Inacc, RelInacc);
            result.Print(prNumbCnt);
        }
    }
}
