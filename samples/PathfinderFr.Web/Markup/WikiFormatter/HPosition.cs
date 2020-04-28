using System.Collections.Generic;

namespace PathfinderFr.Markup.WikiFormatter
{
    /// <summary>Represents a Header.</summary>
    public class HPosition
    {
        public HPosition(int index, string text, int level, int id)
        {
            this.Index = index;
            this.Text = text;
            this.Level = level;
            this.ID = id;
        }

        public int Index { get; set; }

        public string Text { get; set; }

        public int Level { get; set; }

        public int ID { get; set; }
    }


    /// <summary>
    /// Compares HPosition objects.
    /// </summary>
    public class HPositionComparer : IComparer<HPosition>
    {
        /// <summary>
        /// Performs the comparison.
        /// </summary>
        /// <param name="x">The first object.</param>
        /// <param name="y">The second object.</param>
        /// <returns>The comparison result.</returns>
        public int Compare(HPosition x, HPosition y) => x.Index.CompareTo(y.Index);
    }
}
