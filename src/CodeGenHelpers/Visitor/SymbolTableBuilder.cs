using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainModel.Visitor;
using DomainModel.Ast;
using Common;
using System.Diagnostics.Contracts;

namespace Parser.Visitors
{
    public class SymbolTableBuilder : VisitorBase
    {
        public SymbolTableBuilder()
            : this(new SymbolTable())
        {
        }

        public SymbolTableBuilder(SymbolTable st)
        {
            SymbolTable = st;
        }

        public override void CaseApplication(Application x)
        {
            base.CaseApplication(x);
        }
    }

    public class TypeCheckerVisitor : VisitorBase
    {
        public TypeCheckerVisitor()
            : this(new SymbolTable())
        {
        }

        public TypeCheckerVisitor(SymbolTable st)
        {
            SymbolTable = st;
        }
        public override void CaseArtefact(DomainModel.Ast.Artefact x)
        {
            Contract.Requires(x != null);
            Contract.Ensures(Contract.OldValue(ArtefactScope.Count) == ArtefactScope.Count);
            Contract.Requires(SymbolTable != null);
            Contract.RequiresAlways(!String.IsNullOrEmpty(x.ProperName));
            Contract.Ensures(SymbolTable.Artefacts.Count >= Contract.OldValue(SymbolTable.Artefacts.Count));

            if (x == null)
                throw new ArgumentNullException("x");
            if (string.IsNullOrEmpty(x.ProperName))
                throw new ArgumentException("x.ProperName");

            if (SymbolTable.Artefacts.ContainsKey(x.ProperName))
            {
                SymbolTable.Errors.Enqueue("(unknown location) Duplicate artefact definition for " + x.ProperName);
            }

            SymbolTable.RaiseNewArtefactEvent(x);

            if (BasesIncludesSelf(x))
            {
                SymbolTable.Errors.Enqueue(x.ProperName + " cannot derive from itself");
            }

            if (ArtefactDerivesFromAnEnclosingScopeArtefact(x, () => ArtefactScope.Cast<ArtefactBase>()))
            {
                SymbolTable.Errors.Enqueue(x.ProperName + " cannot derive from an enclosing scope artefact");
            }

            ArtefactScope.Push(x);

            foreach (var subArtefact in x.Body)
            {
                subArtefact.Visit(this);
            }

            ArtefactScope.Pop();
        }

        public override void CaseApplication(DomainModel.Ast.Application x)
        {
            Contract.Requires(x != null);
            Contract.Ensures(Contract.OldValue(ApplicationScope.Count) == ApplicationScope.Count);
            Contract.Requires(SymbolTable != null);
            Contract.RequiresAlways(!String.IsNullOrEmpty(x.ProperName));
            Contract.Ensures(SymbolTable.Applications.Count >= Contract.OldValue(SymbolTable.Applications.Count));

            if (x == null)
                throw new ArgumentNullException("x");
            if (string.IsNullOrEmpty(x.ProperName))
                throw new ArgumentException("x.ProperName");

            if (SymbolTable.Applications.ContainsKey(x.ProperName))
            {
                SymbolTable.AddError("(unknown location) Duplicate application definition for {0}", x.ProperName);
            }

            if (BasesIncludesSelf(x))
            {
                SymbolTable.AddError("{0} cannot derive from itself", x.ProperName);
            }

            if (ArtefactDerivesFromAnEnclosingScopeArtefact(x, () => ApplicationScope.Cast<ArtefactBase>()))
            {
                SymbolTable.AddError("{0} cannot derive from an enclosing scope application", x.ProperName);
            }

            ApplicationScope.Push(x);

            foreach (var subArtefact in x.Applications)
            {
                subArtefact.Visit(this);
            }

            bool allDependenciesAreKnown = x.Dependencies.All(i => IsKnownReference(i, () => SymbolTable.Applications.Values.Cast<ArtefactBase>()));
            if (!allDependenciesAreKnown)
            {
                SymbolTable.AddError("Application {0} declares a dependency that has not been defined previously", x.ProperName);
            }

            ApplicationScope.Pop();
            SymbolTable.RaiseNewApplicationEvent(x);
        }

        bool BasesIncludesSelf(ArtefactBase artefact)
        {
            foreach (var @base in artefact.Bases)
            {
                if (@base.ProperName.Equals(artefact.ProperName))
                {
                    return true;
                }
            }

            return false;
        }

        bool ArtefactDerivesFromAnEnclosingScopeArtefact(ArtefactBase artefact, Func<IEnumerable<ArtefactBase>> scopeSelector)
        {
            foreach (var baseArtefact in artefact.Bases)
            {
                if (ReferenceIsInEnclosingScope(baseArtefact, scopeSelector)) return true;
            }

            return false;
        }

        bool ReferenceIsInEnclosingScope(Identifier ident, Func<IEnumerable<ArtefactBase>> scopeSelector)
        {
            foreach (var scopeItem in scopeSelector())
            {
                if (ident.ProperName.Equals(scopeItem.ProperName))
                {
                    return true;
                }
            }
            return false;
        }

        bool IsKnownReference(Identifier ident, Func<IEnumerable<ArtefactBase>> registrySelector)
        {
            return registrySelector().Any(ab => ab.ProperName.Equals(ident.ProperName));
        }
    }
}

/*
 * Rules for the type checker
*/