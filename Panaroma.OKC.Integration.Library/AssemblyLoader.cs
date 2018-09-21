using System;
using System.IO;
using System.Reflection;

namespace Panaroma.OKC.Integration.Library
{
    public sealed class AssemblyLoader
    {
        private readonly Assembly _assembly;
        private Type _type;
        private object _class;
        private MethodInfo _methodInfo;

        public AssemblyLoader(string path)
        {
            if(string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new ArgumentNullException("path", "Assembly bulunamadı.");
            }

            try
            {
                _assembly = Assembly.LoadFile(path);
            }
            catch(Exception ex)
            {
                throw new Exception("Assembly yüklenirken hata oluştu.", ex);
            }
        }

        public AssemblyLoader(byte[] rawAssembly)
        {
            if(rawAssembly == null || rawAssembly.Length < 1)
            {
                throw new ArgumentNullException("rawAssembly", "Assembly bulunamadı.");
            }

            try
            {
                _assembly = Assembly.Load(rawAssembly);
            }
            catch(Exception exception)
            {
                throw new Exception("Assembly yüklenirken hata oluştu.", exception);
            }
        }

        public AssemblyLoader GetClass(string className, object[] constructorParameters)
        {
            if(string.IsNullOrEmpty(className) || string.IsNullOrWhiteSpace(className))
            {
                throw new ArgumentNullException("className", "Class ismi boş olamaz.");
            }

            try
            {
                _type = _assembly.GetType(string.Concat("Panaroma.OKC.Integration.Library.", className), true, true);
                _class = Activator.CreateInstance(_type, constructorParameters);
            }
            catch(Exception exception)
            {
                throw new Exception("Assembly yüklenirken hata oluştu.", exception);
            }

            return this;
        }

        public AssemblyLoader GetClass(string className)
        {
            if(string.IsNullOrEmpty(className) || string.IsNullOrWhiteSpace(className))
            {
                throw new ArgumentNullException("className", "Class ismi boş olamaz.");
            }

            try
            {
                _type = _assembly.GetType(string.Concat("Panaroma.OKC.Integration.Library.", className), true, true);
                _class = Activator.CreateInstance(_type);
            }
            catch(Exception exception)
            {
                throw new Exception("Assembly yüklenirken hata oluştu.", exception);
            }

            return this;
        }

        public object InvokeMethod(string methodName, Type[] methodParamTypes, object[] methodParams)
        {
            object obj;
            if(string.IsNullOrEmpty(methodName) || string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentNullException("methodName", "method ismi boş olamaz.");
            }

            try
            {
                _methodInfo = _type.GetMethod(methodName, methodParamTypes);
                obj = _methodInfo.Invoke(_class, methodParams);
            }
            catch(Exception exception)
            {
                throw new Exception("GetMethod çağrılırken hata oluştu.", exception);
            }

            return obj;
        }

        public object InvokeMethod(string methodName)
        {
            object obj;
            if(string.IsNullOrEmpty(methodName) || string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentNullException("methodName", "method ismi boş olamaz.");
            }

            try
            {
                _methodInfo = _type.GetMethod(methodName);
                obj = _methodInfo.Invoke(_class, null);
            }
            catch(Exception exception)
            {
                throw new Exception("GetMethod çağrılırken hata oluştu.", exception);
            }

            return obj;
        }
    }
}