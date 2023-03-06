using TF_IDF;

const string fileName = "tfidf.txt";
var path = $"{Directory.GetCurrentDirectory()}\\{fileName}";

TFIDF.Analyze(path);
