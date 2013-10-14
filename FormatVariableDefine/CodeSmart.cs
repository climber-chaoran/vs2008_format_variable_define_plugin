using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.Collections;

namespace FormatVariableDefine
{
    public class CodeSmart
    {
        /// <summary>
        /// Aligns the text.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="alignHeader">The align header.</param>
        /// <returns></returns>
        public static string AlignText(string inputText)
        {
            if (inputText.Length == 0)
                return inputText;

            inputText = inputText.Replace("\t", "    ");
            string[] stringLines =inputText.Split('\n');
            StringBuilder outCode = new StringBuilder();
            List<string> lstString = new List<string>();
            Dictionary<int, int> dctPreSpace = new Dictionary<int, int>();
            int nMaxLenPart1 = 0;
            int nMaxLenPart2 = 0;
            int nMaxLenOnlyHavePart2 = 0;
            int nRow = 0;
            foreach (string line in stringLines)
            {
                nRow++;
                string part1 = "";
                string part2 = "";
                string part3 = "";
                int nPreSpace = Split3Part(line, ref part1, ref part2, ref part3);
                //if (0 != nPreSpace)
                {
                    if (dctPreSpace.ContainsKey(nPreSpace))
                        dctPreSpace[nPreSpace]++;
                    else
                        dctPreSpace[nPreSpace] = 1;
                }

                if (part1.Length > nMaxLenPart1)
                    nMaxLenPart1 = part1.Length;

                if (part1.Length > 0 && part2.Length > nMaxLenPart2)
                    nMaxLenPart2 = part2.Length;
                else if (part1.Length == 0 && part2.Length > nMaxLenOnlyHavePart2)
                    nMaxLenOnlyHavePart2 = part2.Length;

                //if (part1.Length + part2.Length > nMaxLenPart2)
                //    nMaxLenPart2 = part1.Length + part2.Length;

                lstString.Add(part1);
                lstString.Add(part2);
                lstString.Add(part3);
            }

            if (0 != nMaxLenPart1)
            {
                nMaxLenPart1 += 4;
                nMaxLenPart1 -= nMaxLenPart1 % 4;
            }
            nMaxLenPart2 += 4;
            nMaxLenPart2 -= nMaxLenPart2 % 4;
            nMaxLenOnlyHavePart2 += 4;
            nMaxLenOnlyHavePart2 -= nMaxLenOnlyHavePart2 % 4;
            if (nMaxLenOnlyHavePart2 > nMaxLenPart1 + nMaxLenPart2)
            {
                nMaxLenPart2 = nMaxLenOnlyHavePart2 - nMaxLenPart1;
            }

            int nPreSpaceLen = 0;
            int nAppearCount = 0;
            foreach (KeyValuePair<int, int> item in dctPreSpace)
            {
                if (item.Value > nAppearCount)
                {
                    nPreSpaceLen = item.Key;
                    nAppearCount = item.Value;
                }
            }
            string strPreSpace = "";
            for (int i = 0; i < nPreSpaceLen; i++)
                strPreSpace += ' ';
            int j = 0;
            for (int i = 0; i < nRow; i++)
            {
                string part1 = lstString[j++];
                for (int n = part1.Length; n != 0 && n < nMaxLenPart1; n++)
                    part1 += ' ';

                string part2 = lstString[j++];

                if (0 == part1.Length)
                {
                    for (int n = part2.Length; n < nMaxLenPart1 + nMaxLenPart2; n++)
                        part2 += ' ';
                } 
                else
                {
                    for (int n = part2.Length; n < nMaxLenPart2; n++)
                        part2 += ' ';
                }

                string part3 = lstString[j++];

                string outstr = String.Concat(strPreSpace, part1, part2, part3);
                outstr = outstr.TrimEnd();
                outstr += "\r\n";
                outCode.Append(outstr);
            }

            string outCodeString = outCode.ToString();
            return outCodeString.Substring(0, outCodeString.Length - 2);// remove the last '\r\n'
        }

        /// <summary>
        /// Gets the last align header position.
        /// </summary>
        /// <param name="alignHeader">The align header.</param>
        /// <param name="stringLines">The string lines.</param>
        /// <returns></returns>
        private static int GetLastAlignHeaderPosition(string alignHeader, string[] stringLines)
        {
            int alignHeaderMaxPos = 0;
            foreach (string line in stringLines)
            {
                int pos           = line.IndexOf(alignHeader);
                alignHeaderMaxPos = pos > alignHeaderMaxPos ? pos : alignHeaderMaxPos;
            }
            return alignHeaderMaxPos;
        }

        private static int Split3Part(string line, ref string part1, ref string part2, ref string part3)
        {
            int nPreSpaceNum = line.Length - line.TrimStart().Length;
            bool bHaveSemicolon = false;
            line = line.Trim();
            if (0 == line.Length)
            {
                part3 = part2 = part1 = "";
                return 0;
            }
            if (line[line.Length-1] == ';')
            {
                bHaveSemicolon = true;
                line = line.Substring(0, line.Length - 1);
                line = line.Trim();
            }

            int nPos = line.LastIndexOf('=');
            if (-1 == nPos)
            {
                part3 = "";
            }
            else 
            {
                part3 = "= ";
                part3 += line.Substring(nPos+1).Trim();
                line = line.Substring(0, nPos);
                line = line.Trim();
            }

            if (part3.Length > 0 && bHaveSemicolon)
            {
                bHaveSemicolon = false;
                part3 += ';';
            }

            // 向前找表示指针的*，表示模板的>，表示分割的空格，来确定part2
            int nPosPoint       = line.LastIndexOf('*');
            int nPostTemplete   = line.LastIndexOf('>');
            int nPosSpace       = line.LastIndexOf(' ');
            nPos = nPosPoint > nPostTemplete ? nPosPoint : nPostTemplete;
            
            if (-1 == nPos || '*' != line[nPos])
                nPos = nPos > nPosSpace ? nPos : nPosSpace;

            if (-1 == nPos)
            {
                part2 = line;
                part1 = "";
            }
            else
            {
                if ('*' == line[nPos])
                {
                    int nStarNum = 0;
                    while (nPos >= 0 && '*' == line[nPos])
                    {
                        nPos--;
                        nStarNum++;
                    }
                    nPos++;
                    part1 = line.Substring(0, nPos);
                    part1 = part1.Trim();
                    part2 = line.Substring(nPos + nStarNum);
                    part2 = part2.Trim();
                    string strTmp = "";
                    for (; nStarNum > 0; nStarNum-- )
                    {
                        strTmp += '*';
                    }
                    part2 = strTmp + part2;
                }
                else
                {
                    part1 = line.Substring(0, nPos + 1);
                    part1 = part1.Trim();
                    part2 = line.Substring(nPos + 1);
                    part2 = part2.Trim();
                }
            }

            if (bHaveSemicolon)
            {
                if (part2.Length > 0)
                    part2 += ';';
                else
                    part1 += ';';
            }

            return nPreSpaceNum;
        }
    }
}
/*
    vector<map<int>>vecMap;
   vector<map<int>>vecMap;
   vector<map<int> >         vecMap;
   vector<map<int> >vecMap;
char*p;
char *p;
char*p ;
char *p ;
   char* p =null;
   doubled=1.0;
    unsignedintnCount=0;
*/