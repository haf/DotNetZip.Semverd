// RandomTextGenerator.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs):
// Time-stamp: <2011-July-13 16:37:19>
//
// ------------------------------------------------------------------
//
// This module defines a class that generates random text sequences
// using a Markov chain.
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Ionic.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Ionic.Zip.Tests.Utilities
{

    public class RandomTextGenerator
    {
        static string[] uris = new string[]
            {
                // "Through the Looking Glass", by Lewis Carroll (~181k)
                "http://www.gutenberg.org/files/12/12.txt",

                // Decl of Independence (~16k)
                "http://www.gutenberg.org/files/16780/16780.txt",

                // Decl of Independence, alternative source
                "http://www.constitution.org/usdeclar.txt",

                // Section 552a of the US code - on privacy for individuals
                "http://www.opm.gov/feddata/usc552a.txt",

                // The Naval War of 1812, by Theodore Roosevelt (968k)
                "http://www.gutenberg.org/dirs/etext05/7trnv10.txt",

                // On Prayer and the Contemplative Life, by Thomas Aquinas (440k)
                "http://www.gutenberg.org/files/22295/22295.txt",

                // IETF RFC 1951 - the DEFLATE format
                "http://www.ietf.org/rfc/rfc1951.txt",

                // pkware's appnote
                "http://www.pkware.com/documents/casestudies/APPNOTE.TXT",
            };

        SimpleMarkovChain markov;

        public RandomTextGenerator()
        {
            System.Random rnd = new System.Random();
            string seedText = null;
            int cycles = 0;
            do {
                try
                {
                    string uri= uris[rnd.Next(uris.Length)];
                    seedText = GetPageMarkup(uri);
                }
                catch (System.Net.WebException)
                {
                    cycles++;
                    if (cycles>8) throw;
                    seedText = null;
                }
            } while (seedText == null);

            markov = new SimpleMarkovChain(seedText);
        }


        public string Generate(int length)
        {
            return markov.GenerateText(length);
        }


        private static string GetPageMarkup(string uri)
        {
            string pageData = null;
            using (WebClient client = new WebClient())
            {
                pageData = client.DownloadString(uri);
            }
            return pageData;
        }
    }


    /// <summary>
    /// Implements a simple Markov chain for text.
    /// </summary>
    ///
    /// <remarks>
    /// Uses a Markov chain starting with some base texts to produce
    /// random natural-ish text. This implementation is based on Pike's
    /// perl implementation, see
    /// http://cm.bell-labs.com/cm/cs/tpop/markov.pl
    /// </remarks>
    public class SimpleMarkovChain
    {
        Dictionary<String, List<String>> table = new Dictionary<String, List<String>>();
        System.Random rnd = new System.Random();

        public SimpleMarkovChain(string seed)
        {
            string NEWLINE = "\n";
            string key = NEWLINE;
            var sr = new StringReader(seed);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                foreach (var word in line.SplitByWords())
                {
                    var w = (word == "") ? NEWLINE : word; // newline
                    if (word == "\r") w = NEWLINE;

                    if (!table.ContainsKey(key)) table.Add(key, new List<string>());
                    table[key].Add(w);
                    key = w.ToLower().TrimPunctuation();
                }
            }
            if (!table.ContainsKey(key)) table.Add(key, new List<string>());
            table[key].Add(NEWLINE);
            key = NEWLINE;
        }


        internal void Diag()
        {
            Console.WriteLine("There are {0} keys in the table", table.Keys.Count);
            foreach (string s in table.Keys)
            {
                string x = s.Replace("\n", "�");
                var y = table[s].ToArray();
                Console.WriteLine("  {0}: {1}", x, String.Join(", ", y));
            }
        }

        internal void ShowList(string word)
        {
            string x = word.Replace("\n", "�");
            if (table.ContainsKey(word))
            {
                var y = table[word].ToArray();
                var z = Array.ConvertAll(y, x1 => x1.Replace("\n", "�"));
                Console.WriteLine("  {0}: {1}", x, String.Join(", ", z));
            }
            else
                Console.WriteLine("  {0}: -key not found-", x);
        }

        private List<string> _keywords;
        private List<string> keywords
        {
            get
            {
                if (_keywords == null)
                    _keywords = new List<String>(table.Keys);
                return _keywords;
            }
        }

        /// <summary>
        /// Generates random text with a minimum character length.
        /// </summary>
        ///
        /// <param name="minimumLength">
        /// The minimum length of text, in characters, to produce.
        /// </param>
        public string GenerateText(int minimumLength)
        {
            var chosenStartWord = keywords[rnd.Next(keywords.Count)];
            return _InternalGenerate(chosenStartWord, StopCriterion.NumberOfChars, minimumLength);
        }

        /// <summary>
        /// Generates random text with a minimum character length.
        /// </summary>
        ///
        /// <remarks>
        /// The first sentence will start with the given start word.
        /// </remarks>
        ///
        /// <param name="minimumLength">
        /// The minimum length of text, in characters, to produce.
        /// </param>
        /// <param name="start">
        /// The word to start with. If this word does not exist in the
        /// seed text, the generation will fail.
        /// </param>
        /// <seealso cref="GenerateText(int)"/>
        /// <seealso cref="GenerateWords(int)"/>
        /// <seealso cref="GenerateWords(string, int)"/>
        public string GenerateText(string start, int minimumLength)
        {
            return _InternalGenerate(start, StopCriterion.NumberOfChars, minimumLength);
        }

        /// <summary>
        /// Generate random text with a minimum number of words.
        /// </summary>
        ///
        /// <remarks>
        /// The first sentence will start with the given start word.
        /// </remarks>
        ///
        /// <param name="minimumWords">
        /// The minimum number of words of text to produce.
        /// </param>
        /// <param name="start">
        /// The word to start with. If this word does not exist in the
        /// seed text, the generation will fail.
        /// </param>
        /// <seealso cref="GenerateText(int)"/>
        /// <seealso cref="GenerateText(string, int)"/>
        /// <seealso cref="GenerateWords(int)"/>
        public string GenerateWords(string start, int minimumWords)
        {
            return _InternalGenerate(start, StopCriterion.NumberOfWords, minimumWords);
        }


        /// <summary>
        /// Generate random text with a minimum number of words.
        /// </summary>
        ///
        /// <param name="minimumWords">
        /// The minimum number of words of text to produce.
        /// </param>
        /// <seealso cref="GenerateText(int)"/>
        /// <seealso cref="GenerateWords(string, int)"/>
        public string GenerateWords(int minimumWords)
        {
            var chosenStartWord = keywords[rnd.Next(keywords.Count)];
            return _InternalGenerate(chosenStartWord, StopCriterion.NumberOfWords, minimumWords);
        }


        private string _InternalGenerate(string start, StopCriterion crit, int limit)
        {
            string w1 = start.ToLower();
            StringBuilder sb = new StringBuilder();
            sb.Append(start.Capitalize());

            int consecutiveNewLines = 0;
            string word = null;
            string priorWord = null;

            // About the stop criteria:
            // we keep going til we reach the specified number of words or chars, with the added
            // proviso that we have to complete the in-flight sentence when the limit is reached.

            for (int i = 0;
                 (crit == StopCriterion.NumberOfWords && i < limit) ||
                     (crit == StopCriterion.NumberOfChars && sb.Length < limit) ||
                     consecutiveNewLines == 0;
                 i++)
            {
                if (table.ContainsKey(w1))
                {
                    var list = table[w1];
                    int ix = rnd.Next(list.Count);
                    priorWord = word;
                    word = list[ix];
                    if (word != "\n")
                    {
                        // capitalize
                        if (consecutiveNewLines > 0)
                            sb.Append(word.Capitalize());
                        else
                            sb.Append(" ").Append(word);

                        // words that end sentences get a newline
                        if (word.EndsWith("."))
                        {
                            if (consecutiveNewLines == 0 || consecutiveNewLines == 1)
                                sb.Append("\n");
                            consecutiveNewLines++;
                        }
                        else consecutiveNewLines = 0;
                    }
                    w1 = word.ToLower().TrimPunctuation();
                }
            }
            return sb.ToString();
        }



        private enum StopCriterion
        {
            NumberOfWords,
            NumberOfChars
        }

    }



    public class RandomTextInputStream : Stream
    {
        RandomTextGenerator _rtg;
        Int64 _desiredLength;
        Int64 _bytesRead;
        System.Text.Encoding _encoding;
        byte[][] _randomText;
        System.Random _rnd;
        int _gnt;
        byte[] src = null;
        private static readonly int _chunkSize = 1024 * 128;
        private static readonly int _chunks = 48;

        public RandomTextInputStream(Int64 length)
            : this(length, System.Text.Encoding.GetEncoding("ascii"))
        {
        }

        public RandomTextInputStream(Int64 length, System.Text.Encoding encoding)
            : base()
        {
            _desiredLength = length;
            _rtg = new RandomTextGenerator();
            _encoding = encoding;
            _randomText = new byte[_chunks][];
            _rnd = new System.Random();
        }

        /// <summary>
        ///   for diagnostic purposes only
        /// </summary>
        public int GetNewTextCount
        {
            get
            {
                return _gnt;
            }
        }

        new public void  Dispose()
        {
             Dispose(true);
        }

        /// <summary>The Dispose method</summary>
        protected override void Dispose(bool disposeManagedResources)
        {
        }

        private byte[] GetNewText()
        {
            _gnt++;
            int nowServing = _rnd.Next(_chunks);
            if (_randomText[nowServing]==null)
                _randomText[nowServing] = _encoding.GetBytes(_rtg.Generate(_chunkSize));
            return _randomText[nowServing];
        }

        public Int64 BytesRead
        {
            get { return _bytesRead; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesToReadThisTime = count;
            if (_desiredLength - _bytesRead < bytesToReadThisTime)
                bytesToReadThisTime = unchecked((int)(_desiredLength - _bytesRead));

            int bytesToRead = bytesToReadThisTime;
            while (bytesToRead > 0)
            {
                src = GetNewText();
                int bytesAvailable = src.Length;
                int chunksize = (bytesToRead > bytesAvailable)
                    ? bytesAvailable
                    : bytesToRead;

                Buffer.BlockCopy(src, 0, buffer, offset, chunksize);
                bytesToRead -= chunksize;
                offset += chunksize;
            }
            _bytesRead += bytesToReadThisTime;
            return bytesToReadThisTime;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }
        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return _desiredLength; }
        }

        public override long Position
        {
            get { return _desiredLength - _bytesRead; }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            if (value < _bytesRead)
                throw new NotSupportedException();
            _desiredLength = value;
        }

        public override void Flush()
        {
        }
    }



}
