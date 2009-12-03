using System.Collections.Generic;
using DomainModel.Visitor;
using System;

namespace DomainModel.Ast
{
    public partial class Identifier : BaseNode
    {
        public Identifier()
        {
            Value = new List<string>();
        }

        public IList<string> Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseIdentifier(this);
        }
    }

    public partial class ConstrainedDottedIdentifier : BaseNode
    {
        public ConstrainedDottedIdentifier()
        {
            Value = new List<ConstrainedIdentifierSegment>();
        }

        public IList<ConstrainedIdentifierSegment> Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseConstrainedDottedIdentifier(this);
        }
    }

    public partial class ConstrainedIdentifierSegment : BaseNode
    {
        public Identifier IdentifierConstrained { get; set; }
        public Expression Constraint { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseConstrainedIdentifierSegment(this);
        }
    }

    public partial class Architecture : BaseNode
    {
        public Architecture()
        {
            Imports = new List<string>();
            Definitions = new List<Definition>();
            Artefacts = new List<ArtefactBase>();
        }

        public Prolog Prolog { get; set; }
        public IList<string> Imports { get; set; }
        public IList<Definition> Definitions { get; set; }
        public IList<ArtefactBase> Artefacts { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseArchitecture(this);
        }
    }

    public partial class Prolog : BaseNode
    {
        public string Author { get; set; }
        public string DateCreated { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseProlog(this);
        }
    }

    public partial class Import : BaseNode
    {
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseImport(this);
        }
    }

    public partial class Definition : BaseNode
    {
        public Identifier Name { get; set; }
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseDefinition(this);
        }
    }

    public partial class Constraint : BaseNode
    {
        public Constraint()
        {
        }

        public Identifier Subject { get; set; }
        public Expression ConstrainedTo { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseConstraint(this);
        }
    }

    public partial class Default : BaseNode
    {
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseDefault(this);
        }
    }

    public partial class PropertyDefinition : BaseNode
    {
        public Identifier Name { get; set; }
        public string Type { get; set; }
        public Constraint TypeConstraint { get; set; }
        public string Default { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CasePropertyDefinition(this);
        }
    }

    public partial class Type : BaseNode
    {
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseType(this);
        }
    }

    public partial class PropertyDeclaration : BaseNode
    {
        public Identifier Name { get; set; }
        public Expression Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CasePropertyDeclaration(this);
        }
    }

    public partial class NamedEntityBase : BaseNode
    {
        public Identifier Name { get; set; }
    }

    public partial class ArtefactBase : NamedEntityBase
    {
        public IList<Identifier> Bases { get; set; }
        public IList<PropertyDefinition> PropertyDefinitions { get; set; }
    }

    public partial class Artefact : ArtefactBase
    {
        public Artefact()
        {
            Bases = new List<Identifier>();
            PropertyDefinitions = new List<PropertyDefinition>();
            Body = new List<ArtefactBase>();
        }

        public IList<ArtefactBase> Body { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseArtefact(this);
        }
    }

    public partial class Enumeration : ArtefactBase
    {
        public Enumeration()
        {
            Values = new List<Identifier>();
        }

        public IList<Identifier> Values { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseEnum(this);
        }
    }

    public partial class Application : ArtefactBase
    {
        public Application()
        {
            Bases = new List<Identifier>();
            PropertyDefinitions = new List<PropertyDefinition>();
            PropertyDeclarations = new List<PropertyDeclaration>();
            Applications = new List<Application>();
            Dependencies = new List<Identifier>();
        }

        public string ApplicationType { get; set; }
        public IList<PropertyDeclaration> PropertyDeclarations { get; set; }
        public IList<Application> Applications { get; set; }
        public IList<Identifier> Dependencies { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseApplication(this);
        }
    }

    public partial class ApplicationInstanceDeclaration : BaseNode
    {
        public ApplicationInstanceDeclaration()
        {
            PropertyDeclarations = new List<PropertyDeclaration>();
        }
        public ConstrainedDottedIdentifier Name { get; set; }
        public IList<PropertyDeclaration> PropertyDeclarations { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseApplicationInstance(this);
        }
    }

    public partial class Network : ArtefactBase
    {
        public Network()
        {
            Servers = new List<Server>();
        }
        public Identifier DomainName { get; set; }
        public IList<Server> Servers { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseNetwork(this);
        }
    }

    public partial class Server : ArtefactBase
    {
        public Server()
        {
            PropertyDeclarations = new List<PropertyDeclaration>();
            Features = new List<ApplicationInstanceDeclaration>();
            
        }
        public Identifier DomainName { get; set; }
        public IList<PropertyDeclaration> PropertyDeclarations { get; set; }
        public IList<ApplicationInstanceDeclaration> Features { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseServer(this);
        }
    }

    public partial class Expression : BaseNode
    {
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseExpression(this);
        }
    }

    public partial class BinaryExpression : Expression
    {
        public string Op { get; set; }
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseBinaryExpression(this);
        }
    }

    public partial class PrimitiveValue : Expression
    {
        public override void Visit(IVisitor visitor)
        {
            visitor.CasePrimitiveValue(this);
        }
    }

    public partial class DateTime : PrimitiveValue
    {
        public System.DateTime Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseDateTime(this);
        }
    }

    public partial class Duration : PrimitiveValue
    {
        public TimeSpan Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseDuration(this);
        }
    }

    public partial class Real : PrimitiveValue
    {
        public decimal Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseReal(this);
        }
    }

    public partial class Guid : PrimitiveValue
    {
        public Guid Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseGuid(this);
        }
    }

    public partial class Int : PrimitiveValue
    {
        public int Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseInt(this);
        }
    }

    public partial class MyString : PrimitiveValue
    {
        public string Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseString(this);
        }
    }

    public partial class Blob : PrimitiveValue
    {
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseBlob(this);
        }
    }

    public partial class XPath : PrimitiveValue
    {
        public Identifier Source { get; set; }
        public string Path { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseXPath(this);
        }
    }

    public partial class ReferenceExpression : PrimitiveValue
    {
        public Identifier Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseIdentifier(Value);
        }
    }
}