using System.Collections.Generic;
using DomainModel.Visitor;
using System;

namespace DomainModel.Ast
{
    public class Identifier : BaseNode
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

    public class ConstrainedDottedIdentifier : BaseNode
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

    public class ConstrainedIdentifierSegment : BaseNode
    {
        public Identifier IdentifierConstrained { get; set; }
        public Expression Constraint { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseConstrainedIdentifierSegment(this);
        }
    }

    public class Architecture : BaseNode
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

    public class Author : BaseNode
    {
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseAuthor(this);
        }
    }

    public class DateCreated : BaseNode
    {
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseDateCreated(this);
        }
    }

    public class Version : BaseNode
    {
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseVersion(this);
        }
    }

    public class Description : BaseNode
    {
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseDescription(this);
        }
    }

    public class Prolog : BaseNode
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

    public class Import : BaseNode
    {
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseImport(this);
        }
    }

    public class Definition : BaseNode
    {
        public Identifier Name { get; set; }
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseDefinition(this);
        }
    }

    public class Constraint : BaseNode
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

    public class Default : BaseNode
    {
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseDefault(this);
        }
    }

    public class PropertyDefinition : BaseNode
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

    public class Type : BaseNode
    {
        public string Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseType(this);
        }
    }

    public class PropertyDeclaration : BaseNode
    {
        public Identifier Name { get; set; }
        public Expression Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CasePropertyDeclaration(this);
        }
    }

    public class ArtefactBase : BaseNode
    {
    }

    public class Artefact : ArtefactBase
    {
        public Artefact()
        {
            Bases = new List<Identifier>();
            PropertyDefinitions = new List<PropertyDefinition>();
            Body = new List<ArtefactBase>();
        }

        public Identifier Name { get; set; }
        public IList<Identifier> Bases { get; set; }
        public IList<PropertyDefinition> PropertyDefinitions { get; set; }
        public IList<ArtefactBase> Body { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseArtefact(this);
        }
    }

    public class Enumeration : ArtefactBase
    {
        public Enumeration()
        {
            Values = new List<Identifier>();
        }

        public Identifier Name { get; set; }
        public IList<Identifier> Values { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseEnum(this);
        }
    }

    public class Application : ArtefactBase
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
        public Identifier Name { get; set; }
        public IList<Identifier> Bases { get; set; }
        public IList<PropertyDefinition> PropertyDefinitions { get; set; }
        public IList<PropertyDeclaration> PropertyDeclarations { get; set; }
        public IList<Application> Applications { get; set; }
        public IList<Identifier> Dependencies { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseApplication(this);
        }
    }

    public class ApplicationInstanceDeclaration : BaseNode
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

    public class Network : ArtefactBase
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

    public class Server : BaseNode
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

    public class Expression : BaseNode
    {
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseExpression(this);
        }
    }

    public class BinaryExpression : Expression
    {
        public string Op { get; set; }
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseBinaryExpression(this);
        }
    }

    public class PrimitiveValue : Expression
    {
        public override void Visit(IVisitor visitor)
        {
            visitor.CasePrimitiveValue(this);
        }
    }

    public class DateTime : PrimitiveValue
    {
        public System.DateTime Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseDateTime(this);
        }
    }

    public class Duration : PrimitiveValue
    {
        public TimeSpan Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseDuration(this);
        }
    }

    public class Real : PrimitiveValue
    {
        public decimal Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseReal(this);
        }
    }

    public class Guid : PrimitiveValue
    {
        public Guid Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseGuid(this);
        }
    }

    public class Int : PrimitiveValue
    {
        public int Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseInt(this);
        }
    }

    public class MyString : PrimitiveValue
    {
        public string Value { get; set; }
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseString(this);
        }
    }

    public class Blob : PrimitiveValue
    {
        public override void Visit(IVisitor visitor)
        {
            visitor.CaseBlob(this);
        }
    }

    public class XPath : PrimitiveValue
    {
        public Identifier Source { get; set; }
        public string Path { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseXPath(this);
        }
    }

    public class ReferenceExpression : PrimitiveValue
    {
        public Identifier Value { get; set; }

        public override void Visit(IVisitor visitor)
        {
            visitor.CaseIdentifier(Value);
        }
    }
}