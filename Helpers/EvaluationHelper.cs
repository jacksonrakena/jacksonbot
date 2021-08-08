using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Disqord.Bot;
using Disqord.Gateway;

namespace Abyss.Helpers
{
    public class EvaluationHelper
    {
        public EvaluationHelper(DiscordGuildCommandContext context)
        {
            Context = context;
        }
        public DiscordGuildCommandContext Context { get; }
        public static string InspectMethods(object obj)
        {
            var type = obj as Type ?? obj.GetType();

            var sb = new StringBuilder();

            var methods = type.GetMethods();

            sb.Append($"<< Inspecting methods for type [{type.Name}] >>");
            sb.AppendLine();

            foreach (var method in methods.Where(m => !m.IsSpecialName))
            {
                if (sb.Length > 1800) break;
                sb.Append(
                    $"[Name: {method.Name}, Return-Type: {method.ReturnType.Name}, Parameters: [{string.Join(", ", method.GetParameters().Select(a => $"({a.ParameterType.Name} {a.Name})"))}]");
                sb.AppendLine();
            }

            return "```ini\n" + sb.ToString() + "```";
        }

        public static object ReadValue(FieldInfo prop, object obj)
        {
            return ReadValue((object) prop, obj);
        }

        public static object ReadValue(PropertyInfo prop, object obj)
        {
            return ReadValue((object) prop, obj);
        }

        private static string ReadValue(object prop, object obj)
        {

            /* PropertyInfo and FieldInfo both derive from MemberInfo, but that does not have a GetValue method, so the only
                supported ancestor is object */
            try
            {
                var value = prop switch
                {
                    PropertyInfo pinfo => pinfo.GetValue(obj),

                    FieldInfo finfo => finfo.GetValue(obj),

                    _ => throw new ArgumentException($"{nameof(prop)} must be PropertyInfo or FieldInfo", nameof(prop)),
                };

                if (value == null) return "Null";

                if (value is IEnumerable e && !(value is string))
                {
                    var enu = e.Cast<object>().ToList();
                    return $"{enu.Count} [{enu.GetType().Name}]";
                } else
                {
                    return value + $" [{value.GetType().Name}]";
                }
            }
            catch (Exception e)
            {
                return $"[[{e.GetType().Name} thrown]]";
            }
        }

        public static string InspectInheritance<T>()
        {
            return InspectInheritance(typeof(T));
        }

        public static string InspectInheritance(Type type)
        {
            var baseTypes = new List<Type>() { type };
            var latestType = type.BaseType;

            while (latestType != null)
            {
                baseTypes.Add(latestType);
                latestType = latestType.BaseType;
            }

            var sb = new StringBuilder().AppendLine($"Inheritance graph for type [{type.FullName}]").AppendLine();

            foreach (var baseType in baseTypes)
            {
                sb.Append($"[{FormatType(baseType)}]");
                IList<Type> inheritors = baseType.GetInterfaces();
                if (baseType.BaseType != null)
                {
                    inheritors = inheritors.ToList();
                    inheritors.Add(baseType.BaseType);
                }
                if (inheritors.Count > 0) sb.Append($": {string.Join(", ", inheritors.Select(b => b.FullName))}");

                sb.AppendLine();
            }

            return "```ini\n" + sb.ToString() + "```";
        }

        private static string FormatType(Type atype)
        {
            var vs = atype.Namespace + "." + atype.Name;

            var t = atype.GenericTypeArguments;

            if (t.Length > 0) vs += $"<{string.Join(", ", t.Select(a => a.Name))}>";

            return vs;
        }

        public static string InspectInheritance(object obj)
        {
            return InspectInheritance(obj.GetType());
        }

        public static string Inspect(object obj)
        {
            var type = obj.GetType();

            var inspection = new StringBuilder();
            inspection.Append("<< Inspecting type [").Append(type.Name).AppendLine("] >>");
            inspection.Append("<< String Representation: [").Append(obj).AppendLine("] >>");
            inspection.AppendLine();

            /* Get list of properties, with no index parameters (to avoid exceptions) */
            var props = type.GetProperties().Where(a => a.GetIndexParameters().Length == 0)
                .OrderBy(a => a.Name).ToList();

            /* Get list of fields */
            var fields = type.GetFields().OrderBy(a => a.Name).ToList();

            /* Handle properties in type */
            if (props.Count != 0)
            {
                /* Add header if we have fields as well */
                if (fields.Count != 0) inspection.AppendLine("<< Properties >>");

                /* Get the longest named property in the list, so we can make the column width that + 5 */
                var columnWidth = props.Max(a => a.Name.Length) + 5;
                foreach (var prop in props)
                {
                    /* Crude skip to avoid request errors */
                    if (inspection.Length > 1800) break;

                    /* Create a blank string gap of the remaining space to the end of the column */
                    var sep = new string(' ', columnWidth - prop.Name.Length);

                    /* Add the property name, then the separator, then the value */
                    inspection.Append(prop.Name).Append(sep).Append(prop.CanRead ? ReadValue(prop, obj) : "Unreadable").AppendLine();
                }
            }

            /* Repeat the same with fields */
            if (fields.Count != 0)
            {
                if (props.Count != 0)
                {
                    inspection.AppendLine();
                    inspection.AppendLine("<< Fields >>");
                }

                var columnWidth = fields.Max(ab => ab.Name.Length) + 5;
                foreach (var prop in fields)
                {
                    if (inspection.Length > 1800) break;

                    var sep = new string(' ', columnWidth - prop.Name.Length);
                    inspection.Append(prop.Name).Append(":").Append(sep).Append(ReadValue(prop, obj)).AppendLine();
                }
            }

            /* If the object is an enumerable type, add a list of it's items */
            // ReSharper disable once InvertIf
            if (obj is IEnumerable objEnumerable)
            {
                inspection.AppendLine();
                inspection.AppendLine("<< Items >>");
                foreach (var prop in objEnumerable) inspection.Append(" - ").Append(prop).AppendLine();
            }

            return "```\n" + inspection.ToString() + "```";
        }

        public CachedMember User(ulong id)
        {
            return Context.Guild.Members[id] as CachedMember;
        }

        public CachedMember User(string username)
        {
            return Context.Guild.Members.Values.OfType<CachedMember>().FirstOrDefault(a => a.Name.Equals(username, StringComparison.OrdinalIgnoreCase) || (a.Nick != null && a.Nick.Equals(username, StringComparison.OrdinalIgnoreCase)));
        }

        public CachedTextChannel TextChannel(ulong id)
        {
            return Context.Guild.GetChannel(id) as CachedTextChannel;
        }

        public CachedTextChannel TextChannel(string name)
        {
            return Context.Guild.GetChannels().Values.OfType<CachedTextChannel>().FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /*public CachedUserMessage? Message(ulong id)
        {
            return (Context.Channel as CachedTextChannel).Message(id);
        }

        public CachedUserMessage? Message(string id) => Message(ulong.Parse(id));*/
    }
}