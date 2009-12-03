using DomainModel.Ast;

namespace DomainModel.Visitor
{
    public interface IVisitor
    {
        void CaseIdentifier(Identifier x);
        void CaseArchitecture(Ast.Architecture x);
        void CaseProlog(Ast.Prolog x);
        void CaseImport(Import x);
        void CaseDefinition(Definition x);
        void CaseConstraint(Constraint x);
        void CaseDefault(Default x);
        void CasePropertyDefinition(Ast.PropertyDefinition x);
        void CaseType(Type x);
        void CasePropertyDeclaration(Ast.PropertyDeclaration x);
        void CaseEnum(Enumeration x);
        void CaseArtefact(Ast.Artefact x);
        void CaseApplication(Ast.Application x);
        void CaseApplicationInstance(Ast.ApplicationInstanceDeclaration x);
        void CaseNetwork(Ast.Network x);
        void CaseServer(Ast.Server x);
        void CaseExpression(Ast.Expression x);
        void CaseBinaryExpression(Ast.BinaryExpression x);
        void CasePrimitiveValue(PrimitiveValue x);
        void CaseDateTime(DateTime x);
        void CaseDuration(Duration x);
        void CaseReal(Real x);
        void CaseGuid(Guid x);
        void CaseInt(Int x);
        void CaseString(MyString x);
        void CaseBlob(Blob x);
        void CaseXPath(XPath x);
        void CaseConstrainedIdentifierSegment(ConstrainedIdentifierSegment constrainedDottedIdentifier);
        void CaseConstrainedDottedIdentifier(ConstrainedDottedIdentifier constrainedDottedIdentifier);
    }
}