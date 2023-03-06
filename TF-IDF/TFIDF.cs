namespace TF_IDF
{
    /// <summary>
    ///  TF*IDF is a numerical value that indicates how important a word is within a document, compared with a larger set of documents (or corpus). 
    ///  It’s used in search engine ranking algorithms, natural language processing, machine learning, and various statistics algorithms.
    /// </summary>
    public static class TFIDF
    {
        /// <summary>
        /// Term frequency (TF) is a count of how many times a particular term appears within a document. 
        /// It is calculated by simply counting the number of times the word is found in the target document 
        /// divived on total count terms in document
        /// </summary>
        /// <param name="term"></param>
        /// <param name="document">Parts of specific document corpus</param>
        /// <returns></returns>
        private static double TermFrequency(string term, IEnumerable<string> document)
        {
            if (string.IsNullOrEmpty(term) || !document.Any())
            {
                return 0.0;
            }

            double termOccurs = document.Count(documentItem => documentItem.Equals(term, StringComparison.CurrentCultureIgnoreCase));
            return termOccurs / document.Count();
        }

        /// <summary>
        /// measure of how much information the word provides, i.e., if it is common or rare across all documents.
        /// It is the logarithmically scaled inverse fraction of the documents that contain the word
        /// </summary>
        /// <param name="stemmedDocuments"></param>
        /// <param name="vocabulary"></param>
        /// <returns></returns>
        private static IDictionary<string,double> InversDataFrequency(IEnumerable<IList<string>> stemmedDocuments, IEnumerable<string> vocabulary)
        {
            var idfDicionary = new Dictionary<string,double>();
            foreach (var vocabularyTerm in vocabulary)
            {
                int numberOfDocsContainingTerm = stemmedDocuments.Where(d => d.Contains(vocabularyTerm, StringComparer.OrdinalIgnoreCase)).Count();
                idfDicionary[vocabularyTerm] = (double)Math.Log(((double)stemmedDocuments.Count()/ (double)numberOfDocsContainingTerm));
            }
            return idfDicionary;
        }

        /// <summary>
        ///  measure of similarity between two sequences of numbers.
        ///  For defining it, the sequences are viewed as vectors in an inner product space, 
        ///  and the cosine similarity is defined as the cosine of the angle between them,
        ///  that is, the dot product of the vectors divided by the product of their lengths
        /// </summary>
        /// <param name="qTfIdfVector"></param>
        /// <param name="vTfIdfVector"></param>
        /// <returns></returns>
        public static double ConsineSimilarity(IEnumerable<TfIdfValue> qTfIdfVector, IEnumerable<TfIdfValue> vTfIdfVector)
        {
            double qVectorLength = VectorLength(qTfIdfVector.Select(x => x.TfIdf));
            double vVectorLength = VectorLength(vTfIdfVector.Select(x => x.TfIdf));
            double dotProductResult = DotProduct(qTfIdfVector, vTfIdfVector);

            return dotProductResult / (qVectorLength * vVectorLength);
        }

        public static double DotProduct(IEnumerable<TfIdfValue> qTfIdfVector, IEnumerable<TfIdfValue> vTfIdfVector) 
        {
            double dotProduct = 0d;
            foreach (var qTfIdf in qTfIdfVector) 
            {
                foreach (var vTfIdf in  vTfIdfVector)
                {
                    if (qTfIdf.Term.Equals(vTfIdf.Term, StringComparison.OrdinalIgnoreCase)) 
                    {
                        dotProduct += vTfIdf.TfIdf * qTfIdf.TfIdf;
                    }
                }
            }

            return dotProduct;
        }

        /// <summary>
        /// Calculate vector length based on TF-IDF values
        /// </summary>
        /// <param name="tfidfVector"></param>
        /// <returns></returns>
        public static double VectorLength(IEnumerable<double> tfidfVector)
        {
            double len = 0d;

            foreach (var tfidfValue in tfidfVector)
            {
                len += (double)Math.Pow(tfidfValue, 2);
            }

            return Math.Sqrt(len);
        }

        /// <summary>
        /// Transform text to TF*IDF vectors
        /// </summary>
        /// <param name="stemmedDocs">Words stem</param>
        /// <param name="indexVocabulary">Words without stop words</param>
        /// <returns></returns>
        public static IEnumerable<IList<TfIdfValue>> TransformToTFIDFVectors(IEnumerable<IList<string>> stemmedDocs, ICollection<string> indexVocabulary) 
        {
            //Remove stop words
            DocumentCorpus.CleanTerms(ref indexVocabulary);

            var tfidfVectors = new List<IList<TfIdfValue>>(stemmedDocs.Count());
            var idfBag = InversDataFrequency(stemmedDocs, indexVocabulary);
            var idfValue = 0d;

            foreach (var docs in stemmedDocs)
            {
                var vector = new List<TfIdfValue>();
                foreach (var term in docs)
                {
                    double idf = idfBag.TryGetValue(term, out idfValue) ? idfBag[term] : 0d;
                    double tf = TermFrequency(term, docs);
                    vector.Add((new TfIdfValue(tf, idf,term)));
                }
                tfidfVectors.Add(vector);
            }
            return tfidfVectors;
        }
        /// <summary>
        /// Similarity Analysis
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cleanDocument"></param>
        public static void Analyze(string path, bool cleanDocument = false)
        {
            (var stemmedDocuments, var vocabulary) = DocumentCorpus.GetLocalDocumentVocabularyAsync(path).Result;

            if (cleanDocument) 
            {
                DocumentCorpus.CleanTerms(ref vocabulary);
            }

            var vectors = TransformToTFIDFVectors(stemmedDocuments, vocabulary);
            var qVector = vectors.Take(1).ToList();
            var dVectors = vectors.Skip(1).ToList();

            //Print TF*IDF result
            //Skip displaying for Query vector
            Console.WriteLine("TF-IDF values:");
            for (int i = 0; i < dVectors.Count; i++)
            {
                for (int j = 0; j < dVectors[i].Count; j++)
                {
                    Console.WriteLine($"Text = {i + 1}: Word = {dVectors[i][j].Term}; TF = {dVectors[i][j].Tf}; IDF = {dVectors[i][j].Idf}; TFIDF = {dVectors[i][j].TfIdf}");
                }
            }

            Console.WriteLine(Environment.NewLine);

            //Calculate vector lengths
            var vectorLengths = new List<double>();
            double vectorLength = VectorLength(qVector.SelectMany(x => x)
                .Select(y => y.TfIdf));

            vectorLengths.Add(vectorLength);

            Console.WriteLine($"Q vector length: {vectorLength}");
            Console.WriteLine("Vectors lengths:");

            for (int i = 0; i < dVectors.Count; i++)
            {
                vectorLength = VectorLength(dVectors.Skip(i).Take(1).SelectMany(x => x)
                .Select(y => y.TfIdf));

                vectorLengths.Add(vectorLength);

                Console.WriteLine($"D{i + 1}: {vectorLength}");
            }

            Console.WriteLine(Environment.NewLine);

            //Calculate Dot products
            Console.WriteLine("Dot products:");
            for (int i = 0; i < dVectors.Count; i++)
            {
                double dotProduct = DotProduct(qVector[0], dVectors[i]);
                Console.WriteLine($"D{i + 1}: {dotProduct}");
            }

            Console.WriteLine(Environment.NewLine);

            var similarityDocumentVector = new List<(string Document, double SimilarityValue)>();
            for (int i = 0; i < dVectors.Count; i++) 
            {
                var document = string.Join(" ", dVectors[i].Select(dv => dv.Term));
                var similarityValue = ConsineSimilarity(qVector[0], dVectors[i]);
                similarityDocumentVector.Add((document, similarityValue));
            }

            ///Sort by Similarity
            similarityDocumentVector.Sort((x, y) => x.SimilarityValue.CompareTo(y.SimilarityValue));
            Console.WriteLine("Similarity Analysis:");
            for (int i = similarityDocumentVector.Count() - 1; i >= 0; i--)
            {
                Console.WriteLine($"{similarityDocumentVector[i].Document}: {similarityDocumentVector[i].SimilarityValue}");
            }
        }

    }
}
