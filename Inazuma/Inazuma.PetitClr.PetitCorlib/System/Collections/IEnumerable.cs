// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/*============================================================
**
** Interface:  IEnumerable
** 
** 
**
**
** Purpose: Interface for classes providing IEnumerators
**
** 
===========================================================*/
namespace System.Collections {
    using System;
    using System.Runtime.InteropServices;

    // Implement this interface if you need to support VB's foreach semantics.
    // Also, COM classes that support an enumerator will also implement this interface.
#if CONTRACTS_FULL
    [ContractClass(typeof(IEnumerableContract))]
#endif // CONTRACTS_FULL
    public interface IEnumerable
    {
        // Interfaces are not serializable
        // Returns an IEnumerator for this enumerable Object.  The enumerator provides
        // a simple way to access all the contents of a collection.
        IEnumerator GetEnumerator();
    }

#if CONTRACTS_FULL
    [ContractClassFor(typeof(IEnumerable))]
    internal abstract class IEnumerableContract : IEnumerable
    {
        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            Contract.Ensures(Contract.Result<IEnumerator>() != null);
            return default(IEnumerator);
        }
    }
#endif // CONTRACTS_FULL
}
