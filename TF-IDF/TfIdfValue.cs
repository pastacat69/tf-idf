namespace TF_IDF
{
    /// <summary>
    /// Represent stroring TF*IDF algorithms values
    /// </summary>
    public class TfIdfValue
    {
        /// <summary>
        /// Term frequency
        /// </summary>
        public double Tf { get; }
        /// <summary>
        /// Invers data frequency
        /// </summary>
        public double Idf { get; }
        /// <summary>
        /// Numerical statistic that is intended to reflect how important a word is to a document in a collection or corpus
        /// </summary>
        public double TfIdf { get => Tf * Idf; }
        /// <summary>
        /// Part of the document corpus
        /// </summary>
        public string Term { get; }

        public TfIdfValue(double tf, double idf, string term)
        {
            Tf = tf;
            Idf = idf;
            Term = term;
        }
    }
}
