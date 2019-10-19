using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Qmmands;

namespace Abyss
{
    internal enum UnixParserState
    {
        Neutral,
        ArgumentName,
        ArgumentValue,
        DashSequence
    }

    public class UnixArgumentParser : IArgumentParser
    {
        public static readonly UnixArgumentParser Instance = new UnixArgumentParser();

        private UnixArgumentParser() { }

        private static readonly Type _booleanType = typeof(bool);

        private Parameter? GetParameter(CommandContext context, string name)
        {
            for (var i = 0; i < context.Command.Parameters.Count; i++)
            {
                var parameter = context.Command.Parameters[i];
                if (parameter.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return parameter;
            }
            return null;
        }

        public ValueTask<ArgumentParserResult> ParseAsync(CommandContext context)
        {
            var state = UnixParserState.Neutral;
            var parameters = new Dictionary<Parameter, object>();
            var inQuote = false;
            var parameterName = new StringBuilder();
            var parameterValue = new StringBuilder();
            var rawArguments = new StringBuilder(context.RawArguments).Append(' '); // big smart plays
            Parameter? currentParameter = null;

            for (var tokenPosition = 0; tokenPosition < rawArguments.Length; tokenPosition++)
            {
                var token = rawArguments[tokenPosition];

                switch (token)
                {
                    // second dash in dash sequence
                    case '-' when state == UnixParserState.DashSequence:
                        state = UnixParserState.ArgumentName;
                        break;

                    // first dash in dash sequence
                    case '-':
                        state = UnixParserState.DashSequence;
                        break;

                    // name set
                    case '=' when state == UnixParserState.ArgumentName:
                        // lookup parameter
                        var argumentName = parameterName.ToString();
                        currentParameter = GetParameter(context, argumentName);
                        if (currentParameter == null) return UnixArgumentParserResult.UnknownParameter(context, parameters, argumentName, tokenPosition);
                        
                        // begin reading value
                        state = UnixParserState.ArgumentValue;
                        break;

                    // handle spaces when in quote
                    case ' ' when inQuote:
                        parameterValue.Append(' ');
                        break;

                    // if the argument name is interrupted, check for a boolean (making sure not to overwrite a default value) otherwise return error
                    case ' ' when state == UnixParserState.ArgumentName:
                        {
                            currentParameter = GetParameter(context, parameterName.ToString());
                            if (currentParameter != null && currentParameter.Type == _booleanType)
                            {
                                parameters.TryAdd(currentParameter, true);
                                parameterName.Clear();
                                parameterValue.Clear();
                                state = UnixParserState.Neutral;
                            }
                            else
                            {
                                state = UnixParserState.ArgumentValue;
                            }
                            break;
                        }

                    // end argument k/v
                    case ' ' when state == UnixParserState.ArgumentValue:
                        {
                            state = UnixParserState.Neutral;
                            if (currentParameter == null)
                            {
                                // theoretically, this should never happen
                                return UnixArgumentParserResult.UnknownParameter(context, parameters, parameterName.ToString(), tokenPosition);
                            }
                            parameters.TryAdd(currentParameter, parameterValue.ToString());
                            parameterName.Clear();
                            parameterValue.Clear();
                            break;
                        }

                    // quote start
                    case '"' when state == UnixParserState.ArgumentValue && !inQuote:
                        inQuote = true;
                        break;

                    // quote end
                    case '"' when state == UnixParserState.ArgumentValue && inQuote:
                        {
                            inQuote = false;
                            if (currentParameter == null)
                            {
                                // theoretically, this should never happen
                                return UnixArgumentParserResult.UnknownParameter(context, parameters, parameterName.ToString(), tokenPosition);
                            }
                            parameters.TryAdd(currentParameter, parameterValue.ToString());
                            parameterName.Clear();
                            parameterValue.Clear();
                            state = UnixParserState.Neutral;
                            break;
                        }

                    // unexpected quote
                    case '"':
                        return UnixArgumentParserResult.UnexpectedQuote(context, parameters, tokenPosition);

                    // data value
                    default:
                        if (state == UnixParserState.ArgumentName)
                        {
                            parameterName.Append(token);
                            break;
                        }
                        if (state == UnixParserState.ArgumentValue)
                        {
                            parameterValue.Append(token);
                            break;
                        }
                        break;
                }
            }

            // unclosed quote
            if (inQuote) return UnixArgumentParserResult.UnclosedQuote(context, parameters, rawArguments.Length);

            foreach (var expectedParameter in context.Command.Parameters)
            {
                if (!parameters.ContainsKey(expectedParameter))
                {
                    if (expectedParameter.Type == _booleanType && expectedParameter.DefaultValue == null)
                    {
                        parameters.TryAdd(expectedParameter, false);
                        continue;
                    }
                    if (expectedParameter.IsOptional)
                    {
                        parameters.TryAdd(expectedParameter, expectedParameter.DefaultValue!);
                        continue;
                    }
                    else
                    {
                        return UnixArgumentParserResult.TooFewArguments(context, parameters, expectedParameter);
                    }
                }
            }

            return UnixArgumentParserResult.Successful(context, parameters);
        }
    }
}