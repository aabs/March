//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace MGraphXamlReader
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Dataflow;
    using System.Linq;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Xml.Linq;

    public class MGraphXamlReader : XamlReader
    {
        static MemberIdentifier x_args = MemberIdentifier.Get(XamlServices.DirectiveTypeName2006, XamlServices.TypeArgumentsPropertyName);

        List<Pair<XNamespace, string>> additionalNamespaces;
        IGraphBuilder graphBuilder;
        XamlReader innerReader;
        Dictionary<Identifier, Type> labelMap = new Dictionary<Identifier, Type>();
        Dictionary<string, Type> lowercaseLabelMap;
        NamespaceResovler namespaceResolver = new NamespaceResovler();
        object root;
        XamlReaderSettings settings;

        public MGraphXamlReader() : this(null) { }
        public MGraphXamlReader(XamlReaderSettings settings)
        {
            this.IsCaseSensitive = true;
            this.settings = (settings ?? new XamlReaderSettings());
        }

        // A principle that should be shared for all of these properties is that retrieving properties should not
        // count as "use", so you can go ahead and read all of the properties before setting them if you want. Once
        // you've started to read, you can no longer change any of the properties, including the object root and the
        // graph builder.
        //

        public List<Pair<XNamespace, string>> AdditionalNamespaces
        {
            get { return additionalNamespaces; }
            set
            {
                if (this.innerReader != null) { throw new InvalidOperationException(); }
                this.additionalNamespaces = value;

                foreach (var pair in value)
                {
                    namespaceResolver.ForceAllocatePrefix(pair.First, pair.Second);
                }
            }
        }

        public bool AllowMultiwordIdentifiers { get; set; }

        public override XamlNode Current
        {
            get
            {
                if (this.innerReader == null) { return null; }
                return this.innerReader.Current;
            }
        }

        public override bool EndOfInput
        {
            get
            {
                if (this.innerReader == null) { return false; }
                return this.innerReader.EndOfInput;
            }
            protected set
            {
                throw new NotSupportedException();
            }
        }

        public IGraphBuilder GraphBuilder
        {
            get { return this.graphBuilder; }
            set
            {
                if (this.innerReader != null) { throw new InvalidOperationException(); }
                this.graphBuilder = value;
            }
        }

        public object GraphRoot
        {
            get { return this.root; }
            set
            {
                if (this.innerReader != null) { throw new InvalidOperationException(); }
                this.root = value;
            }
        }

        public bool IsCaseSensitive { get; set; }

        public Dictionary<Identifier, Type> LabelMap
        {
            get { return labelMap; }
            set
            {
                if (this.innerReader != null) { throw new InvalidOperationException(); }
                this.labelMap = value;
            }
        }

        public TypeReference RecordRoot { get; set; }

        public override XamlReaderSettings Settings
        {
            get { return this.settings; }
        }

        string ConvertTypeReferences(IEnumerable<TypeReference> tr)
        {
            var context = new Context(this);
            var trtc = new TypeReferenceTypeConverter();

            return String.Join(",", (from t in tr
                                     select trtc.ConvertToString(context, t)).ToArray());
        }

        public override XamlReader CreateReaderForSubtree()
        {
            return GetReader().CreateReaderForSubtree();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.innerReader != null) { this.innerReader.Dispose(); }
            }
        }

        List<SchemaMember> GetAllMembersForType(SchemaType type)
        {
            List<SchemaMember> members = new List<SchemaMember>();

            SchemaType st = type;
            while (st != null)
            {
                members.AddRange(st.Members);

                TypeReference baseType = st.BaseType;

                st = baseType != null
                    ? XamlSchemaTypeResolver.Default.Resolve(baseType)
                    : null;
            }

            return members;
        }

        IEnumerable<XamlNode> GetAtomNodes(object node, bool needsRecord)
        {
            TypeReference typeReference = null;
            MemberIdentifier contentProperty = null;

            if (needsRecord)
            {
                typeReference = XamlSchemaTypeResolver.Default.GetTypeReference(node.GetType());
                contentProperty = MemberIdentifier.Get(typeReference.Name, null);

                yield return new XamlStartRecordNode { TypeName = typeReference.Name, NamespaceResolver = namespaceResolver };
                if (typeReference.HasTypeArguments)
                {
                    yield return new XamlStartMemberNode { MemberIdentifier = x_args };
                    yield return new XamlAtomNode { Value = ConvertTypeReferences(typeReference.Arguments) };
                    yield return new XamlEndMemberNode { MemberIdentifier = x_args };
                }
                yield return new XamlStartMemberNode { MemberIdentifier = contentProperty };
            }

            yield return new XamlAtomNode { Value = node };

            if (needsRecord)
            {
                yield return new XamlEndMemberNode { MemberIdentifier = contentProperty };
                yield return new XamlEndRecordNode { TypeName = typeReference.Name };
            }
        }

        IEnumerable<XamlNode> GetCollectionNodes(object node, bool needsRecord)
        {
            return GetCollectionOrSequenceNodes(this.graphBuilder.GetSuccessors(node).ToList(), needsRecord);
        }

        IEnumerable<XamlNode> GetCollectionOrSequenceNodes(List<object> elements, bool needsRecord)
        {
            TypeReference typeReference = null;
            MemberIdentifier contentPropertyIdentifier = null;

            if (elements.Count > 0)
            {
                if (needsRecord)
                {
                    // TODO: Should this be some unordered collection, if the collection is unordered?
                    typeReference = XamlSchemaTypeResolver.Default.GetTypeReference(typeof(List<object>));
                    contentPropertyIdentifier = MemberIdentifier.Get(typeReference.Name, null);

                    yield return new XamlStartRecordNode { TypeName = typeReference.Name, NamespaceResolver = namespaceResolver };
                    yield return new XamlStartMemberNode { MemberIdentifier = x_args };
                    yield return new XamlAtomNode { Value = ConvertTypeReferences(typeReference.Arguments) };
                    yield return new XamlEndMemberNode { MemberIdentifier = x_args };
                    yield return new XamlStartMemberNode { MemberIdentifier = contentPropertyIdentifier };
                }

                foreach (object element in elements)
                {
                    foreach (XamlNode xamlNode in GetNodes(element, true)) { yield return xamlNode; }
                }

                if (needsRecord)
                {
                    yield return new XamlEndMemberNode { MemberIdentifier = contentPropertyIdentifier };
                    yield return new XamlEndRecordNode { TypeName = typeReference.Name };
                }
            }
        }

        IEnumerable<XamlNode> GetEntityNodes(object node, bool needsRecord)
        {
            var typeReference = GetTypeReference(node);

            yield return new XamlStartRecordNode { TypeName = typeReference.Name, NamespaceResolver = namespaceResolver };
            if (typeReference.HasTypeArguments)
            {
                yield return new XamlStartMemberNode { MemberIdentifier = x_args };
                yield return new XamlAtomNode { Value = ConvertTypeReferences(typeReference.Arguments) };
                yield return new XamlEndMemberNode { MemberIdentifier = x_args };
            }
            foreach (KeyValuePair<object, object> pair in graphBuilder.GetEntityMembers(node))
            {
                var fieldValues = GetNodes(pair.Value, false).ToList();
                if (fieldValues.Count > 0) // We cannot write out empty members
                {
                    var memberIdentifier = GetMemberIdentifier(pair.Key.ToString(), typeReference);

                    yield return new XamlStartMemberNode { MemberIdentifier = memberIdentifier };
                    foreach (XamlNode xamlNode in fieldValues)
                    {
                        yield return xamlNode;
                    }
                    yield return new XamlEndMemberNode { MemberIdentifier = memberIdentifier };
                }
            }
            yield return new XamlEndRecordNode { TypeName = typeReference.Name };
        }

        IEnumerable<XamlNode> GetEntityNodes(TypeReference typeReference, object node, bool needsRecord)
        {
            yield return new XamlStartRecordNode { TypeName = typeReference.Name, NamespaceResolver = namespaceResolver };
            if (typeReference.HasTypeArguments)
            {
                yield return new XamlStartMemberNode { MemberIdentifier = x_args };
                yield return new XamlAtomNode { Value = ConvertTypeReferences(typeReference.Arguments) };
                yield return new XamlEndMemberNode { MemberIdentifier = x_args };
            }
            foreach (KeyValuePair<object, object> pair in graphBuilder.GetEntityMembers(node))
            {
                var fieldValues = GetNodes(pair.Value, false).ToList();
                if (fieldValues.Count > 0) // We cannot write out empty members
                {
                    var memberIdentifier = GetMemberIdentifier(pair.Key.ToString(), typeReference);

                    yield return new XamlStartMemberNode { MemberIdentifier = memberIdentifier };
                    foreach (XamlNode xamlNode in fieldValues)
                    {
                        yield return xamlNode;
                    }
                    yield return new XamlEndMemberNode { MemberIdentifier = memberIdentifier };
                }
            }
            yield return new XamlEndRecordNode { TypeName = typeReference.Name };
        }

        MemberIdentifier GetMemberIdentifier(string name, TypeReference type)
        {
            if (AllowMultiwordIdentifiers)
            {
                name = name.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
            }

            if (name == "Xaml.Content")
            {
                // Content property
                //
                return MemberIdentifier.Get(type.Name, null);
            }
            else if (name.Contains('.'))
            {
                // Attached property
                //
                var parts = name.Split('.');
                if (parts.Length != 2) { throw new NotImplementedException(); }
                type = GetTypeReference(parts[0]);
                name = parts[1];
            }
            else
            {
                // Normal property
                //
            }

            if (!IsCaseSensitive && !XamlServices.IsXamlDirectiveNamespace(type.Name))
            {
                var schemaType = XamlSchemaTypeResolver.Default.Resolve(type);
                name = GetAllMembersForType(schemaType)
                    .OfType<SchemaProperty>()
                    .First(m => m.Name.ToLowerInvariant() == name.ToLowerInvariant())
                    .Name;
            }

            return MemberIdentifier.Get(type.Name, name);
        }

        IEnumerable<XamlNode> GetNodes(object node, bool needsRecord)
        {
            if (this.RecordRoot != null)
            {
                var recordRoot = RecordRoot;
                RecordRoot = null;

                return GetEntityNodes(recordRoot, node, needsRecord);
            }

            if (!this.graphBuilder.IsNode(node))
            {
                return GetAtomNodes(node, needsRecord);
            }

            if (this.graphBuilder.IsSequence(node))
            {
                return GetSequenceNodes(node, needsRecord);
            }

            if (this.graphBuilder.IsEntity(node))
            {
                return GetEntityNodes(node, needsRecord);
            }

            return GetCollectionNodes(node, needsRecord);
        }

        IEnumerable<XamlNode> GetSequenceNodes(object node, bool needsRecord)
        {
            if (IsSpecialStringOp(node))
            {
                return HandleStringOp(node);
            }

            return GetCollectionOrSequenceNodes(this.graphBuilder.GetSequenceElements(node).ToList(), needsRecord);
        }

        Type GetTypeFromLabel(Identifier label)
        {
            if (label == null) { return null; }

            if (AllowMultiwordIdentifiers)
            {
                var labelText = label.Text.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                if (labelText != label.Text)
                {
                    label = (Identifier)labelText;
                }
            }

            Type type;

            if (IsCaseSensitive && LabelMap != null && LabelMap.TryGetValue(label, out type))
            {
                return type;
            }
            else if (!IsCaseSensitive && lowercaseLabelMap != null && lowercaseLabelMap.TryGetValue(label.Text.ToLowerInvariant(), out type))
            {
                return type;
            }

            return null;
        }

        TypeReference GetTypeReference(object node)
        {
            var label = graphBuilder.GetLabel(node);
            if (label == null)
            {
                return XamlSchemaTypeResolver.Default.GetTypeReference(typeof(object));
            }

            Type type = GetTypeFromLabel(label as Identifier);
            if (type != null)
            {
                var typeReference = XamlSchemaTypeResolver.Default.GetTypeReference(type);
                namespaceResolver.LookupPrefix(typeReference.Name.Namespace);
                return typeReference;
            }

            return new TypeReference { Name = XName.Get(label.ToString()) };
        }

        TypeReference GetTypeReference(string name)
        {
            switch (name)
            {
                case "Xaml":
                case "Xaml2006":
                    return new TypeReference { Name = XamlServices.DirectiveTypeName2006 };

                case "Xaml2008":
                    return new TypeReference { Name = XamlServices.DirectiveTypeName2008 };
            }

            Type type = GetTypeFromLabel(name);
            if (type != null)
            {
                var typeReference = XamlSchemaTypeResolver.Default.GetTypeReference(type);
                namespaceResolver.LookupPrefix(typeReference.Name.Namespace);
                return typeReference;
            }

            return new TypeReference { Name = XName.Get(name) };
        }

        XamlReader GetReader()
        {
            if (this.innerReader == null)
            {
                this.innerReader = new XamlReader(GetNodes(this.root, true), this.settings);

                this.lowercaseLabelMap = new Dictionary<string, Type>();
                foreach (var entry in labelMap)
                {
                    this.lowercaseLabelMap[entry.Key.Text.ToLowerInvariant()] = entry.Value;
                }
            }
            return this.innerReader;
        }

        IEnumerable<XamlNode> HandleStringOp(object node)
        {
            var label = (Identifier)graphBuilder.GetLabel(node);
            if (label == "String.Join")
            {
                var elements = graphBuilder.GetSequenceElements(node).Cast<string>();
                var first = elements.First();
                var rest = elements.Skip(1).Cast<string>().ToArray();
                yield return new XamlAtomNode { Value = String.Join(first, rest) };
            }
            else if (label == "String.TrimQuotes")
            {
                var elements = graphBuilder.GetSequenceElements(node).Cast<string>();
                var single = elements.Single();
                yield return new XamlAtomNode { Value = single.Substring(1, single.Length - 2) };
            }
        }

        bool IsSpecialStringOp(object node)
        {
            if (graphBuilder.IsNode(node))
            {
                var label = graphBuilder.GetLabel(node) as Identifier;
                if (label == "String.Join") { return true; }
                if (label == "String.TrimQuotes") { return true; }
            }
            return false;
        }

        public override bool Read()
        {
            return GetReader().Read();
        }

        public override IEnumerable<XamlNode> ReadToEnd()
        {
            return GetReader().ReadToEnd();
        }

        class Context : ITypeDescriptorContext
        {
            MGraphXamlReader reader;

            public Context(MGraphXamlReader reader)
            {
                this.reader = reader;
            }

            public IContainer Container
            {
                get { throw new NotImplementedException(); }
            }

            public object Instance
            {
                get { throw new NotImplementedException(); }
            }

            public void OnComponentChanged()
            {
                throw new NotImplementedException();
            }

            public bool OnComponentChanging()
            {
                throw new NotImplementedException();
            }

            public PropertyDescriptor PropertyDescriptor
            {
                get { throw new NotImplementedException(); }
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IXNameResolver))
                {
                    return reader.namespaceResolver;
                }
                if (serviceType == typeof(IXNamespacePrefixLookup))
                {
                    return reader.namespaceResolver;
                }
                return null;
            }
        }

        class NamespaceResovler : IXNameResolver, IXNamespacePrefixLookup
        {
            Dictionary<string, XNamespace> namespaces = new Dictionary<string, XNamespace>();
            Dictionary<XNamespace, string> prefixes = new Dictionary<XNamespace, string>();

            int cnt = 0;

            public NamespaceResovler()
            {
                namespaces[""] = XNamespace.None;
                prefixes[XNamespace.None] = "";
            }

            public void ForceAllocatePrefix(XNamespace ns, string prefix)
            {
                if (namespaces.ContainsKey(prefix))
                {
                    if (namespaces[prefix] != ns) { throw new NotImplementedException(); }
                    return;
                }

                prefixes[ns] = prefix;
                namespaces[prefix] = ns;
            }

            public string AllocatePrefix(XNamespace ns)
            {
                string prefix;
                if (prefixes.TryGetValue(ns, out prefix))
                {
                    return prefix;
                }

                prefix = "n" + cnt.ToString();
                cnt++;

                prefixes[ns] = prefix;
                namespaces[prefix] = ns;
                return prefix;
            }

            public XNamespace LookupNamespace(string prefix)
            {
                XNamespace ns;
                return namespaces.TryGetValue(prefix, out ns) ? ns : null;
            }

            public string LookupPrefix(XNamespace name)
            {
                return AllocatePrefix(name);
            }

            public IEnumerable<KeyValuePair<string, XNamespace>> GetNamespacePrefixes()
            {
                return namespaces;
            }

            public XName Resolve(string qualifiedName)
            {
                if (qualifiedName == null)
                {
                    throw new ArgumentNullException("qualifiedName");
                }

                string prefix, local;
                var colonIndex = qualifiedName.IndexOf(':');
                if (colonIndex == -1)
                {
                    prefix = "";
                    local = qualifiedName.ToString();
                }
                else
                {
                    prefix = qualifiedName.Substring(0, colonIndex);
                    local = qualifiedName.Substring(colonIndex + 1);
                }

                XNamespace @namespace = LookupNamespace(prefix);
                if (@namespace == null)
                {
                    throw new FormatException("Unresolved namespace");
                }

                return @namespace.GetName(local);
            }
        }
    }
}
