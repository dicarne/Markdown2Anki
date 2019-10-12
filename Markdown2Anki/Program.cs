using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings;

namespace Markdown2Anki
{
    class Program
    {
        static void Main(string[] _args)
        {
            var FlagA = "### Q";
            var FlagB = "### A";
            var path = "unknown.md";
            try
            {
                List<string> args = new List<string>(_args);
                path = args[0];
                // TODO: 完成更多参数化配置，例如自定义标识符等
            }
            catch (Exception r)
            {
                Console.WriteLine(r.Message);
                throw;
            }
            var lines = File.ReadAllLines(path);
            var flags = new List<string>();
            flags.Add(FlagA);
            flags.Add(FlagB);
            var cards = SplitCards(lines, flags.ToArray());
            ConvertToAnkiJax(cards);

            var tab = UTF8Encoding.UTF8.GetBytes("\t");
            var n = UTF8Encoding.UTF8.GetBytes("\n");
            using (var file = new FileStream(path + "output.csv", FileMode.Create))
            {
                foreach (var item in cards)
                {
                    foreach (var p in item.Parts)
                    {
                        foreach (var l in p.lines)
                        {
                            var by = UTF8Encoding.UTF8.GetBytes(l);
                            file.Write(by ,0,by.Length);
                        }
                        file.Write(tab, 0, tab.Length);
                    }
                    file.Write(n, 0, n.Length);
                }
                
            }

        }

        /// <summary>
        /// 用分割线---将每张卡片分开，并分割每个字段
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="Flags">所有字段标识符</param>
        /// <returns></returns>
        static List<Card> SplitCards(string[] lines, string[] Flags)
        {
            var result = new List<Card>();

            for (int i = 0; i < lines.Length; i++)
            {
                bool hasARealCard = false;

                int j = 0;
                Card card = new Card();
                for (var _t = 0; _t < Flags.Length; _t++)
                    card.Parts.Add(new Card.Part());

                List<string> linesInCard = new List<string>();
                int part_now = -1;
                for (; j < lines.Length - i; j++)
                {
                    bool find = false;
                    if (lines[i + j].StartsWith("---"))
                    {
                        if (part_now != -1 && linesInCard.Count > 0)
                        {
                            card.Parts[part_now].lines = linesInCard;
                            linesInCard = new List<string>();
                            hasARealCard = true;
                        }
                        j++;
                        break;
                    }
                    for (var ft = 0; ft < Flags.Length; ft++)
                    {
                        if (lines[i + j].StartsWith(Flags[ft]))
                        {
                            if (part_now != -1 && linesInCard.Count > 0)
                            {
                                card.Parts[part_now].lines = linesInCard;
                                linesInCard = new List<string>();
                                hasARealCard = true;
                            }
                            part_now = ft;
                            find = true;
                        }
                    }
                    if (!find && part_now >= 0)
                    {
                        linesInCard.Add(lines[i + j]);
                    }

                }
                i = i + j - 1;
                if (hasARealCard) result.Add(card);

            }

            return result;
        }

        /// <summary>
        /// 转换$...$到\(...\)，$$...$$到\[...\]
        /// </summary>
        /// <param name="cards"></param>
        static void ConvertToAnkiJax(List<Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                for (int j = 0; j < cards[i].Parts.Count; j++)
                {
                    for (int k = 0; k < cards[i].Parts[j].lines.Count; k++)
                    {
                        string old = cards[i].Parts[j].lines[k];
                        var sb = new StringBuilder(old);
                        bool open = false;
                        for (int z = 0; z < sb.Length; z++)
                        {
                            if (sb[z] == '$')
                            {
                                if (z + 1 < sb.Length && sb[z + 1] == '$')
                                {
                                    if (open)
                                    {
                                        sb[z] = '\\';
                                        sb[z + 1] = ']';
                                        open = false;
                                    }
                                    else
                                    {
                                        sb[z] = '\\';
                                        sb[z + 1] = '[';
                                        open = true;
                                    }
                                }
                                else
                                {
                                    if (open)
                                    {
                                        sb[z] = '\\';
                                        sb.Insert(z + 1, ')');
                                        open = false;
                                    }
                                    else
                                    {
                                        sb[z] = '\\';
                                        sb.Insert(z + 1, '(');
                                        open = true;
                                    }
                                }
                            }
                        }
                        sb.Append("<br>");
                        cards[i].Parts[j].lines[k] = sb.ToString();

                    }
                }
            }
        }
    }

    class Card
    {
        public List<Part> Parts = new List<Part>();
        public class Part
        {
            public List<string> lines;
        }
    }
}
