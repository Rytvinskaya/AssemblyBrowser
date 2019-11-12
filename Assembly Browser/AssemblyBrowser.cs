using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Assembly_Browser
{
    public class AssemblyBrowser
    {
        public List<Container> GetAssemblyInfo(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            var types = assembly.GetTypes();
            Dictionary<string, Container> namespaces = new Dictionary<string, Container>();
            foreach (var type in types)
            {
                Container namespaceInfo = null;
                if (!namespaces.ContainsKey(type.Namespace))
                {
                    namespaceInfo = new NamespaceInformation();
                    namespaceInfo.Name = type.Namespace; 
                    namespaceInfo.FullName = type.Namespace;
                    namespaces.Add(type.Namespace, namespaceInfo);
                }
                else
                {
                    namespaces.TryGetValue(type.Namespace, out namespaceInfo);
                }
                namespaceInfo.Members.Add(GetTypeInfo(type));
            }
            return namespaces.Values.ToList();
        }

        public TypeInformation GetTypeInfo(Type type)
        {
            var typeInfo = new TypeInformation() 
            { 
                Name = type.Name, 
                FullName = type.FullName, 
                AccessModifier = GetTypeAccessorModifiers(type.GetTypeInfo())};
            typeInfo.Name = type.Name;
            var members = type.GetMembers(BindingFlags.NonPublic
                                          | BindingFlags.Instance
                                          | BindingFlags.Public
                                          | BindingFlags.Static);
            foreach (var member in members)
            {
                var memberInfo = new MemberInformation();
                if (member.MemberType == MemberTypes.Property)
                {
                    var property = (PropertyInfo)member;
                    memberInfo.Type = GetTypeName(property.PropertyType);
                    memberInfo.Name = property.Name;
                    memberInfo.DeclarationInfo = GetPropertyAccessors(property);
                    memberInfo.FullName = GetPropertyDeclaration(property);
                    memberInfo.AccessModifier = GetTypeAccessorModifiers(property.PropertyType.GetTypeInfo());

                }
                else if (member.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo)member;
                    memberInfo.Type = GetTypeName(field.FieldType);
                    memberInfo.Name = field.Name;
                    memberInfo.DeclarationInfo = memberInfo.FullName = GetFieldDeclaration(field);
                    memberInfo.AccessModifier = GetTypeAccessorModifiers(field.FieldType.GetTypeInfo());
                }
                else if (member.MemberType == MemberTypes.Event)
                {
                    var @event = (EventInfo)member;
                    memberInfo.Type = GetTypeName(@event.EventHandlerType);
                    memberInfo.Name = @event.Name;
                    memberInfo.FullName = GetEventDeclaration(@event);
                    memberInfo.DeclarationInfo = GetEventAccessors(@event);
                    memberInfo.AccessModifier = GetTypeAccessorModifiers(@event.EventHandlerType.GetTypeInfo());

                }
                else if (member.MemberType == MemberTypes.Constructor)
                {
                    var constructor = (ConstructorInfo)member;
                    memberInfo.FullName = memberInfo.Name = GetConstructorDeclaration(constructor);
                    memberInfo.Type = type.FullName;
                    memberInfo.DeclarationInfo = GetMethodParametersString(constructor.GetParameters());
                    memberInfo.AccessModifier = GetMethodModificators(constructor);
                }
                else if (member.MemberType == MemberTypes.Method)
                {
                    var method = (MethodInfo)member;
                    memberInfo.Type = GetTypeName(method.ReturnType);
                    memberInfo.Name = method.Name;
                    memberInfo.FullName = GetMethodDeclarationString(method);
                    memberInfo.DeclarationInfo = GetMethodParametersString(method.GetParameters());
                    memberInfo.AccessModifier = GetMethodModificators(method);
                }
                else
                {
                    var otherType = (TypeInfo)member;
                    memberInfo.DeclarationInfo = GetTypeDeclaration(otherType);
                    memberInfo.Type = GetTypeName(otherType.GetType());
                    memberInfo.Name = otherType.Name;
                    memberInfo.FullName = otherType.FullName;
                    memberInfo.AccessModifier = GetTypeAccessorModifiers(otherType.GetTypeInfo());

                }
                typeInfo.Members.Add(memberInfo);
            }
            return typeInfo;
        }

        private string GetTypeName(Type type)
        {
            var result = string.Format("{0}.{1}", type.Namespace, type.Name);
            if (type.IsGenericType)
            {
                result += GetGenericArgumentsString(type.GetGenericArguments());
            }
            return result;
        }

        private string GetMethodName(MethodBase method)
        {

            if (method.IsGenericMethod)
            {
                return method.Name + GetGenericArgumentsString(method.GetGenericArguments());
            }
            return method.Name;
        }

        private string GetGenericArgumentsString(Type[] arguments)
        {
            var genericArgumentsString = new StringBuilder("<");
            for (int i = 0; i < arguments.Length; i++)
            {
                genericArgumentsString.Append(GetTypeName(arguments[i]));
                if (i != arguments.Length - 1)
                {
                    genericArgumentsString.Append(", ");
                }
            }
            genericArgumentsString.Append(">");

            return genericArgumentsString.ToString();
        }

        private string GetMethodDeclarationString(MethodInfo methodInfo)
        {
            var returnType = GetTypeName(methodInfo.ReturnType);
            var parameters = methodInfo.GetParameters();
            var declaration = string.Format("{0} {1} {2} {3}",
                                            GetMethodModificators(methodInfo),
                                            returnType,
                                            GetMethodName(methodInfo),
                                            GetMethodParametersString(parameters));

            return declaration;
        }

        private string GetMethodParametersString(ParameterInfo[] parameters)
        {
            var parametersString = new StringBuilder("(");
            for (int i = 0; i < parameters.Length; i++)
            {
                parametersString.Append(GetTypeName(parameters[i].ParameterType));
                if (i != parameters.Length - 1)
                {
                    parametersString.Append(", ");
                }
            }
            parametersString.Append(")");

            return parametersString.ToString();
        }

        private string GetTypeAccessorModifiers (TypeInfo typeInfo)
        {
            var result = new StringBuilder();

            if (typeInfo.IsNestedPublic || typeInfo.IsPublic)
                result.Append("public ");
            else if (typeInfo.IsNestedPrivate)
                result.Append("private ");
            else if (typeInfo.IsNestedFamily)
                result.Append("protected ");
            else if (typeInfo.IsNestedAssembly)
                result.Append("internal ");
            else if (typeInfo.IsNestedFamORAssem)
                result.Append("protected internal ");
            else if (typeInfo.IsNestedFamANDAssem)
                result.Append("private protected ");
            else if (typeInfo.IsNotPublic)
                result.Append("private ");

            if (typeInfo.IsAbstract && typeInfo.IsSealed)
                result.Append("static ");
            else if (typeInfo.IsAbstract)
                result.Append("abstract ");
            else if (typeInfo.IsSealed)
                result.Append("sealed ");

            if (typeInfo.IsClass)
                result.Append("class ");
            else if (typeInfo.IsEnum)
                result.Append("enum ");
            else if (typeInfo.IsInterface)
                result.Append("interface ");
            else if (typeInfo.IsGenericType)
                result.Append("generic ");
            else if (typeInfo.IsValueType && !typeInfo.IsPrimitive)
                result.Append("struct ");
            return result.ToString();
        }

        private string GetTypeDeclaration(TypeInfo typeInfo)
        {
            var result = new StringBuilder();

            result.Append(GetTypeAccessorModifiers(typeInfo));

            result.Append(string.Format("{0} ", GetTypeName(typeInfo.AsType())));

            return result.ToString();
        }

        private string GetMethodModificators(MethodBase methodBase)
        {
            var result = new StringBuilder();

            if (methodBase.IsAssembly)
                result.Append("internal ");
            else if (methodBase.IsFamily)
                result.Append("protected ");
            else if (methodBase.IsFamilyOrAssembly)
                result.Append("protected internal ");
            else if (methodBase.IsFamilyAndAssembly)
                result.Append("private protected ");
            else if (methodBase.IsPrivate)
                result.Append("private ");
            else if (methodBase.IsPublic)
                result.Append("public ");

            if (methodBase.IsStatic)
                result.Append("static ");
            else if (methodBase.IsAbstract)
                result.Append("abstract ");
            else if (methodBase.IsVirtual)
                result.Append("virtual ");

            return result.ToString();
        }

        private string GetPropertyDeclaration(PropertyInfo propertyInfo)
        {
            var result = new StringBuilder(GetTypeName(propertyInfo.PropertyType));
            result.Append(" ");
            result.Append(propertyInfo.Name);

            result.Append(GetPropertyAccessors(propertyInfo));

            return result.ToString();
        }

        private string GetPropertyAccessors(PropertyInfo propertyInfo)
        {
            var result = new StringBuilder();
            var accessors = propertyInfo.GetAccessors(true);
            foreach (var accessor in accessors)
            {
                if (accessor.IsSpecialName)
                {
                    result.Append(" { ");
                    result.Append(accessor.Name);
                    result.Append(" } ");
                }
            }
            return result.ToString();
        }

        private string GetEventDeclaration(EventInfo eventInfo)
        {
            var result = new StringBuilder();
            result.Append(string.Format("{0} {1}", GetTypeName(eventInfo.EventHandlerType), eventInfo.Name));
            result.Append(string.Format(" {0} ", GetEventAccessors(eventInfo)));

            return result.ToString();
        }

        private string GetEventAccessors(EventInfo eventInfo)
        {
            var result = new StringBuilder();
            result.Append(string.Format(" [{0}] ", eventInfo.AddMethod.Name));
            result.Append(string.Format(" [{0}] ", eventInfo.RemoveMethod.Name));
            return result.ToString();
        }

        private string GetFieldDeclaration(FieldInfo fieldInfo)
        {
            var result = new StringBuilder();
            if (fieldInfo.IsAssembly)
                result.Append("internal ");
            else if (fieldInfo.IsFamily)
                result.Append("protected ");
            else if (fieldInfo.IsFamilyOrAssembly)
                result.Append("protected internal ");
            else if (fieldInfo.IsFamilyAndAssembly)
                result.Append("private protected ");
            else if (fieldInfo.IsPrivate)
                result.Append("private ");
            else if (fieldInfo.IsPublic)
                result.Append("public ");

            if (fieldInfo.IsInitOnly)
                result.Append("readonly ");
            if (fieldInfo.IsStatic)
                result.Append("static ");

            result.Append(GetTypeName(fieldInfo.FieldType));
            result.Append(" ");
            result.Append(fieldInfo.Name);

            return result.ToString();
        }

        private string GetConstructorDeclaration(ConstructorInfo constructorInfo)
        {
            return string.Format("{0} {1} {2}", GetMethodModificators(constructorInfo),
                                            GetMethodName(constructorInfo),
                                            GetMethodParametersString(constructorInfo.GetParameters()));
        }



    }
}
