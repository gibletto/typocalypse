using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Typocalypse.Trie;

namespace Typocalypse
{
    public class EnemyInputManager
    {

        private static readonly Random rand = new Random();
        private readonly Trie<Enemy> trie;
        private readonly List<String> texts;
        private readonly Game game;
        private List<Enemy> currentlyMatched;

        public EnemyInputManager(List<String> texts, Game game)
        {
            trie = new Trie<Enemy>();
            this.texts = texts;
            this.game = game;
            currentlyMatched = new List<Enemy>();
        }

        /// <summary>
        /// See http://geekyisawesome.blogspot.com/2010/07/biasing-uniform-random-fraction.html
        /// </summary>
        private static double BiasedRandom(double bias)
        {
            return Math.Pow(rand.NextDouble(), Math.Log(0.5) / Math.Log(bias));
        }

        public EnemyTextBox RegisterEnemy(Enemy enemy, double bias)
        {
            var text = texts[(int)(texts.Count * BiasedRandom(bias))];
            var textBox = new EnemyTextBox(game, text);
            trie.Put(text, enemy);

            return textBox;
        }

        public Enemy InputKey(char chr)
        {
            var matchResult = trie.Matcher.NextMatch(chr);
            if (!matchResult)
            {
                currentlyMatched.ForEach(e => e.TextBox.ResetMatchedPrefixLength());
                currentlyMatched.Clear();
                trie.Matcher.ResetMatch();
            }
            else
            {
                var newlyMatched = trie.Matcher.GetPrefixMatches();
                currentlyMatched.Except(newlyMatched).ToList().ForEach(e => e.TextBox.ResetMatchedPrefixLength());
                newlyMatched.ForEach(e => e.TextBox.MatchedPrefixLength(trie.Matcher.GetPrefix().Length));
                currentlyMatched = newlyMatched;
                var exactMatch = trie.Matcher.GetExactMatch();
                if (exactMatch != null)
                {
                    trie.Matcher.ResetMatch();
                    currentlyMatched.ForEach(e => e.TextBox.ResetMatchedPrefixLength());
                    currentlyMatched.Clear();
                    trie.Remove(exactMatch.TextBox.Text);
                }
                return exactMatch;
            }
            return null;
        }

    }
}