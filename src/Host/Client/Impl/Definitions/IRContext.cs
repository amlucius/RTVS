﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Representation of <c>struct RCTXT</c> in R.
    /// </summary>
    public interface IRContext {
        RContextType CallFlag { get; }
    }
}
