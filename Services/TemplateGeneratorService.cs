using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using AISIots.Interfaces;
using AISIots.Models.DbTables;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AISIots.Services
{
    public class TemplateGeneratorService : ITemplateGeneratorService
    {

    public async Task<(Stream stream, string fileName)> GenerateDocumentAsync(Rpd rpd, string templateName)
    {
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", templateName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}");
        }

        var templateBytes = await File.ReadAllBytesAsync(templatePath);

        using var documentStream = new MemoryStream();
        await documentStream.WriteAsync(templateBytes);
        documentStream.Position = 0;

        using (var document = WordprocessingDocument.Open(documentStream, true))
        {
            var body = document.MainDocumentPart.Document.Body;
            var replacements = CreateReplacementsDictionary(rpd);
            
            ApplyReplacementsToBody(body, replacements);
            
            document.Save();
        }

        var fileName = GenerateFileName(rpd, templateName);
        return (new MemoryStream(documentStream.ToArray()), fileName);
    }

        private Dictionary<string, string> CreateReplacementsDictionary(Rpd rpd)
        {
            var replacements = new Dictionary<string, string>();
            var aliasMap = new Dictionary<string, string>
            {
                { nameof(Rpd.Fos), "fosb" },
                { nameof(Rpd.FosItog), "fositog" },
                { nameof(Rpd.DopProgObesp), "dopprogrobesp" }
            };

            var properties = typeof(Rpd).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (IsIgnoredProperty(prop.Name)) continue;

                string baseKey = aliasMap.TryGetValue(prop.Name, out var alias) 
                    ? alias.ToLower() 
                    : prop.Name.ToLower();

                AddPropertyReplacements(replacements, rpd, prop, baseKey);
            }
            return replacements;
        }

        private bool IsIgnoredProperty(string propName) => 
            propName is nameof(Rpd.Id) or nameof(Rpd.UpdateDateTime) or nameof(Rpd.IsDeleted) or nameof(Rpd.GetFormatedDateTime);

        private void AddPropertyReplacements(Dictionary<string, string> replacements, Rpd rpd, PropertyInfo prop, string baseKey)
        {
            if (prop.PropertyType == typeof(string))
            {
                var value = (string?)prop.GetValue(rpd);
                replacements[$"{{{{{baseKey}}}}}"] = value ?? string.Empty;
            }
            else if (prop.PropertyType == typeof(List<string>))
            {
                var list = (List<string>?)prop.GetValue(rpd);
                if (list == null) return;

                for (int i = 0; i < list.Count; i++)
                {
                    replacements[$"{{{{{baseKey}{i + 1}}}}}"] = list[i] ?? string.Empty;
                }
            }
        }

        private void ApplyReplacementsToBody(Body body, Dictionary<string, string> replacements)
        {
            foreach (var paragraph in body.Descendants<Paragraph>().ToList())
            {
                ProcessParagraphReplacements(paragraph, replacements);
            }
        }

        private void ProcessParagraphReplacements(Paragraph paragraph, Dictionary<string, string> replacements)
        {
            var runsInParagraph = paragraph.Elements<Run>().ToList();
            if (runsInParagraph.Count == 0) return;

            var (runTextPieces, runStartOffsets, paragraphText) = BuildParagraphTextMap(runsInParagraph);
            if (string.IsNullOrEmpty(paragraphText)) return;

            foreach (var kvp in replacements)
            {
                var placeholder = kvp.Key;
                if (string.IsNullOrEmpty(placeholder)) continue;

                int idx = IndexOfIgnoreCase(paragraphText, placeholder);
                if (idx < 0) continue;

                var replacement = kvp.Value ?? string.Empty;
                var endExclusive = idx + placeholder.Length;

                int startRunIndex = FindRunIndexByGlobalCharIndex(runStartOffsets, runTextPieces, idx);
                int endRunIndex = FindRunIndexByGlobalCharIndex(runStartOffsets, runTextPieces, endExclusive - 1);

                if (startRunIndex < 0 || endRunIndex < 0 || startRunIndex >= runsInParagraph.Count || endRunIndex >= runsInParagraph.Count)
                {
                    continue;
                }

                UpdateParagraphRuns(paragraph, runsInParagraph, startRunIndex, endRunIndex, idx, endExclusive, replacement, runStartOffsets, runTextPieces);

                // Refresh map after modification
                runsInParagraph = paragraph.Elements<Run>().ToList();
                (runTextPieces, runStartOffsets, paragraphText) = BuildParagraphTextMap(runsInParagraph);
                if (string.IsNullOrEmpty(paragraphText)) break;
            }
        }

        private (List<string> pieces, List<int> offsets, string fullText) BuildParagraphTextMap(List<Run> runs)
        {
            var pieces = new List<string>(runs.Count);
            var offsets = new List<int>(runs.Count);
            int offset = 0;

            foreach (var run in runs)
            {
                var piece = string.Concat(run.Descendants<Text>().Select(t => t.Text));
                pieces.Add(piece);
                offsets.Add(offset);
                offset += piece.Length;
            }

            return (pieces, offsets, string.Concat(pieces));
        }

        private void UpdateParagraphRuns(Paragraph paragraph, List<Run> runs, int startIdx, int endIdx, int globalIdx, int globalEnd, string replacement, List<int> offsets, List<string> pieces)
        {
            // 1. Handle the first run: Keep text before placeholder, insert replacement
            var firstRun = runs[startIdx];
            var firstRunTexts = firstRun.Descendants<Text>().ToList();
            
            if (firstRunTexts.Any())
            {
                int localStart = globalIdx - offsets[startIdx];
                var localOriginal = pieces[startIdx];
                var textBefore = localOriginal.Substring(0, Math.Clamp(localStart, 0, localOriginal.Length));
                
                foreach (var t in firstRunTexts) t.Remove();
                firstRun.AppendChild(new Text(textBefore + replacement));
            }
            else
            {
                firstRun.AppendChild(new Text(replacement));
            }

            // 2. Handle the last run: Keep text after the placeholder
            if (endIdx < runs.Count)
            {
                var lastRun = runs[endIdx];
                var lastRunTexts = lastRun.Descendants<Text>().ToList();
                if (lastRunTexts.Any())
                {
                    int localEnd = globalEnd - offsets[endIdx];
                    var localOriginal = pieces[endIdx];
                    var textAfter = localOriginal.Substring(Math.Clamp(localEnd, 0, localOriginal.Length));
                    
                    // We will move this 'textAfter' to the firstRun to keep the document structure simple
                    // and then delete the lastRun and everything in between.
                    var firstRunText = firstRun.Descendants<Text>().FirstOrDefault();
                    if (firstRunText != null)
                    {
                        firstRunText.Text += textAfter;
                    }
                    else
                    {
                        firstRun.AppendChild(new Text(textAfter));
                    }
                }
            }

            // 3. Delete all runs from startIdx + 1 up to endIdx
            for (int i = startIdx + 1; i <= endIdx; i++)
            {
                if (i < runs.Count)
                {
                    runs[i].Remove();
                }
            }
        }

        private static int IndexOfIgnoreCase(string source, string value)
        {
            return source.IndexOf(value, StringComparison.InvariantCultureIgnoreCase);
        }

        private static int FindRunIndexByGlobalCharIndex(List<int> runStartOffsets, List<string> runTextPieces, int globalIndex)
        {
            for (int i = 0; i < runTextPieces.Count; i++)
            {
                var len = runTextPieces[i].Length;
                if (globalIndex >= runStartOffsets[i] && globalIndex < runStartOffsets[i] + len)
                {
                    return i;
                }
            }
            return -1;
        }

        private string GenerateFileName(Rpd rpd, string templateName)
        {
            string prefix = templateName.StartsWith("fos", StringComparison.OrdinalIgnoreCase) ? "ФОС" : "РПД";
            var safeTitle = Regex.Replace(rpd.Title, @"[<>:""\/\\|?*]+", "_");
            if (string.IsNullOrWhiteSpace(safeTitle))
                safeTitle = "document";

            return $"{prefix}. {safeTitle}.docx";
        }
    }
}
