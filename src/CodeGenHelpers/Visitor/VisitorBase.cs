using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainModel.Ast;
using DomainModel.Visitor;
using Common;

namespace Parser.Visitors
{
    public class VisitorBase : IVisitor
    {
        public Stack<Artefact> ArtefactScope = new Stack<Artefact>();
        public Stack<Application> ApplicationScope = new Stack<Application>();
        public SymbolTable SymbolTable { get; set; }

        virtual public void CaseArchitecture(DomainModel.Ast.Architecture x)
        {
            x.Prolog.Visit(this);
            x.Imports.ForEach(s => SymbolTable.Imports.Add(s));
            x.Artefacts.ForEach(ar => ar.Visit(this));
            x.Definitions.ForEach(d => d.Visit(this));
        }

        virtual public void CaseIdentifier(DomainModel.Ast.Identifier x) { }

        virtual public void CaseProlog(DomainModel.Ast.Prolog x) { }

        virtual public void CaseImport(DomainModel.Ast.Import x) { }

        virtual public void CaseDefinition(DomainModel.Ast.Definition x) { }

        virtual public void CaseConstraint(DomainModel.Ast.Constraint x) { }

        virtual public void CaseDefault(DomainModel.Ast.Default x) { }

        virtual public void CasePropertyDefinition(DomainModel.Ast.PropertyDefinition x)
        {
            x.Name.Visit(this);
            x.TypeConstraint.Visit(this);
        }

        virtual public void CaseType(DomainModel.Ast.Type x) { }

        virtual public void CasePropertyDeclaration(DomainModel.Ast.PropertyDeclaration x) 
        {
            x.Name.Visit(this);
            x.Value.Visit(this);
        }

        virtual public void CaseEnum(DomainModel.Ast.Enumeration x) {
            x.Name.Visit(this);
            x.Bases.ForEach(y => y.Visit(this));
            x.PropertyDefinitions.ForEach(y => y.Visit(this));
            x.Values.ForEach(y => y.Visit(this));
        }

        virtual public void CaseArtefact(DomainModel.Ast.Artefact x)
        {
            x.Name.Visit(this);
            x.Bases.ForEach(y => y.Visit(this)); 
            x.Body.ForEach(a => a.Visit(this));
            x.PropertyDefinitions.ForEach(y => y.Visit(this));
        }

        virtual public void CaseApplication(DomainModel.Ast.Application x)
        {
            x.Name.Visit(this);
            x.Bases.ForEach(y => y.Visit(this));
            x.Applications.ForEach(y => y.Visit(this));
            x.Dependencies.ForEach(y => y.Visit(this));
            x.PropertyDefinitions.ForEach(y => y.Visit(this));
            x.PropertyDeclarations.ForEach(y => y.Visit(this));
        }

        virtual public void CaseApplicationInstance(DomainModel.Ast.ApplicationInstanceDeclaration x)
        {
            x.Name.Visit(this);
            x.PropertyDeclarations.ForEach(y => y.Visit(this));
        }

        virtual public void CaseNetwork(DomainModel.Ast.Network x)
        {
            x.DomainName.Visit(this);
            x.Name.Visit(this);
            x.Bases.ForEach(y => y.Visit(this));
            x.PropertyDefinitions.ForEach(y => y.Visit(this));
            x.Servers.ForEach(y => y.Visit(this));
        }

        virtual public void CaseServer(DomainModel.Ast.Server x)
        {
            x.DomainName.Visit(this);
            x.Features.ForEach(y => y.Visit(this));
            x.PropertyDefinitions.ForEach(y => y.Visit(this));
            x.PropertyDeclarations.ForEach(y => y.Visit(this));
        }

        virtual public void CaseExpression(DomainModel.Ast.Expression x) { }

        virtual public void CaseBinaryExpression(DomainModel.Ast.BinaryExpression x)
        {
            x.Left.Visit(this);
            x.Right.Visit(this);
        }

        virtual public void CasePrimitiveValue(DomainModel.Ast.PrimitiveValue x) { }

        virtual public void CaseDateTime(DomainModel.Ast.DateTime x) { }

        virtual public void CaseDuration(DomainModel.Ast.Duration x) { }

        virtual public void CaseReal(DomainModel.Ast.Real x) { }

        virtual public void CaseGuid(DomainModel.Ast.Guid x) { }

        virtual public void CaseInt(DomainModel.Ast.Int x) { }

        virtual public void CaseString(DomainModel.Ast.MyString x) { }

        virtual public void CaseBlob(DomainModel.Ast.Blob x) { }

        virtual public void CaseXPath(DomainModel.Ast.XPath x) { }

        virtual public void CaseConstrainedIdentifierSegment(DomainModel.Ast.ConstrainedIdentifierSegment x)
        {
            x.IdentifierConstrained.Visit(this);
            x.Constraint.Visit(this);
        }

        virtual public void CaseConstrainedDottedIdentifier(DomainModel.Ast.ConstrainedDottedIdentifier x)
        {
            x.Value.ForEach(y => y.Visit(this));
        }
    }
}
