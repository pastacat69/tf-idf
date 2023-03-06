using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace TF_IDF
{
    internal class DocumentCorpus
    {
        /// <summary>
        /// Read file in asynchronous way and splitting line by spaces.
        /// This method use steamming -  reducing inflected (or sometimes derived) words to their word stem, base or root form
        /// </summary>
        /// <param name="path">Local file with data for futher analyzing</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static async Task<(IList<IList<string>>, ICollection<string>)> GetLocalDocumentVocabularyAsync(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File does not exists");
            }
            var _documents = new List<IList<string>>();
            var _vocabulary = new HashSet<string>();

            using (var streamReader = File.OpenText(path))
            {
                var line = string.Empty;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        //Stemming
                        var terms = Regex.Matches(line, "([a-zA-Z]+)")
                           .Where(g => g.Groups.Count > 1)
                           .Select(g => g.Value)
                           .ToList();

                        _documents.Add(terms);

                        foreach (var term in terms)
                        {
                            _vocabulary.Add(term);
                        }
                    }
                }
            }
            return (_documents, _vocabulary);
        }

        /// <summary>
        /// Get all stop world list terms like “the, is, at, on”, etc.
        /// Load from application configuration
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string?> LoadStopWordList()
        {
            var config = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("local.config.json", optional: true, reloadOnChange: true)

                     .Build();

            var stppWordList = config.GetSection("StopWordList")
                .AsEnumerable()
                .Select(kvp => kvp.Value);

            return stppWordList ??= new List<string>();
        }

        /// <summary>
        /// Remove from list stop words
        /// </summary>
        /// <param name="terms"></param>
        /// <param name="stopWords"></param>
        public static void CleanTerms(ref ICollection<string> terms, IEnumerable<string?> stopWords = null)
        {
            stopWords ??= LoadStopWordList();
            string[] copyTerms = new string[terms.Count];

            terms.CopyTo(copyTerms, 0);

            for (int i = 0; i < copyTerms.Length; i++)
            {
                if (stopWords.Contains(copyTerms[i], StringComparer.CurrentCultureIgnoreCase))
                {
                    terms.Remove(copyTerms[i]);
                }
            }
        }
    }
}
