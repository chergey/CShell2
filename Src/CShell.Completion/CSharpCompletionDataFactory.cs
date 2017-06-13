using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CShell.Completion.DataItems;
using CShell.Completion.Images;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;

namespace CShell.Completion
{
    sealed class CSharpCompletionDataFactory : ICompletionDataFactory, IParameterCompletionDataFactory
    {
        readonly CSharpTypeResolveContext _contextAtCaret;
        private readonly CSharpCompletionContext _context;

        public CSharpCompletionDataFactory(CSharpTypeResolveContext contextAtCaret, CSharpCompletionContext context)
        {
            Debug.Assert(contextAtCaret != null);
            this._contextAtCaret = contextAtCaret;
            this._context = context;
        }

        #region ICompletionDataFactory implementation
        ICompletionData ICompletionDataFactory.CreateEntityCompletionData(IEntity entity) => new EntityCompletionData(entity);

        ICompletionData ICompletionDataFactory.CreateEntityCompletionData(IEntity entity, string text) => new EntityCompletionData(entity)
        {
            CompletionText = text,
            DisplayText = text
        };

        ICompletionData ICompletionDataFactory.CreateTypeCompletionData(IType type, bool showFullName, bool isInAttributeContext, bool addForTypeCreation)
        {
            var typeDef = type.GetDefinition();
            if (typeDef != null)
                return new EntityCompletionData(typeDef);
            else
            {
                string name = showFullName ? type.FullName : type.Name;
                if (isInAttributeContext && name.EndsWith("Attribute") && name.Length > "Attribute".Length)
                {
                    name = name.Substring(0, name.Length - "Attribute".Length);
                }
                return new CompletionData(name);
            }
        }

        ICompletionData ICompletionDataFactory.CreateMemberCompletionData(IType type, IEntity member) => new CompletionData(type.Name + "." + member.Name);

        ICompletionData ICompletionDataFactory.CreateLiteralCompletionData(string title, string description, string insertText) => new CompletionData(title)
        {
            Description = description,
            CompletionText = insertText ?? title,
            Image = CompletionImage.Literal.BaseImage,
            Priority = 2
        };

        ICompletionData ICompletionDataFactory.CreateNamespaceCompletionData(INamespace name) => new CompletionData(name.Name)
        {
            Image = CompletionImage.NamespaceImage,
        };

        ICompletionData ICompletionDataFactory.CreateVariableCompletionData(IVariable variable) => new VariableCompletionData(variable);

        ICompletionData ICompletionDataFactory.CreateVariableCompletionData(ITypeParameter parameter) => new CompletionData(parameter.Name);

        ICompletionData ICompletionDataFactory.CreateEventCreationCompletionData(string varName, IType delegateType, IEvent evt, string parameterDefinition, IUnresolvedMember currentMember, IUnresolvedTypeDefinition currentType) => new CompletionData("TODO: event creation");

        ICompletionData ICompletionDataFactory.CreateNewOverrideCompletionData(int declarationBegin, IUnresolvedTypeDefinition type, IMember m) => new OverrideCompletionData(declarationBegin, m, _contextAtCaret);

        ICompletionData ICompletionDataFactory.CreateNewPartialCompletionData(int declarationBegin, IUnresolvedTypeDefinition type, IUnresolvedMember m) => new CompletionData("TODO: partial completion");

        IEnumerable<ICompletionData> ICompletionDataFactory.CreateCodeTemplateCompletionData()
        {
            yield break;
        }

        IEnumerable<ICompletionData> ICompletionDataFactory.CreatePreProcessorDefinesCompletionData()
        {
            yield return new CompletionData("DEBUG");
            yield return new CompletionData("TEST");
        }

        ICompletionData ICompletionDataFactory.CreateImportCompletionData(IType type, bool useFullName, bool addForTypeCreation)
        {
            ITypeDefinition typeDef = type.GetDefinition();
            if (typeDef != null)
                return new ImportCompletionData(typeDef, _contextAtCaret, useFullName);
            else
                throw new InvalidOperationException("Should never happen");
        }

        ICompletionData ICompletionDataFactory.CreateFormatItemCompletionData(string format, string description, object example)
        {
            throw new NotImplementedException();
        }

        ICompletionData ICompletionDataFactory.CreateXmlDocCompletionData(string tag, string description = null, string tagInsertionText = null)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IParameterCompletionDataFactory implementation
        IParameterDataProvider CreateMethodDataProvider(int startOffset, IEnumerable<IParameterizedMember> methods) => new CSharpOverloadProvider(_context, startOffset, from m in methods where m != null select new CSharpInsightItem(m));

        IParameterDataProvider IParameterCompletionDataFactory.CreateConstructorProvider(int startOffset, IType type) => CreateMethodDataProvider(startOffset, type.GetConstructors());

        IParameterDataProvider IParameterCompletionDataFactory.CreateConstructorProvider(int startOffset, IType type, AstNode thisInitializer) => CreateMethodDataProvider(startOffset, type.GetConstructors());

        IParameterDataProvider IParameterCompletionDataFactory.CreateMethodDataProvider(int startOffset, IEnumerable<IMethod> methods) => CreateMethodDataProvider(startOffset, methods);

        IParameterDataProvider IParameterCompletionDataFactory.CreateDelegateDataProvider(int startOffset, IType type) => CreateMethodDataProvider(startOffset, new[] { type.GetDelegateInvokeMethod() });

        public IParameterDataProvider CreateIndexerParameterDataProvider(int startOffset, IType type, IEnumerable<IProperty> accessibleIndexers, AstNode resolvedNode)
        {
            throw new NotImplementedException();
            //return CreateMethodDataProvider(startOffset, accessibleIndexers);
        }

        IParameterDataProvider IParameterCompletionDataFactory.CreateTypeParameterDataProvider(int startOffset, IEnumerable<IType> types) => null;

        public IParameterDataProvider CreateTypeParameterDataProvider(int startOffset, IEnumerable<IMethod> methods) => CreateMethodDataProvider(startOffset, methods);

        #endregion

    }
}
