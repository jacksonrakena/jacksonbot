using Abyssal.Common;
using Disqord;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Abyss.Commands.Default
{
    [Name("Random")]
    [Description("Commands that involve a computerised RNG calculator.")]
    public class RandomModule : AbyssModuleBase
    {
        public enum DiceExpressionOptions
        {
            None,
            SimplifyStringValue
        }

        private readonly Random _random;

        public RandomModule(Random random)
        {
            _random = random;
        }

        [Command("otp")]
        [Description("One True Pairing: Ships two random members of this server.")]
        [RunMode(RunMode.Parallel)]
        public Task<ActionResult> Command_OtpAsync()
        {
            try
            {
                var guildUsers = Context.Guild.Members.Values.Where(c => !c.IsBot).ToArray();

                if (guildUsers.Length < 2) return Ok("This guild is too small!");

                var member1 = guildUsers.Random(_random);
                var member2 = guildUsers.Random(_random);

                while (member1 == member2) member1 = guildUsers.Random(_random);

                return Ok(
                    $":heart: I ship **{member1.DisplayName}** x **{member2.DisplayName}**! :heart:");

            } catch (Exception)
            {
                return Ok("Can't ship. This server is probably too big for my awful code.");
            }
        }

        [Command("roll", "dice")]
        [Remarks("This command also supports complex dice types, like `d20+d18+4`.")]
        [Description("Rolls a dice of the supplied size.")]
        public Task<ActionResult> Command_DiceRollAsync(
            [Name("Dice")]
            [Description("The dice configuration to use. It can be simple, like `6`, or complex, like `d20+d18+4`.")]
            string dice, [Name("Number of Dice")]
            [Description("The number of dice to roll.")]
            [Range(1, 100)]
            int numberOfDice = 1)
        {
            if (!dice.Contains("d" /* No dice */) && int.TryParse(dice, out var diceParsed))
            {
                if (diceParsed < 1) return BadRequest("Your dice roll must be 1 or above!");

                return numberOfDice == 1
                    ? Ok($"I rolled **{_random.Next(1, diceParsed)}** on a **{dice}**-sided die.")
                    : Ok(
                        string.Join("\n",
                            Enumerable.Range(1, numberOfDice)
                                .Select(a => $"- **Die {a}:** {_random.Next(1, diceParsed)}")));
            }

            try
            {
                if (numberOfDice == 1) return Ok($"I rolled **{DiceExpression.Evaluate(dice)}** on a **{dice}** die.");
                return Ok(string.Join("\n", Enumerable.Range(1, numberOfDice)
                    .Select(a => $"- **Die {a}:** {DiceExpression.Evaluate(dice)}")));
            }
            catch (ArgumentException)
            {
                return BadRequest("Invalid dice!");
            }
        }

        [Command("is")]
        [Description("Determines if a user has a specific attribute.")]
        [ResponseFormatOptions(ResponseFormatOptions.DontAttachTimestamp | ResponseFormatOptions.DontAttachFooter)]
        public Task<ActionResult> IsUserAsync(CachedMember target, [Remainder] string attribute)
        {
            var @is = _random.Next(0, 2) == 1;
            attribute = attribute.Replace("?", ".");
            var username = target is CachedMember u ? u.DisplayName : target.Name;

            var response =
                $"{(@is ? "Yes" : "No")}, {username} is {(@is ? "" : "not ")}{attribute}{(attribute.EndsWith(".") ? "" : ".")}";
            return Ok(response);
        }

        [Command("does")]
        [Description("Determines if a user does something, or has an attribute.")]
        [ResponseFormatOptions(ResponseFormatOptions.DontAttachTimestamp | ResponseFormatOptions.DontAttachFooter)]
        public Task<ActionResult> DoesUserAsync(CachedMember target, [Remainder] string attribute)
        {
            var does = _random.Next(0, 2) == 1;
            attribute = attribute.Replace("?", ".");
            var username = target is CachedMember u ? u.DisplayName: target.Name;

            var response =
                $"{(does ? "Yes" : "No")}, {username} does {(does ? "" : "not ")}{attribute}{(attribute.EndsWith(".") ? "" : ".")}";
            return Ok(response);
        }

        [Command("choose")]
        [Description("Picks an option out of a list.")]
        [ResponseFormatOptions(ResponseFormatOptions.DontAttachTimestamp | ResponseFormatOptions.DontAttachFooter)]
        public Task<ActionResult> Command_PickOptionAsync(
            [Name("Options")] [Description("The options to choose from.")]
            params string[] options)
        {
            if (options.Length == 0) return BadRequest("You have to give me options to pick from!");

            var roll = _random.Next(0, options.Length);
            return Ok($"I choose **{options[roll]}**.");
        }

        private class DiceExpression
        {
            private static readonly Regex NumberToken = new Regex("^[0-9]+$");
            private static readonly Regex DiceRollToken = new Regex("^([0-9]*)d([0-9]+|%)$");
            
            public static readonly DiceExpression Zero = new DiceExpression("0");

            private readonly List<KeyValuePair<int, IDiceExpressionNode>> _nodes =
                new List<KeyValuePair<int, IDiceExpressionNode>>();

            public DiceExpression(string expression, DiceExpressionOptions options = DiceExpressionOptions.None)
            {
                // A well-formed dice expression's tokens will be either +, -, an integer, or XdY.
                var tokens = expression.Replace("+", " + ").Replace("-", " - ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Blank dice expressions end up being DiceExpression.Zero.
                if (tokens.Length == 0) tokens = new[] { "0" };

                // Since we parse tokens in operator-then-operand pairs, make sure the first token is an operand.
                if (tokens[0] != "+" && tokens[0] != "-") tokens = new[] { "+" }.Concat(tokens).ToArray();

                // This is a precondition for the below parsing loop to make any sense.
                if (tokens.Length % 2 != 0)
                {
                    throw new ArgumentException(
                       "The given dice expression was not in an expected format: even after normalization, it contained an odd number of tokens.");
                }

                // Parse operator-then-operand pairs into nodes.
                for (var tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex += 2)
                {
                    var token = tokens[tokenIndex];
                    var nextToken = tokens[tokenIndex + 1];

                    if (token != "+" && token != "-")
                        throw new ArgumentException("The given dice expression was not in an expected format.");

                    var multiplier = token == "+" ? +1 : -1;

                    if (NumberToken.IsMatch(nextToken))
                    {
                        _nodes.Add(new KeyValuePair<int, IDiceExpressionNode>(multiplier,
                            new NumberNode(int.Parse(nextToken))));
                    }
                    else if (DiceRollToken.IsMatch(nextToken))
                    {
                        var match = DiceRollToken.Match(nextToken);
                        var numberOfDice = match.Groups[1].Value?.Length == 0 ? 1 : int.Parse(match.Groups[1].Value);
                        var diceType = match.Groups[2].Value == "%" ? 100 : int.Parse(match.Groups[2].Value);
                        _nodes.Add(new KeyValuePair<int, IDiceExpressionNode>(multiplier,
                            new DiceRollNode(numberOfDice, diceType)));
                    }
                    else
                    {
                        throw new ArgumentException(
                            "The given dice expression was not in an expected format: the non-operand token was neither a number nor a dice-roll expression.");
                    }
                }

                // Sort the nodes in an aesthetically-pleasing fashion.
                var diceRollNodes = _nodes.Where(pair => pair.Value.GetType() == typeof(DiceRollNode))
                    .OrderByDescending(node => node.Key)
                    .ThenByDescending(node => ((DiceRollNode) node.Value).DiceType)
                    .ThenByDescending(node => ((DiceRollNode) node.Value).NumberOfDice).ToList();
                var numberNodes = _nodes.Where(pair => pair.Value.GetType() == typeof(NumberNode))
                    .OrderByDescending(node => node.Key)
                    .ThenByDescending(node => node.Value.Evaluate());

                // If desired, merge all number nodes together, and merge dice nodes of the same type together.
                if (options == DiceExpressionOptions.SimplifyStringValue)
                {
                    var number = numberNodes.Sum(pair => pair.Key * pair.Value.Evaluate());
                    var diceTypes = diceRollNodes.Select(node => ((DiceRollNode) node.Value).DiceType).Distinct();
                    var normalizedDiceRollNodes = from type in diceTypes
                                                  let numDiceOfThisType = diceRollNodes
                                                      .Where(node => ((DiceRollNode) node.Value).DiceType == type).Sum(node =>
                                                          node.Key * ((DiceRollNode) node.Value).NumberOfDice)
                                                  where numDiceOfThisType != 0
                                                  let multiplicand = numDiceOfThisType > 0 ? +1 : -1
                                                  let absNumDice = Math.Abs(numDiceOfThisType)
                                                  orderby multiplicand descending, type descending
                                                  select new KeyValuePair<int, IDiceExpressionNode>(multiplicand,
                                                      new DiceRollNode(absNumDice, type));

                    _nodes = (number == 0
                            ? normalizedDiceRollNodes
                            : normalizedDiceRollNodes.Concat(new[]
                            {
                                new KeyValuePair<int, IDiceExpressionNode>(number > 0 ? +1 : -1, new NumberNode(number))
                            }))
                        .ToList();
                }
                // Otherwise, just put the dice-roll nodes first, then the number nodes.
                else
                {
                    _nodes = diceRollNodes.Concat(numberNodes).ToList();
                }
            }

            public static int Evaluate(string expression, DiceExpressionOptions options = DiceExpressionOptions.None)
            {
                return new DiceExpression(expression, options).Evaluate();
            }

            public override string ToString()
            {
                var result = (_nodes[0].Key == -1 ? "-" : string.Empty) + _nodes[0].Value;
                foreach (var pair in _nodes.Skip(1))
                {
                    result += pair.Key == +1 ? " + " : " − "; // NOTE: unicode minus sign, not hyphen-minus '-'.
                    result += pair.Value.ToString();
                }

                return result;
            }

            public int Evaluate()
            {
                var result = 0;
                foreach (var pair in _nodes) result += pair.Key * pair.Value.Evaluate();

                return result;
            }

            public decimal GetCalculatedAverage()
            {
                decimal result = 0;
                foreach (var pair in _nodes) result += pair.Key * pair.Value.GetCalculatedAverage();

                return result;
            }

            private interface IDiceExpressionNode
            {
                int Evaluate();

                decimal GetCalculatedAverage();
            }

            private class NumberNode : IDiceExpressionNode
            {
                private readonly int _theNumber;

                public NumberNode(int theNumber)
                {
                    _theNumber = theNumber;
                }

                public int Evaluate()
                {
                    return _theNumber;
                }

                public decimal GetCalculatedAverage()
                {
                    return _theNumber;
                }

                public override string ToString()
                {
                    return _theNumber.ToString();
                }
            }

            private class DiceRollNode : IDiceExpressionNode
            {
                private static readonly Random Roller = new Random();

                public DiceRollNode(int numberOfDice, int diceType)
                {
                    NumberOfDice = numberOfDice;
                    DiceType = diceType;
                }

                public int NumberOfDice { get; }

                public int DiceType { get; }

                public int Evaluate()
                {
                    var total = 0;
                    for (var i = 0; i < NumberOfDice; ++i) total += Roller.Next(1, DiceType + 1);

                    return total;
                }

                public decimal GetCalculatedAverage()
                {
                    return NumberOfDice * ((DiceType + 1.0m) / 2.0m);
                }

                public override string ToString()
                {
                    return $"{NumberOfDice}d{DiceType}";
                }
            }
        }
    }
}