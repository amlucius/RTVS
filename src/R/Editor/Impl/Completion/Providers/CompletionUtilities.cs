﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Completion.Providers {
    internal static class CompletionUtilities {
        public static string BacktickName(string name) {
            if (!string.IsNullOrEmpty(name)) {
                var t = new RTokenizer();
                var tokens = t.Tokenize(name);
                if (tokens.Count > 1) {
                    return $"`{name}`";
                }
            }
            return name;
        }
    }
}
