using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

//Turns text into the token ids the GoEmotions (RoBERTa) model understands.
//Plain C# with no Unity code, so it can be tested on its own. It reproduces Hugging Face's
//byte-level BPE tokenizer: split text into chunks, map each UTF-8 byte to a printable character,
//then repeatedly merge the highest-priority pair of symbols until no listed merge applies.
public class RobertaTokenizer
{
    public const int StartTokenId = 0; // <s>
    public const int EndTokenId = 2;   // </s>
    public const int UnknownTokenId = 3; // <unk>

    //Same chunking pattern as GPT-2/RoBERTa: contractions, words, numbers, symbols, whitespace
    private static readonly Regex ChunkPattern = new Regex(
        @"'s|'t|'re|'ve|'m|'ll|'d| ?\p{L}+| ?\p{N}+| ?[^\s\p{L}\p{N}]+|\s+(?!\S)|\s+",
        RegexOptions.Compiled);

    private static readonly string[] ByteToSymbol = BuildByteToSymbol();

    private readonly Dictionary<string, int> tokenIds = new Dictionary<string, int>();
    private readonly Dictionary<string, int> mergeRanks = new Dictionary<string, int>();
    private readonly Dictionary<string, int[]> cache = new Dictionary<string, int[]>();
    private readonly int maxLength;

    //vocabLines: one token per line, line number = token id. mergeLines: "a b" pairs, highest priority first.
    public RobertaTokenizer(IList<string> vocabLines, IList<string> mergeLines, int maxLength = 512)
    {
        for (int id = 0; id < vocabLines.Count; id++)
        {
            tokenIds[vocabLines[id]] = id;
        }
        for (int rank = 0; rank < mergeLines.Count; rank++)
        {
            mergeRanks[mergeLines[rank]] = rank;
        }
        this.maxLength = maxLength;
    }

    //Builds the tokenizer from file text (handles both \n and \r\n line endings)
    public static RobertaTokenizer FromText(string vocabText, string mergesText, int maxLength = 512)
    {
        return new RobertaTokenizer(SplitLines(vocabText), SplitLines(mergesText), maxLength);
    }

    //Returns <s> + tokens + </s>, cut to maxLength
    public int[] Encode(string text)
    {
        List<int> ids = new List<int> { StartTokenId };
        foreach (Match chunk in ChunkPattern.Matches(text ?? ""))
        {
            ids.AddRange(EncodeChunk(chunk.Value));
        }
        if (ids.Count > maxLength - 1)
        {
            ids.RemoveRange(maxLength - 1, ids.Count - (maxLength - 1));
        }
        ids.Add(EndTokenId);
        return ids.ToArray();
    }

    private int[] EncodeChunk(string chunk)
    {
        if (cache.TryGetValue(chunk, out int[] cached))
        {
            return cached;
        }

        //Each UTF-8 byte becomes one printable symbol, e.g. a space becomes "Ġ"
        List<string> symbols = new List<string>();
        foreach (byte b in Encoding.UTF8.GetBytes(chunk))
        {
            symbols.Add(ByteToSymbol[b]);
        }

        //Merge the pair with the best (lowest) rank until no pair has a rank
        while (symbols.Count > 1)
        {
            int bestRank = int.MaxValue;
            string bestPair = null;
            for (int i = 0; i < symbols.Count - 1; i++)
            {
                string pair = symbols[i] + " " + symbols[i + 1];
                if (mergeRanks.TryGetValue(pair, out int rank) && rank < bestRank)
                {
                    bestRank = rank;
                    bestPair = pair;
                }
            }
            if (bestPair == null)
            {
                break;
            }

            //Merge every occurrence of the best pair, left to right
            int split = bestPair.IndexOf(' ');
            string first = bestPair.Substring(0, split);
            string second = bestPair.Substring(split + 1);
            List<string> merged = new List<string>();
            for (int i = 0; i < symbols.Count; i++)
            {
                if (i < symbols.Count - 1 && symbols[i] == first && symbols[i + 1] == second)
                {
                    merged.Add(first + second);
                    i++;
                }
                else
                {
                    merged.Add(symbols[i]);
                }
            }
            symbols = merged;
        }

        int[] result = new int[symbols.Count];
        for (int i = 0; i < symbols.Count; i++)
        {
            result[i] = tokenIds.TryGetValue(symbols[i], out int id) ? id : UnknownTokenId;
        }
        cache[chunk] = result;
        return result;
    }

    //GPT-2's byte-to-character table: printable bytes keep their character, the rest move to 256+
    private static string[] BuildByteToSymbol()
    {
        string[] table = new string[256];
        int next = 0;
        for (int b = 0; b < 256; b++)
        {
            bool printable = (b >= '!' && b <= '~') || (b >= 0xA1 && b <= 0xAC) || (b >= 0xAE && b <= 0xFF);
            table[b] = printable ? ((char)b).ToString() : ((char)(256 + next++)).ToString();
        }
        return table;
    }

    private static List<string> SplitLines(string text)
    {
        List<string> lines = new List<string>(text.Split('\n'));
        for (int i = 0; i < lines.Count; i++)
        {
            lines[i] = lines[i].TrimEnd('\r');
        }
        if (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }
        return lines;
    }
}
