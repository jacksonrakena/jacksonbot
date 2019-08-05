using Abyss.Core.Entities;
using Discord;
using Discord.Rest;
using Qmmands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Core.Helpers
{
    [DoNotAdd]
    public class EvaluationHelper : AbyssModuleBase
    {
        public EvaluationHelper(AbyssRequestContext context)
        {
            Context = context;
        }

        public new AbyssRequestContext Context { get; }

        public Task<RestUserMessage> ReplyAsync(string content, Embed embed = null)
        {
            return Context.Channel.SendMessageAsync(content, embed: embed);
        }

        public string InspectMethods(object obj)
        {
            var type = obj as Type ?? obj.GetType();

            var sb = new StringBuilder();

            var methods = type.GetMethods();

            sb.Append("<< Inspecting methods for type [").Append(type.Name).AppendLine("] >>");
            sb.AppendLine();

            foreach (var method in methods.Where(m => !m.IsSpecialName))
            {
                if (sb.Length > 1800) break;
                sb.Append(
                    "[Name: ").Append(method.Name).Append(", Return-Type: ").Append(method.ReturnType.Name).Append(", Parameters: [").Append(string.Join(", ", method.GetParameters().Select(a => $"({a.ParameterType.Name} {a.Name})"))).AppendLine("]]");
                sb.AppendLine();
            }

            return Format.Code(sb.ToString(), "ini");
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
            object value;

            /* PropertyInfo and FieldInfo both derive from MemberInfo, but that does not have a GetValue method, so the only
                supported ancestor is object */
            try
            {
                switch (prop)
                {
                    case PropertyInfo pinfo:
                        value = pinfo.GetValue(obj);
                        break;

                    case FieldInfo finfo:
                        value = finfo.GetValue(obj);
                        break;

                    default:
                        throw new ArgumentException($"{nameof(prop)} must be PropertyInfo or FieldInfo", nameof(prop));
                }
            }
            catch (Exception)
            {
                return "{{failed to read property or field}}";
            }

            if (value == null) return "Null";

            string HandleEnumerable(IEnumerable @enum)
            {
                var enu = @enum.Cast<object>().ToList();
                return $"{enu.Count} [{enu.GetType().Name}]";
            }

            string HandleNormal()
            {
                return value + $" [{value.GetType().Name}]";
            }

            return value is IEnumerable enumerable && !(value is string)
                ? HandleEnumerable(enumerable)
                : HandleNormal();
        }

        public static string InspectInheritance<T>()
        {
            return InspectInheritance(typeof(T));
        }

        public static string InspectInheritance(Type type)
        {
            var parents = new List<List<Type>>();
            var latestType = type.BaseType;

            while (latestType != null)
            {
                var l = new List<Type> { latestType };
                l.AddRange(latestType.GetInterfaces());
                l.Reverse();
                parents.Add(l);
                latestType = latestType.BaseType;
            }

            if (parents.Count != 1)
            {
                var l = new List<Type> { type };
                l.AddRange(type.GetInterfaces());
                l.Reverse();
                parents.Insert(0, l);
            }

            parents.Reverse();

            string FormatType(Type atype)
            {
                var vs = atype.Namespace + "." + atype.Name;

                var t = atype.GenericTypeArguments;

                if (t.Length > 0) vs += $"<{string.Join(", ", t.Select(a => a.Name))}>";

                return vs;
            }

            var index = 1;
            return Format.Code(new StringBuilder()
                .Append("Inheritance graph for type [").Append(type.FullName).AppendLine("]")
                .AppendLine()
                .AppendLine(string.Join("\n\n",
                    parents.Select(ab => index++ + ") " + string.Join(" -> ", ab.Select(b => $"[{FormatType(b)}]")))))
                .AppendLine()
                .AppendLine(string.Join(" -> ",
                    parents.Where(a => a.Any(b => !b.IsInterface))
                        .Select(d => "[" + d.Find(bx => !bx.IsInterface)?.Name + "]")))
                .ToString(), "ini");
        }

        public string InspectInheritance(object obj)
        {
            return InspectInheritance(obj.GetType());
        }

        public string Inspect(object obj)
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

            return Format.Code(inspection.ToString(), "ini");
        }
    }
}