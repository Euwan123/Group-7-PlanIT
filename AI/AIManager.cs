using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Data;
using Porter2Stemmer;

namespace PlanIT2
{
    // STUB CLASS: Represents the Word Embedding Model (Requires real model data/loading logic)
    public class WordEmbeddingModel
    {
        // Placeholder dimension size for the word vectors
        private const int VectorDimension = 100;
        private Dictionary<string, double[]> wordVectors;

        public WordEmbeddingModel()
        {
            // --- Placeholder Data for Semantic Demonstration ---
            wordVectors = new Dictionary<string, double[]>();
            Random rand = new Random(42);

            // Function to generate a random vector of the required dimension
            Func<double[]> GenerateVector = () =>
                Enumerable.Range(0, VectorDimension).Select(i => rand.NextDouble() * 2 - 1).ToArray();

            // Store vectors for common words that might appear in your knowledge base
            wordVectors.Add("earth", GenerateVector());
            wordVectors.Add("planet", GenerateVector());
            wordVectors.Add("sun", GenerateVector());
            wordVectors.Add("water", GenerateVector());
            wordVectors.Add("freeze", GenerateVector());
            wordVectors.Add("boil", GenerateVector());
            wordVectors.Add("bird", GenerateVector());
            wordVectors.Add("feather", GenerateVector());
            wordVectors.Add("organism", GenerateVector());

            // Add similar word pairs that should match (The essence of semantic AI)
            wordVectors.Add("develop", wordVectors["earth"].Select(v => v + 0.1).ToArray());
            wordVectors.Add("create", wordVectors["earth"].Select(v => v + 0.15).ToArray());
            wordVectors.Add("build", wordVectors["earth"].Select(v => v + 0.2).ToArray());
            // --------------------------------------------------
        }

        public double[] GetVector(string word)
        {
            if (wordVectors.TryGetValue(word, out var vector))
            {
                return vector;
            }
            // Return a zero vector for OOV (Out of Vocabulary) words
            return new double[VectorDimension];
        }

        public int Dimension => VectorDimension;
    }


    public class AIManager
    {
        private List<string> knowledgeBase;
        private List<string> processedKnowledge;

        // This constant MUST point to your file with the QA pairs.
        private const string KnowledgeFileName = "Knowledge.txt";
        private string knowledgeFilePath;

        private Dictionary<string, string> commonSenseAnswers;
        private Dictionary<string, string> qaPairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> definitionsMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private List<string> aiHistory = new List<string>();

        // Context Tracking
        private string lastSubject = null;

        // Semantic Matching Model
        private WordEmbeddingModel embeddingModel;

        private static readonly HashSet<string> StopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "the", "is", "am", "are", "was", "were", "be", "being", "been",
            "and", "or", "but", "if", "because", "as", "of", "at", "by", "for", "with",
            "about", "to", "from", "in", "out", "on", "off", "up", "down", "only",
            "who", "what", "where", "when", "why", "how", "which", "whom", "this", "that",
            "these", "those", "can", "will", "would", "shall", "should", "i", "me", "my",
            "you", "your", "he", "she", "it", "his", "her", "its", "we", "us", "our",
            "they", "them", "their", "not", "no", "do", "does", "did", "dont", "doent",
            "say", "says", "said", "also", "get", "go", "has", "have", "had", "just", "know",
            "like", "make", "made", "many", "most", "must", "need", "see", "seen", "take",
            "took", "want", "went", "well", "where", "why", "weve", "youve", "ill", "theres",
            "whatis", "whats", "s", "t", "m", "re", "ve", "ll"
        };


        public AIManager()
        {
            // Point to the new generalized knowledge file
            knowledgeFilePath = Path.Combine(Application.StartupPath, KnowledgeFileName);

            knowledgeBase = new List<string>();
            processedKnowledge = new List<string>();

            embeddingModel = new WordEmbeddingModel();

            LoadAdditionalKnowledge();
            InitializeCommonSense();
        }

        private void LoadAdditionalKnowledge()
        {
            if (!File.Exists(knowledgeFilePath)) return;
            var lines = File.ReadAllLines(knowledgeFilePath);
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Load explicit QA pairs (e.g., "What is rain? Rain is...")
                if (line.Contains("?"))
                {
                    int idx = line.IndexOf("?");
                    string question = line.Substring(0, idx + 1).Trim();
                    string answer = line.Substring(idx + 1).Trim();
                    string qkey = CleanText(question);
                    if (!qaPairs.ContainsKey(qkey))
                        qaPairs.Add(qkey, answer);
                }
                else
                {
                    // Load facts for definition and semantic search
                    knowledgeBase.Add(line);
                    processedKnowledge.Add(CleanText(line));
                    TryIndexDefinition(line);
                }
            }
        }

        private void TryIndexDefinition(string sentence)
        {
            var m = Regex.Match(sentence, @"^\s*(?:the\s+|a\s+|an\s+)?(?<subject>[\w\s]+?)\s+(?:is|are|means|refers to|refers|'s)\s+.+", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                string subj = m.Groups["subject"].Value.Trim();
                if (string.IsNullOrWhiteSpace(subj)) return;
                string key = NormalizeKey(subj);
                if (!definitionsMap.ContainsKey(key))
                    definitionsMap.Add(key, sentence);
                string withoutArticle = RemoveLeadingArticle(subj);
                string key2 = NormalizeKey(withoutArticle);
                if (!definitionsMap.ContainsKey(key2))
                    definitionsMap.Add(key2, sentence);
                if (key.EndsWith("s") && key.Length > 1)
                {
                    string sing = key.Substring(0, key.Length - 1);
                    if (!definitionsMap.ContainsKey(sing))
                        definitionsMap.Add(sing, sentence);
                }
                var tokens = key.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 1)
                {
                    string last = tokens[tokens.Length - 1];
                    if (!definitionsMap.ContainsKey(last))
                        definitionsMap.Add(last, sentence);
                }
            }
        }

        private void InitializeCommonSense()
        {
            commonSenseAnswers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "euwan", "Euwan is a talented developer and the creator of this application." },
                { "who are you", "I am PlanIT AI, a helpful assistant built to help you with your notes and plans." },
                { "what can you do", "I can answer questions from your notes and knowledge file, assist with planning, and even solve math." },
                { "hi", "Hello! How can I help you today?" },
                { "hello", "Hello! What can I do for you?" },
                { "how are you", "I am a program, so I don't have feelings, but I'm ready to help you with your tasks." }
            };
        }

        // MODIFIED: Merges all logic, with Semantic Matching as the primary retrieval method.
        public string GetAiResponse(string query, string notesContent)
        {
            if (string.IsNullOrWhiteSpace(query)) return "";
            string originalQuery = query;

            // 1. Contextual Query Rewriting (e.g., "rain" -> "what is rain", or "it" becomes "rain")
            query = ResolveContext(query);
            string cleanQuery = CleanText(query);


            string mathAnswer = TrySolveMath(originalQuery);
            if (mathAnswer != null)
            {
                return FormatResponseAndStore(mathAnswer, null);
            }

            // 2. Direct QA Match (This now catches "rain" -> "what is rain" -> answer)
            if (qaPairs.TryGetValue(cleanQuery, out string directAnswer))
                return FormatResponseAndStore(directAnswer, ExtractSubjectFromQuery(cleanQuery));

            // 3. Common Sense (Strictly matched)
            string commonAnswer = CheckCommonSense(cleanQuery);
            if (commonAnswer != null)
            {
                return FormatResponseAndStore(commonAnswer, ExtractSubjectFromQuery(cleanQuery));
            }

            // 4. Setup Knowledge Base
            var combinedKnowledge = new List<(string Original, string Processed)>();
            combinedKnowledge.AddRange(knowledgeBase.Zip(processedKnowledge, (orig, proc) => (orig, proc)));
            var noteSentences = Regex.Split(notesContent ?? "", @"(?<=[.!?])\s+(?=[A-Z])", RegexOptions.Multiline)
                                     .Where(s => !string.IsNullOrWhiteSpace(s))
                                     .Select(s => (Original: s.Trim(), Processed: CleanText(s)));
            combinedKnowledge.AddRange(noteSentences);

            // 5. Definition Check (for robust subject identification)
            string definitionAnswer = CheckDefinitions(cleanQuery, combinedKnowledge);
            if (definitionAnswer != null)
                return FormatResponseAndStore(definitionAnswer, ExtractSubjectFromQuery(cleanQuery));

            // 6. Semantic Matching (The core advanced retrieval)
            string bestSemanticMatch = FindBestSemanticMatch(cleanQuery, combinedKnowledge);
            if (bestSemanticMatch != null)
            {
                string subject = ExtractSubjectFromQuery(cleanQuery);
                return FormatResponseAndStore("Based on the meaning of your question, I found: " + bestSemanticMatch, subject);
            }

            // 7. Fallback to Suggest Closest Match (uses old TF-IDF logic for keyword fallback)
            string suggestion = SuggestClosestMatch(cleanQuery, combinedKnowledge);
            if (suggestion != null)
            {
                return FormatResponseAndStore("I do not have a strong match, but this might be related: " + suggestion, null);
            }

            // 8. Default Response
            return FormatResponseAndStore("I do not have information on that topic. Would you like me to remember the answer for next time?", null);
        }

        // NEW METHOD: FindBestSemanticMatch using Word Embeddings
        private string FindBestSemanticMatch(string query, List<(string Original, string Processed)> knowledge)
        {
            var documents = knowledge.Select(k => k.Processed).ToList();
            if (documents.Count == 0) return null;

            // 1. Get the Semantic Vector for the Query
            var queryVector = ToSemanticVector(query);

            double bestScore = 0;
            string bestMatch = null;

            // 2. Calculate Semantic Vector for each document and find the best match
            for (int i = 0; i < documents.Count; i++)
            {
                var docVector = ToSemanticVector(documents[i]);
                double score = CosineSimilarity(queryVector, docVector);

                // Use a higher threshold for semantic match (you can tune this)
                if (score > 0.75)
                {
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = knowledge[i].Original;
                    }
                }
            }
            return bestMatch;
        }

        // NEW METHOD: Creates an Averaged Word Vector (Semantic Vector)
        private double[] ToSemanticVector(string document)
        {
            var words = document.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double[] averageVector = new double[embeddingModel.Dimension];
            int wordCount = 0;

            foreach (var word in words)
            {
                var wordVector = embeddingModel.GetVector(word);

                if (wordVector.Any(v => v != 0))
                {
                    // Sum the vectors
                    for (int i = 0; i < embeddingModel.Dimension; i++)
                    {
                        averageVector[i] += wordVector[i];
                    }
                    wordCount++;
                }
            }

            // Average the summed vector
            if (wordCount > 0)
            {
                for (int i = 0; i < embeddingModel.Dimension; i++)
                {
                    averageVector[i] /= wordCount;
                }
            }

            return averageVector;
        }

        // CORRECTED: ResolveContext for conversation flow - Fixed variable naming conflict
        private string ResolveContext(string query)
        {
            // Use 'lowerQuery' in the outer scope
            string lowerQuery = query.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(lastSubject))
            {
                // Logic for generalization: if query is a short subject (e.g., "rain", "sun"), 
                // rephrase it as a question to hit the QA pairs.

                // Use 'cleanSubject' here to avoid conflict with 'lowerQuery'
                string cleanSubject = CleanText(query);

                // Check if the query is short (1-2 clean words), not already a question, and not a command.
                if (cleanSubject.Split(' ').Length <= 2 && !query.Contains("?") && !query.Contains("!") && !lowerQuery.StartsWith("what ") && !lowerQuery.StartsWith("who "))
                {
                    // If the user just types "rain", rewrite it as "what is rain"
                    return $"what is {query}";
                }
                return query;
            }

            // Continue with co-reference resolution if lastSubject is set
            string[] vagueStarts = { "what is it", "what's it", "tell me about it", "how about it", "its ", "it's " };
            string[] pronouns = { "it", "he", "she", "they" };

            // Check for pronoun usage using 'lowerQuery'
            if (pronouns.Contains(lowerQuery) || vagueStarts.Any(s => lowerQuery.StartsWith(s)) || Regex.IsMatch(lowerQuery, @"^\s*(its|it's|their|his|her)\s+[\w\s]+\s*[\.!?]?\s*$", RegexOptions.IgnoreCase))
            {
                string rewrittenQuery = query.ToLower()
                                             .Replace("what is its", $"what is {lastSubject}'s")
                                             .Replace("what's its", $"what is {lastSubject}'s")
                                             .Replace("tell me about it", $"tell me about {lastSubject}")
                                             .Replace("what is it", $"what is {lastSubject}")
                                             .Replace("how about it", $"how about {lastSubject}")
                                             .Replace("its ", $"{lastSubject}'s ")
                                             .Replace("it's ", $"{lastSubject}'s ")
                                             .Replace("it", lastSubject);

                if (rewrittenQuery.Equals(query, StringComparison.OrdinalIgnoreCase))
                {
                    return $"what is {lastSubject}";
                }

                return rewrittenQuery;
            }

            return query;
        }


        // MODIFIED: CheckCommonSense (Uses CleanText output for strict matching)
        private string CheckCommonSense(string cleanQuery)
        {
            foreach (var pair in commonSenseAnswers)
            {
                string cleanKey = CleanText(pair.Key);

                // Option 1: Exact Match
                if (cleanQuery.Equals(cleanKey, StringComparison.OrdinalIgnoreCase))
                    return pair.Value;

                // Option 2: Loose Match
                if (cleanQuery.Contains(cleanKey))
                    return pair.Value;
            }
            return null;
        }

        private string CheckDefinitions(string cleanQuery, List<(string Original, string Processed)> combinedKnowledge)
        {
            string subject = ExtractSubjectFromQuery(cleanQuery);
            if (string.IsNullOrWhiteSpace(subject))
            {
                if (cleanQuery.Split(' ').Length <= 3)
                    subject = cleanQuery;
                else
                    return null;
            }

            string key = NormalizeKey(subject);
            if (definitionsMap.TryGetValue(key, out string val))
                return val;

            if (key.EndsWith("s") && key.Length > 1)
            {
                string sing = key.Substring(0, key.Length - 1);
                if (definitionsMap.TryGetValue(sing, out val)) return val;
            }
            else
            {
                string plural = key + "s";
                if (definitionsMap.TryGetValue(plural, out val)) return val;
            }

            var best = definitionsMap.Keys
                        .Select(k => new { Key = k, Score = WordOverlap(k, key) })
                        .OrderByDescending(x => x.Score)
                        .FirstOrDefault();
            if (best != null && best.Score > 0)
                return definitionsMap[best.Key];

            foreach (var item in combinedKnowledge)
            {
                string s = item.Processed;
                if (s.Contains(key) && (s.Contains(" is ") || s.Contains(" are ") || s.Contains(" means ") || s.Contains(" refers to ")))
                    return item.Original;
            }

            var scored = combinedKnowledge.Select(k => new { k.Original, Score = WordOverlap(k.Processed, key) })
                                         .OrderByDescending(x => x.Score)
                                         .FirstOrDefault();
            if (scored != null && scored.Score > 0)
                return scored.Original;

            return null;
        }

        private string ExtractSubjectFromQuery(string cleanQuery)
        {
            string q = cleanQuery.Trim();
            string[] prefixes = new[] { "what is ", "what are ", "who is ", "who are ", "define ", "tell me about ", "whats ", "whatis ", "what " };
            foreach (var p in prefixes)
            {
                if (q.StartsWith(p))
                {
                    q = q.Substring(p.Length).Trim();
                    break;
                }
            }
            q = q.Trim();
            return q;
        }

        private string NormalizeKey(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            string noArticle = RemoveLeadingArticle(s);
            return CleanText(noArticle);
        }

        private string RemoveLeadingArticle(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            string t = s.Trim().ToLower();
            if (t.StartsWith("the ")) return s.Substring(4);
            if (t.StartsWith("a ")) return s.Substring(2);
            if (t.StartsWith("an ")) return s.Substring(3);
            return s;
        }

        private int WordOverlap(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return 0;
            var wa = a.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var wb = b.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return wa.Intersect(wb).Count();
        }

        private string TrySolveMath(string query)
        {
            try
            {
                string expr = query.ToLower().Replace("what is", "").Replace("calculate", "").Replace("solve", "").Trim();
                expr = expr.Replace("×", "*").Replace("x", "*").Replace("÷", "/").Replace("^", "**");
                if (expr.Contains("sqrt"))
                {
                    var match = Regex.Match(expr, @"sqrt\(?([0-9.]+)\)?");
                    if (match.Success)
                    {
                        double val = double.Parse(match.Groups[1].Value);
                        return $"The square root of {val} is {Math.Sqrt(val)}";
                    }
                }
                expr = Regex.Replace(expr, @"[^0-9+\-*/().]", "");
                if (string.IsNullOrWhiteSpace(expr)) return null;
                DataTable dt = new DataTable();
                var result = dt.Compute(expr, "");
                return $"{query.Trim()} = {result}";
            }
            catch
            {
                return null;
            }
        }

        // This is a fallback to the old TF-IDF logic for suggestion/low-relevance keyword matching.
        private string SuggestClosestMatch(string query, List<(string Original, string Processed)> knowledge)
        {
            var queryWords = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Where(w => w.Length > 2)
                                  .ToList();
            if (!queryWords.Any()) return null;
            var documents = knowledge.Select(k => k.Processed).ToList();
            if (documents.Count == 0) return null;

            // TF-IDF Calculation
            var vocabulary = documents.SelectMany(d => d.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).Distinct().ToList();
            var docVectors = documents.Select(d => ToTfidfVector(d, vocabulary, documents)).ToList();
            var queryVector = ToTfidfVector(string.Join(" ", queryWords), vocabulary, documents);
            var ranked = knowledge.Select((k, i) => new
            {
                k.Original,
                Score = CosineSimilarity(queryVector, docVectors[i])
            }).OrderByDescending(x => x.Score).ToList();
            return ranked.FirstOrDefault(x => x.Score > 0.1)?.Original;
        }

        private double[] ToTfidfVector(string document, List<string> vocabulary, List<string> allDocs)
        {
            var words = document.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double[] vector = new double[vocabulary.Count];
            for (int i = 0; i < vocabulary.Count; i++)
            {
                string term = vocabulary[i];
                double tf = words.Count(w => w == term);
                if (tf > 0) tf = 1 + Math.Log10(tf);
                double idf = Math.Log10((double)allDocs.Count / (1 + allDocs.Count(d => d.Contains(term))));
                vector[i] = tf * idf;
            }
            return vector;
        }

        private double CosineSimilarity(double[] v1, double[] v2)
        {
            double dot = 0, mag1 = 0, mag2 = 0;
            int length = Math.Min(v1.Length, v2.Length);

            for (int i = 0; i < length; i++)
            {
                dot += v1[i] * v2[i];
                mag1 += v1[i] * v1[i];
                mag2 += v2[i] * v2[i];
            }
            if (mag1 == 0 || mag2 == 0) return 0;
            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }

        // CleanText now includes Stemming and Stop Word Removal
        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string cleaned = Regex.Replace(text.ToLower(), "[^a-z0-9 ]", " ").Trim();
            var tokens = cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var stemmer = new EnglishPorter2Stemmer();
            var finalTokens = new List<string>();

            foreach (var token in tokens)
            {
                if (StopWords.Contains(token))
                {
                    continue;
                }

                if (token.Length > 2)
                {
                    finalTokens.Add(stemmer.Stem(token).Value);
                }
                else
                {
                    finalTokens.Add(token);
                }
            }
            return string.Join(" ", finalTokens);
        }

        private string FormatResponseAndStore(string response, string subject)
        {
            if (!string.IsNullOrWhiteSpace(subject))
            {
                string cleanSubject = CleanText(subject);
                if (!string.IsNullOrWhiteSpace(cleanSubject) && !StopWords.Contains(cleanSubject.Split(' ').First()))
                {
                    lastSubject = subject.Trim();
                }
            }

            aiHistory.Add($"AI: {response}");
            return response;
        }

        public string GetHistory()
        {
            return string.Join(Environment.NewLine, aiHistory);
        }

        public void ClearHistory()
        {
            aiHistory.Clear();
            lastSubject = null;
        }

        public void SaveHistory(string filePath)
        {
            File.WriteAllLines(filePath, aiHistory);
        }

        public void LoadHistory(string filePath)
        {
            if (File.Exists(filePath))
                aiHistory = File.ReadAllLines(filePath).ToList();
        }

        public void TeachAI(string info)
        {
            if (string.IsNullOrWhiteSpace(info)) return;
            knowledgeBase.Add(info);
            processedKnowledge.Add(CleanText(info));
            TryIndexDefinition(info);
            File.AppendAllText(knowledgeFilePath, Environment.NewLine + info);
        }
    }
}
