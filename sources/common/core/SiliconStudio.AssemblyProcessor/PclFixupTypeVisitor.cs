﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Replace basic mscorlib types to their corresponding PCL types in System.Runtime.
    /// </summary>
    public class PclFixupTypeVisitor : CecilTypeReferenceVisitor
    {
        /// <summary>
        /// Initialize instance using <param name="core"/> as representing System.Runtime.
        /// </summary>
        /// <param name="core"></param>
        public PclFixupTypeVisitor(AssemblyDefinition core)
        {
            CoreAssembly = core;
        }

        /// <inheritDoc/>
        public override TypeReference Visit(TypeReference type)
        {
            // If `type' is defined in `mscorlib', we look for the same type in `System.Runtime'.
            // If we find it, this is the type we will return.
            if (type.Scope.Name == "mscorlib")
            {
                var coreType = CoreAssembly.MainModule.GetType(type.FullName);
                if (coreType != null)
                {
                    type = coreType;
                }
            }
            return base.Visit(type);
        }

        /// <summary>
        /// Update all types used in methode using our type visitor.
        /// </summary>
        /// <param name="meth">Method to update.</param>
        /// <remarks>We do in place modification of <param name="meth"/> so this might have side effects if some code relies on the original definition.</remarks>
        public void VisitMethod(MethodDefinition meth)
        {
            meth.ReturnType = Visit(meth.ReturnType);
            var nb = meth.Parameters.Count;
            for (var i = 0; i < nb; i++)
            {
                meth.Parameters[i].ParameterType = Visit(meth.Parameters[i].ParameterType);
            }
        }

        /// <summary>
        /// Reference to System.Runtime
        /// </summary>
        AssemblyDefinition CoreAssembly { get; }
    }
}
