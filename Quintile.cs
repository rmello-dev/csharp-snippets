
namespace Game.Lib.World.Settlement
{

/// <summary>
/// Defines quintile groups and range distribution parameters.
/// </summary>
public static class Quintile
    {
    // --- Internal Members:

    /// <summary>
    /// Enumeration of quintile group definitions.
    /// </summary>
    public enum Group : byte
        {
        top,
        high,
        median,
        low,
        bottom
        }

    /// <summary>
    /// Encapsulation structure for membership range of quintile groups.
    /// </summary>
    public readonly struct Range
        {
        // --- Fields/Properties:
        
        private readonly int first;     // list index of first member
        private readonly int last;      // list index of last member
        
        // --- Initialization:

        /// <summary>
        /// Defines quintile range from index of first and last members.
        /// </summary>
        public Range(int firstMember, int lastMember)
            {
            first = firstMember;
            last = lastMember;
            }

        // --- External Behaviours:

        /// <summary>
        /// Evaluates whether population index belongs to quintile range.
        /// </summary>
        public bool IsMember(int populationIndex) { return((populationIndex <= last && populationIndex >= first)); }

        }

    /// <summary>
    /// Encapsulation structure for quintile range membership breakdown.
    /// </summary>
    public readonly struct Distribution
        {
        // --- Properties:

        public Range Top { get; }
        public Range High { get; }
        public Range Median { get; }
        public Range Low { get; }
        public Range Bottom {get; }

        // --- Initialization:

        /// <summary>
        /// Defines quintile breakdown from size of population.
        /// Target population must be sorted from high to low.
        /// </summary>
        public Distribution(ushort populationCount)
            {
            Range[] breakdown;
            
            if (populationCount % 5 == 0) { breakdown = calculateIdeal(populationCount); }
            else {
                 if (populationCount < 5) { breakdown = calculateSmall(populationCount); }
                 else { breakdown = calculateUnbalanced(populationCount); }
                 }

            Top = breakdown[0];
            High = breakdown[1];
            Median = breakdown[2];
            Low = breakdown[3];
            Bottom = breakdown[4];
            }

        // --- External Behaviours:

        /// <summary>
        /// Determines quintile group of individual with given population index.
        /// </summary>
        public Group FindQuintile(int populationIndex)
            {
            if (Median.IsMember(populationIndex)) { return(Group.median); }          // NOTE: returns Median for 0-index 0-population case
            else {
                 if (Top.IsMember(populationIndex)) { return(Group.top); }
                 else {
                      if (High.IsMember(populationIndex)) { return(Group.high); }
                      else {
                           if (Low.IsMember(populationIndex)) { return(Group.low); }
                           else { return(Group.bottom); }
                           }
                      }
                 }
            }

        // --- Internal Procedures:

        private static Range[] calculateSmall(ushort populationCount)
            {
            Range[] breakdown;

            switch(populationCount)
                {
                case(4): {                                              // N=4
                         breakdown = new Range[] { new Range(0,0),      // T <
                                                   new Range(1,1),      // H <
                                                   new Range(0,-1),     // M
                                                   new Range(2,2),      // L <
                                                   new Range(3,3) };    // B <
                         break;
                         }

                case(3): {                                              // N=3
                         breakdown = new Range[] { new Range(0,-1),     // T
                                                   new Range(0,0),      // H <
                                                   new Range(1,1),      // M <
                                                   new Range(2,2),      // L <
                                                   new Range(0,-1) };   // B
                         break;
                         }

                case(2): {                                              // N=2
                         breakdown = new Range[] { new Range(0,-1),     // T
                                                   new Range(0,0),      // H <
                                                   new Range(0,-1),     // M
                                                   new Range(1,1),      // L <
                                                   new Range(0,-1) };   // B
                         break;
                         }

                case(1):                                                // N=1
                default: {                                              // N=0
                         breakdown = new Range[] { new Range(0,-1),     // T
                                                   new Range(0,-1),     // H
                                                   new Range(0,0),      // M <
                                                   new Range(0,-1),     // L
                                                   new Range(0,-1) };   // B
                         break;
                         }
                }

            return(breakdown);
            }

        private static Range[] calculateUnbalanced(ushort populationCount)
            {
            int extra = populationCount % 5;        // extra balance
            int size = populationCount - extra / 5; // baseline group size
            int index = size-1;                     // zero-based index

            int headT = 0;
            int tailT = index;

            int headH = index+1;
            int tailH = index+size;

            int headM = index+size+1;
            int tailM = index+size*2;

            int headL = index+size*2+1;
            int tailL = index+size*3;

            int headB = index+size*3+1;
            int tailB = populationCount-1;

            switch(extra)
                {
                case(1): {                  // M+1
                         tailM++;
                         headL++; tailL++;
                         headB++;
                         break;
                         }

                case(2): {                  // H+1, L+1
                         tailH++;
                         headM++; tailM++;
                         headL++; tailL+=2;
                         headB+=2;
                         break;
                         }

                case(3): {                  // H+1, M+1, L+1
                         tailH++;
                         headM++; tailM+=2;
                         headL+=2; tailL+=3;
                         headB+=3;
                         break;
                         }

                case(4): {                  // H+2, L+2
                         tailH+=2;
                         headM+=2; tailM+=2;
                         headL+=2; tailL+=4;
                         headB+=4;
                         break;
                         }
                }

            Range[] breakdown = { 
                                new Range(headT,tailT),
                                new Range(headH,tailH),
                                new Range(headM,tailM),
                                new Range(headL,tailL),
                                new Range(headB,tailB)
                                };

            return(breakdown);
            }

        private static Range[] calculateIdeal(ushort populationCount)
            {
            int size = populationCount/5;                                   //  1           2           3       // size of each group
            int index = size-1;                                             //  0           1           2       // zero-based index

            Range[] breakdown = {                                           // N=5         N=10        N=15     
                                new Range(0,index),                         // 0~0         0~1         0~2      // T
                                new Range(index+1,index+size),              // 1~1         2~3         3~5      // H
                                new Range(index+size+1,index+size*2),       // 2~2         4~5         6~8      // M
                                new Range(index+size*2+1,index+size*3),     // 3~3         6~7         9~11     // L
                                new Range(index+size*3+1,populationCount-1) // 4~4         8~9        12~14     // B
                                };                                          // OK          OK          OK

            return(breakdown);
            }

        }

    }


}
