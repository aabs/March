using System;
using System.Collections.Generic;
using System.Dataflow;
using Common;

namespace DomainModel
{
    public class Architecture
    {
        public Prolog Prolog { get; set; }
        public IEnumerable<Artefact> Artefacts { get; set; }
        public IEnumerable<Application> Applications { get; set; }
        public IEnumerable<Network> Networks { get; set; }
    }

    public class Prolog
    {
        public string Author { get; set; }
        public string DateCreated { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }

    public class Artefact
    {
        public IEnumerable<Artefact> Bases { get; set; }
        public IEnumerable<Artefact> SubSystems { get; set; }
        public IEnumerable<PropertyDefinition> PropertyDefinitions { get; set; }
    }

    public class PropertyDefinition
    {
        public string Name { get; set; }
        public object DefaultValue { get; set; }
        public PropertyType Type { get; set; }
        public TypeConstraint TypeConstraint { get; set; }
    }

    public class PropertyDeclaration
    {
        public PropertyDefinition PropertyBase { get; set; }
        public object Value { get; set; }
    }

    public enum PropertyType
    {
        ObjectReference,
        Date,
        DateTime,
        Time,
        DateTimeOffset,
        Decimal,
        Guid,
        Integer,
        Scientific,
        TextLiteral,
        Binary
    }

    public class Application
    {
        public IEnumerable<Artefact> Bases { get; set; }
        public IEnumerable<Application> SubSystems { get; set; }
        public IEnumerable<PropertyDefinition> PropertyDefinitions { get; set; }
        public IEnumerable<PropertyDeclaration> PropertyDeclarations { get; set; }
    }

    public class ApplicationInstance
    {
        public Application Base { get; set; }
        public IEnumerable<PropertyDeclaration> PropertyDeclarations { get; set; }
    }

    public class Network
    {
        public IEnumerable<PropertyDefinition> PropertyDefinitions { get; set; }
        public IEnumerable<PropertyDeclaration> PropertyDeclarations { get; set; }
        public IEnumerable<Server> Servers { get; set; }
    }

    public class Server
    {
        public IEnumerable<PropertyDefinition> PropertyDefinitions { get; set; }
        public IEnumerable<PropertyDeclaration> PropertyDeclarations { get; set; }
        public IEnumerable<Feature> FeatureList { get; set; }
    }

    [Serializable]
    public class Feature
    {
        public ApplicationInstance Application { get; set; }
        public Func<ApplicationInstance, bool> Constraint { get; set; }
    }

    public class FeatureConstraint
    {
        public FeatureConstraint(Expression theExpression)
        {
            TheExpression = theExpression;
        }

        public Expression TheExpression { get; set; }
    }

    public class TypeConstraint
    {
        public TypeConstraint()
        {
            AcceptableTypes = new PropertyType[] {};
        }

        public TypeConstraint(IEnumerable<PropertyType> acceptableTypes)
        {
            AcceptableTypes = acceptableTypes;
        }

        public IEnumerable<PropertyType> AcceptableTypes { get; set; }
    }

    public class DefaultConstraint
    {
    }

    public class ApplicationOrArtefact : Tuple<Application, Artefact>
    {
        #region ValueType enum

        public enum ValueType
        {
            Neither,
            Application,
            Artefact
        }

        #endregion

        public ApplicationOrArtefact()
            : base(null, null)
        {
        }

        public ApplicationOrArtefact(Application app)
            : base(app, null)
        {
        }

        public ApplicationOrArtefact(Artefact art)
            : base(null, art)
        {
        }

        public ValueType ValueStored { get; set; }

        public object Get()
        {
            switch (ValueStored)
            {
                case ValueType.Application:
                    return First;
                case ValueType.Artefact:
                    return Item2;
                default:
                    return null;
            }
        }
    }
}

