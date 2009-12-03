using System;
using System.Collections.Generic;
using System.Dataflow;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using DomainModel;
using Common;

namespace arch
{
    public static class AstVistor
    {
        public static readonly Logger Logger = new Logger(typeof(AstVistor));
        public static object Visit(this GraphBuilder b, object o)
        {
            Contract.Requires(b != null);
            Contract.Requires(o != null);

            string entityLabel = b.GetLabelText(o);

            switch (entityLabel)
            {
                case "Architecture":
                    return b.VisitArchitecture(o);
                default:
                    throw new ApplicationException("unknown node type");
            }
        }
        public static object VisitArchitecture(this GraphBuilder b, object sn)
        {
            string entityLabel = b.GetLabelText(sn);
            Contract.Requires(sn != null);
            Contract.Requires(b.IsNode((sn)));
            Contract.Requires(entityLabel == "Architecture");
            Contract.Requires(b.GetSequenceCount(sn) == 4);
            Contract.Ensures(Contract.Result<object>() != null);

            var seq = b.GetSuccessors(sn);
            Prolog prolog = b.VisitProlog(seq.ElementAt(0)) as Prolog;
            var artefacts = b.VisitArtefacts(sn).ToList();
            Architecture architecture = new Architecture
                                            {
                                                Prolog = prolog,
                                                Artefacts = artefacts
                                            };
            //Logger.Dump(architecture);
            return architecture;
        }

        public static object VisitProlog(this GraphBuilder b, object sn)
        {
            Contract.Requires(sn != null);
            Contract.Requires(b.IsEntity(sn));
            Contract.Requires(b.GetLabelText(sn) == "Prolog");
            Contract.Requires(b.GetSequenceCount(sn) == 4);

            var successors = b.GetSuccessors(sn);
            var sequenceElement = successors.ElementAt(0);

            var author = b.GetPair<string>(sequenceElement);

            Prolog result = new Prolog { Author = author.Item2 };
            return result;
        }

        public static string GetLabelText(this GraphBuilder b, object ident)
        {
            Contract.Requires(b != null);
            Contract.Requires(ident != null);
            Contract.Ensures(Contract.Result<string>() != null);
            object y = null;
            if (ident is Identifier)
            {
                y = ident;
            }
            else if (b.GetEntityLabel(ident) != null)
            {
                y = b.GetEntityLabel(ident);
            }

            System.Dataflow.Identifier x = (Identifier)y;
            string result = x.Text;
            if (result == null)
            {
                result = "";
            }
            return result;
        }

        public static IEnumerable<PropertyDefinition> VisitPropertyDefinitions(this GraphBuilder b, object obj)
        {
            return b.ForEachSuccessor("PropertyDefinition", obj, (builder, o) => builder.VisitPropertyDefinition(o))
                    .Cast<PropertyDefinition>();
        }

        public static PropertyDefinition VisitPropertyDefinition(this GraphBuilder b, object pd)
        {
            var result = new PropertyDefinition
                             {
                                 DefaultValue = b.VisitDefaultConstraint(b.GetSingleByLabel(pd, "Default")),
                                 Name = b.VisitText(b.GetSingleByLabel(pd, "Name")),
                                 Type = b.VisitTypeName(b.GetSingleByLabel(pd, "Type")),
                                 TypeConstraint = b.VisitTypeConstraint(b.GetSingleByLabel(pd, "TypeConstraint"))
                             };
            return result;
        }

        public static IEnumerable<Artefact> VisitArtefacts(this GraphBuilder b, object obj)
        {

            object seq = b.GetSingleByLabel(obj, "Artefacts");
            var successors = b.GetSequenceElements(seq);
            foreach (var o in successors.Where(m => b.TestLabel(m, "Artefact")))
            {
                yield return b.VisitArtefact(o);
            }

//            var element = b.ForEachSequenceElement("Artefact", obj, (builder, o) => builder.VisitArtefact(o));
//            return element
//                    .Cast<Artefact>();
        }

        private static Artefact VisitArtefact(this GraphBuilder b, object obj)
        {
            var result = new Artefact
                             {
                                 PropertyDefinitions = b.VisitPropertyDefinitions(obj)
                             };
            return result;
        }
        private static TypeConstraint VisitTypeConstraint(this GraphBuilder b, object o)
        {
            var result = new TypeConstraint
            {
                AcceptableTypes = new PropertyType[] { (PropertyType)b.Visit(b.GetSingleByLabel(o, "Value")) }
            };
            return result;
        }

        private static string VisitText(this GraphBuilder b, object o)
        {
            Debug.Assert(o is string);
            return o.ToString().TrimQuotes();
        }

        private static PropertyType VisitTypeName(this GraphBuilder b, object o)
        {
            throw new NotImplementedException();
        }

        private static DefaultConstraint VisitDefaultConstraint(this GraphBuilder b, object o)
        {
            var result = new DefaultConstraint
                             {

                             };
            return result;
        }

        #region Helper Methods

        public static IEnumerable<T> VisitObjectsWithLabel<T>(this GraphBuilder b, string label, object obj, Func<GraphBuilder, object, T> visit)
        {
            foreach (var o in b.GetSuccessors(obj).Where(m => b.TestLabel(m, label)))
            {
                yield return visit(b, o);
            }
        }

        public static string TrimQuotes(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1 && ((str.StartsWith("'") && str.EndsWith("'")) || (str.StartsWith("\"") && str.EndsWith("\""))))
            {
                return str.Substring(1, str.Length - 1);
            }
            return str;
        }

        public static IEnumerable<object> ForEachSuccessor(this GraphBuilder b, string label, object obj, Func<GraphBuilder, object, object> visit)
        {
            var successors = b.GetSuccessors(obj);
            foreach (var o in successors.Where(m => b.TestLabel(m, label)))
            {
                yield return visit(b, o);
            }
        }

        public static IEnumerable<object> ForEachSequenceElement(this GraphBuilder b, string label, object obj, Func<GraphBuilder, object, object> visit)
        {
            object seq = b.GetSingleByLabel(obj, label+"s");
            var successors = b.GetSequenceElements(seq);
            foreach (var o in successors.Where(m => b.TestLabel(m, label)))
            {
                yield return visit(b, o);
            }
        }

        public static object GetSingleByLabel(this GraphBuilder b, object obj, string label)
        {
            return b.GetSuccessors(obj).Where(o => b.TestLabel(o, label)).SingleOrDefault();
        }

        public static bool TestLabel(this GraphBuilder b, object o, string label)
        {
            System.Dataflow.Identifier label1 = (Identifier) b.GetEntityLabel(o);
            return label1.Text == label;
        }

        // syntactic elements
        public static IEnumerable<PropertyDeclaration> GetPropertyDeclarations(this GraphBuilder b, object obj)
        {
            return b.VisitObjectsWithLabel("PropertyDeclaration", obj, (builder, o) => b.GetPair<string>(obj))
                .Map(t => new PropertyDeclaration { Value = t.Item2 });
        }

        public static Tuple<string, T> GetPair<T>(this GraphBuilder b, object ident)
        {
            //            Contract.Requires(b != null);
            //            Contract.Requires(ident != null);
            //Contract.Requires(null != (ident as Identifier));

            var label = b.GetLabelText(ident);
            T x = default(T);
            var tmp = b.GetSuccessors(ident).ElementAt(0);
            if (tmp != null)
            {
                x = (T)Convert.ChangeType(tmp, typeof(T));
            }
            return new Tuple<string, T>(label, x);
        }

        #endregion


    }
}